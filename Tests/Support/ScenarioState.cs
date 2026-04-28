namespace Tests.Support;

public class ScenarioState
{
    public HttpResponseMessage? Response { get; set; }
    public int TargetCategoryId { get; set; }
    public int TargetProductId { get; set; }
    public HttpClient Client { get; set; } = null!;
    public CatalogApiFactory Factory { get; set; } = null!;
}