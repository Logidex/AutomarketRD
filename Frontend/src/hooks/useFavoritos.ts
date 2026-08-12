import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { favoritoService, type AnuncioFavorito } from '../services/favorito.service';

export const useMisFavoritos = () => {
  return useQuery<AnuncioFavorito[]>({
    queryKey: ['favoritos'],
    queryFn: () => favoritoService.obtenerMisFavoritos(),
    staleTime: 1000 * 60 * 2,
  });
};

export const useAgregarFavorito = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (anuncioId: number) => favoritoService.agregar(anuncioId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['favoritos'] }),
  });
};

export const useQuitarFavorito = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (anuncioId: number) => favoritoService.quitar(anuncioId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['favoritos'] }),
  });
};
