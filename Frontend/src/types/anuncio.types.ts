export interface AnuncioListado {
  id: number;
  usuarioId: number;
  nombreAnuncio: string;
  marca: string;
  modelo: string;
  tipoVehiculo: string;
  colorExterior: string;
  colorInterior: string;
  anio: number;
  precio: number;
  kilometraje: number;
  transmision: string;
  combustible: string;
  accesorios: string[];
  ubicacion: string;
  descripcion: string;
  estado: string;
  fotos: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalRegistros: number;
  paginaActual: number;
  cantidadPorPagina: number;
}

export interface AnuncioCreateDto {
  usuarioId: number;
  marca: string;
  modelo: string;
  tipoVehiculo: string;
  colorExterior: string;
  colorInterior: string;
  anio: number;
  precio: number;
  kilometraje: number;
  transmision: string;
  combustible: string;
  accesorios: string[];
  ubicacion: string;
  descripcion: string;
}

export type AnuncioCreateFormDto = Omit<AnuncioCreateDto, "usuarioId" | "accesorios">;
export type AnuncioCreateRequestDto = Omit<AnuncioCreateDto, "usuarioId">;