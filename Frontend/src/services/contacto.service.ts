import api from './api';

export interface ContactoCreateDto {
  nombre: string;
  email: string;
  asunto: string;
  mensaje: string;
  website?: string;
}

export const contactoService = {
  async enviarMensaje(dto: ContactoCreateDto): Promise<{ mensaje: string }> {
    const response = await api.post<{ mensaje: string }>(
      '/api/contacto',
      dto,
    );
    return response.data;
  },
};
