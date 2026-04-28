namespace Application.Products.DTOs;

public record ProductRequest(string Name, string? Description, List<int> CategoryIds);

public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    List<CategorySummary> Categories);

public record CategorySummary(int Id, string Name);