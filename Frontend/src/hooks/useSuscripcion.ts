import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  suscripcionService,
  type SuscripcionDealer,
  type PagoSuscripcion,
} from '../services/suscripcion.service';
import { vendedorService, type SuscripcionVendedor } from '../services/vendedor.service';
import { planesService, type PlanCatalogo } from '../services/planes.service';
import { pagosService } from '../services/pagos.service';
import { cuponesService } from '../services/cupones.service';
import { authService } from '../services/auth.service';

export const useSuscripcion = () => {
  const rol = authService.getRole();
  const esVendedor = rol === 'Vendedor';

  return useQuery<SuscripcionVendedor | SuscripcionDealer>({
    queryKey: esVendedor ? ['suscripcion-vendedor'] : ['suscripcion'],
    queryFn: esVendedor
      ? () => vendedorService.obtenerSuscripcion()
      : () => suscripcionService.obtenerSuscripcion(),
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

export const useMaxFotosAnuncio = (defaultMaxFotos = 8) => {
  const { data: suscripcion } = useSuscripcion();
  const { data: planes = [] } = usePlanesCatalogo();

  if (!suscripcion) return defaultMaxFotos;

  if ('maxFotos' in suscripcion && typeof suscripcion.maxFotos === 'number') {
    return suscripcion.maxFotos;
  }

  const plan = planes.find((p) => p.nivel === (suscripcion as SuscripcionDealer).nivel);
  return plan?.maxFotos ?? defaultMaxFotos;
};

export const useCuotaDestacadosPlan = (): number => {
  const { data: suscripcion } = useSuscripcion();
  const { data: planes = [] } = usePlanesCatalogo();

  if (!suscripcion) return 0;

  if ('cuotaDestacados' in suscripcion && typeof suscripcion.cuotaDestacados === 'number') {
    return suscripcion.cuotaDestacados;
  }

  const plan = planes.find((p) => p.nivel === (suscripcion as SuscripcionDealer).nivel);
  return plan?.cuotaDestacados ?? 0;
};

/** Indica si el plan del usuario permite destacar anuncios (cuota > 0). */
export const usePermiteDestacarAnuncio = (): boolean => {
  return useCuotaDestacadosPlan() > 0;
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
      queryClient.invalidateQueries({ queryKey: ['historial-pagos'] });
    },
  });
};

export const useAplicarCupon = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (codigo: string) => cuponesService.aplicarCupon(codigo),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['suscripcion'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard-resumen'] });
      queryClient.invalidateQueries({ queryKey: ['planes'] });
    },
  });
};

export const useRegistrarTransferencia = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ plan, ciclo, imagen }: { plan: string; ciclo: string; imagen: File }) =>
      pagosService.registrarTransferencia(plan, ciclo, imagen),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['suscripcion'] });
      queryClient.invalidateQueries({ queryKey: ['historial-pagos'] });
    },
  });
};