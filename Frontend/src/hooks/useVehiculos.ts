import { useQuery } from '@tanstack/react-query';
import { catalogoService, type AnuncioBusquedaDto } from '../services/catalogo.service';
import { anuncioService } from '../services/anuncio.service';
import type { PagedResult, AnuncioListado, AnuncioDetalle } from '../types/anuncio.types';

const TAMANO_PAGINA = 12;

interface UseVehiculosOptions {
  filtros: Omit<AnuncioBusquedaDto, 'paginaActual' | 'cantidadAnuncios'>;
  pagina: number;
  enabled?: boolean;
}

export const useVehiculos = ({
  filtros,
  pagina,
  enabled = true,
}: UseVehiculosOptions) => {
  return useQuery<PagedResult<AnuncioListado>>({
    queryKey: ['vehiculos', { ...filtros, pagina }],
    queryFn: () =>
      catalogoService.buscar({
        ...filtros,
        paginaActual: pagina,
        cantidadAnuncios: TAMANO_PAGINA,
      }),
    placeholderData: (prev) => prev,
    enabled,
    staleTime: 1000 * 60 * 5,
  });
};

export const useVehiculoDetalle = (id: number, enabled = true) => {
  return useQuery<AnuncioDetalle>({
    queryKey: ['vehiculo', id],
    queryFn: () => anuncioService.obtenerPorId(String(id)),
    enabled: enabled && id > 0,
    staleTime: 1000 * 60 * 5,
  });
};