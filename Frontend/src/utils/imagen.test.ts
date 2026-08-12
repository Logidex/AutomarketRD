import { describe, expect, it, vi } from "vitest";
import { urlImagen } from "./imagen";

const API = "http://localhost:8080";

vi.mock("../services/api", () => ({
  API_BASE_URL: "http://localhost:8080",
}));

describe("urlImagen", () => {
  it("devuelve una cadena vacía si la referencia es nula o vacía", () => {
    expect(urlImagen(undefined)).toBe("");
    expect(urlImagen(null)).toBe("");
    expect(urlImagen("")).toBe("");
  });

  it("convierte una clave de objeto en la URL del proxy firmado", () => {
    expect(urlImagen("uploads/abc.jpg")).toBe(`${API}/api/archivos/uploads/abc.jpg`);
  });

  it("soporta una URL pública legada extrayendo la clave", () => {
    const legada = "https://automarketrd-s3.s3.us-east-2.amazonaws.com/uploads/logo.png";
    expect(urlImagen(legada)).toBe(`${API}/api/archivos/uploads/logo.png`);
  });

  it("ignora query strings de una URL legada", () => {
    const legada = "https://bucket.s3.amazonaws.com/uploads/auto.jpg?X-Amz-Signature=abc";
    expect(urlImagen(legada)).toBe(`${API}/api/archivos/uploads/auto.jpg`);
  });

  it("recorta una barra inicial de las claves", () => {
    expect(urlImagen("/uploads/auto.jpg")).toBe(`${API}/api/archivos/uploads/auto.jpg`);
  });
});
