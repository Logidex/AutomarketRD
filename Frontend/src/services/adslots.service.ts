import api from './api';
import type {
  AdSlotPublico,
  AdSlot,
  AdSlotAnuncioPublico,
  AdSlotAnuncioAdmin,
  AdSlotStats,
  CrearAdSlotAnuncioDto,
  CrearAdSlotAdminDto,
  ActualizarAdSlotAdminDto,
} from '../types/adslot.types';

export const adslotsService = {
  // Públicos
  async obtenerSlotsPublicos(ubicacion: string): Promise<AdSlotPublico[]> {
    const response = await api.get<AdSlotPublico[]>(
      `/api/adslots/publico?ubicacion=${ubicacion}`
    );
    return response.data;
  },

  async registrarImpresion(anuncioId: number): Promise<void> {
    await api.post(`/api/adslots/track/impresion/${anuncioId}`);
  },

  async registrarClick(anuncioId: number): Promise<void> {
    await api.post(`/api/adslots/track/click/${anuncioId}`);
  },

  // Dealer
  async obtenerSlotsDisponibles(): Promise<AdSlotPublico[]> {
    const response = await api.get<AdSlotPublico[]>('/api/adslots/disponibles');
    return response.data;
  },

  async obtenerMisAnuncios(): Promise<AdSlotAnuncioPublico[]> {
    const response = await api.get<AdSlotAnuncioPublico[]>('/api/adslots/mis-anuncios');
    return response.data;
  },

  async crearAnuncio(dto: CrearAdSlotAnuncioDto): Promise<AdSlotAnuncioPublico> {
    const response = await api.post<AdSlotAnuncioPublico>('/api/adslots/crear', dto);
    return response.data;
  },

  async subirImagen(
    imagen: File,
    ubicacion: string
  ): Promise<{ exito: boolean; imagenOriginal: string; imagenRedimensionada: string }> {
    const formData = new FormData();
    formData.append('imagen', imagen);
    const response = await api.post(
      `/api/adslots/subir-imagen?ubicacion=${ubicacion}`,
      formData,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return response.data;
  },

  async cancelarAnuncio(anuncioId: number): Promise<void> {
    await api.delete(`/api/adslots/cancelar/${anuncioId}`);
  },

  async obtenerEstadisticas(): Promise<AdSlotStats> {
    const response = await api.get<AdSlotStats>('/api/adslots/mis-estadisticas');
    return response.data;
  },

  // Admin
  async obtenerSlots(): Promise<AdSlot[]> {
    const response = await api.get<AdSlot[]>('/api/admin/adslots');
    return response.data;
  },

  async crearSlot(dto: CrearAdSlotAdminDto): Promise<AdSlot> {
    const response = await api.post<AdSlot>('/api/admin/adslots', dto);
    return response.data;
  },

  async actualizarSlot(id: number, dto: ActualizarAdSlotAdminDto): Promise<AdSlot> {
    const response = await api.put<AdSlot>(`/api/admin/adslots/${id}`, dto);
    return response.data;
  },

  async eliminarSlot(id: number): Promise<void> {
    await api.delete(`/api/admin/adslots/${id}`);
  },

  async obtenerAnunciosAdmin(): Promise<AdSlotAnuncioAdmin[]> {
    const response = await api.get<AdSlotAnuncioAdmin[]>('/api/admin/adslots/anuncios');
    return response.data;
  },

  async rechazarAnuncio(id: number): Promise<void> {
    await api.post(`/api/admin/adslots/anuncios/${id}/rechazar`);
  },

  async eliminarAnuncioAdmin(id: number): Promise<void> {
    await api.delete(`/api/admin/adslots/anuncios/${id}`);
  },
};
