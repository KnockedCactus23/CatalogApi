# CatalogApi

REST API para gestión de catálogo de productos y categorías, desarrollada con .NET 8 siguiendo una arquitectura limpia por capas.

## Tecnologías

- **.NET 8** — Framework principal
- **ASP.NET Core Web API** — Capa HTTP
- **Entity Framework Core** — ORM para acceso a datos
- **SQL Server** — Base de datos (vía Docker)
- **FluentValidation** — Validación de requests
- **Reqnroll + xUnit** — Tests de integración BDD

## Arquitectura

El proyecto está dividido en cuatro capas:

```
CatalogApi/
├── Api/               # Controllers, Program.cs, configuración HTTP
├── Application/       # DTOs, interfaces, validadores, excepciones de dominio
├── Domain/            # Entidades de negocio (Category, Product, ProductCategory)
├── Infrastructure/    # Implementación de servicios, DbContext, migraciones
└── Tests/             # Features .feature + StepDefinitions (Reqnroll + xUnit)
```

## Entidades

### Category
| Campo       | Tipo    | Descripción              |
|-------------|---------|--------------------------|
| Id          | int     | Identificador único      |
| Name        | string  | Nombre único requerido   |
| Description | string? | Descripción opcional     |

### Product
| Campo       | Tipo    | Descripción                        |
|-------------|---------|------------------------------------|
| Id          | int     | Identificador único                |
| Name        | string  | Nombre único por categoría         |
| Description | string? | Descripción opcional               |
| Categories  | lista   | Categorías a las que pertenece     |

La relación entre productos y categorías es **muchos a muchos** — un producto puede pertenecer a N categorías y una categoría puede tener N productos.

## Endpoints

### Categorías

| Método | Ruta                        | Descripción                        | Status |
|--------|-----------------------------|------------------------------------|--------|
| GET    | /api/v1/categories          | Listar todas las categorías        | 200    |
| GET    | /api/v1/categories/{id}     | Obtener una categoría por ID       | 200    |
| POST   | /api/v1/categories          | Crear una categoría                | 201    |
| PUT    | /api/v1/categories/{id}     | Editar una categoría               | 200    |
| DELETE | /api/v1/categories/{id}     | Eliminar una categoría             | 204    |

### Productos

| Método | Ruta                        | Descripción                        | Status |
|--------|-----------------------------|------------------------------------|--------|
| GET    | /api/v1/products            | Listar todos los productos         | 200    |
| GET    | /api/v1/products/{id}       | Obtener un producto por ID         | 200    |
| POST   | /api/v1/products            | Crear un producto                  | 201    |
| PUT    | /api/v1/products/{id}       | Editar un producto y sus categorías| 200    |
| DELETE | /api/v1/products/{id}       | Eliminar un producto               | 204    |

### Reglas de negocio

- El nombre de una categoría es único globalmente.
- El nombre de un producto es único dentro de cada categoría.
- No se puede eliminar una categoría que tenga productos asignados.
- Un producto debe pertenecer a al menos una categoría.
- Al ver un producto, la respuesta incluye el nombre de sus categorías (no solo el ID).

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (para SQL Server)
- [dotnet-ef](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) instalado globalmente

```bash
dotnet tool install --global dotnet-ef
```

## Configuración local

### 1. Clonar el repositorio

```bash
git clone https://github.com/TU_USUARIO/CatalogApi.git
cd CatalogApi
```

### 2. Levantar SQL Server con Docker

```bash
docker run -d \
  --name sqlserver-catalog \
  -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=Your_password123" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server
```

### 3. Configurar cadena de conexión

En `Api/appsettings.json` verifica que la cadena de conexión apunte a tu instancia:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=CatalogDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True"
  }
}
```

### 4. Aplicar migraciones

```bash
dotnet ef database update --project Infrastructure --startup-project Api
```

### 5. Correr la API

```bash
cd Api
dotnet run
```

La API estará disponible en `https://localhost:5001`. Swagger UI en `https://localhost:5001/swagger`.

## Correr los tests

Los tests de integración usan una base de datos en memoria y **no requieren Docker ni la API corriendo**.

```bash
cd Tests
dotnet test
```

Resultado esperado:

```
Correctas! - Con error: 0, Superado: 17, Omitido: 0, Total: 17
```

### Escenarios cubiertos

**Categorías**
- Ver listado de categorías
- Ver una categoría por ID
- Crear una categoría
- Intentar crear una categoría con nombre duplicado → 422
- Editar una categoría
- Intentar editar con nombre vacío → 422
- Eliminar una categoría sin productos
- Eliminar una categoría después de eliminar sus productos

**Productos**
- Ver listado de productos con sus categorías
- Ver un producto por ID con sus categorías
- Crear un producto
- Intentar crear un producto con nombre duplicado en la misma categoría → 422
- Editar un producto y sus categorías
- Intentar editar con nombre vacío → 422
- Eliminar un producto
- Eliminar todos los productos de una categoría y luego eliminar la categoría

## Uso diario

Para retomar el trabajo después de apagar la máquina:

```bash
# Iniciar SQL Server
docker start sqlserver-catalog

# Correr la API
cd Api
dotnet run
```

Para detener:

```bash
# Ctrl+C para detener la API
docker stop sqlserver-catalog
```
