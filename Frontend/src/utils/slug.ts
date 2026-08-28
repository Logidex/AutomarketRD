/**
 * URLs descriptivas (slugs) para anuncios y vendedores.
 *
 * Patrón: el texto es decorativo y el ID viaja al final como fuente de
 * verdad. La página destino extrae el ID con `idDesdeSlug`, por lo que:
 *  - /anuncio/honda-civic-2019-25 y /anuncio/25 funcionan igual
 *  - cambiar el formato del texto nunca rompe links existentes
 *  - sin columna slug en BD ni cambios de API
 */

/** Minúsculas, sin acentos, solo [a-z0-9-], sin guiones repetidos ni extremos. */
export function slugificar(texto: string): string {
  return texto
    .toLowerCase()
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "") // acentos diacríticos -> letra base
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "");
}

interface AnuncioParaSlug {
  id: number;
  marca: string;
  modelo: string;
  anio: number;
}

/** /anuncio/honda-civic-2019-25 */
export function urlAnuncio(a: AnuncioParaSlug): string {
  const texto = slugificar(`${a.marca}-${a.modelo}-${a.anio}`) || "vehiculo";
  return `/anuncio/${texto}-${a.id}`;
}

/** /vendedor/autoventas-rd-5 */
export function urlVendedor(id: number, nombre: string): string {
  const texto = slugificar(nombre) || "vendedor";
  return `/vendedor/${texto}-${id}`;
}

/**
 * Extrae el ID de un parámetro de ruta: acepta slugs completos
 * ("honda-civic-2019-25" -> 25) e IDs puros ("25" -> 25).
 */
export function idDesdeSlug(param: string | undefined): number {
  if (!param) return NaN;

  const coincidencia = param.match(/(\d+)$/);
  return coincidencia ? Number(coincidencia[1]) : NaN;
}
