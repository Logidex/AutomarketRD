import { describe, expect, it } from 'vitest';
import { ROLES } from './roles';
import {
  TIPOS_VEHICULO,
  TRANSMISIONES,
  COMBUSTIBLES,
  etiquetaDe,
} from './vehiculo.opciones';

describe('ROLES', () => {
  it('define los cuatro roles del sistema', () => {
    expect(ROLES.ADMIN).toBe('Admin');
    expect(ROLES.DEALER).toBe('Dealer');
    expect(ROLES.VENDEDOR).toBe('Vendedor');
    expect(ROLES.COMPRADOR).toBe('Comprador');
  });
});

describe('opciones de vehículo', () => {
  it('tienen valores y etiquetas no vacías', () => {
    for (const lista of [TIPOS_VEHICULO, TRANSMISIONES, COMBUSTIBLES]) {
      expect(lista.length).toBeGreaterThan(0);
      for (const opcion of lista) {
        expect(opcion.valor.trim()).not.toBe('');
        expect(opcion.etiqueta.trim()).not.toBe('');
      }
    }
  });

  it('los valores no se repiten dentro de cada lista', () => {
    const valores = TIPOS_VEHICULO.map((o) => o.valor);
    expect(new Set(valores).size).toBe(valores.length);
  });
});

describe('etiquetaDe', () => {
  it('traduce un valor conocido a su etiqueta', () => {
    expect(etiquetaDe('Sedan', TIPOS_VEHICULO)).toBe('Sedán');
    expect(etiquetaDe('Automatica', TRANSMISIONES)).toBe('Automática');
    expect(etiquetaDe('Diesel', COMBUSTIBLES)).toBe('Diésel');
  });

  it('devuelve el valor si no encuentra la etiqueta', () => {
    expect(etiquetaDe('Inexistente', TIPOS_VEHICULO)).toBe('Inexistente');
  });
});
