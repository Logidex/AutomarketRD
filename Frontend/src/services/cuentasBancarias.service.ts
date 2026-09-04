import api from "./api";

export interface CuentaBancaria {
  id: number;
  banco: number;
  nombreTitular: string;
  numeroCuenta: string;
  tipoCuenta: string;
  documento: string;
  conceptoReferencia: string;
  activa: boolean;
}

// La API serializa enums como strings; normaliza a number para el frontend.
const BANCO_A_NUMERO: Record<string, number> = {
  Popular: 1,
  QIK: 2,
  // Valor legacy por si el backend aún corre el enum viejo
  BHDLleon: 2,
};

function normalizarCuenta(raw: CuentaBancaria | (Omit<CuentaBancaria, "banco"> & { banco: string | number })): CuentaBancaria {
  const bancoNumero =
    typeof raw.banco === "string" ? BANCO_A_NUMERO[raw.banco] ?? 0 : raw.banco;
  return { ...raw, banco: bancoNumero };
}

export const cuentasBancariasService = {
  obtenerActivas: async (): Promise<CuentaBancaria[]> => {
    const { data } = await api.get("/api/cuentas-bancarias");
    return (data as (CuentaBancaria | (Omit<CuentaBancaria, "banco"> & { banco: string | number }))[]).map(normalizarCuenta);
  },

  obtenerTodas: async (): Promise<CuentaBancaria[]> => {
    const { data } = await api.get("/api/admin/cuentas-bancarias");
    return (data as (CuentaBancaria | (Omit<CuentaBancaria, "banco"> & { banco: string | number }))[]).map(normalizarCuenta);
  },

  crear: async (cuenta: Omit<CuentaBancaria, "id" | "activa">): Promise<CuentaBancaria> => {
    const { data } = await api.post("/api/admin/cuentas-bancarias", cuenta);
    return normalizarCuenta(data);
  },

  actualizar: async (id: number, cuenta: Omit<CuentaBancaria, "id" | "activa">): Promise<CuentaBancaria> => {
    const { data } = await api.put(`/api/admin/cuentas-bancarias/${id}`, cuenta);
    return normalizarCuenta(data);
  },

  toggle: async (id: number): Promise<void> => {
    await api.patch(`/api/admin/cuentas-bancarias/${id}/toggle`);
  },
};
