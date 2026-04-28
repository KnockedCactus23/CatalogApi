Feature: El usuario puede gestionar los productos

  Scenario: El usuario puede ver el listado de los productos
    Given Existen productos en la base de datos
    When hago un GET a /api/v1/products
    Then Deberia recibir status 200
    And Deberia recibir una lista de productos con sus categorias

  Scenario: El usuario puede ver cada uno de los productos
    Given Existe un producto con id conocido
    When hago un GET a /api/v1/products/el-id
    Then Deberia recibir status 200
    And Deberia recibir los datos del producto con sus categorias

  Scenario: El usuario puede crear un producto
    Given Tengo datos para crear un producto con al menos una categoria
    When hago un POST a /api/v1/products
    Then Deberia recibir status 201
    And Deberia haber creado el producto en la base de datos

  Scenario: El usuario intenta crear un producto pero el nombre ya esta en uso en la misma categoria
    Given Existe un producto con nombre "Coca-Cola" en la categoria "Refrescos"
    And Tengo datos para crear un producto con nombre "Coca-Cola" en la categoria "Refrescos"
    When hago un POST a /api/v1/products
    Then Deberia recibir status 422
    And Deberia recibir un mensaje de error que el nombre ya esta en uso para producto

  Scenario: El usuario puede editar un producto con sus categorias
    Given Existe un producto que puedo editar
    When hago un PUT a /api/v1/products/el-id con nuevos datos
    Then Deberia recibir status 200
    And Deberia haber actualizado el producto en la base de datos

  Scenario: El usuario intenta editar un producto con un nombre vacio
    Given Existe un producto que puedo editar
    When hago un PUT a /api/v1/products/el-id con nombre vacio
    Then Deberia recibir status 422

  Scenario: El usuario puede eliminar un producto
    Given Existe un producto que puedo eliminar
    When hago un DELETE a /api/v1/products/el-id
    Then Deberia recibir status 204

  Scenario: Si el usuario elimina todos los productos de una categoria ya puede eliminar la categoria
    Given Existe una categoria con un unico producto asignado
    When elimino ese producto
    And hago un DELETE a /api/v1/categories/la-categoria
    Then Deberia recibir status 204