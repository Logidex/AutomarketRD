import { useMutation, useQuery } from '@tanstack/react-query';
import { historialService, type AnuncioReciente } from '../services/historial.service';

export const useHistorialReciente = (cantidad = 12) => {
  return useQuery<AnuncioReciente[]>({
    queryKey: ['historial', cantidad],
    queryFn: () => historialService.obtenerRecientes(cantidad),
    staleTime: 1000 * 60 * 2,
  });
};

export const useRegistrarVisita = () => {
  return useMutation({
    mutationFn: (anuncioId: number) => historialService.registrarVista(anuncioId),
  });
};
