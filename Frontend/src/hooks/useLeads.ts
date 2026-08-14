import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { leadService } from '../services/lead.service';
import type { LeadDealer, LeadContactoUsuario } from '../types/lead.types';

export const useMisLeads = () => {
  return useQuery<LeadDealer[]>({
    queryKey: ['leads'],
    queryFn: () => leadService.obtenerMisLeads(),
    staleTime: 1000 * 60,
  });
};

export const useMisContactos = () => {
  return useQuery<LeadContactoUsuario[]>({
    queryKey: ['contactos'],
    queryFn: () => leadService.obtenerMisContactos(),
    staleTime: 1000 * 60,
  });
};

export const useMarcarLeido = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => leadService.marcarLeido(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leads'] });
      queryClient.invalidateQueries({ queryKey: ['leads-resumen-no-leidos'] });
    },
  });
};

export const useMarcarTodosLeidos = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => leadService.marcarTodosLeidos(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leads'] });
      queryClient.invalidateQueries({ queryKey: ['leads-resumen-no-leidos'] });
    },
  });
};

export const useCrearLead = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: leadService.crearLead,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leads'] });
      queryClient.invalidateQueries({ queryKey: ['leads-resumen-no-leidos'] });
      queryClient.invalidateQueries({ queryKey: ['contactos'] });
    },
  });
};
