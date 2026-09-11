import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { adslotsService } from '../services/adslots.service';
import type {
  CrearAdSlotAnuncioDto,
  CrearAdSlotAdminDto,
  ActualizarAdSlotAdminDto,
} from '../types/adslot.types';

// ========================
// PÚBLICOS
// ========================

export const useSlotsPublicos = (ubicacion: string) => {
  return useQuery({
    queryKey: ['adslots-publico', ubicacion],
    queryFn: () => adslotsService.obtenerSlotsPublicos(ubicacion),
    staleTime: 1000 * 60 * 5,
  });
};

// ========================
// DEALER
// ========================

export const useSlotsDisponibles = () => {
  return useQuery({
    queryKey: ['adslots-disponibles'],
    queryFn: () => adslotsService.obtenerSlotsDisponibles(),
    staleTime: 1000 * 60 * 5,
  });
};

export const useMisAnunciosPublicitarios = () => {
  return useQuery({
    queryKey: ['adslots-mis-anuncios'],
    queryFn: () => adslotsService.obtenerMisAnuncios(),
    staleTime: 1000 * 60 * 2,
  });
};

export const useCrearAnuncioPublicitario = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CrearAdSlotAnuncioDto) => adslotsService.crearAnuncio(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adslots-mis-anuncios'] });
      queryClient.invalidateQueries({ queryKey: ['adslots-mis-estadisticas'] });
    },
  });
};

export const useSubirImagenAdSlot = () => {
  return useMutation({
    mutationFn: ({ imagen, ubicacion }: { imagen: File; ubicacion: string }) =>
      adslotsService.subirImagen(imagen, ubicacion),
  });
};

export const useCancelarAnuncioPublicitario = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (anuncioId: number) => adslotsService.cancelarAnuncio(anuncioId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adslots-mis-anuncios'] });
      queryClient.invalidateQueries({ queryKey: ['adslots-mis-estadisticas'] });
    },
  });
};

export const useEstadisticasAnuncios = () => {
  return useQuery({
    queryKey: ['adslots-mis-estadisticas'],
    queryFn: () => adslotsService.obtenerEstadisticas(),
    staleTime: 1000 * 60 * 2,
  });
};

// ========================
// ADMIN
// ========================

export const useAdminAdSlots = () => {
  return useQuery({
    queryKey: ['admin-adslots'],
    queryFn: () => adslotsService.obtenerSlots(),
    staleTime: 1000 * 60 * 2,
  });
};

export const useCrearAdSlot = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CrearAdSlotAdminDto) => adslotsService.crearSlot(dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-adslots'] }),
  });
};

export const useActualizarAdSlot = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: ActualizarAdSlotAdminDto }) =>
      adslotsService.actualizarSlot(id, dto),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-adslots'] }),
  });
};

export const useEliminarAdSlot = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adslotsService.eliminarSlot(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-adslots'] }),
  });
};

export const useAdminAdSlotAnuncios = () => {
  return useQuery({
    queryKey: ['admin-adslots-anuncios'],
    queryFn: () => adslotsService.obtenerAnunciosAdmin(),
    staleTime: 1000 * 60 * 2,
  });
};

export const useRechazarAnuncioAdSlot = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adslotsService.rechazarAnuncio(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-adslots-anuncios'] }),
  });
};

export const useEliminarAnuncioAdSlotAdmin = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adslotsService.eliminarAnuncioAdmin(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-adslots-anuncios'] }),
  });
};
