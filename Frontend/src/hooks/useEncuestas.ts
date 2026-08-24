import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  encuestasService,
  type EncuestaResultados,
} from '../services/encuestas.service';

/**
 * Encuesta activa. Solo se consulta para usuarios autenticados: el backend
 * responde 404 si no hay encuesta (retry: false evita reintentos ruidosos).
 */
export const useEncuestaActiva = (habilitada: boolean) => {
  return useQuery({
    queryKey: ['encuesta-activa'],
    queryFn: () => encuestasService.obtenerActiva(),
    enabled: habilitada,
    staleTime: 1000 * 60 * 5,
    retry: false,
  });
};

export const useResponderEncuesta = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      encuestaId,
      respuestas,
    }: {
      encuestaId: number;
      respuestas: { preguntaId: number; valorEscala?: number; valorTexto?: string }[];
    }) => encuestasService.responder(encuestaId, respuestas),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['encuesta-activa'] });
    },
  });
};

export const useResultadosEncuesta = (encuestaId: number | null) => {
  return useQuery<EncuestaResultados>({
    queryKey: ['encuesta-resultados', encuestaId],
    queryFn: () => encuestasService.obtenerResultados(encuestaId as number),
    enabled: encuestaId !== null,
  });
};
