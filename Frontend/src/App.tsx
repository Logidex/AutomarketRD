import { Routes, Route } from 'react-router-dom';

// Páginas Públicas
import Login from './pages/Login';
import Registro from './pages/Registro';

// Componentes de Estructura
import ProtectedRoute from './components/ProtectedRoute';
import DashboardLayout from './components/layout/DashboardLayout';

// Páginas del Dashboard
import DashboardIndex from './pages/DashboardIndex';
import MisAnuncios from './pages/MisAnuncios';
import PublicarVehiculo from './pages/PublicarVehiculo';

function App() {
  return (
    <Routes>
      {/* --- RUTAS PÚBLICAS --- */}
      <Route path="/" element={<h1 className="text-3xl font-bold p-4">AutoMarket RD - Inicio</h1>} />
      <Route path="/login" element={<Login />} />
      <Route path="/registro" element={<Registro />} />

      {/* --- RUTAS PROTEGIDAS --- */}
      <Route element={<ProtectedRoute />}>
        {/* Todas las rutas dentro de /dashboard usarán el DashboardLayout */}
        <Route path="/dashboard" element={<DashboardLayout />}>
          
          {/* Index: /dashboard */}
          <Route index element={<DashboardIndex />} />
          
          {/* Lista de vehículos: /dashboard/mis-anuncios */}
          <Route path="mis-anuncios" element={<MisAnuncios />} />
          
          {/* Formulario: /dashboard/publicar */}
          <Route path="publicar" element={<PublicarVehiculo />} />
          
        </Route>
      </Route>
    </Routes>
  );
}

export default App;
