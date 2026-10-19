# 📚 CONTRATO DE API - MarketGO Backend

**Backend:** .NET 10 + Entity Framework Core + MongoDB  
**Formato de Datos:** JSON (Serialización con propiedades en **PascalCase**)  
**Autenticación:** JWT Token Bearer  
**Base URL:** `/api`

---

## 🔐 NOTA IMPORTANTE SOBRE SERIALIZACIÓN JSON

Todos los atributos en las respuestas JSON siguen el patrón **PascalCase** (primera letra mayúscula). Por ejemplo:
- `NombreUsuario` (no `nombreUsuario`)
- `FechaPedido` (no `fechaPedido`)
- `EsPredeterminada` (no `isPredeterminada`)

Esto es importante para que el frontend en TypeScript cree sus interfaces de manera exacta.

---

## 📋 TABLA DE CONTENIDOS

1. [Usuarios (Auth/Registro)](#1-usuarios-authregistro)
2. [Productos (Catálogo)](#2-productos-catálogo)
3. [Publicaciones de Inventario](#3-publicaciones-de-inventario)
4. [Pedidos](#4-pedidos)
5. [Colecciones](#5-colecciones)
6. [Franquicias](#6-franquicias)
7. [Direcciones](#7-direcciones)

---

## 1️⃣ USUARIOS (AUTH/REGISTRO)

**Ruta Base:** `/api/Usuarios`

### 1.1 POST - Registrar Usuario

**Endpoint:** `POST /api/Usuarios`  
**Autenticación:** NO requerida  
**Propósito:** Crear una nueva cuenta de usuario

**Body esperado (Input):**
```json
{
  "nombreUsuario": "string (5-50 caracteres)",
  "correo": "string (email válido)",
  "password": "string (5-100 caracteres)"
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "El Usuario fue creado con exito",
  "id": "string (ObjectId de MongoDB)"
}
```

**Códigos de Respuesta:**
- `200 OK` - Registro exitoso
- `400 Bad Request` - Email o nombre de usuario ya existen, validación fallida

---

### 1.2 POST - Login (Autenticación)

**Endpoint:** `POST /api/Usuarios/login`  
**Autenticación:** NO requerida  
**Propósito:** Obtener token JWT para acceso autorizado

**Body esperado (Input):**
```json
{
  "correo": "string (email válido)",
  "password": "string"
}
```

**Respuesta esperada (Output):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "usuario": {
    "id": "string (ObjectId)",
    "nombreUsuario": "string",
    "rol": "string (Cliente|Admin)"
  }
}
```

**Códigos de Respuesta:**
- `200 OK` - Login exitoso
- `400 Bad Request` - Credenciales incorrectas

**Nota:** El token expira en **2 horas**.

---

### 1.3 GET - Obtener Todos los Usuarios

**Endpoint:** `GET /api/Usuarios`  
**Autenticación:** **REQUERIDA** (Rol: Admin)  
**Propósito:** Listar todos los usuarios del sistema

**Parámetros:** Ninguno

**Respuesta esperada (Output):**
```json
[
  {
    "id": "string (ObjectId)",
    "nombreUsuario": "string",
    "correo": "string",
    "rol": "string (Cliente|Admin)",
    "fechaRegistro": "2025-01-15T10:30:00.000Z"
  },
  ...
]
```

**Códigos de Respuesta:**
- `200 OK` - Lista obtenida
- `401 Unauthorized` - Token inválido o no autorizado
- `403 Forbidden` - No es Admin

---

### 1.4 GET - Obtener Usuario por ID

**Endpoint:** `GET /api/Usuarios/{id}`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Obtener datos de un usuario específico

**Parámetros de Ruta:**
- `id` (string) - ObjectId del usuario

**Respuesta esperada (Output):**
```json
{
  "id": "string (ObjectId)",
  "nombreUsuario": "string",
  "correo": "string",
  "rol": "string (Cliente|Admin)",
  "fechaRegistro": "2025-01-15T10:30:00.000Z"
}
```

**Códigos de Respuesta:**
- `200 OK` - Usuario encontrado
- `401 Unauthorized` - Token inválido
- `404 Not Found` - Usuario no existe

---

### 1.5 PUT - Editar Usuario

**Endpoint:** `PUT /api/Usuarios/{id}`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Actualizar nombre de usuario y correo

**Parámetros de Ruta:**
- `id` (string) - ObjectId del usuario

**Body esperado (Input):**
```json
{
  "nombreUsuario": "string (5-50 caracteres)",
  "correo": "string (email válido)"
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "El usuario fue editado con exito"
}
```

**Códigos de Respuesta:**
- `200 OK` - Actualización exitosa
- `400 Bad Request` - Validación fallida
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No es el dueño de la cuenta (solo Admins pueden editar otros)
- `404 Not Found` - Usuario no existe
- `500 Internal Server Error` - Error en el servidor

---

### 1.6 DELETE - Eliminar Usuario

**Endpoint:** `DELETE /api/Usuarios/{id}`  
**Autenticación:** **REQUERIDA** (Rol: Admin)  
**Propósito:** Eliminar un usuario del sistema

**Parámetros de Ruta:**
- `id` (string) - ObjectId del usuario

**Respuesta esperada (Output):**
```json
{
  "message": "El Usuario ha sido eliminado!!"
}
```

**Códigos de Respuesta:**
- `200 OK` - Eliminación exitosa
- `400 Bad Request` - No puedes eliminar tu propia cuenta
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No es Admin
- `404 Not Found` - Usuario no existe

---

## 2️⃣ PRODUCTOS (CATÁLOGO)

**Ruta Base:** `/api/Productos`

### 2.1 GET - Obtener Todos los Productos

**Endpoint:** `GET /api/Productos`  
**Autenticación:** NO requerida  
**Propósito:** Listar todos los productos del catálogo

**Parámetros:** Ninguno

**Respuesta esperada (Output):**
```json
[
  {
    "id": "string (ObjectId)",
    "nombreColeccion": "string",
    "nombreProducto": "string",
    "sku": "string",
    "tipoProducto": "string",
    "imagenUrl": "string (URL)"
  },
  ...
]
```

**Códigos de Respuesta:**
- `200 OK` - Lista obtenida

---

### 2.2 POST - Crear Producto

**Endpoint:** `POST /api/Productos`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Crear un nuevo producto en el catálogo

**Body esperado (Input):**
```json
{
  "coleccionId": "string (ObjectId válido)",
  "nombreProducto": "string (5-100 caracteres)",
  "sku": "string (3-30 caracteres)",
  "tipoProducto": "string (máx 50 caracteres)",
  "imagenUrl": "string (URL válida https://...)"
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "Producto creado exitosamente",
  "id": "string (ObjectId)"
}
```

**Códigos de Respuesta:**
- `200 OK` - Producto creado
- `400 Bad Request` - Validación fallida o ColeccionId inválido
- `401 Unauthorized` - Token inválido

---

### 2.3 PUT - Editar Producto

**Endpoint:** `PUT /api/Productos/{id}`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Actualizar datos de un producto

**Parámetros de Ruta:**
- `id` (string) - ObjectId del producto

**Body esperado (Input):**
```json
{
  "coleccionId": "string (ObjectId válido)",
  "nombreProducto": "string (5-100 caracteres)",
  "sku": "string (3-30 caracteres)",
  "tipoProducto": "string (máx 50 caracteres)",
  "imagenUrl": "string (URL válida)"
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "Producto editado exitosamente",
  "id": "string (ObjectId)"
}
```

**Códigos de Respuesta:**
- `200 OK` - Actualización exitosa
- `400 Bad Request` - Validación fallida
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No eres el dueño del producto (solo Admins pueden editar de otros)
- `404 Not Found` - Producto no existe
- `500 Internal Server Error` - Error en el servidor

---

### 2.4 DELETE - Eliminar Producto

**Endpoint:** `DELETE /api/Productos/{id}`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Eliminar un producto del catálogo

**Parámetros de Ruta:**
- `id` (string) - ObjectId del producto

**Respuesta esperada (Output):**
```json
{
  "message": "Producto eliminado exitosamente",
  "id": "string (ObjectId)"
}
```

**Códigos de Respuesta:**
- `200 OK` - Eliminación exitosa
- `400 Bad Request` - Error al eliminar
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No eres el dueño del producto (solo Admins pueden eliminar de otros)
- `404 Not Found` - Producto no existe

---

## 3️⃣ PUBLICACIONES DE INVENTARIO

**Ruta Base:** `/api/PublicacionesInventario`

### 3.1 GET - Obtener Todas las Publicaciones

**Endpoint:** `GET /api/PublicacionesInventario`  
**Autenticación:** NO requerida  
**Propósito:** Listar todas las publicaciones de inventario disponibles

**Parámetros:** Ninguno

**Respuesta esperada (Output):**
```json
[
  {
    "id": "string (ObjectId)",
    "productoCatalogoId": "string (ObjectId)",
    "coleccionId": "string (ObjectId)",
    "sku": "string",
    "tipoProducto": "string",
    "nombreProducto": "string",
    "imagenUrl": "string (URL)",
    "nombreVendedor": "string",
    "estado": "string",
    "precio": 99.99,
    "notas": "string"
  },
  ...
]
```

**Códigos de Respuesta:**
- `200 OK` - Lista obtenida

---

### 3.2 GET - Mis Publicaciones

**Endpoint:** `GET /api/PublicacionesInventario/MisPublicaciones`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Obtener solo las publicaciones del usuario logueado

**Parámetros:** Ninguno

**Respuesta esperada (Output):**
```json
[
  {
    "id": "string (ObjectId)",
    "productoCatalogoId": "string (ObjectId)",
    "coleccionId": "string (ObjectId)",
    "sku": "string",
    "tipoProducto": "string",
    "nombreProducto": "string",
    "imagenUrl": "string (URL)",
    "nombreVendedor": "string",
    "estado": "string",
    "precio": 99.99,
    "notas": "string"
  },
  ...
]
```

**Códigos de Respuesta:**
- `200 OK` - Lista obtenida
- `401 Unauthorized` - Token inválido
- `500 Internal Server Error` - Error al obtener publicaciones

---

### 3.3 GET - Buscar Publicaciones (con Paginación y Filtros)

**Endpoint:** `GET /api/PublicacionesInventario/buscar`  
**Autenticación:** NO requerida  
**Propósito:** Búsqueda avanzada con filtros dinámicos y paginación

**Parámetros de Query (todos opcionales):**
- `nombre` (string) - Filtrar por nombre del producto
- `franquicia` (string) - Filtrar por nombre de franquicia
- `coleccion` (string) - Filtrar por nombre de colección
- `precioMin` (decimal) - Precio mínimo
- `precioMax` (decimal) - Precio máximo
- `pagina` (int, default: 1) - Número de página
- `limite` (int, default: 10) - Items por página

**Ejemplo de Llamada:**
```
GET /api/PublicacionesInventario/buscar?nombre=Pikachu&franquicia=Pokemon&precioMin=50&precioMax=200&pagina=1&limite=20
```

**Respuesta esperada (Output):**
```json
{
  "totalItems": 150,
  "totalPaginas": 8,
  "paginaActual": 1,
  "items": [
    {
      "id": "string (ObjectId)",
      "productoCatalogoId": "string (ObjectId)",
      "coleccionId": "string (ObjectId)",
      "sku": "string",
      "tipoProducto": "string",
      "nombreProducto": "string",
      "nombreColeccion": "string",
      "nombreFranquicia": "string",
      "imagenUrl": "string (URL)",
      "nombreVendedor": "string",
      "estado": "string",
      "precio": 99.99,
      "notas": "string"
    },
    ...
  ]
}
```

**Códigos de Respuesta:**
- `200 OK` - Búsqueda exitosa
- `500 Internal Server Error` - Error al buscar

---

### 3.4 POST - Crear Publicación de Inventario

**Endpoint:** `POST /api/PublicacionesInventario`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Publicar un producto en el inventario (crear un anuncio de venta)

**Body esperado (Input):**
```json
{
  "productoCatalogoId": "string (ObjectId válido)",
  "estado": "string (máx 50 caracteres)",
  "precio": 99.99,
  "stock": 10,
  "notas": "string (máx 500 caracteres, opcional)"
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "Se ha creado con exito la publicación de inventario"
}
```

**Códigos de Respuesta:**
- `200 OK` - Publicación creada
- `400 Bad Request` - Validación fallida
- `401 Unauthorized` - Token inválido

---

### 3.5 PUT - Editar Publicación de Inventario

**Endpoint:** `PUT /api/PublicacionesInventario/{id}`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Actualizar datos de una publicación

**Parámetros de Ruta:**
- `id` (string) - ObjectId de la publicación

**Body esperado (Input):**
```json
{
  "productoCatalogoId": "string (ObjectId)",
  "estado": "string (máx 50 caracteres)",
  "precio": 99.99,
  "stock": 10,
  "notas": "string (máx 500 caracteres, opcional)"
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "Publicación de inventario editada con exito"
}
```

**Códigos de Respuesta:**
- `200 OK` - Actualización exitosa
- `400 Bad Request` - Validación fallida
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No eres el dueño de la publicación
- `404 Not Found` - Publicación no existe
- `500 Internal Server Error` - Error al actualizar

---

## 4️⃣ PEDIDOS

**Ruta Base:** `/api/Pedidos`

### 4.1 POST - Checkout (Crear Pedido)

**Endpoint:** `POST /api/Pedidos/checkout`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Procesar compra y crear un pedido

**Body esperado (Input):**
```json
{
  "direccionId": "string (ObjectId de dirección válida)",
  "articulos": [
    {
      "publicacionId": "string (ObjectId)",
      "cantidad": 5
    },
    {
      "publicacionId": "string (ObjectId)",
      "cantidad": 2
    }
  ]
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "¡Pedido realizado con éxito!",
  "pedidoId": "string (ObjectId)",
  "totalPagar": 599.99
}
```

**Códigos de Respuesta:**
- `200 OK` - Pedido creado exitosamente
- `400 Bad Request` - Dirección inválida, stock insuficiente, carrito vacío
- `401 Unauthorized` - Token inválido

**Nota:** La transacción se revierte automáticamente si ocurre algún error.

---

### 4.2 GET - Mis Pedidos

**Endpoint:** `GET /api/Pedidos/mis-pedidos`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Obtener historial de pedidos del usuario logueado

**Parámetros:** Ninguno

**Respuesta esperada (Output):**
```json
[
  {
    "id": "string (ObjectId)",
    "usuarioId": "string (ObjectId)",
    "direccionId": "string (ObjectId)",
    "direccion": {
      "id": "string (ObjectId)",
      "usuarioId": "string (ObjectId)",
      "region": "string",
      "comuna": "string",
      "calle": "string",
      "numero": "string",
      "departamento": "string",
      "codigoPostal": "string",
      "label": "string",
      "esPredeterminada": true
    },
    "fechaPedido": "2025-01-15T10:30:00.000Z",
    "total": 599.99,
    "estado": "Pagado",
    "articulos": [
      {
        "id": "string (ObjectId)",
        "pedidoId": "string (ObjectId)",
        "publicacionId": "string (ObjectId)",
        "publicacion": {
          "id": "string (ObjectId)",
          "productoCatalogoId": "string (ObjectId)",
          "vendedorId": "string (ObjectId)",
          "estado": "string",
          "precio": 99.99,
          "stock": 5,
          "notas": "string",
          "productoCatalogo": {
            "id": "string (ObjectId)",
            "coleccionId": "string (ObjectId)",
            "nombreProducto": "string",
            "sku": "string",
            "tipoProducto": "string",
            "imagenUrl": "string (URL)",
            "usuarioId": "string (ObjectId)"
          }
        },
        "cantidad": 2,
        "precioUnitario": 99.99
      }
    ]
  },
  ...
]
```

**Códigos de Respuesta:**
- `200 OK` - Lista de pedidos obtenida
- `401 Unauthorized` - Token inválido

---

### 4.3 PUT - Actualizar Estado de Pedido

**Endpoint:** `PUT /api/Pedidos/{id}/estado`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Cambiar el estado de un pedido (Pendiente, Pagado, Enviado, Entregado)

**Parámetros de Ruta:**
- `id` (string) - ObjectId del pedido

**Body esperado (Input):**
```json
{
  "estado": "string (Pendiente|Pagado|Enviado|Entregado)"
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "El estado del pedido ha sido actualizado a 'Enviado'."
}
```

**Códigos de Respuesta:**
- `200 OK` - Estado actualizado
- `400 Bad Request` - Estado no válido
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No tienes permiso para actualizar este pedido (solo vendedores de artículos o Admin)
- `404 Not Found` - Pedido no existe
- `500 Internal Server Error` - Error al actualizar

---

## 5️⃣ COLECCIONES

**Ruta Base:** `/api/Colecciones`

### 5.1 GET - Obtener Todas las Colecciones

**Endpoint:** `GET /api/Colecciones`  
**Autenticación:** NO requerida  
**Propósito:** Listar todas las colecciones disponibles

**Parámetros:** Ninguno

**Respuesta esperada (Output):**
```json
[
  {
    "id": "string (ObjectId)",
    "nombreFranquicia": "string",
    "nombreColeccion": "string",
    "anioLanzamiento": 2024
  },
  ...
]
```

**Códigos de Respuesta:**
- `200 OK` - Lista obtenida

---

### 5.2 POST - Crear Colección

**Endpoint:** `POST /api/Colecciones`  
**Autenticación:** **REQUERIDA** (Rol: Admin)  
**Propósito:** Crear una nueva colección

**Body esperado (Input):**
```json
{
  "franquiciaId": "string (ObjectId válido)",
  "nombreColeccion": "string (8-150 caracteres)",
  "fechaLanzamiento": "2024-01-15T00:00:00Z"
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "Colección creada con exito"
}
```

**Códigos de Respuesta:**
- `200 OK` - Colección creada
- `400 Bad Request` - Validación fallida, FranquiciaId inválido
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No es Admin

---

### 5.3 PUT - Editar Colección

**Endpoint:** `PUT /api/Colecciones/{id}`  
**Autenticación:** **REQUERIDA** (Rol: Admin)  
**Propósito:** Actualizar datos de una colección

**Parámetros de Ruta:**
- `id` (string) - ObjectId de la colección

**Body esperado (Input):**
```json
{
  "franquiciaId": "string (ObjectId válido)",
  "nombreColeccion": "string (8-150 caracteres)",
  "fechaLanzamiento": "2024-01-15T00:00:00Z"
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "Coleción editada con exito"
}
```

**Códigos de Respuesta:**
- `200 OK` - Actualización exitosa
- `400 Bad Request` - Validación fallida
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No es Admin
- `404 Not Found` - Colección no existe
- `500 Internal Server Error` - Error al actualizar

---

### 5.4 DELETE - Eliminar Colección

**Endpoint:** `DELETE /api/Colecciones/{id}`  
**Autenticación:** **REQUERIDA** (Rol: Admin)  
**Propósito:** Eliminar una colección

**Parámetros de Ruta:**
- `id` (string) - ObjectId de la colección

**Respuesta esperada (Output):**
```json
{
  "message": "La coleccion ha sido borrada con exito"
}
```

**Códigos de Respuesta:**
- `200 OK` - Eliminación exitosa
- `400 Bad Request` - Error al eliminar
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No es Admin
- `404 Not Found` - Colección no existe

---

## 6️⃣ FRANQUICIAS

**Ruta Base:** `/api/Franquicias`

### 6.1 GET - Obtener Todas las Franquicias

**Endpoint:** `GET /api/Franquicias`  
**Autenticación:** NO requerida  
**Propósito:** Listar todas las franquicias disponibles

**Parámetros:** Ninguno

**Respuesta esperada (Output):**
```json
[
  {
    "id": "string (ObjectId)",
    "nombre": "string"
  },
  ...
]
```

**Códigos de Respuesta:**
- `200 OK` - Lista obtenida

---

### 6.2 POST - Crear Franquicia

**Endpoint:** `POST /api/Franquicias`  
**Autenticación:** **REQUERIDA** (Rol: Admin)  
**Propósito:** Crear una nueva franquicia

**Body esperado (Input):**
```json
{
  "nombre": "string (6-100 caracteres)"
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "La Franquicia fue creada con exito"
}
```

**Códigos de Respuesta:**
- `200 OK` - Franquicia creada
- `400 Bad Request` - Validación fallida
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No es Admin

---

### 6.3 PUT - Editar Franquicia

**Endpoint:** `PUT /api/Franquicias/{id}`  
**Autenticación:** **REQUERIDA** (Rol: Admin)  
**Propósito:** Actualizar datos de una franquicia

**Parámetros de Ruta:**
- `id` (string) - ObjectId de la franquicia

**Body esperado (Input):**
```json
{
  "nombre": "string (6-100 caracteres)"
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "Franquicia editada con exito"
}
```

**Códigos de Respuesta:**
- `200 OK` - Actualización exitosa
- `400 Bad Request` - Validación fallida
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No es Admin
- `404 Not Found` - Franquicia no existe
- `500 Internal Server Error` - Error al actualizar

---

### 6.4 DELETE - Eliminar Franquicia

**Endpoint:** `DELETE /api/Franquicias/{id}`  
**Autenticación:** **REQUERIDA** (Rol: Admin)  
**Propósito:** Eliminar una franquicia

**Parámetros de Ruta:**
- `id` (string) - ObjectId de la franquicia

**Respuesta esperada (Output):**
```json
{
  "message": "La franquicia ha sido borrada con exito"
}
```

**Códigos de Respuesta:**
- `200 OK` - Eliminación exitosa
- `400 Bad Request` - Error al eliminar
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No es Admin
- `404 Not Found` - Franquicia no existe

---

## 7️⃣ DIRECCIONES

**Ruta Base:** `/api/Direcciones`

### 7.1 GET - Mis Direcciones

**Endpoint:** `GET /api/Direcciones/mis-direcciones`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Obtener todas las direcciones de envío del usuario logueado

**Parámetros:** Ninguno

**Respuesta esperada (Output):**
```json
[
  {
    "id": "string (ObjectId)",
    "usuarioId": "string (ObjectId)",
    "region": "string",
    "comuna": "string",
    "calle": "string",
    "numero": "string",
    "departamento": "string",
    "codigoPostal": "string",
    "label": "string (Casa|Trabajo|Otro)",
    "esPredeterminada": true
  },
  ...
]
```

**Códigos de Respuesta:**
- `200 OK` - Lista de direcciones obtenida
- `401 Unauthorized` - Token inválido

---

### 7.2 POST - Crear Dirección

**Endpoint:** `POST /api/Direcciones`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Agregar una nueva dirección de envío

**Body esperado (Input):**
```json
{
  "region": "string (obligatorio)",
  "comuna": "string (obligatorio)",
  "calle": "string (obligatorio)",
  "numero": "string",
  "departamento": "string (opcional)",
  "codigoPostal": "string (opcional)",
  "label": "string (Casa, Trabajo, Otro)",
  "esPredeterminada": false
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "Dirección guardada",
  "id": "string (ObjectId)"
}
```

**Códigos de Respuesta:**
- `200 OK` - Dirección creada
- `400 Bad Request` - Validación fallida
- `401 Unauthorized` - Token inválido

**Nota:** Si se marca como predeterminada, automáticamente se desmarcan las otras.

---

### 7.3 PUT - Editar Dirección

**Endpoint:** `PUT /api/Direcciones/{id}`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Actualizar datos de una dirección

**Parámetros de Ruta:**
- `id` (string) - ObjectId de la dirección

**Body esperado (Input):**
```json
{
  "region": "string (obligatorio)",
  "comuna": "string (obligatorio)",
  "calle": "string (obligatorio)",
  "numero": "string",
  "departamento": "string (opcional)",
  "codigoPostal": "string (opcional)",
  "label": "string (Casa, Trabajo, Otro)",
  "esPredeterminada": false
}
```

**Respuesta esperada (Output):**
```json
{
  "message": "Dirección actualizada con éxito"
}
```

**Códigos de Respuesta:**
- `200 OK` - Actualización exitosa
- `400 Bad Request` - Validación fallida
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No eres el dueño de esta dirección
- `404 Not Found` - Dirección no existe

---

### 7.4 DELETE - Eliminar Dirección

**Endpoint:** `DELETE /api/Direcciones/{id}`  
**Autenticación:** **REQUERIDA**  
**Propósito:** Eliminar una dirección de envío

**Parámetros de Ruta:**
- `id` (string) - ObjectId de la dirección

**Respuesta esperada (Output):**
```json
{
  "message": "Dirección eliminada correctamente"
}
```

**Códigos de Respuesta:**
- `200 OK` - Eliminación exitosa
- `400 Bad Request` - Error al eliminar
- `401 Unauthorized` - Token inválido
- `403 Forbidden` - No eres el dueño de esta dirección
- `404 Not Found` - Dirección no existe

---

## 🔒 SEGURIDAD Y AUTENTICACIÓN

### Headers Requeridos para Endpoints Protegidos

Todos los endpoints marcados como "Autenticación: **REQUERIDA**" necesitan este header:

```
Authorization: Bearer {token_jwt}
```

**Ejemplo:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI2NzhhYWE...
```

### Roles disponibles:
- `Cliente` - Usuario regular (default)
- `Admin` - Administrador del sistema

### Token JWT
- **Tiempo de Expiración:** 2 horas
- **Algoritmo:** HS256
- **Issuer:** MarketGO Backend
- **Claims incluidos:** `NameIdentifier`, `Name`, `Role`

---

## 📊 MODELOS DE DATOS PRINCIPALES

### Usuario
```json
{
  "id": "ObjectId (MongoDB)",
  "nombreUsuario": "string",
  "correo": "string",
  "passwordHash": "string (nunca enviado al frontend)",
  "fechaRegistro": "DateTime",
  "rol": "string (Cliente|Admin)"
}
```

### Producto
```json
{
  "id": "ObjectId",
  "coleccionId": "ObjectId",
  "nombreProducto": "string",
  "sku": "string",
  "tipoProducto": "string",
  "imagenUrl": "string",
  "usuarioId": "ObjectId (creador)"
}
```

### PublicacionInventario
```json
{
  "id": "ObjectId",
  "productoCatalogoId": "ObjectId",
  "vendedorId": "ObjectId",
  "estado": "string",
  "precio": "decimal",
  "stock": "int",
  "notas": "string"
}
```

### Pedido
```json
{
  "id": "ObjectId",
  "usuarioId": "ObjectId",
  "direccionId": "ObjectId",
  "fechaPedido": "DateTime",
  "total": "decimal",
  "estado": "string (Pendiente|Pagado|Enviado|Entregado)",
  "articulos": "DetallePedido[]"
}
```

### Dirección
```json
{
  "id": "ObjectId",
  "usuarioId": "ObjectId",
  "region": "string",
  "comuna": "string",
  "calle": "string",
  "numero": "string",
  "departamento": "string",
  "codigoPostal": "string",
  "label": "string",
  "esPredeterminada": "boolean"
}
```

---

## ⚠️ CÓDIGOS DE ESTADO HTTP

| Código | Significado |
|--------|-------------|
| `200 OK` | Solicitud exitosa |
| `400 Bad Request` | Datos inválidos o validación fallida |
| `401 Unauthorized` | Token faltante o inválido |
| `403 Forbidden` | Permiso denegado (rol insuficiente o falta de propiedad) |
| `404 Not Found` | Recurso no existe |
| `500 Internal Server Error` | Error en el servidor |

---

## 📝 NOTAS IMPORTANTES PARA EL FRONTEND

1. **Serialización JSON:** Todos los atributos usan **PascalCase** (ej: `nombreUsuario`, no `nombre_usuario`)
2. **ObjectId de MongoDB:** Los IDs son strings en formato MongoDB ObjectId
3. **Fechas:** Se envían en formato ISO 8601 (ej: `2025-01-15T10:30:00.000Z`)
4. **Token JWT:** Válido por 2 horas, después requiere nuevo login
5. **Control de Propiedad:** Los usuarios solo pueden editar/eliminar sus propios recursos (a menos que sean Admin)
6. **Validaciones del lado del servidor:** Siempre valida en el backend, no confíes solo en validaciones del frontend
7. **Transacciones:** El endpoint de checkout implementa transacciones ACID para garantizar consistencia

---

## 🚀 EJEMPLO DE FLUJO COMPLETO

### 1. Usuario se registra
```bash
POST /api/Usuarios
Body: {
  "nombreUsuario": "carlos123",
  "correo": "carlos@example.com",
  "password": "MiPassword123"
}
Response: { "message": "...", "id": "507f1f77bcf86cd799439011" }
```

### 2. Usuario hace login
```bash
POST /api/Usuarios/login
Body: {
  "correo": "carlos@example.com",
  "password": "MiPassword123"
}
Response: {
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "usuario": { "id": "507f1f77bcf86cd799439011", "nombreUsuario": "carlos123", "rol": "Cliente" }
}
```

### 3. Usuario busca productos
```bash
GET /api/PublicacionesInventario/buscar?franquicia=Pokemon&pagina=1&limite=20
Response: { "totalItems": 150, "totalPaginas": 8, "paginaActual": 1, "items": [...] }
```

### 4. Usuario agrega dirección de envío
```bash
POST /api/Direcciones
Header: Authorization: Bearer {token}
Body: {
  "region": "Metropolitana",
  "comuna": "Santiago",
  "calle": "Avenida Providencia",
  "numero": "1234"
}
Response: { "message": "Dirección guardada", "id": "507f1f77bcf86cd799439012" }
```

### 5. Usuario realiza compra
```bash
POST /api/Pedidos/checkout
Header: Authorization: Bearer {token}
Body: {
  "direccionId": "507f1f77bcf86cd799439012",
  "articulos": [
    { "publicacionId": "507f1f77bcf86cd799439013", "cantidad": 2 },
    { "publicacionId": "507f1f77bcf86cd799439014", "cantidad": 1 }
  ]
}
Response: { "message": "¡Pedido realizado con éxito!", "pedidoId": "507f1f77bcf86cd799439015", "totalPagar": 599.99 }
```

### 6. Usuario visualiza sus pedidos
```bash
GET /api/Pedidos/mis-pedidos
Header: Authorization: Bearer {token}
Response: [{ "id": "507f1f77bcf86cd799439015", "fechaPedido": "...", "total": 599.99, "estado": "Pagado", "articulos": [...] }]
```

---

**Documento generado:** 15 de Enero de 2025  
**Backend:** .NET 10 + Entity Framework Core + MongoDB  
**Estado:** ✅ Compilación exitosa (0 errores)

