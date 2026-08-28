import api from './api';

export interface EncuestaActiva {
  id: number;
  titulo: string;
  descripcion?: string | null;
  yaRespondio: boolean;
  preguntas: EncuestaPregunta[];
}

export interface EncuestaPregunta {
  id: number;
  texto: string;
  orden: number;
  /** El backend serializa el enum como string (JsonStringEnumConverter) */
  tipo: 'Escala' | 'Abierta';
}

export interface EncuestaResultados {
  encuestaId: number;
  titulo: string;
  activa: boolean;
  totalRespondentes: number;
  preguntas: ResultadoPregunta[];
}

export interface ResultadoPregunta {
  preguntaId: number;
  texto: string;
  tipo: 'Escala' | 'Abierta';
  totalRespuestas: number;
  promedioEscala?: number | null;
  distribucion: Record<string, number>;
  comentarios: { fechaUtc: string; texto: string }[];
}

export const encuestasService = {
  async obtenerActiva(): Promise<EncuestaActiva> {
    const response = await api.get<EncuestaActiva>('/api/encuestas/activa');
    return response.data;
  },

  async responder(
    encuestaId: number,
    respuestas: { preguntaId: number; valorEscala?: number; valorTexto?: string }[]
  ): Promise<void> {
    await api.post('/api/encuestas/respuestas', { encuestaId, respuestas });
  },

  async obtenerResultados(encuestaId: number): Promise<EncuestaResultados> {
    const response = await api.get<EncuestaResultados>(`/api/encuestas/${encuestaId}/resultados`);
    return response.data;
  },
};
