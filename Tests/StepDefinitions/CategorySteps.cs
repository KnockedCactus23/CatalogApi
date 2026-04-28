using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.Categories.DTOs;
using Application.Products.DTOs;
using Reqnroll;
using Tests.Support;

namespace Tests.StepDefinitions;

[Binding]
public class CategorySteps
{
    private readonly ScenarioState _state;
    private readonly ScenarioContext _scenarioContext;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public CategorySteps(ScenarioState state, ScenarioContext scenarioContext)
    {
        _state = state;
        _scenarioContext = scenarioContext;
    }

    private async Task<CategoryResponse> CreateCategoryViaApiAsync(string name, string? description = null)
    {
        var response = await _state.Client.PostAsJsonAsync("/api/v1/categories",
            new CategoryRequest(name, description));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CategoryResponse>())!;
    }

    private async Task<ProductResponse> CreateProductViaApiAsync(string name, List<int> categoryIds)
    {
        var response = await _state.Client.PostAsJsonAsync("/api/v1/products",
            new ProductRequest(name, null, categoryIds));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    [Given("Existen categorias en la base de datos")]
    public async Task GivenExistenCategorias()
    {
        await CreateCategoryViaApiAsync("Bebidas", "Líquidos");
        await CreateCategoryViaApiAsync("Snacks");
    }

    [Given("Existe una categoria con id conocido")]
    public async Task GivenExisteUnaCategoriaConId()
    {
        var cat = await CreateCategoryViaApiAsync("TestCat");
        _state.TargetCategoryId = cat.Id;
    }

    [Given("Tengo datos para crear una categoria")]
    public void GivenTengoDatosParaCrearCategoria() { }

    [Given(@"Tengo una categoria con nombre ""(.*)""")]
    public async Task GivenTengoUnaCategoriaConNombre(string nombre)
    {
        await CreateCategoryViaApiAsync(nombre);
    }

    [Given(@"Tengo datos para crear una categoria con nombre ""(.*)""")]
    public void GivenTengoDatosConNombre(string nombre)
    {
        _scenarioContext["categoryName"] = nombre;
    }

    [Given("Existe una categoria que puedo editar")]
    public async Task GivenExisteUnaCategoriaParaEditar()
    {
        var cat = await CreateCategoryViaApiAsync("Editable");
        _state.TargetCategoryId = cat.Id;
    }

    [Given("Existe una categoria sin productos asignados")]
    public async Task GivenExisteCategoríaSinProductos()
    {
        var cat = await CreateCategoryViaApiAsync("SinProductos");
        _state.TargetCategoryId = cat.Id;
    }

    [Given("Existe una categoria con productos asignados")]
    public async Task GivenExisteCategoriaConProductos()
    {
        var cat = await CreateCategoryViaApiAsync("ConProductos");
        var product = await CreateProductViaApiAsync("Producto1", new List<int> { cat.Id });
        _state.TargetCategoryId = cat.Id;
        _scenarioContext["productId"] = product.Id;
    }

    [When(@"hago un GET a \/api\/v1\/categories")]
    public async Task WhenGetAll()
    {
        _state.Response = await _state.Client.GetAsync("/api/v1/categories");
    }

    [When(@"hago un GET a \/api\/v1\/categories\/el-id")]
    public async Task WhenGetById()
    {
        _state.Response = await _state.Client.GetAsync($"/api/v1/categories/{_state.TargetCategoryId}");
    }

    [When(@"hago un POST a \/api\/v1\/categories")]
    public async Task WhenPost()
    {
        var name = _scenarioContext.ContainsKey("categoryName")
            ? (string)_scenarioContext["categoryName"]
            : "Nueva Categoria";

        _state.Response = await _state.Client.PostAsJsonAsync("/api/v1/categories",
            new CategoryRequest(name, "Descripción de prueba"));
    }

    [When(@"hago un PUT a \/api\/v1\/categories\/el-id con nuevos datos")]
    public async Task WhenPutWithData()
    {
        _state.Response = await _state.Client.PutAsJsonAsync($"/api/v1/categories/{_state.TargetCategoryId}",
            new CategoryRequest("Nombre Actualizado", "Descripción actualizada"));
    }

    [When(@"hago un PUT a \/api\/v1\/categories\/el-id con nombre vacio")]
    public async Task WhenPutWithEmptyName()
    {
        _state.Response = await _state.Client.PutAsJsonAsync($"/api/v1/categories/{_state.TargetCategoryId}",
            new CategoryRequest("", null));
    }

    [When(@"hago un DELETE a \/api\/v1\/categories\/el-id")]
    public async Task WhenDelete()
    {
        _state.Response = await _state.Client.DeleteAsync($"/api/v1/categories/{_state.TargetCategoryId}");
    }

    [When("elimino todos los productos de la categoria")]
    public async Task WhenEliminoProductosDeLaCategoria()
    {
        var productId = (int)_scenarioContext["productId"];
        await _state.Client.DeleteAsync($"/api/v1/products/{productId}");
    }

    [Then("Deberia recibir status 200")]
    public void ThenStatus200() => Assert.Equal(HttpStatusCode.OK, _state.Response!.StatusCode);

    [Then("Deberia recibir status 201")]
    public void ThenStatus201() => Assert.Equal(HttpStatusCode.Created, _state.Response!.StatusCode);

    [Then("Deberia recibir status 204")]
    public void ThenStatus204() => Assert.Equal(HttpStatusCode.NoContent, _state.Response!.StatusCode);

    [Then("Deberia recibir status 422")]
    public void ThenStatus422() => Assert.Equal(HttpStatusCode.UnprocessableEntity, _state.Response!.StatusCode);

    [Then("Deberia recibir una lista de categorias")]
    public async Task ThenReciboListaDeCategorias()
    {
        var json = await _state.Response!.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<CategoryResponse>>(json, JsonOpts);
        Assert.NotNull(list);
        Assert.NotEmpty(list);
    }

    [Then("Deberia recibir los datos de la categoria")]
    public async Task ThenReciboLaCategoria()
    {
        var cat = await _state.Response!.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(cat);
        Assert.Equal(_state.TargetCategoryId, cat!.Id);
    }

    [Then("Deberia haber creado la categoria en la base de datos")]
    public async Task ThenCategoriaCreada()
    {
        var created = await _state.Response!.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
    }

    [Then("Deberia recibir un mensaje de error que el nombre ya esta en uso para categoria")]
    public async Task ThenMensajeNombreEnUsoCategoria()
    {
        var body = await _state.Response!.Content.ReadAsStringAsync();
        Assert.Contains("ya está en uso", body, StringComparison.OrdinalIgnoreCase);
    }

    [Then("Deberia haber actualizado la categoria en la base de datos")]
    public async Task ThenCategoriaActualizada()
    {
        var updated = await _state.Response!.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.Equal("Nombre Actualizado", updated!.Name);
    }
}