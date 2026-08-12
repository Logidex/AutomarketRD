import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  suscripcionService,
  type SuscripcionDealer,
  type PagoSuscripcion,
} from '../services/suscripcion.service';
import { planesService, type PlanCatalogo } from '../services/planes.service';
import { pagosService } from '../services/pagos.service';

export const useSuscripcion = () => {
  return useQuery<SuscripcionDealer>({
    queryKey: ['suscripcion'],
    queryFn: () => suscripcionService.obtenerSuscripcion(),
    staleTime: 1000 * 60 * 2,
    retry: false,
  });
};

export const usePlanesCatalogo = () => {
  return useQuery<PlanCatalogo[]>({
    queryKey: ['planes'],
    queryFn: () => planesService.obtenerCatalogo(),
    staleTime: 1000 * 60 * 60,
  });
};

export const useHistorialPagos = () => {
  return useQuery<PagoSuscripcion[]>({
    queryKey: ['historial-pagos'],
    queryFn: () => suscripcionService.obtenerHistorialPagos(),
    staleTime: 1000 * 60 * 2,
    retry: false,
  });
};

export const useCancelarSuscripcion = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => suscripcionService.cancelarSuscripcion(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['suscripcion'] });
    },
  });
};

export const useGenerarLinkPago = () => {
  return useMutation({
    mutationFn: ({ plan, ciclo }: { plan: string; ciclo: string }) =>
      pagosService.generarLinkPago(plan, ciclo),
  });
};

export const useConfirmarPago = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (orderId: string) => pagosService.confirmarPago(orderId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['suscripcion'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard-resumen'] });
    },
  });
};
