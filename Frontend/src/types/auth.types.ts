export interface LoginDto {
  email: string;
  password: string;
}

export interface AuthResponse {
  exito: boolean;
  mensaje: string;
  token?: string;
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