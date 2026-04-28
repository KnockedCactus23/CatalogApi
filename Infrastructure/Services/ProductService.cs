using Application.Common.Exceptions;
using Application.Products.DTOs;
using Application.Products.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        var products = await _db.Products
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .ToListAsync();

        return products.Select(p => MapToResponse(p)).ToList();
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        var product = await _db.Products
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : MapToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(ProductRequest request)
    {
        var categories = await ValidateAndGetCategoriesAsync(request.CategoryIds);
        await EnsureNameIsUniqueInCategoriesAsync(request.Name, request.CategoryIds);

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            ProductCategories = request.CategoryIds
                .Select(cId => new ProductCategory { CategoryId = cId })
                .ToList()
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return MapToResponse(product, categories);
    }

    public async Task<ProductResponse?> UpdateAsync(int id, ProductRequest request)
    {
        var product = await _db.Products
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null) return null;

        var categories = await ValidateAndGetCategoriesAsync(request.CategoryIds);
        await EnsureNameIsUniqueInCategoriesAsync(request.Name, request.CategoryIds, excludeProductId: id);

        product.Name = request.Name;
        product.Description = request.Description;

        // Replace categories
        _db.ProductCategories.RemoveRange(product.ProductCategories);
        product.ProductCategories = request.CategoryIds
            .Select(cId => new ProductCategory { ProductId = id, CategoryId = cId })
            .ToList();

        await _db.SaveChangesAsync();

        return MapToResponse(product, categories);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _db.Products
            .Include(p => p.ProductCategories)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null) return false;

        _db.ProductCategories.RemoveRange(product.ProductCategories);
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<List<Category>> ValidateAndGetCategoriesAsync(List<int> categoryIds)
    {
        var categories = await _db.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync();

        var missingIds = categoryIds.Except(categories.Select(c => c.Id)).ToList();
        if (missingIds.Any())
            throw new NotFoundException(
                $"Las siguientes categorías no existen: {string.Join(", ", missingIds)}.");

        return categories;
    }

    private async Task EnsureNameIsUniqueInCategoriesAsync(
        string name, List<int> categoryIds, int? excludeProductId = null)
    {
        // A product name must be unique within each of its assigned categories
        var conflict = await _db.ProductCategories
            .Where(pc => categoryIds.Contains(pc.CategoryId)
                      && pc.Product.Name == name
                      && (excludeProductId == null || pc.ProductId != excludeProductId))
            .AnyAsync();

        if (conflict)
            throw new UnprocessableEntityException(
                $"Ya existe un producto con el nombre '{name}' en una de las categorías seleccionadas.");
    }

    private static ProductResponse MapToResponse(Product product, List<Category>? categories = null)
    {
        var cats = categories
            ?? product.ProductCategories.Select(pc => pc.Category).ToList();

        return new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            cats.Select(c => new CategorySummary(c.Id, c.Name)).ToList());
    }
}