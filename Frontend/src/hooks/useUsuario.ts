import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { usuarioService, type UsuarioCuenta } from '../services/usuario.service';

export const useUsuarioCuenta = (usuarioId?: number) => {
  return useQuery<UsuarioCuenta>({
    queryKey: ['usuario-cuenta', usuarioId],
    queryFn: () => usuarioService.obtenerCuenta(),
    enabled: usuarioId != null,
    staleTime: 0,
    retry: false,
  });
};

export const useActualizarDatos = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (datos: {
      nombre: string;
      apellido: string;
      telefonoPersonal?: string | null;
    }) => usuarioService.actualizarDatos(datos),

    onSuccess: (cuenta) => {
      queryClient.setQueryData(
        ['usuario-cuenta', cuenta.usuarioId],
        cuenta,
      );
    },
  });
};

export const useCambiarPassword = () => {
  return useMutation({
    mutationFn: ({ passwordActual, nuevaPassword }: { passwordActual: string; nuevaPassword: string }) =>
      usuarioService.cambiarPassword(passwordActual, nuevaPassword),
  });
};

export const useConfirmarCambioPassword = () => {
  return useMutation({
    mutationFn: (codigo: string) => usuarioService.confirmarCambioPassword(codigo),
  });
};

export const useSolicitarCambioEmail = () => {
  return useMutation({
    mutationFn: ({ passwordActual, nuevoEmail }: { passwordActual: string; nuevoEmail: string }) =>
      usuarioService.solicitarCambioEmail(passwordActual, nuevoEmail),
  });
};

export const useConfirmarCambioEmail = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (codigo: string) => usuarioService.confirmarCambioEmail(codigo),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuario-cuenta'] });
    },
  });
};
