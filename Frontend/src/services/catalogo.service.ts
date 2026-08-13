import api from "./api";
import type {
  AnuncioListado,
  PagedResult,
} from "../types/anuncio.types";

export interface AnuncioBusquedaDto {
  marca?: string;
  modelo?: string;
  tipoVehiculo?: string;
  transmision?: string;
  combustible?: string;
  ubicacion?: string;
  condicion?: string;
  enOferta?: boolean;
  precioMinimo?: number;
  precioMaximo?: number;
  moneda?: string;
  anioDesde?: number;
  anioHasta?: number;
  kilometrajeMaximo?: number;
  vendedorId?: number;
  paginaActual: number;
  cantidadAnuncios: number;
}

export const catalogoService = {
  async buscar(dto: AnuncioBusquedaDto): Promise<PagedResult<AnuncioListado>> {
    const params = new URLSearchParams();

    if (dto.marca?.trim()) params.set("Marca", dto.marca.trim());
    if (dto.modelo?.trim()) params.set("Modelo", dto.modelo.trim());
    if (dto.tipoVehiculo?.trim())
      params.set("TipoVehiculo", dto.tipoVehiculo.trim());
    if (dto.transmision?.trim())
      params.set("Transmision", dto.transmision.trim());
    if (dto.combustible?.trim())
      params.set("Combustible", dto.combustible.trim());
    if (dto.ubicacion?.trim())
      params.set("Ubicacion", dto.ubicacion.trim());
    if (dto.condicion?.trim())
      params.set("Condicion", dto.condicion.trim());
    if (dto.enOferta != null)
      params.set("EnOferta", String(dto.enOferta));
    if (dto.precioMinimo != null)
      params.set("PrecioMinimo", String(dto.precioMinimo));
    if (dto.precioMaximo != null)
      params.set("PrecioMaximo", String(dto.precioMaximo));
    if (dto.moneda?.trim()) params.set("Moneda", dto.moneda.trim());
    if (dto.anioDesde != null) params.set("AnioDesde", String(dto.anioDesde));
    if (dto.anioHasta != null) params.set("AnioHasta", String(dto.anioHasta));
    if (dto.kilometrajeMaximo != null)
      params.set("KilometrajeMaximo", String(dto.kilometrajeMaximo));
    if (dto.vendedorId != null)
      params.set("VendedorId", String(dto.vendedorId));

    params.set("PaginaActual", String(dto.paginaActual));
    params.set("CantidadAnuncios", String(dto.cantidadAnuncios));

    const response = await api.get<PagedResult<AnuncioListado>>(
      `/api/anuncios/buscar?${params.toString()}`,
    );

    return response.data;
  },
};