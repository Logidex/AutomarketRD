import { useQuery } from '@tanstack/react-query';
import { dealerService, type PerfilDealerPublico } from '../services/dealer.service';
import { catalogoService } from '../services/catalogo.service';
import type { AnuncioListado } from '../types/anuncio.types';

const CANTIDAD_ANUNCIOS = 50;

export const usePerfilDealerPublico = (dealerId: number, enabled = true) => {
  return useQuery<PerfilDealerPublico>({
    queryKey: ['perfil-dealer', dealerId],
    queryFn: () => dealerService.obtenerPerfilPublico(dealerId),
    enabled: enabled && dealerId > 0,
    staleTime: 1000 * 60 * 5,
    retry: false,
  });
};

export const useAnunciosVendedor = (vendedorId: number, enabled = true) => {
  return useQuery<AnuncioListado[]>({
    queryKey: ['anuncios-vendedor', vendedorId],
    queryFn: async () => {
      const resultado = await catalogoService.buscar({
        vendedorId,
        paginaActual: 1,
        cantidadAnuncios: CANTIDAD_ANUNCIOS,
      });
      return resultado.items.map((a) => ({
        ...a,
        precio: Number(a.precio ?? 0),
        kilometraje: Number(a.kilometraje ?? 0),
      }));
    },
    enabled: enabled && vendedorId > 0,
    staleTime: 1000 * 60 * 5,
  });
};
