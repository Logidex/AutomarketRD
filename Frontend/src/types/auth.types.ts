export interface LoginDto {
  email: string;
  password: string;
}

export interface UsuarioAuth {
  usuarioId: number;
  nombre: string;
  apellido: string;
  email: string;
  rol: string;
}

export interface AuthResponse {
  exito: boolean;
  mensaje: string;
  token?: string;
  usuario?: UsuarioAuth;
}

export interface RegistroDto {
  nombre: string;
  apellido: string;
  email: string;
  password: string;
  rol: string;
  telefonoPersonal?: string;
  nombreAgencia?: string;
  agenciaRNC?: string;
  ubicacionAgencia: string;
  telefonoAgencia: string;
}