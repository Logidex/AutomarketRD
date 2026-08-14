import { beforeEach, describe, expect, it, vi } from 'vitest';
import { getUserIdFromToken } from './jwt.util';

const storage = new Map<string, string>();

vi.stubGlobal('localStorage', {
  getItem: (key: string) => storage.get(key) ?? null,
  setItem: (key: string, value: string) => storage.set(key, value),
  removeItem: (key: string) => storage.delete(key),
  clear: () => storage.clear(),
});

describe('getUserIdFromToken', () => {
  beforeEach(() => storage.clear());

  it('devuelve null sin usuario guardado', () => {
    expect(getUserIdFromToken()).toBeNull();
  });

  it('extrae el id del usuario guardado (user:v1)', () => {
    localStorage.setItem(
      'user:v1',
      JSON.stringify({ usuarioId: 42, nombre: 'Juan' }),
    );
    expect(getUserIdFromToken()).toBe(42);
  });

  it('falla a la clave legacy "user" si no existe user:v1', () => {
    localStorage.setItem(
      'user',
      JSON.stringify({ usuarioId: 7, nombre: 'Ana' }),
    );
    expect(getUserIdFromToken()).toBe(7);
  });

  it('devuelve null si el id no es numérico', () => {
    localStorage.setItem(
      'user:v1',
      JSON.stringify({ usuarioId: 'abc', nombre: 'Juan' }),
    );
    expect(getUserIdFromToken()).toBeNull();
  });

  it('devuelve null si el JSON es inválido', () => {
    localStorage.setItem('user:v1', 'no-es-json');
    expect(getUserIdFromToken()).toBeNull();
  });
});