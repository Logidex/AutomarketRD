import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  adminService,
  type AdminResumen,
  type UsuarioAdmin,
  type AnuncioAdmin,
  type PlanAdmin,
  type CambiarRolAdminDto,
  type PlanAdminForm,
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
