import { lazy, Suspense } from 'react';
import { Routes, Route } from 'react-router-dom';

// Páginas públicas
const Home = lazy(() => import('./pages/Home'));
const Vehiculos = lazy(() => import('./pages/Vehiculos'));
const Comparador = lazy(() => import('./pages/Comparador'));
const Agencias = lazy(() => import('./pages/Agencias'));
const DetalleAnuncio = lazy(() => import('./pages/DetalleAnuncio'));
const VendedorPublico = lazy(() => import('./pages/VendedorPublico'));
const Login = lazy(() => import('./pages/Login'));
const Registro = lazy(() => import('./pages/Registro'));
const RecuperarPassword = lazy(() => import('./pages/RecuperarPassword'));
const ConfirmarCorreo = lazy(() => import('./pages/ConfirmarCorreo'));
const Precios = lazy(() => import('./pages/Precios'));
const Suscripcion = lazy(() => import('./pages/Suscripcion'));
const Checkout = lazy(() => import('./pages/Checkout'));
const PagoExitoso = lazy(() => import('./pages/PagoExitoso'));
const PagoCancelado = lazy(() => import('./pages/PagoCancelado'));

// Páginas legales y de contenido estático
const Terminos = lazy(() => import('./pages/legales/Terminos'));
const Privacidad = lazy(() => import('./pages/legales/Privacidad'));
const Reembolso = lazy(() => import('./pages/legales/Reembolso'));
const Contacto = lazy(() => import('./pages/legales/Contacto'));

// Componentes de estructura
import ProtectedRoute from './components/ProtectedRoute';
const DashboardLayout = lazy(() => import('./components/layout/DashboardLayout'));
const AdminLayout = lazy(() => import('./components/layout/AdminLayout'));
const VendedorLayout = lazy(() => import('./components/layout/VendedorLayout'));
import { ROLES } from './constants/roles';

// Páginas del dashboard de Dealers
const DashboardIndex = lazy(() => import('./pages/DashboardIndex'));
const MisAnuncios = lazy(() => import('./pages/MisAnuncios'));
const PublicarVehiculo = lazy(() => import('./pages/CrearAnuncio'));
const EditarVehiculo = lazy(() => import('./pages/EditarVehiculo'));
const Leads = lazy(() => import('./pages/Leads'));
const Soporte = lazy(() => import('./pages/Soporte'));
const MiPerfil = lazy(() => import('./pages/MiPerfil'));
const MiCuenta = lazy(() => import('./pages/MiCuenta'));
const Favoritos = lazy(() => import('./pages/Favoritos'));
const Historial = lazy(() => import('./pages/Historial'));
const Contactados = lazy(() => import('./pages/Contactados'));
const EditarCuenta = lazy(() => import('./pages/EditarCuenta'));
const DashboardSuscripcion = lazy(() => import('./pages/DashboardSuscripcion'));

// Páginas del panel de administración
const AdminIndex = lazy(() => import('./pages/admin/AdminIndex'));
const AdminUsuarios = lazy(() => import('./pages/admin/AdminUsuarios'));
const AdminAnuncios = lazy(() => import('./pages/admin/AdminAnuncios'));
const AdminPlanes = lazy(() => import('./pages/admin/AdminPlanes'));
  const AdminPagos = lazy(() => import('./pages/admin/AdminPagos'));
  const AdminTickets = lazy(() => import('./pages/admin/AdminTickets'));
const AdminReportes = lazy(() => import('./pages/admin/AdminReportes'));

// Páginas del área de Vendedor
const MiVehiculoVendedor = lazy(() => import('./pages/vendedor/MiVehiculoVendedor'));
const PublicarVehiculoVendedor = lazy(() => import('./pages/vendedor/PublicarVehiculoVendedor'));
const EditarVehiculoVendedor = lazy(() => import('./pages/vendedor/EditarVehiculoVendedor'));
const InteresadosVendedor = lazy(() => import('./pages/vendedor/InteresadosVendedor'));
const AscenderVendedor = lazy(() => import('./pages/vendedor/AscenderVendedor'));
const CuentaVendedor = lazy(() => import('./pages/vendedor/CuentaVendedor'));

function App() {
  return (
    <Suspense
      fallback={
        <div className="flex min-h-screen items-center justify-center bg-[#0c101b]">
          <div className="h-10 w-10 animate-spin rounded-full border-2 border-white/10 border-t-blue-500" />
        </div>
      }
    >
      <Routes>
        {/* INICIO PÚBLICO */}
        <Route path="/" element={<Home />} />

        {/* TODOS LOS VEHÍCULOS CON FILTROS DETALLADOS */}
        <Route path="/vehiculos" element={<Vehiculos />} />

        {/* COMPARADOR DE VEHÍCULOS */}
        <Route path="/comparador" element={<Comparador />} />

        {/* DIRECTORIO DE AGENCIAS */}
        <Route path="/agencias" element={<Agencias />} />

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

        {/* PÁGINAS LEGALES Y DE CONTENIDO ESTÁTICO */}
        <Route path="/terminos" element={<Terminos />} />
        <Route path="/privacidad" element={<Privacidad />} />
        <Route path="/reembolso" element={<Reembolso />} />
        <Route path="/contacto" element={<Contacto />} />

        {/* AUTENTICACIÓN */}
        <Route path="/login" element={<Login />} />
        <Route path="/registro" element={<Registro />} />
        <Route path="/recuperar-password" element={<RecuperarPassword />} />
        <Route path="/confirmar-correo" element={<ConfirmarCorreo />} />

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

            {/* /vendedor/contactados */}
            <Route
              path="contactados"
              element={<Contactados tema="claro" rutaVolver="/vendedor" />}
            />

            {/* /vendedor/ascender */}
            <Route path="ascender" element={<AscenderVendedor />} />

            {/* /vendedor/soporte */}
            <Route path="soporte" element={<Soporte />} />

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

            {/* /dashboard/contactados */}
            <Route
              path="contactados"
              element={<Contactados tema="claro" rutaVolver="/dashboard" />}
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

            {/* /dashboard/soporte */}
            <Route
              path="soporte"
              element={<Soporte />}
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

            {/* /admin/pagos */}
            <Route path="pagos" element={<AdminPagos />} />

            {/* /admin/soporte */}
            <Route path="soporte" element={<AdminTickets />} />

            {/* /admin/reportes */}
            <Route path="reportes" element={<AdminReportes />} />
          </Route>
        </Route>

        {/* CUALQUIER RUTA DESCONOCIDA */}
        <Route
          path="*"
          element={
            <div className="flex min-h-screen flex-col items-center justify-center gap-4 bg-[#0c101b] px-6 text-center">
              <h1 className="text-2xl font-bold text-white">
                Página no encontrada
              </h1>
              <p className="text-sm text-slate-400">
                La dirección que buscas no existe o fue movida.
              </p>
              <a
                href="/"
                className="rounded-lg bg-blue-500 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-600"
              >
                Volver al inicio
              </a>
            </div>
          }
        />
      </Routes>
    </Suspense>
  );
}

export default App;
