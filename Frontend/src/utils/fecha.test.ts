import { describe, expect, it } from 'vitest';
import { formatearFecha } from './fecha';

describe('formatearFecha', () => {
  it('devuelve un guion cuando no hay fecha', () => {
    expect(formatearFecha(null)).toBe('—');
    expect(formatearFecha(undefined)).toBe('—');
    expect(formatearFecha('')).toBe('—');
  });

  it('formatea una fecha ISO en es-DO', () => {
    const resultado = formatearFecha('2026-08-11T14:30:00');
    expect(resultado).toMatch(/^\d{2} [a-záéíóúñ]+( de)? \d{4}$/i);
    expect(resultado).not.toContain(':');
  });

  it('incluye la hora cuando conHora es true', () => {
    const resultado = formatearFecha('2026-08-11T14:30:00', true);
    expect(resultado).toMatch(/:\d{2}/);
  });
});
