# 📋 REFERENCIA RÁPIDA DE API - MarketGO

**Serialización:** PascalCase | **Auth:** Bearer JWT | **Expira en:** 2 horas

---

## 🔑 ENDPOINTS CLAVE POR FEATURE

### AUTENTICACIÓN
| Método | Ruta | Auth | Propósito |
|--------|------|------|----------|
| POST | `/api/Usuarios` | ❌ | Registrar usuario |
| POST | `/api/Usuarios/login` | ❌ | Obtener token JWT |

### USUARIOS
| Método | Ruta | Auth | Propósito |
|--------|------|------|----------|
| GET | `/api/Usuarios` | ✅ Admin | Listar todos |
| GET | `/api/Usuarios/{id}` | ✅ | Obtener usuario |
| PUT | `/api/Usuarios/{id}` | ✅ | Editar usuario |
| DELETE | `/api/Usuarios/{id}` | ✅ Admin | Eliminar usuario |

### PRODUCTOS (Catálogo)
| Método | Ruta | Auth | Propósito |
|--------|------|------|----------|
| GET | `/api/Productos` | ❌ | Listar productos |
| POST | `/api/Productos` | ✅ | Crear producto |
| PUT | `/api/Productos/{id}` | ✅ | Editar producto |
| DELETE | `/api/Productos/{id}` | ✅ | Eliminar producto |

### PUBLICACIONES DE INVENTARIO
| Método | Ruta | Auth | Propósito |
|--------|------|------|----------|
| GET | `/api/PublicacionesInventario` | ❌ | Listar publicaciones |
| GET | `/api/PublicacionesInventario/MisPublicaciones` | ✅ | Mis publicaciones |
| GET | `/api/PublicacionesInventario/buscar` | ❌ | Buscar con filtros |
| POST | `/api/PublicacionesInventario` | ✅ | Crear publicación |
| PUT | `/api/PublicacionesInventario/{id}` | ✅ | Editar publicación |

### PEDIDOS
| Método | Ruta | Auth | Propósito |
|--------|------|------|----------|
| POST | `/api/Pedidos/checkout` | ✅ | Crear pedido |
| GET | `/api/Pedidos/mis-pedidos` | ✅ | Ver mis pedidos |
| PUT | `/api/Pedidos/{id}/estado` | ✅ | Cambiar estado |

### COLECCIONES
| Método | Ruta | Auth | Propósito |
|--------|------|------|----------|
| GET | `/api/Colecciones` | ❌ | Listar colecciones |
| POST | `/api/Colecciones` | ✅ Admin | Crear colección |
| PUT | `/api/Colecciones/{id}` | ✅ Admin | Editar colección |
| DELETE | `/api/Colecciones/{id}` | ✅ Admin | Eliminar colección |

### FRANQUICIAS
| Método | Ruta | Auth | Propósito |
|--------|------|------|----------|
| GET | `/api/Franquicias` | ❌ | Listar franquicias |
| POST | `/api/Franquicias` | ✅ Admin | Crear franquicia |
| PUT | `/api/Franquicias/{id}` | ✅ Admin | Editar franquicia |
| DELETE | `/api/Franquicias/{id}` | ✅ Admin | Eliminar franquicia |

### DIRECCIONES
| Método | Ruta | Auth | Propósito |
|--------|------|------|----------|
| GET | `/api/Direcciones/mis-direcciones` | ✅ | Ver mis direcciones |
| POST | `/api/Direcciones` | ✅ | Crear dirección |
| PUT | `/api/Direcciones/{id}` | ✅ | Editar dirección |
| DELETE | `/api/Direcciones/{id}` | ✅ | Eliminar dirección |

---

## 🔑 DTOs DE ENTRADA (Request Body)

### RegistroUsuarioDTO
```json
{
  "nombreUsuario": "string (5-50 chars)",
  "correo": "string (email)",
  "password": "string (5-100 chars)"
}
```

### LoginDTO
```json
{
  "correo": "string (email)",
  "password": "string"
}
```

### UsuarioEdicionDTO
```json
{
  "nombreUsuario": "string (5-50 chars)",
  "correo": "string (email)"
}
```

### ProductoCreacionDTO
```json
{
  "coleccionId": "string (ObjectId)",
  "nombreProducto": "string (5-100 chars)",
  "sku": "string (3-30 chars)",
  "tipoProducto": "string (max 50 chars)",
  "imagenUrl": "string (valid URL)"
}
```

### PublicacionInventarioCreacionDTO
```json
{
  "productoCatalogoId": "string (ObjectId)",
  "estado": "string (max 50 chars)",
  "precio": 99.99,
  "stock": 10,
  "notas": "string (max 500 chars, optional)"
}
```

### ColeccionCreacionDTO
```json
{
  "franquiciaId": "string (ObjectId)",
  "nombreColeccion": "string (8-150 chars)",
  "fechaLanzamiento": "2024-01-15T00:00:00Z"
}
```

### FranquiciasCreacionDTO
```json
{
  "nombre": "string (6-100 chars)"
}
```

### CheckoutDTO
```json
{
  "direccionId": "string (ObjectId)",
  "articulos": [
    {
      "publicacionId": "string (ObjectId)",
      "cantidad": 5
    }
  ]
}
```

### DireccionDTO
```json
{
  "region": "string",
  "comuna": "string",
  "calle": "string",
  "numero": "string",
  "departamento": "string (optional)",
  "codigoPostal": "string (optional)",
  "label": "string",
  "esPredeterminada": false
}
```

### ActualizarEstadoDTO
```json
{
  "estado": "string (Pendiente|Pagado|Enviado|Entregado)"
}
```

---

## 📤 DTOs DE SALIDA (Response Body)

### UsuarioDTO
```json
{
  "id": "ObjectId",
  "nombreUsuario": "string",
  "correo": "string",
  "rol": "string (Cliente|Admin)",
  "fechaRegistro": "DateTime"
}
```

### ProductoDTO
```json
{
  "id": "ObjectId",
  "nombreColeccion": "string",
  "nombreProducto": "string",
  "sku": "string",
  "tipoProducto": "string",
  "imagenUrl": "string",
  "vendedorId": "ObjectId",
  "nombreVendedor": "string"
}
```

### PublicacionInventarioDTO
```json
{
  "id": "ObjectId",
  "productoCatalogoId": "ObjectId",
  "coleccionId": "ObjectId",
  "sku": "string",
  "tipoProducto": "string",
  "nombreProducto": "string",
  "nombreColeccion": "string",
  "nombreFranquicia": "string",
  "imagenUrl": "string",
  "nombreVendedor": "string",
  "estado": "string",
  "precio": 99.99,
  "notas": "string"
}
```

### ColeccionesDTO
```json
{
  "id": "ObjectId",
  "nombreFranquicia": "string",
  "nombreColeccion": "string",
  "anioLanzamiento": 2024
}
```

### RespuestaPaginadaDTO (para búsquedas)
```json
{
  "totalItems": 150,
  "totalPaginas": 8,
  "paginaActual": 1,
  "items": [...]
}
```

---

## 🔐 HEADERS DE AUTENTICACIÓN

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI2NzhhYWE...
```

**Para obtener token:** Usar endpoint `POST /api/Usuarios/login`

---

## ❌ CÓDIGOS DE ERROR COMUNES

| Código | Causa |
|--------|-------|
| `400` | Datos inválidos o validación fallida |
| `401` | Token faltante, inválido o expirado |
| `403` | Sin permisos (rol insuficiente o no es propietario) |
| `404` | Recurso no existe |
| `500` | Error interno del servidor |

---

## 🔄 FLUJO DE COMPRA COMPLETO

```
1. POST /api/Usuarios/login (Obtener token)
   ↓
2. GET /api/PublicacionesInventario/buscar (Buscar productos)
   ↓
3. POST /api/Direcciones (Crear/seleccionar dirección)
   ↓
4. POST /api/Pedidos/checkout (Crear pedido)
   ↓
5. GET /api/Pedidos/mis-pedidos (Ver pedidos)
   ↓
6. PUT /api/Pedidos/{id}/estado (Seguimiento)
```

---

## 🎯 PARÁMETROS DE BÚSQUEDA

### GET /api/PublicacionesInventario/buscar

**Query Params:**
- `nombre` - Buscar por nombre de producto
- `franquicia` - Filtrar por franquicia
- `coleccion` - Filtrar por colección
- `precioMin` - Precio mínimo
- `precioMax` - Precio máximo
- `pagina` - Página (default: 1)
- `limite` - Items por página (default: 10)

**Ejemplo:**
```
GET /api/PublicacionesInventario/buscar?nombre=Pikachu&franquicia=Pokemon&precioMin=50&precioMax=200&pagina=1&limite=20
```

---

## 💾 TIPOS DE DATOS

- `string` - Texto
- `int` - Número entero
- `decimal` - Número con decimales (precios)
- `boolean` - true/false
- `DateTime` - Fecha y hora (formato ISO 8601)
- `ObjectId` - ID de MongoDB (string en JSON)

---

## ⚡ ESTADOS DE PEDIDO

- `Pendiente` - Orden creada, pendiente de pago
- `Pagado` - Pago confirmado
- `Enviado` - Pedido en tránsito
- `Entregado` - Entrega completada

---

## 👥 ROLES DE USUARIO

- `Cliente` - Usuario regular (default)
- `Admin` - Administrador del sistema

---

**Última actualización:** 15 Enero 2025
**Versión API:** 1.0
**Base de datos:** MongoDB + SQL Server (Híbrido)

