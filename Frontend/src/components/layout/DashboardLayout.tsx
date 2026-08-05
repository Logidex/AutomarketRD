import { Outlet, Link, useNavigate, useLocation } from 'react-router-dom';
import { authService } from '../../services/auth.service';
import { FaCar, FaPlusCircle, FaChartPie, FaSignOutAlt } from 'react-icons/fa';

export default function DashboardLayout() {
  const navigate = useNavigate();
  const location = useLocation(); // Para saber en qué ruta estamos y marcar el menú activo

  const handleLogout = () => {
    authService.logout();
    navigate('/login');
  };

  // Definimos las opciones del menú para iterarlas fácilmente
  const menuItems = [
    { path: '/dashboard', label: 'Resumen', icon: <FaChartPie /> },
    { path: '/dashboard/mis-anuncios', label: 'Mi Inventario', icon: <FaCar /> },
    { path: '/dashboard/publicar', label: 'Publicar Vehículo', icon: <FaPlusCircle /> },
  ];

  return (
    <div className="flex h-screen w-screen overflow-hidden bg-[#f4f6f9] font-sans">
      
      {/* SIDEBAR */}
      <aside className="w-[260px] bg-[#11141a] flex flex-col h-full text-white shadow-lg z-10">
        <div className="h-[70px] flex items-center justify-center px-6 border-b border-white/5">
          <h2 className="text-xl font-bold tracking-wider text-white">AutoMarket RD</h2>
        </div>
        
        <nav className="flex-1 p-6 flex flex-col gap-2">
          {menuItems.map((item) => {
            // Lógica para marcar el botón activo según la URL
            const isActive = location.pathname === item.path || 
                            (item.path !== '/dashboard' && location.pathname.startsWith(item.path));
            
            return (
              <Link 
                key={item.path}
                to={item.path} 
                className={`flex items-center px-4 py-3 rounded-lg font-medium transition-colors ${
                  isActive 
                    ? 'bg-blue-600 text-white' 
                    : 'text-[#8a94a6] hover:bg-white/5 hover:text-white'
                }`}
              >
                <span className="mr-3 text-lg">{item.icon}</span>
                {item.label}
              </Link>
            );
          })}
        </nav>

        <div className="p-4 border-t border-white/5">
          <button 
            onClick={handleLogout}
            className="w-full flex items-center justify-center px-4 py-3 text-red-500 font-medium rounded-lg hover:bg-red-500/10 transition-colors"
          >
            <FaSignOutAlt className="mr-3" /> Cerrar Sesión
          </button>
        </div>
      </aside>

      {/* ÁREA PRINCIPAL */}
      <main className="flex-1 flex flex-col h-full overflow-hidden">
        
        {/* NAVBAR / HEADER */}
        <header className="h-[70px] bg-white border-b border-gray-200 flex items-center justify-between px-8 shrink-0">
          <h3 className="text-xl font-semibold text-gray-800">Panel de Control</h3>
          {/* Badge (Próximamente dinámico según el perfil del usuario) */}
          <span className="bg-[#e6f4ea] text-[#137333] border border-[#81c995] px-3 py-1 rounded-full text-sm font-semibold">
            Dealer PRO
          </span>
        </header>

        {/* CONTENIDO DINÁMICO (Aquí se renderizan las páginas) */}
        <div className="flex-1 p-8 overflow-y-auto">
          <Outlet />
        </div>
      </main>
    </div>
  );
}