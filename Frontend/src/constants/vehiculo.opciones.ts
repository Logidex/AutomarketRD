export interface OpcionVehiculo {
  valor: string;
  etiqueta: string;
}

export const TIPOS_VEHICULO: OpcionVehiculo[] = [
  // Autos
  { valor: "Sedan", etiqueta: "Sedán" },
  { valor: "Jeepeta", etiqueta: "Jeepeta (SUV)" },
  { valor: "Camioneta", etiqueta: "Camioneta (Pick-up)" },
  { valor: "Deportivo", etiqueta: "Deportivo" },
  { valor: "SuperDeportivo", etiqueta: "Súper Deportivo" },
  { valor: "Hypercar", etiqueta: "Hypercar" },
  { valor: "Coupe", etiqueta: "Coupé" },
  { valor: "Convertible", etiqueta: "Convertible" },
  { valor: "Minivan", etiqueta: "Minivan" },
  { valor: "Hatchback", etiqueta: "Hatchback" },
  // Motos
  { valor: "Motor", etiqueta: "Motocicleta" },
  // Vehículos pesados
  { valor: "Camion", etiqueta: "Camión" },
  // Otros
  { valor: "Otro", etiqueta: "Otro" },
];

export const TRANSMISIONES: OpcionVehiculo[] = [
  { valor: "Automatica", etiqueta: "Automática" },
  { valor: "Manual", etiqueta: "Manual" },
  { valor: "Secuencial", etiqueta: "Secuencial" },
  { valor: "CVT", etiqueta: "CVT" },
  { valor: "DobleEmbrague", etiqueta: "Doble Embrague (DCT)" },
  { valor: "Otra", etiqueta: "Otra" },
];

export const COMBUSTIBLES: OpcionVehiculo[] = [
  { valor: "Gasolina", etiqueta: "Gasolina" },
  { valor: "Diesel", etiqueta: "Diésel" },
  { valor: "Gas", etiqueta: "GLP / Gas Natural" },
  { valor: "Electrico", etiqueta: "Eléctrico" },
  { valor: "Hibrido", etiqueta: "Híbrido" },
];

export function etiquetaDe(valor: string, opciones: OpcionVehiculo[]): string {
  return opciones.find((o) => o.valor === valor)?.etiqueta ?? valor;
}