using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.Categories.DTOs;
using Application.Products.DTOs;
using Reqnroll;
using Tests.Support;

namespace Tests.StepDefinitions;

[Binding]
public class ProductSteps
{
    private readonly ScenarioState _state;
    private readonly ScenarioContext _scenarioContext;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public ProductSteps(ScenarioState state, ScenarioContext scenarioContext)
    {
        _state = state;
        _scenarioContext = scenarioContext;
    }

    private async Task<CategoryResponse> CreateCategoryViaApiAsync(string name)
    {
        var response = await _state.Client.PostAsJsonAsync("/api/v1/categories",
            new CategoryRequest(name, null));
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

    [Given("Existen productos en la base de datos")]
    public async Task GivenExistenProductos()
    {
        var cat = await CreateCategoryViaApiAsync("Cat-Listado");
        _state.TargetCategoryId = cat.Id;
        await CreateProductViaApiAsync("Producto A", new List<int> { cat.Id });
        await CreateProductViaApiAsync("Producto B", new List<int> { cat.Id });
    }

    [Given("Existe un producto con id conocido")]
    public async Task GivenExisteUnProducto()
    {
        var cat = await CreateCategoryViaApiAsync("Cat-GetById");
        var product = await CreateProductViaApiAsync("Producto Known", new List<int> { cat.Id });
        _state.TargetProductId = product.Id;
        _state.TargetCategoryId = cat.Id;
    }

    [Given("Tengo datos para crear un producto con al menos una categoria")]
    public async Task GivenTengoDatosProducto()
    {
        var cat = await CreateCategoryViaApiAsync("Cat-Create");
        _state.TargetCategoryId = cat.Id;
    }

    [Given(@"Existe un producto con nombre ""(.*)"" en la categoria ""(.*)""")]
    public async Task GivenProductoConNombreEnCategoria(string nombre, string categoria)
    {
        var cat = await CreateCategoryViaApiAsync(categoria);
        _state.TargetCategoryId = cat.Id;
        await CreateProductViaApiAsync(nombre, new List<int> { cat.Id });
    }

    [Given(@"Tengo datos para crear un producto con nombre ""(.*)"" en la categoria ""(.*)""")]
    public void GivenDatosConNombreEnCategoria(string nombre, string categoria)
    {
        _scenarioContext["productName"] = nombre;
    }

    [Given("Existe un producto que puedo editar")]
    public async Task GivenProductoParaEditar()
    {
        var cat = await CreateCategoryViaApiAsync("Cat-Edit");
        _state.TargetCategoryId = cat.Id;
        var product = await CreateProductViaApiAsync("Editable", new List<int> { cat.Id });
        _state.TargetProductId = product.Id;
    }

    [Given("Existe un producto que puedo eliminar")]
    public async Task GivenProductoParaEliminar()
    {
        var cat = await CreateCategoryViaApiAsync("Cat-Delete");
        _state.TargetCategoryId = cat.Id;
        var product = await CreateProductViaApiAsync("Eliminable", new List<int> { cat.Id });
        _state.TargetProductId = product.Id;
    }

    [Given("Existe una categoria con un unico producto asignado")]
    public async Task GivenCategoriaConUnicoProducto()
    {
        var cat = await CreateCategoryViaApiAsync("Cat-UnicoProducto");
        _state.TargetCategoryId = cat.Id;
        var product = await CreateProductViaApiAsync("Unico", new List<int> { cat.Id });
        _state.TargetProductId = product.Id;
        _scenarioContext["categoryIdToDelete"] = cat.Id;
    }

    [When(@"hago un GET a \/api\/v1\/products")]
    public async Task WhenGetAll()
    {
        _state.Response = await _state.Client.GetAsync("/api/v1/products");
    }

    [When(@"hago un GET a \/api\/v1\/products\/el-id")]
    public async Task WhenGetById()
    {
        _state.Response = await _state.Client.GetAsync($"/api/v1/products/{_state.TargetProductId}");
    }

    [When(@"hago un POST a \/api\/v1\/products")]
    public async Task WhenPost()
    {
        var name = _scenarioContext.ContainsKey("productName")
            ? (string)_scenarioContext["productName"]
            : "Nuevo Producto";

        _state.Response = await _state.Client.PostAsJsonAsync("/api/v1/products",
            new ProductRequest(name, null, new List<int> { _state.TargetCategoryId }));
    }

    [When(@"hago un PUT a \/api\/v1\/products\/el-id con nuevos datos")]
    public async Task WhenPutWithData()
    {
        _state.Response = await _state.Client.PutAsJsonAsync($"/api/v1/products/{_state.TargetProductId}",
            new ProductRequest("Producto Actualizado", "Nueva descripción", new List<int> { _state.TargetCategoryId }));
    }

    [When(@"hago un PUT a \/api\/v1\/products\/el-id con nombre vacio")]
    public async Task WhenPutEmptyName()
    {
        _state.Response = await _state.Client.PutAsJsonAsync($"/api/v1/products/{_state.TargetProductId}",
            new ProductRequest("", null, new List<int> { _state.TargetCategoryId }));
    }

    [When(@"hago un DELETE a \/api\/v1\/products\/el-id")]
    public async Task WhenDeleteProduct()
    {
        _state.Response = await _state.Client.DeleteAsync($"/api/v1/products/{_state.TargetProductId}");
    }

    [When("elimino ese producto")]
    public async Task WhenEliminoEseProducto()
    {
        await _state.Client.DeleteAsync($"/api/v1/products/{_state.TargetProductId}");
    }

    [When(@"hago un DELETE a \/api\/v1\/categories\/la-categoria")]
    public async Task WhenDeleteCategoria()
    {
        var catId = (int)_scenarioContext["categoryIdToDelete"];
        _state.Response = await _state.Client.DeleteAsync($"/api/v1/categories/{catId}");
    }

    [Then("Deberia recibir una lista de productos con sus categorias")]
    public async Task ThenListaConCategorias()
    {
        var json = await _state.Response!.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<ProductResponse>>(json, JsonOpts);
        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.All(list!, p => Assert.NotEmpty(p.Categories));
    }

    [Then("Deberia recibir los datos del producto con sus categorias")]
    public async Task ThenProductoConCategorias()
    {
        var product = await _state.Response!.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(product);
        Assert.Equal(_state.TargetProductId, product!.Id);
        Assert.NotEmpty(product.Categories);
        Assert.All(product.Categories, c => Assert.False(string.IsNullOrEmpty(c.Name)));
    }

    [Then("Deberia haber creado el producto en la base de datos")]
    public async Task ThenProductoCreado()
    {
        var created = await _state.Response!.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.NotEmpty(created.Categories);
    }

    [Then("Deberia recibir un mensaje de error que el nombre ya esta en uso para producto")]
    public async Task ThenMensajeNombreEnUsoProducto()
    {
        var body = await _state.Response!.Content.ReadAsStringAsync();
        Assert.Contains("ya existe", body, StringComparison.OrdinalIgnoreCase);
    }

    [Then("Deberia haber actualizado el producto en la base de datos")]
    public async Task ThenProductoActualizado()
    {
        var updated = await _state.Response!.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.Equal("Producto Actualizado", updated!.Name);
    }
}