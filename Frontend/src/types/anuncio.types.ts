export interface AnuncioListado {
  id: number;
  usuarioId: number;

  nombreAnuncio: string;

  marca: string;
  modelo: string;
  version: string;

  tipoVehiculo: string;
  motor: string;
  traccion: string;

  colorExterior: string;
  colorInterior: string;

  anio: number;
  precio: number;
  moneda: string;
  precioAnterior?: number | null;
  kilometraje: number;

  condicion: string;
  enOferta: boolean;

  transmision: string;
  combustible: string;

  accesorios: string[];
  ubicacion: string;
  descripcion: string;

  // Estado del anuncio:
  // Borrador, Publicado, Vendido, Pausado, etc.
  estado: string;

  vistas: number;

  fotos: string[];

  // Nivel de suscripción del vendedor (para prioridad)
  badgeSuscripcion?: string;

  // true si el vendedor es un dealer verificado (suscripción pagada + correo confirmado)
  esDealerVerificado?: boolean;

  // Fecha de vencimiento del anuncio publicado (null = sin vigencia definida)
  fechaVencimiento?: string | null;

  // Para ordenamiento
  createdAt?: string;

  // Destacados
  esDestacado?: boolean;
  fechaDestacadoHasta?: string | null;
}

export interface PagedResult<T> {
  items: T[];
  totalRegistros: number;
  paginaActual: number;
  cantidadPorPagina: number;
  totalPaginas?: number;
}

export interface AnuncioCreateDto {
  usuarioId: number;

  marca: string;
  modelo: string;
  version: string;

  tipoVehiculo: string;
  motor: string;
  traccion: string;

  colorExterior: string;
  colorInterior: string;

  anio: number;
  precio: number;
  moneda: string;
  precioAnterior?: number | null;
  kilometraje: number;

  transmision: string;
  combustible: string;

  accesorios: string[];
  ubicacion: string;
  descripcion: string;
}

export type AnuncioCreateFormDto = Omit<
  AnuncioCreateDto,
  'usuarioId' | 'accesorios'
>;

export type AnuncioCreateRequestDto = Omit<
  AnuncioCreateDto,
  'usuarioId'
>;

export interface AnuncioDetalle {
  id: number;
  usuarioId: number;
  nombreAnuncio: string;
  marca: string;
  modelo: string;
  version: string;
  tipoVehiculo: string;
  motor: string;
  traccion: string;
  colorExterior: string;
  colorInterior: string;
  anio: number;
  precio: number;
  moneda: string;
  precioAnterior?: number | null;
  kilometraje: number;
  transmision: string;
  combustible: string;
  ubicacion: string;
  descripcion: string;
  accesorios: string[];
  fotos: string[];
  estado: string;

  // Datos de contacto del vendedor, expuestos solo en el detalle público.
  nombreVendedor?: string;
  whatsAppContacto?: string;

  // true si es cuenta Vendedor (particular); false si es Dealer (agencia).
  esVendedorParticular: boolean;

  // Estado de destacado (para gestión desde el formulario del dueño).
  esDestacado?: boolean;
  fechaDestacadoHasta?: string | null;

  // Vigencia del anuncio publicado.
  fechaVencimiento?: string | null;

  // true si el vendedor es un dealer verificado (suscripción pagada + correo confirmado).
  esDealerVerificado?: boolean;
}