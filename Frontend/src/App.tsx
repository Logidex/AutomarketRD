import { Routes, Route } from 'react-router-dom';

// Páginas públicas
import Login from './pages/Login';
import Registro from './pages/Registro';

// Componentes de estructura
import ProtectedRoute from './components/ProtectedRoute';
import DashboardLayout from './components/layout/DashboardLayout';

// Páginas del dashboard de Dealers
import DashboardIndex from './pages/DashboardIndex';
import MisAnuncios from './pages/MisAnuncios';
import PublicarVehiculo from './pages/PublicarVehiculo';
import EditarVehiculo from './pages/EditarVehiculo';

function App() {
  return (
    <Routes>
      {/* INICIO PÚBLICO */}
      <Route
        path="/"
        element={
          <h1 className="p-4 text-3xl font-bold">
            AutoMarket RD - Inicio
          </h1>
        }
      />

      {/* AUTENTICACIÓN */}
      <Route path="/login" element={<Login />} />
      <Route path="/registro" element={<Registro />} />

      {/* RUTA TEMPORAL PARA VENDEDORES */}
      <Route
        path="/vendedor"
        element={
          <div className="p-8">
            <h1 className="text-3xl font-bold">
              Interfaz de Vendedor
            </h1>

            <p className="mt-2 text-gray-600">
              Esta sección estará disponible próximamente.
            </p>
          </div>
        }
      />

      {/* DASHBOARD EXCLUSIVO PARA DEALERS */}
      <Route
        element={
          <ProtectedRoute allowedRoles={['Dealer']} />
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
