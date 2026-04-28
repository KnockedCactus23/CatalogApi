Feature: El usuario puede gestionar las categorias

  Scenario: El usuario puede ver el listado de las categorias
    Given Existen categorias en la base de datos
    When hago un GET a /api/v1/categories
    Then Deberia recibir status 200
    And Deberia recibir una lista de categorias

  Scenario: El usuario puede ver cada una de las categorias
    Given Existe una categoria con id conocido
    When hago un GET a /api/v1/categories/el-id
    Then Deberia recibir status 200
    And Deberia recibir los datos de la categoria

  Scenario: El usuario puede crear una categoria
    Given Tengo datos para crear una categoria
    When hago un POST a /api/v1/categories
    Then Deberia recibir status 201
    And Deberia haber creado la categoria en la base de datos

  Scenario: El usuario intenta crear una categoria pero el nombre ya esta en uso
    Given Tengo una categoria con nombre "Refrescos"
    And Tengo datos para crear una categoria con nombre "Refrescos"
    When hago un POST a /api/v1/categories
    Then Deberia recibir status 422
    And Deberia recibir un mensaje de error que el nombre ya esta en uso para categoria

  Scenario: El usuario puede editar la categoria
    Given Existe una categoria que puedo editar
    When hago un PUT a /api/v1/categories/el-id con nuevos datos
    Then Deberia recibir status 200
    And Deberia haber actualizado la categoria en la base de datos

  Scenario: El usuario intenta editar la categoria con un nombre vacio
    Given Existe una categoria que puedo editar
    When hago un PUT a /api/v1/categories/el-id con nombre vacio
    Then Deberia recibir status 422

  Scenario: El usuario puede destruir una categoria
    Given Existe una categoria sin productos asignados
    When hago un DELETE a /api/v1/categories/el-id
    Then Deberia recibir status 204

  Scenario: El usuario intenta destruir una categoria pero tiene productos asignados. Entonces eliminas los productos y ya puedes eliminar la categoria
    Given Existe una categoria con productos asignados
    When elimino todos los productos de la categoria
    And hago un DELETE a /api/v1/categories/el-id
    Then Deberia recibir status 204