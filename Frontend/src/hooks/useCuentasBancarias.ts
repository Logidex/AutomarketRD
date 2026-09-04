import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { cuentasBancariasService } from "../services/cuentasBancarias.service";
import type { CuentaBancaria } from "../services/cuentasBancarias.service";

export function useCuentasBancariasActivas() {
  return useQuery({
    queryKey: ["cuentas-bancarias"],
    queryFn: () => cuentasBancariasService.obtenerActivas(),
    staleTime: 1000 * 60 * 10,
  });
}

export function useAdminCuentasBancarias() {
  return useQuery({
    queryKey: ["admin-cuentas-bancarias"],
    queryFn: () => cuentasBancariasService.obtenerTodas(),
  });
}

export function useCrearCuentaBancaria() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (cuenta: Omit<CuentaBancaria, "id" | "activa">) =>
      cuentasBancariasService.crear(cuenta),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-cuentas-bancarias"] });
      queryClient.invalidateQueries({ queryKey: ["cuentas-bancarias"] });
    },
  });
}

export function useActualizarCuentaBancaria() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, cuenta }: { id: number; cuenta: Omit<CuentaBancaria, "id" | "activa"> }) =>
      cuentasBancariasService.actualizar(id, cuenta),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-cuentas-bancarias"] });
      queryClient.invalidateQueries({ queryKey: ["cuentas-bancarias"] });
    },
  });
}

export function useToggleCuentaBancaria() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => cuentasBancariasService.toggle(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-cuentas-bancarias"] });
      queryClient.invalidateQueries({ queryKey: ["cuentas-bancarias"] });
    },
  });
}
