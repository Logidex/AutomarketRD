import { describe, expect, it } from 'vitest';
import {
  formatearRD$,
  formatearPrecio,
  precioCicloDe,
  type Ciclo,
} from './formato';

describe('formatearRD$', () => {
  it('devuelve "Gratis" cuando el valor es 0 o nulo', () => {
    expect(formatearRD$(0)).toBe('Gratis');
    expect(formatearRD$(null)).toBe('Gratis');
    expect(formatearRD$(undefined)).toBe('Gratis');
  });

  it('formatea montos positivos con el símbolo RD$', () => {
    expect(formatearRD$(1000)).toBe('RD$ 1,000');
    expect(formatearRD$(1234567.5)).toBe('RD$ 1,234,567.5');
  });
});

describe('formatearPrecio', () => {
  it('devuelve "Gratis" cuando el valor es 0 o nulo y la moneda es DOP', () => {
    expect(formatearPrecio(0)).toBe('Gratis');
    expect(formatearPrecio(null)).toBe('Gratis');
    expect(formatearPrecio(undefined)).toBe('Gratis');
  });

  it('formatea en RD$ por defecto (sin moneda o DOP)', () => {
    expect(formatearPrecio(1000)).toBe('RD$ 1,000');
    expect(formatearPrecio(1000, 'DOP')).toBe('RD$ 1,000');
    expect(formatearPrecio(1000, 'dop')).toBe('RD$ 1,000');
  });

  it('formatea en dólares cuando la moneda es USD', () => {
    expect(formatearPrecio(15000, 'USD')).toBe('US$ 15,000');
    expect(formatearPrecio(15000, 'usd')).toBe('US$ 15,000');
  });
});

describe('precioCicloDe', () => {
  const plan = {
    precioMensual: 1000,
    precioTrimestral: 2700,
    precioAnual: 9600,
  };

  it('devuelve el precio según el ciclo', () => {
    expect(precioCicloDe(plan, 'Mensual')).toBe(1000);
    expect(precioCicloDe(plan, 'Trimestral')).toBe(2700);
    expect(precioCicloDe(plan, 'Anual')).toBe(9600);
  });

  it('usa el mensual como valor por defecto para ciclos desconocidos', () => {
    expect(precioCicloDe(plan, 'Otro' as Ciclo)).toBe(1000);
  });
});
