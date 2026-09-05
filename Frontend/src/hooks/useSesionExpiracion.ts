import { useEffect, useRef, useCallback, useState } from 'react';
import { authService } from '../services/auth.service';

const DURACION_TOKEN_MS = 2 * 60 * 60 * 1000; // 2 horas
const AVISO_ANTES_MS = 5 * 60 * 1000; // 5 minutos antes

export function useSesionExpiracion() {
  const [mostrarToast, setMostrarToast] = useState(false);
  const [segundosRestantes, setSegundosRestantes] = useState(0);
  const timerAvisoRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const timerConteoRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const fechaExpiracionRef = useRef<number>(Date.now() + DURACION_TOKEN_MS);

  const limpiarTimers = useCallback(() => {
    if (timerAvisoRef.current) {
      clearTimeout(timerAvisoRef.current);
      timerAvisoRef.current = null;
    }
    if (timerConteoRef.current) {
      clearInterval(timerConteoRef.current);
      timerConteoRef.current = null;
    }
  }, []);

  const programarAviso = useCallback(() => {
    limpiarTimers();
    const ahora = Date.now();
    const tiempoParaAviso = fechaExpiracionRef.current - ahora - AVISO_ANTES_MS;

    if (tiempoParaAviso <= 0) {
      setMostrarToast(true);
      setSegundosRestantes(Math.max(0, Math.floor((fechaExpiracionRef.current - ahora) / 1000)));
    } else {
      timerAvisoRef.current = setTimeout(() => {
        setMostrarToast(true);
        setSegundosRestantes(Math.floor(AVISO_ANTES_MS / 1000));
      }, tiempoParaAviso);
    }
  }, [limpiarTimers]);

  const extenderSesion = useCallback(async () => {
    try {
      await authService.refrescarSesion();
      fechaExpiracionRef.current = Date.now() + DURACION_TOKEN_MS;
      setMostrarToast(false);
      programarAviso();
    } catch {
      setMostrarToast(false);
      limpiarTimers();
      authService.logout();
      window.location.assign('/login');
    }
  }, [programarAviso, limpiarTimers]);

  const cerrarToast = useCallback(() => {
    setMostrarToast(false);
  }, []);

  // Al login: inicializar el timer
  useEffect(() => {
    if (!authService.isAuthenticated()) return;

    fechaExpiracionRef.current = Date.now() + DURACION_TOKEN_MS;
    programarAviso();

    return limpiarTimers;
  }, [programarAviso, limpiarTimers]);

  // Conteo regresivo cuando el toast está visible
  useEffect(() => {
    if (!mostrarToast) return;

    timerConteoRef.current = setInterval(() => {
      const restantes = Math.max(0, Math.floor((fechaExpiracionRef.current - Date.now()) / 1000));
      setSegundosRestantes(restantes);
      if (restantes <= 0) {
        setMostrarToast(false);
        clearInterval(timerConteoRef.current!);
      }
    }, 1000);

    return () => {
      if (timerConteoRef.current) clearInterval(timerConteoRef.current);
    };
  }, [mostrarToast]);

  // Resetear timer tras cada refresh exitoso
  useEffect(() => {
    const handler = () => {
      fechaExpiracionRef.current = Date.now() + DURACION_TOKEN_MS;
      programarAviso();
    };

    window.addEventListener('sesion:refrescada', handler);
    return () => window.removeEventListener('sesion:refrescada', handler);
  }, [programarAviso]);

  return { mostrarToast, segundosRestantes, extenderSesion, cerrarToast };
}
