export interface LeadAnuncioResumen {
  id: number;
  nombreAnuncio: string;
  marca: string;
  modelo: string;
  anio: number;
}

export interface LeadDealer {
  id: number;
  anuncioId: number;
  anuncio?: LeadAnuncioResumen;
  nombreContacto: string;
  emailContacto: string;
  telefonoContacto: string;
  mensaje: string;
  canal: string;
  fechaCreacionUtc: string;
  leido: boolean;
}
