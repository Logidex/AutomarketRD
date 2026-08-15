import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ticketService, type TicketCreateDto } from "../services/ticket.service";
import type { TicketDetalle, TicketEstado, TicketListado } from "../types/ticket.types";

export const useMisTickets = () => {
  return useQuery<TicketListado[]>({
    queryKey: ["tickets"],
    queryFn: () => ticketService.obtenerMisTickets(),
    staleTime: 1000 * 60,
  });
};

export const useTicket = (id: number | null) => {
  return useQuery<TicketDetalle>({
    queryKey: ["tickets", id],
    queryFn: () => ticketService.obtenerTicket(id!),
    enabled: id != null,
    staleTime: 1000 * 30,
  });
};

export const useCrearTicket = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: TicketCreateDto) => ticketService.crearTicket(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["tickets"] });
      queryClient.invalidateQueries({ queryKey: ["tickets-resumen-admin"] });
    },
  });
};

export const useResponderTicket = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, mensaje }: { id: number; mensaje: string }) =>
      ticketService.responderTicket(id, mensaje),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["tickets"] });
      queryClient.invalidateQueries({ queryKey: ["tickets", variables.id] });
    },
  });
};

export const useCerrarTicket = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => ticketService.cerrarTicket(id),
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ["tickets"] });
      queryClient.invalidateQueries({ queryKey: ["tickets", id] });
    },
  });
};

// ===== Admin =====

export const useTicketsAdmin = () => {
  return useQuery<TicketListado[]>({
    queryKey: ["tickets-admin"],
    queryFn: () => ticketService.obtenerTicketsAdmin(),
    staleTime: 1000 * 60,
  });
};

export const useTicketAdmin = (id: number | null) => {
  return useQuery<TicketDetalle>({
    queryKey: ["tickets-admin", id],
    queryFn: () => ticketService.obtenerTicketAdmin(id!),
    enabled: id != null,
    staleTime: 1000 * 30,
  });
};

export const useResponderTicketAdmin = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, mensaje }: { id: number; mensaje: string }) =>
      ticketService.responderTicketAdmin(id, mensaje),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["tickets-admin"] });
      queryClient.invalidateQueries({ queryKey: ["tickets-admin", variables.id] });
      queryClient.invalidateQueries({ queryKey: ["tickets-resumen-admin"] });
    },
  });
};

export const useCambiarEstadoTicket = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, estado }: { id: number; estado: TicketEstado }) =>
      ticketService.cambiarEstadoAdmin(id, estado),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["tickets-admin"] });
      queryClient.invalidateQueries({ queryKey: ["tickets-admin", variables.id] });
      queryClient.invalidateQueries({ queryKey: ["tickets-resumen-admin"] });
    },
  });
};

export const useResumenTicketsAdmin = () => {
  return useQuery({
    queryKey: ["tickets-resumen-admin"],
    queryFn: () => ticketService.obtenerResumenAdmin(),
    staleTime: 1000 * 60,
  });
};
