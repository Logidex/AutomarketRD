export type UbicacionAdSlot =
  | 'HomepageLateral'
  | 'HomepageBuscador'
  | 'HomepageFooter'
  | 'VehiculosLateral'
  | 'VehiculosGrid'
  | 'VehiculosFooter'
  | 'DetalleLateral'
  | 'DetalleFooter'
  | 'AgenciasLateral'
  | 'AgenciasFooter';

export type EstadoAdSlot = 'Activo' | 'Vencido' | 'Rechazado';

export interface AdSlotPrecio {
  id: number;
  duracionDias: number;
  precio: number;
  descuentoProElitePorcentaje: number;
  activo: boolean;
}

export interface AdSlot {
  id: number;
  titulo: string;
  ubicacion: UbicacionAdSlot;
  anchoPx: number;
  altoPx: number;
  intervaloRotacionSeg: number;
  maxAnunciosSimultaneos: number;
  activo: boolean;
  orden: number;
  precios: AdSlotPrecio[];
  anunciosActivosCount: number;
}

export interface AdSlotAnuncioPublico {
  id: number;
  imagenUrl: string;
  enlace: string | null;
  titulo: string | null;
  nombreDealer: string;
  prioridad: number;
  ubicacion: UbicacionAdSlot;
  fechaInicioUtc: string;
  fechaFinUtc: string;
  estado: EstadoAdSlot;
  impresiones: number;
  clicks: number;
}

export interface AdSlotPublico {
  id: number;
  titulo: string;
  ubicacion: UbicacionAdSlot;
  anchoPx: number;
  altoPx: number;
  intervaloRotacionSeg: number;
  precios: AdSlotPrecio[];
  anuncios: AdSlotAnuncioPublico[];
}

export interface CrearAdSlotAnuncioDto {
  adSlotId: number;
  duracionDias: number;
  imagenUrl: string;
  enlace?: string;
  titulo?: string;
}

export interface AdSlotAnuncioAdmin {
  id: number;
  adSlotId: number;
  tituloSlot: string;
  ubicacion: UbicacionAdSlot;
  perfilDealerId: number;
  nombreDealer: string;
  imagenUrl: string;
  enlace: string | null;
  titulo: string | null;
  fechaInicioUtc: string;
  fechaFinUtc: string;
  estado: EstadoAdSlot;
  montoPagado: number;
  impresiones: number;
  clicks: number;
  fechaCreacionUtc: string;
}

export interface AdSlotAnuncioStats {
  id: number;
  ubicacion: string;
  tituloSlot: string;
  impresiones: number;
  clicks: number;
  ctr: number;
  fechaFinUtc: string;
  estaVigente: boolean;
}

export interface AdSlotStats {
  totalImpresiones: number;
  totalClicks: number;
  ctrPromedio: number;
  anunciosActivos: number;
  topAnuncios: AdSlotAnuncioStats[];
}

export interface CrearAdSlotAdminDto {
  titulo: string;
  ubicacion: UbicacionAdSlot;
  anchoPx: number;
  altoPx: number;
  intervaloRotacionSeg?: number;
  maxAnunciosSimultaneos?: number;
  activo?: boolean;
  orden?: number;
  precios: CrearAdSlotPrecioDto[];
}

export interface CrearAdSlotPrecioDto {
  duracionDias: number;
  precio: number;
  descuentoProElitePorcentaje: number;
}

export interface ActualizarAdSlotAdminDto {
  titulo?: string;
  ubicacion?: UbicacionAdSlot;
  anchoPx?: number;
  altoPx?: number;
  intervaloRotacionSeg?: number;
  maxAnunciosSimultaneos?: number;
  activo?: boolean;
  orden?: number;
}
