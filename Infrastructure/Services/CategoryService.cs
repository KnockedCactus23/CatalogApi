using Application.Categories.DTOs;
using Application.Categories.Interfaces;
using Application.Common.Exceptions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        return await _db.Categories
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .ToListAsync();
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        return category is null ? null : new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryResponse> CreateAsync(CategoryRequest request)
    {
        await EnsureNameIsUniqueAsync(request.Name);

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryResponse?> UpdateAsync(int id, CategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return null;

        await EnsureNameIsUniqueAsync(request.Name, excludeId: id);

        category.Name = request.Name;
        category.Description = request.Description;

        await _db.SaveChangesAsync();

        return new CategoryResponse(category.Id, category.Name, category.Description);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _db.Categories
            .Include(c => c.ProductCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null) return false;

        if (category.ProductCategories.Any())
            throw new UnprocessableEntityException(
                "No se puede eliminar una categoría que tiene productos asignados.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task EnsureNameIsUniqueAsync(string name, int? excludeId = null)
    {
        var exists = await _db.Categories
            .AnyAsync(c => c.Name == name && (excludeId == null || c.Id != excludeId));

        if (exists)
            throw new UnprocessableEntityException($"El nombre '{name}' ya está en uso.");
    }
}