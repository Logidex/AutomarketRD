import { useInfiniteQuery } from '@tanstack/react-query';
import { catalogoService, type AnuncioBusquedaDto } from '../services/catalogo.service';
import type { PagedResult, AnuncioListado } from '../types/anuncio.types';

const TAMANO_PAGINA = 12;

interface UseVehiculosInfiniteOptions {
  filtros: Omit<AnuncioBusquedaDto, 'paginaActual' | 'cantidadAnuncios'>;
  enabled?: boolean;
}

export const useVehiculosInfinite = ({
  filtros,
  enabled = true,
}: UseVehiculosInfiniteOptions) => {
  return useInfiniteQuery<PagedResult<AnuncioListado>, Error>({
    queryKey: ['vehiculos-infinite', filtros],
    queryFn: ({ pageParam = 1 }) =>
      catalogoService.buscar({
        ...filtros,
        paginaActual: pageParam as number,
        cantidadAnuncios: TAMANO_PAGINA,
      }),
    initialPageParam: 1,
    getNextPageParam: (lastPage, allPages) => {
      const totalPaginas = lastPage.totalPaginas ?? Math.ceil(lastPage.totalRegistros / TAMANO_PAGINA);
      return allPages.length < totalPaginas ? allPages.length + 1 : undefined;
    },
    enabled,
    staleTime: 1000 * 60 * 2,
    refetchInterval: 1000 * 30,
  });
};

export const useVehiculosInfiniteFlat = (filtros: Omit<AnuncioBusquedaDto, 'paginaActual' | 'cantidadAnuncios'>, enabled = true) => {
  const query = useVehiculosInfinite({ filtros, enabled });
  const items = query.data?.pages.flatMap((p) => p.items) ?? [];
  const total = query.data?.pages[0]?.totalRegistros ?? 0;
  return { ...query, items, total };
};
