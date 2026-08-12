import { API_BASE_URL } from '../services/api';

function extraerClave(referencia: string): string {
  const valor = referencia.trim();

  if (/^https?:\/\//i.test(valor)) {
    const coincidencia = valor.match(/\/\/[^/]+\/([^?#]+)/);
    if (coincidencia) return coincidencia[1];
  }

  return valor.replace(/^\/+/, '');
}

export function urlImagen(referencia?: string | null): string {
  if (!referencia) return '';
  return `${API_BASE_URL}/api/archivos/${extraerClave(referencia)}`;
}