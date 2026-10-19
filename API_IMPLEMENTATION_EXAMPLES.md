# 🚀 EJEMPLOS DE IMPLEMENTACIÓN - MarketGO API

**Para:** Equipo Frontend (React Native + TypeScript)  
**Última actualización:** 15 Enero 2025

---

## 📌 CONFIGURACIÓN BASE

### URL Base
```typescript
const API_BASE_URL = 'https://backend.marketgo.com/api';
// O en desarrollo:
const API_BASE_URL = 'http://localhost:5000/api';
```

### Cliente HTTP Base (Axios)
```typescript
import axios, { AxiosInstance } from 'axios';

const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para agregar token automáticamente
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token'); // O AsyncStorage en React Native
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default apiClient;
```

---

## 👤 MÓDULO DE USUARIOS

### Interfaces TypeScript
```typescript
// Usuario
interface Usuario {
  id: string;
  nombreUsuario: string;
  correo: string;
  rol: 'Cliente' | 'Admin';
  fechaRegistro: string; // ISO 8601
}

// Login Response
interface LoginResponse {
  token: string;
  usuario: {
    id: string;
    nombreUsuario: string;
    rol: string;
  };
}

// Registro
interface RegistroDTO {
  nombreUsuario: string;
  correo: string;
  password: string;
}

// Edición de Usuario
interface UsuarioEdicionDTO {
  nombreUsuario: string;
  correo: string;
}
```

### 1. Registrar Usuario
```typescript
async function registrarUsuario(datos: RegistroDTO): Promise<{ message: string; id: string }> {
  try {
    const response = await apiClient.post('/Usuarios', datos);
    return response.data;
  } catch (error) {
    console.error('Error al registrar:', error);
    throw error;
  }
}

// Uso:
const resultado = await registrarUsuario({
  nombreUsuario: 'carlos123',
  correo: 'carlos@example.com',
  password: 'MiPassword123'
});
console.log('Usuario creado:', resultado.id);
```

### 2. Login
```typescript
async function loginUsuario(correo: string, password: string): Promise<LoginResponse> {
  try {
    const response = await apiClient.post('/Usuarios/login', {
      correo,
      password
    });

    // Guardar token
    localStorage.setItem('token', response.data.token);
    localStorage.setItem('userId', response.data.usuario.id);
    localStorage.setItem('userRole', response.data.usuario.rol);

    return response.data;
  } catch (error) {
    console.error('Error al hacer login:', error);
    throw error;
  }
}

// Uso:
const loginData = await loginUsuario('carlos@example.com', 'MiPassword123');
console.log('Bienvenido:', loginData.usuario.nombreUsuario);
```

### 3. Obtener Perfil
```typescript
async function obtenerPerfil(userId: string): Promise<Usuario> {
  try {
    const response = await apiClient.get(`/Usuarios/${userId}`);
    return response.data;
  } catch (error) {
    console.error('Error al obtener perfil:', error);
    throw error;
  }
}

// Uso:
const perfil = await obtenerPerfil(userId);
console.log('Nombre:', perfil.nombreUsuario);
```

### 4. Editar Perfil
```typescript
async function editarPerfil(userId: string, datos: UsuarioEdicionDTO): Promise<{ message: string }> {
  try {
    const response = await apiClient.put(`/Usuarios/${userId}`, datos);
    return response.data;
  } catch (error) {
    console.error('Error al editar perfil:', error);
    throw error;
  }
}

// Uso:
await editarPerfil(userId, {
  nombreUsuario: 'carlitos456',
  correo: 'carlitos@example.com'
});
```

---

## 🛍️ MÓDULO DE PRODUCTOS

### Interfaces TypeScript
```typescript
interface Producto {
  id: string;
  nombreColeccion: string;
  nombreProducto: string;
  sku: string;
  tipoProducto: string;
  imagenUrl: string;
  vendedorId?: string;
  nombreVendedor?: string;
}

interface ProductoCreacionDTO {
  coleccionId: string;
  nombreProducto: string;
  sku: string;
  tipoProducto: string;
  imagenUrl: string;
}
```

### 1. Obtener Todos los Productos
```typescript
async function obtenerProductos(): Promise<Producto[]> {
  try {
    const response = await apiClient.get('/Productos');
    return response.data;
  } catch (error) {
    console.error('Error al obtener productos:', error);
    throw error;
  }
}

// Uso:
const productos = await obtenerProductos();
console.log(`Se encontraron ${productos.length} productos`);
```

### 2. Crear Producto
```typescript
async function crearProducto(datos: ProductoCreacionDTO): Promise<{ message: string; id: string }> {
  try {
    const response = await apiClient.post('/Productos', datos);
    return response.data;
  } catch (error) {
    console.error('Error al crear producto:', error);
    throw error;
  }
}

// Uso:
const nuevoProducto = await crearProducto({
  coleccionId: '507f1f77bcf86cd799439011',
  nombreProducto: 'Charizard Holográfico',
  sku: 'CHAR-001',
  tipoProducto: 'Carta Pokémon',
  imagenUrl: 'https://example.com/charizard.jpg'
});
```

### 3. Editar Producto
```typescript
async function editarProducto(id: string, datos: ProductoCreacionDTO): Promise<{ message: string; id: string }> {
  try {
    const response = await apiClient.put(`/Productos/${id}`, datos);
    return response.data;
  } catch (error) {
    console.error('Error al editar producto:', error);
    throw error;
  }
}
```

### 4. Eliminar Producto
```typescript
async function eliminarProducto(id: string): Promise<{ message: string; id: string }> {
  try {
    const response = await apiClient.delete(`/Productos/${id}`);
    return response.data;
  } catch (error) {
    console.error('Error al eliminar producto:', error);
    throw error;
  }
}
```

---

## 📦 MÓDULO DE PUBLICACIONES DE INVENTARIO

### Interfaces TypeScript
```typescript
interface PublicacionInventario {
  id: string;
  productoCatalogoId: string;
  coleccionId: string;
  sku: string;
  tipoProducto: string;
  nombreProducto: string;
  nombreColeccion: string;
  nombreFranquicia: string;
  imagenUrl: string;
  nombreVendedor: string;
  estado: string;
  precio: number;
  notas: string;
}

interface PublicacionInventarioCreacionDTO {
  productoCatalogoId: string;
  estado: string;
  precio: number;
  stock: number;
  notas?: string;
}

interface RespuestaPaginada<T> {
  totalItems: number;
  totalPaginas: number;
  paginaActual: number;
  items: T[];
}

interface FiltrosBusqueda {
  nombre?: string;
  franquicia?: string;
  coleccion?: string;
  precioMin?: number;
  precioMax?: number;
  pagina?: number;
  limite?: number;
}
```

### 1. Obtener Todas las Publicaciones
```typescript
async function obtenerPublicaciones(): Promise<PublicacionInventario[]> {
  try {
    const response = await apiClient.get('/PublicacionesInventario');
    return response.data;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}
```

### 2. Mis Publicaciones (Usuario Logueado)
```typescript
async function misPub licaciones(): Promise<PublicacionInventario[]> {
  try {
    const response = await apiClient.get('/PublicacionesInventario/MisPublicaciones');
    return response.data;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}
```

### 3. Buscar Publicaciones (CON PAGINACIÓN Y FILTROS)
```typescript
async function buscarPublicaciones(filtros: FiltrosBusqueda): Promise<RespuestaPaginada<PublicacionInventario>> {
  try {
    const params = new URLSearchParams();

    if (filtros.nombre) params.append('nombre', filtros.nombre);
    if (filtros.franquicia) params.append('franquicia', filtros.franquicia);
    if (filtros.coleccion) params.append('coleccion', filtros.coleccion);
    if (filtros.precioMin) params.append('precioMin', filtros.precioMin.toString());
    if (filtros.precioMax) params.append('precioMax', filtros.precioMax.toString());
    params.append('pagina', (filtros.pagina || 1).toString());
    params.append('limite', (filtros.limite || 10).toString());

    const response = await apiClient.get(`/PublicacionesInventario/buscar?${params.toString()}`);
    return response.data;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}

// Uso:
const resultados = await buscarPublicaciones({
  nombre: 'Pikachu',
  franquicia: 'Pokemon',
  precioMin: 50,
  precioMax: 200,
  pagina: 1,
  limite: 20
});
console.log(`Página 1 de ${resultados.totalPaginas}`);
console.log(`Total de items: ${resultados.totalItems}`);
resultados.items.forEach(item => console.log(item.nombreProducto, '$' + item.precio));
```

### 4. Crear Publicación
```typescript
async function crearPublicacion(datos: PublicacionInventarioCreacionDTO): Promise<{ message: string }> {
  try {
    const response = await apiClient.post('/PublicacionesInventario', datos);
    return response.data;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}

// Uso:
await crearPublicacion({
  productoCatalogoId: '507f1f77bcf86cd799439011',
  estado: 'Disponible',
  precio: 150.00,
  stock: 5,
  notas: 'Producto en excelente estado'
});
```

### 5. Editar Publicación
```typescript
async function editarPublicacion(id: string, datos: PublicacionInventarioCreacionDTO): Promise<{ message: string }> {
  try {
    const response = await apiClient.put(`/PublicacionesInventario/${id}`, datos);
    return response.data;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}
```

---

## 🛒 MÓDULO DE PEDIDOS

### Interfaces TypeScript
```typescript
interface ItemCarrito {
  publicacionId: string;
  cantidad: number;
}

interface CheckoutDTO {
  direccionId: string;
  articulos: ItemCarrito[];
}

interface Pedido {
  id: string;
  usuarioId: string;
  direccionId: string;
  fechaPedido: string;
  total: number;
  estado: 'Pendiente' | 'Pagado' | 'Enviado' | 'Entregado';
  articulos: DetallePedido[];
  direccion?: Direccion;
}

interface DetallePedido {
  id: string;
  pedidoId: string;
  publicacionId: string;
  cantidad: number;
  precioUnitario: number;
  publicacion?: PublicacionInventario;
}

interface ActualizarEstadoDTO {
  estado: 'Pendiente' | 'Pagado' | 'Enviado' | 'Entregado';
}
```

### 1. Crear Pedido (Checkout)
```typescript
async function realizarCompra(checkout: CheckoutDTO): Promise<{ message: string; pedidoId: string; totalPagar: number }> {
  try {
    const response = await apiClient.post('/Pedidos/checkout', checkout);
    return response.data;
  } catch (error) {
    console.error('Error al procesar compra:', error);
    throw error;
  }
}

// Uso:
const resultado = await realizarCompra({
  direccionId: '507f1f77bcf86cd799439012',
  articulos: [
    { publicacionId: '507f1f77bcf86cd799439013', cantidad: 2 },
    { publicacionId: '507f1f77bcf86cd799439014', cantidad: 1 }
  ]
});
console.log(`Pedido creado: ${resultado.pedidoId}`);
console.log(`Total a pagar: $${resultado.totalPagar}`);
```

### 2. Obtener Mis Pedidos
```typescript
async function misPedidos(): Promise<Pedido[]> {
  try {
    const response = await apiClient.get('/Pedidos/mis-pedidos');
    return response.data;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}

// Uso:
const pedidos = await misPedidos();
pedidos.forEach(pedido => {
  console.log(`Pedido #${pedido.id}`);
  console.log(`Estado: ${pedido.estado}`);
  console.log(`Total: $${pedido.total}`);
  pedido.articulos.forEach(item => {
    console.log(`  - ${item.publicacion?.nombreProducto} (x${item.cantidad})`);
  });
});
```

### 3. Actualizar Estado de Pedido
```typescript
async function actualizarEstadoPedido(pedidoId: string, nuevoEstado: ActualizarEstadoDTO): Promise<{ message: string }> {
  try {
    const response = await apiClient.put(`/Pedidos/${pedidoId}/estado`, nuevoEstado);
    return response.data;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}

// Uso:
await actualizarEstadoPedido('507f1f77bcf86cd799439015', {
  estado: 'Enviado'
});
```

---

## 📍 MÓDULO DE DIRECCIONES

### Interfaces TypeScript
```typescript
interface Direccion {
  id: string;
  usuarioId: string;
  region: string;
  comuna: string;
  calle: string;
  numero: string;
  departamento?: string;
  codigoPostal?: string;
  label?: string; // 'Casa', 'Trabajo', etc.
  esPredeterminada: boolean;
}

interface DireccionDTO {
  region: string;
  comuna: string;
  calle: string;
  numero: string;
  departamento?: string;
  codigoPostal?: string;
  label?: string;
  esPredeterminada?: boolean;
}
```

### 1. Obtener Mis Direcciones
```typescript
async function misDirecciones(): Promise<Direccion[]> {
  try {
    const response = await apiClient.get('/Direcciones/mis-direcciones');
    return response.data;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}

// Uso:
const direcciones = await misDirecciones();
const direccionPorDefecto = direcciones.find(d => d.esPredeterminada);
```

### 2. Crear Dirección
```typescript
async function crearDireccion(datos: DireccionDTO): Promise<{ message: string; id: string }> {
  try {
    const response = await apiClient.post('/Direcciones', datos);
    return response.data;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}

// Uso:
const nuevaDireccion = await crearDireccion({
  region: 'Metropolitana',
  comuna: 'Santiago',
  calle: 'Avenida Providencia',
  numero: '1234',
  departamento: '502',
  codigoPostal: '8320000',
  label: 'Casa',
  esPredeterminada: true
});
console.log('Dirección creada:', nuevaDireccion.id);
```

### 3. Editar Dirección
```typescript
async function editarDireccion(id: string, datos: DireccionDTO): Promise<{ message: string }> {
  try {
    const response = await apiClient.put(`/Direcciones/${id}`, datos);
    return response.data;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}
```

### 4. Eliminar Dirección
```typescript
async function eliminarDireccion(id: string): Promise<{ message: string }> {
  try {
    const response = await apiClient.delete(`/Direcciones/${id}`);
    return response.data;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}
```

---

## 🏷️ MÓDULO DE COLECCIONES Y FRANQUICIAS

### Interfaces TypeScript
```typescript
interface Coleccion {
  id: string;
  nombreFranquicia: string;
  nombreColeccion: string;
  anioLanzamiento: number;
}

interface Franquicia {
  id: string;
  nombre: string;
}

interface ColeccionCreacionDTO {
  franquiciaId: string;
  nombreColeccion: string;
  fechaLanzamiento: string; // ISO 8601
}

interface FranquiciaCreacionDTO {
  nombre: string;
}
```

### Obtener Colecciones
```typescript
async function obtenerColecciones(): Promise<Coleccion[]> {
  try {
    const response = await apiClient.get('/Colecciones');
    return response.data;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}
```

### Obtener Franquicias
```typescript
async function obtenerFranquicias(): Promise<Franquicia[]> {
  try {
    const response = await apiClient.get('/Franquicias');
    return response.data;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}
```

---

## 🛡️ MANEJO DE ERRORES

```typescript
import { AxiosError } from 'axios';

interface ErrorResponse {
  message: string;
  error?: string;
  detalle?: string;
}

async function conManejoDeErrores<T>(
  operacion: Promise<T>
): Promise<T | null> {
  try {
    return await operacion;
  } catch (error) {
    const axiosError = error as AxiosError<ErrorResponse>;

    if (axiosError.response) {
      // Error del servidor (4xx, 5xx)
      const status = axiosError.response.status;
      const data = axiosError.response.data;

      switch (status) {
        case 400:
          console.error('Datos inválidos:', data.message);
          break;
        case 401:
          console.error('No autorizado. Redirigir a login.');
          localStorage.removeItem('token');
          window.location.href = '/login';
          break;
        case 403:
          console.error('Acceso denegado:', data.message);
          break;
        case 404:
          console.error('Recurso no encontrado:', data.message);
          break;
        case 500:
          console.error('Error del servidor:', data.message);
          break;
        default:
          console.error('Error:', data.message);
      }
    } else if (axiosError.request) {
      console.error('Sin respuesta del servidor');
    } else {
      console.error('Error:', axiosError.message);
    }

    return null;
  }
}

// Uso:
const resultado = await conManejoDeErrores(
  obtenerPublicaciones()
);
if (resultado) {
  // Hacer algo con el resultado
}
```

---

## 📱 EJEMPLO DE FLUJO COMPLETO EN REACT NATIVE

```typescript
import AsyncStorage from '@react-native-async-storage/async-storage';

// Custom hook para autenticación
function useAuth() {
  const [usuario, setUsuario] = React.useState<Usuario | null>(null);
  const [token, setToken] = React.useState<string | null>(null);
  const [cargando, setCargando] = React.useState(true);

  React.useEffect(() => {
    // Verificar si ya hay sesión
    const verificarSesion = async () => {
      try {
        const tokenGuardado = await AsyncStorage.getItem('token');
        const usuarioGuardado = await AsyncStorage.getItem('usuario');

        if (tokenGuardado && usuarioGuardado) {
          setToken(tokenGuardado);
          setUsuario(JSON.parse(usuarioGuardado));
        }
      } finally {
        setCargando(false);
      }
    };

    verificarSesion();
  }, []);

  const login = async (correo: string, password: string) => {
    try {
      const datos = await loginUsuario(correo, password);
      setToken(datos.token);
      setUsuario(datos.usuario as any);

      await AsyncStorage.setItem('token', datos.token);
      await AsyncStorage.setItem('usuario', JSON.stringify(datos.usuario));

      return true;
    } catch (error) {
      return false;
    }
  };

  const logout = async () => {
    setToken(null);
    setUsuario(null);
    await AsyncStorage.removeItem('token');
    await AsyncStorage.removeItem('usuario');
  };

  return { usuario, token, cargando, login, logout };
}

// Uso en componente
function App() {
  const { usuario, token, cargando } = useAuth();

  if (cargando) {
    return <LoadingScreen />;
  }

  return token && usuario ? <MainApp /> : <LoginScreen />;
}
```

---

## 🔍 TIPADO COMPLETO PARA TODO EL PROYECTO

```typescript
// types/api.ts
export namespace API {
  // Usuarios
  export interface Usuario {
    id: string;
    nombreUsuario: string;
    correo: string;
    rol: 'Cliente' | 'Admin';
    fechaRegistro: string;
  }

  export interface LoginResponse {
    token: string;
    usuario: {
      id: string;
      nombreUsuario: string;
      rol: string;
    };
  }

  // Productos
  export interface Producto {
    id: string;
    nombreColeccion: string;
    nombreProducto: string;
    sku: string;
    tipoProducto: string;
    imagenUrl: string;
  }

  // ... (continuar con todas las interfaces)
}

// Uso en componentes:
import { API } from './types/api';

const usuario: API.Usuario = { /* ... */ };
```

---

**Documento generado:** 15 Enero 2025  
**Versión:** 1.0  
**Para:** Equipo Frontend (React Native + TypeScript)

