import { beforeEach, describe, expect, it, vi } from 'vitest';
import { getUserIdFromToken } from './jwt.util';

const storage = new Map<string, string>();

vi.stubGlobal('localStorage', {
  getItem: (key: string) => storage.get(key) ?? null,
  setItem: (key: string, value: string) => storage.set(key, value),
  removeItem: (key: string) => storage.delete(key),
  clear: () => storage.clear(),
});

const b64 = (obj: Record<string, unknown>) =>
  btoa(JSON.stringify(obj));

describe('getUserIdFromToken', () => {
  beforeEach(() => storage.clear());

  it('devuelve null sin token', () => {
    expect(getUserIdFromToken()).toBeNull();
  });

  it('extrae el id del claim nameid', () => {
    const token = `header.${b64({ nameid: 42, exp: 9999999999 })}.sig`;
    localStorage.setItem('token', token);
    expect(getUserIdFromToken()).toBe(42);
  });

  it('extrae el id del claim sub', () => {
    const token = `header.${b64({ sub: '7' })}.sig`;
    localStorage.setItem('token', token);
    expect(getUserIdFromToken()).toBe(7);
  });

  it('devuelve null si el id no es numérico', () => {
    const token = `header.${b64({ sub: 'abc' })}.sig`;
    localStorage.setItem('token', token);
    expect(getUserIdFromToken()).toBeNull();
  });

  it('devuelve null si el payload no es JSON válido', () => {
    localStorage.setItem('token', 'a.badsig.c');
    expect(getUserIdFromToken()).toBeNull();
  });
});
