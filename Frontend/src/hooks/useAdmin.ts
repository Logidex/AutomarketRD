import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  adminService,
  type AdminResumen,
  type UsuarioAdmin,
  type AnuncioAdmin,
  type PlanAdmin,
  type PagoAdmin,
  type CambiarRolAdminDto,
  type PlanAdminForm,
  type ReporteAdmin,
} from '../services/admin.service';

export const useAdminResumen = () => {
  return useQuery<AdminResumen>({
    queryKey: ['admin-resumen'],
    queryFn: () => adminService.obtenerResumen(),
    staleTime: 1000 * 60 * 2,
  });
};

export const useAdminUsuarios = () => {
  return useQuery<UsuarioAdmin[]>({
    queryKey: ['admin-usuarios'],
    queryFn: () => adminService.listarUsuarios(),
    staleTime: 1000 * 60 * 2,
  });
};

export const useAdminAnuncios = () => {
  return useQuery<AnuncioAdmin[]>({
    queryKey: ['admin-anuncios'],
    queryFn: () => adminService.listarAnuncios(),
    staleTime: 1000 * 60 * 2,
  });
};

export const useAdminPlanes = () => {
  return useQuery<PlanAdmin[]>({
    queryKey: ['admin-planes'],
    queryFn: () => adminService.listarPlanes(),
    staleTime: 1000 * 60 * 2,
  });
};

export const useAdminPagos = () => {
  return useQuery<PagoAdmin[]>({
    queryKey: ['admin-pagos'],
    queryFn: () => adminService.listarPagos(),
    staleTime: 1000 * 60 * 2,
  });
};

export const useReembolsarPago = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adminService.reembolsarPago(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-pagos'] }),
  });
};

export const useSuspenderUsuario = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adminService.suspenderUsuario(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-usuarios'] }),
  });
};

export const useReactivarUsuario = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adminService.reactivarUsuario(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-usuarios'] }),
  });
};

export const useCambiarRolUsuario = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, datos }: { id: number; datos: CambiarRolAdminDto }) =>
      adminService.cambiarRol(id, datos),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-usuarios'] }),
  });
};

export const useEliminarAnuncioAdmin = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adminService.eliminarAnuncio(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-anuncios'] });
      queryClient.invalidateQueries({ queryKey: ['vehiculos'] });
    },
  });
};

export const useCrearPlan = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (datos: PlanAdminForm) => adminService.crearPlan(datos),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-planes'] }),
  });
};

export const useActualizarPlan = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, datos }: { id: number; datos: PlanAdminForm }) =>
      adminService.actualizarPlan(id, datos),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-planes'] }),
  });
};

export const useEliminarPlan = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adminService.eliminarPlan(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-planes'] }),
  });
};

export const useCambiarPlanDealer = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ dealerId, nivel }: { dealerId: number; nivel: string }) =>
      adminService.cambiarPlan(dealerId, nivel),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-usuarios'] }),
  });
};

export const useRenovarSuscripcion = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ dealerId, nuevaFechaVencimiento }: { dealerId: number; nuevaFechaVencimiento: string }) =>
      adminService.renovarSuscripcion(dealerId, nuevaFechaVencimiento),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-usuarios'] }),
  });
};

// ===== Reportes de anuncios =====
export type EstadoReporte = 'Pendiente' | 'Descartado' | 'Resuelto';

export const useAdminReportes = (estado: EstadoReporte = 'Pendiente') => {
  return useQuery<ReporteAdmin[]>({
    queryKey: ['admin-reportes', estado],
    queryFn: () => adminService.listarReportes(estado),
    staleTime: 1000 * 30,
  });
};

export const useContarReportesPendientes = () => {
  return useQuery<{ total: number }>({
    queryKey: ['admin-reportes-pendientes'],
    queryFn: () => adminService.contarReportesPendientes(),
    staleTime: 1000 * 60,
    refetchInterval: 1000 * 60 * 5,
  });
};

const invalidarReportes = (queryClient: ReturnType<typeof useQueryClient>) => {
  queryClient.invalidateQueries({ queryKey: ['admin-reportes'] });
  queryClient.invalidateQueries({ queryKey: ['admin-reportes-pendientes'] });
};

export const useDescartarReporte = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adminService.descartarReporte(id),
    onSuccess: () => invalidarReportes(queryClient),
  });
};

export const useResolverReporte = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adminService.resolverReporte(id),
    onSuccess: () => {
      // Resolver elimina el anuncio: refrescar también catálogos admin
      invalidarReportes(queryClient);
      queryClient.invalidateQueries({ queryKey: ['admin-anuncios'] });
    },
  });
};

export const useEliminarUsuario = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adminService.eliminarUsuario(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-usuarios'] });
      queryClient.invalidateQueries({ queryKey: ['admin-resumen'] });
    },
  });
};

// ===== Transferencias bancarias =====
export const useAdminTransferencias = () => {
  return useQuery<PagoAdmin[]>({
    queryKey: ['admin-transferencias'],
    queryFn: () => adminService.listarTransferenciasPendientes(),
    staleTime: 1000 * 60 * 2,
  });
};

export const useAprobarTransferencia = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, notas }: { id: number; notas?: string }) =>
      adminService.aprobarTransferencia(id, notas),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-transferencias'] });
      queryClient.invalidateQueries({ queryKey: ['admin-pagos'] });
      queryClient.invalidateQueries({ queryKey: ['admin-usuarios'] });
    },
  });
};

export const useRechazarTransferencia = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, notas }: { id: number; notas?: string }) =>
      adminService.rechazarTransferencia(id, notas),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-transferencias'] });
      queryClient.invalidateQueries({ queryKey: ['admin-pagos'] });
      queryClient.invalidateQueries({ queryKey: ['admin-usuarios'] });
    },
  });
};
