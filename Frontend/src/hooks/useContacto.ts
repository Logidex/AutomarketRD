import { useMutation } from '@tanstack/react-query';
import { contactoService, type ContactoCreateDto } from '../services/contacto.service';

export const useEnviarContacto = () => {
  return useMutation({
    mutationFn: (dto: ContactoCreateDto) => contactoService.enviarMensaje(dto),
  });
};
