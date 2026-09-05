import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { anuncioService } from '../services/anuncio.service';
import type { PagedResult, AnuncioListado, AnuncioCreateRequestDto } from '../types/anuncio.types';

export const useCrearAnuncio = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (dto: AnuncioCreateRequestDto) => anuncioService.crearAnuncio(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehiculos'] });
      queryClient.invalidateQueries({ queryKey: ['mis-anuncios'] });
    },
  });
};

export const useActualizarAnuncio = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: AnuncioCreateRequestDto }) =>
      anuncioService.actualizarAnuncio(id, dto),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['vehiculos'] });
      queryClient.invalidateQueries({ queryKey: ['mis-anuncios'] });
      queryClient.invalidateQueries({ queryKey: ['vehiculo', id] });
    },
  });
};

export const usePublicarAnuncio = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => anuncioService.publicarAnuncio(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['vehiculos'] });
      queryClient.invalidateQueries({ queryKey: ['mis-anuncios'] });
      queryClient.invalidateQueries({ queryKey: ['vehiculo', id] });
    },
  });
};

export const useDestacarAnuncio = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => anuncioService.marcarComoDestacado(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['vehiculos'] });
      queryClient.invalidateQueries({ queryKey: ['mis-anuncios'] });
      queryClient.invalidateQueries({ queryKey: ['vehiculo', id] });
      queryClient.invalidateQueries({ queryKey: ['anuncios-destacados'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard-resumen'] });
    },
  });
};

export const useQuitarDestacadoAnuncio = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => anuncioService.quitarDestacado(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['vehiculos'] });
      queryClient.invalidateQueries({ queryKey: ['mis-anuncios'] });
      queryClient.invalidateQueries({ queryKey: ['vehiculo', id] });
      queryClient.invalidateQueries({ queryKey: ['anuncios-destacados'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard-resumen'] });
    },
  });
};

export const useCambiarEstadoAnuncio = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, estado }: { id: number; estado: string }) =>
      anuncioService.cambiarEstado(id, estado),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['vehiculos'] });
      queryClient.invalidateQueries({ queryKey: ['mis-anuncios'] });
      queryClient.invalidateQueries({ queryKey: ['vehiculo', id] });
    },
  });
};

export const useSubirImagenesAnuncio = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, imagenes }: { id: number; imagenes: File[] }) =>
      anuncioService.subirImagenes(id, imagenes),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['vehiculo', id] });
      queryClient.invalidateQueries({ queryKey: ['mis-anuncios'] });
    },
  });
};

export const useEstablecerFotoPrincipal = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, urlImagen }: { id: number; urlImagen: string }) =>
      anuncioService.establecerFotoPrincipal(id, urlImagen),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['vehiculo', id] });
      queryClient.invalidateQueries({ queryKey: ['mis-anuncios'] });
    },
  });
};

export const useEliminarImagenAnuncio = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, urlImagen }: { id: number; urlImagen: string }) =>
      anuncioService.eliminarImagen(id, urlImagen),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['vehiculo', id] });
      queryClient.invalidateQueries({ queryKey: ['mis-anuncios'] });
    },
  });
};

export const useEliminarAnuncio = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => anuncioService.eliminarAnuncio(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehiculos'] });
      queryClient.invalidateQueries({ queryKey: ['mis-anuncios'] });
    },
  });
};

export const useMisAnuncios = (usuarioId: number, enabled = true) => {
  const queryClient = useQueryClient();

  const query = useQuery<PagedResult<AnuncioListado>>({
    queryKey: ['mis-anuncios', usuarioId],
    queryFn: () => anuncioService.obtenerMisAnuncios(usuarioId),
    enabled: enabled && usuarioId > 0,
    staleTime: 1000 * 60 * 2,
    refetchInterval: 1000 * 45,
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['mis-anuncios', usuarioId] });
  };

  return { ...query, invalidate };
};
