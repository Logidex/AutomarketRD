import { Routes, Route } from 'react-router-dom';

// Páginas públicas
import Home from './pages/Home';
import Vehiculos from './pages/Vehiculos';
import Comparador from './pages/Comparador';
import DetalleAnuncio from './pages/DetalleAnuncio';
import VendedorPublico from './pages/VendedorPublico';
import Login from './pages/Login';
import Registro from './pages/Registro';
import RecuperarPassword from './pages/RecuperarPassword';
import Precios from './pages/Precios';
import Suscripcion from './pages/Suscripcion';
import Checkout from './pages/Checkout';
import PagoExitoso from './pages/PagoExitoso';
import PagoCancelado from './pages/PagoCancelado';

// Componentes de estructura
import ProtectedRoute from './components/ProtectedRoute';
import DashboardLayout from './components/layout/DashboardLayout';
import AdminLayout from './components/layout/AdminLayout';
import { ROLES } from './constants/roles';

// Páginas del dashboard de Dealers
import DashboardIndex from './pages/DashboardIndex';
import MisAnuncios from './pages/MisAnuncios';
import PublicarVehiculo from './pages/CrearAnuncio';
import EditarVehiculo from './pages/EditarVehiculo';
import Leads from './pages/Leads';
import MiPerfil from './pages/MiPerfil';
import MiCuenta from './pages/MiCuenta';
import Favoritos from './pages/Favoritos';
import Historial from './pages/Historial';
import Contactados from './pages/Contactados';
import EditarCuenta from './pages/EditarCuenta';
import DashboardSuscripcion from './pages/DashboardSuscripcion';

// Páginas del panel de administración
import AdminIndex from './pages/admin/AdminIndex';
import AdminUsuarios from './pages/admin/AdminUsuarios';
import AdminAnuncios from './pages/admin/AdminAnuncios';
import AdminPlanes from './pages/admin/AdminPlanes';

// Páginas del área de Vendedor
import MiVehiculoVendedor from './pages/vendedor/MiVehiculoVendedor';
import PublicarVehiculoVendedor from './pages/vendedor/PublicarVehiculoVendedor';
import EditarVehiculoVendedor from './pages/vendedor/EditarVehiculoVendedor';
import InteresadosVendedor from './pages/vendedor/InteresadosVendedor';
import AscenderVendedor from './pages/vendedor/AscenderVendedor';
import CuentaVendedor from './pages/vendedor/CuentaVendedor';
import VendedorLayout from './components/layout/VendedorLayout';

function App() {
  return (
    <Routes>
      {/* INICIO PÚBLICO */}
      <Route path="/" element={<Home />} />

      {/* TODOS LOS VEHÍCULOS CON FILTROS DETALLADOS */}
      <Route path="/vehiculos" element={<Vehiculos />} />

      {/* COMPARADOR DE VEHÍCULOS */}
      <Route path="/comparador" element={<Comparador />} />

      {/* DETALLE PÚBLICO DE UN VEHÍCULO */}
      <Route path="/anuncio/:id" element={<DetalleAnuncio />} />

      {/* PERFIL PÚBLICO DE UN VENDEDOR */}
      <Route path="/vendedor/:id" element={<VendedorPublico />} />

      {/* PRECIOS PÚBLICOS */}
      <Route path="/precios" element={<Precios />} />

      {/* SUSCRIPCIÓN: ELECCIÓN DE PLAN */}
      <Route path="/suscripcion" element={<Suscripcion />} />

      {/* CHECKOUT DE SUSCRIPCIÓN */}
      <Route path="/checkout" element={<Checkout />} />

      {/* RESULTADO DE PAGO PAYPAL */}
      <Route path="/pago-exitoso" element={<PagoExitoso />} />
      <Route path="/pago-cancelado" element={<PagoCancelado />} />

      {/* AUTENTICACIÓN */}
      <Route path="/login" element={<Login />} />
      <Route path="/registro" element={<Registro />} />
      <Route path="/recuperar-password" element={<RecuperarPassword />} />

      {/* CUENTA DE COMPRADOR (INICIO DE SU ACTIVIDAD) */}
      <Route
        element={
          <ProtectedRoute allowedRoles={[ROLES.COMPRADOR]} />
        }
      >
        <Route path="/perfil" element={<MiCuenta />} />
        <Route path="/perfil/favoritos" element={<Favoritos />} />
        <Route path="/perfil/historial" element={<Historial />} />
        <Route path="/perfil/contactados" element={<Contactados />} />
        <Route path="/perfil/editar" element={<EditarCuenta />} />
      </Route>

      {/* ÁREA DE VENDEDOR (UNA CUENTA = UN ANUNCIO) */}
      <Route
        element={
          <ProtectedRoute allowedRoles={[ROLES.VENDEDOR]} />
        }
      >
        <Route path="/vendedor" element={<VendedorLayout />}>
          {/* /vendedor */}
          <Route index element={<MiVehiculoVendedor />} />

          {/* /vendedor/publicar */}
          <Route path="publicar" element={<PublicarVehiculoVendedor />} />

          {/* /vendedor/editar-anuncio/:id */}
          <Route path="editar-anuncio/:id" element={<EditarVehiculoVendedor />} />

          {/* /vendedor/interesados */}
          <Route path="interesados" element={<InteresadosVendedor />} />

          {/* /vendedor/ascender */}
          <Route path="ascender" element={<AscenderVendedor />} />

          {/* /vendedor/cuenta */}
          <Route path="cuenta" element={<CuentaVendedor />} />
        </Route>
      </Route>

      {/* DASHBOARD EXCLUSIVO PARA DEALERS */}
      <Route
        element={
          <ProtectedRoute allowedRoles={[ROLES.DEALER]} />
        }
      >
        <Route path="/dashboard" element={<DashboardLayout />}>
          {/* /dashboard */}
          <Route
            index
            element={<DashboardIndex />}
          />

          {/* /dashboard/mis-anuncios */}
          <Route
            path="mis-anuncios"
            element={<MisAnuncios />}
          />

          {/* /dashboard/publicar */}
          <Route
            path="publicar"
            element={<PublicarVehiculo />}
          />
          {/* /dashboard/editar-anuncio/:id --> NUEVA PUERTA */}
          <Route
            path="editar-anuncio/:id"
            element={<EditarVehiculo />}
          />

          {/* /dashboard/leads */}
          <Route
            path="leads"
            element={<Leads />}
          />

          {/* /dashboard/mi-perfil */}
          <Route
            path="mi-perfil"
            element={<MiPerfil />}
          />

          {/* /dashboard/suscripcion */}
          <Route
            path="suscripcion"
            element={<DashboardSuscripcion />}
          />
        </Route>
      </Route>

      {/* PANEL DE ADMINISTRACIÓN */}
      <Route
        element={
          <ProtectedRoute allowedRoles={[ROLES.ADMIN]} />
        }
      >
        <Route path="/admin" element={<AdminLayout />}>
          {/* /admin */}
          <Route index element={<AdminIndex />} />

          {/* /admin/usuarios */}
          <Route path="usuarios" element={<AdminUsuarios />} />

          {/* /admin/anuncios */}
          <Route path="anuncios" element={<AdminAnuncios />} />

          {/* /admin/planes */}
          <Route path="planes" element={<AdminPlanes />} />
        </Route>
      </Route>

      {/* CUALQUIER RUTA DESCONOCIDA */}
      <Route
        path="*"
        element={
          <h1 className="p-8 text-2xl font-bold">
            Página no encontrada
          </h1>
        }
      />
    </Routes>
  );
}

export default App;
