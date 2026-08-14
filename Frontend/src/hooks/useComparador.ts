import { useMutation, useQuery } from '@tanstack/react-query';
import { comparadorService, type VehiculoComparador } from '../services/comparador.service';
import { catalogoService } from '../services/catalogo.service';
import type { AnuncioListado } from '../types/anuncio.types';

export const useCompararVehiculos = (ids: number[], enabled = true) => {
  const activos = Array.from(new Set(ids)).filter((id) => id > 0);

  return useQuery<VehiculoComparador[]>({
    queryKey: ['comparar', activos.toSorted((a, b) => a - b)],
    queryFn: () => comparadorService.comparar(activos),
    enabled: enabled && activos.length >= 2,
    staleTime: 1000 * 60 * 5,
  });
};

export const useBuscarComparador = () => {
  return useMutation({
    mutationFn: (termino: string) =>
      catalogoService.buscar({
        marca: termino,
        paginaActual: 1,
        cantidadAnuncios: 6,
      }).then((resultado) =>
        resultado.items.map((a) => ({
          ...a,
          precio: Number(a.precio ?? 0),
          kilometraje: Number(a.kilometraje ?? 0),
        } as AnuncioListado)),
      ),
  });
};
