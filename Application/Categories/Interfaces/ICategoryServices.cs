using Application.Categories.DTOs;

namespace Application.Categories.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse?> GetByIdAsync(int id);
    Task<CategoryResponse> CreateAsync(CategoryRequest request);
    Task<CategoryResponse?> UpdateAsync(int id, CategoryRequest request);
    Task<bool> DeleteAsync(int id);
}