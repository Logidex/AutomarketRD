import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { historialService, type AnuncioReciente } from '../services/historial.service';
import { anuncioService } from '../services/anuncio.service';

export const useHistorialReciente = (cantidad = 12) => {
  return useQuery<AnuncioReciente[]>({
    queryKey: ['historial', cantidad],
    queryFn: () => historialService.obtenerRecientes(cantidad),
    staleTime: 1000 * 60 * 2,
  });
};

export const useRegistrarVisita = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (anuncioId: number) => {
      await Promise.all([
        historialService.registrarVista(anuncioId),
        anuncioService.registrarVista(anuncioId),
      ]);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['historial'] });
    },
  });
};
