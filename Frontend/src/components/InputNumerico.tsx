import React, { useState } from "react";
import { formatearNumeroInput, parsearNumeroInput } from "../utils/formato";

interface InputNumericoProps
  extends Omit<React.InputHTMLAttributes<HTMLInputElement>, "value" | "onChange"> {
  nombre: string;
  value: number | string;
  minimo?: number;
  onCambio: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

/**
 * Input numérico que formatea en vivo el valor: separa los miles con comas y
 * permite decimales con punto. Emite al padre el valor numérico "crudo" (sin
 * separadores) mediante un evento sintético, para no cambiar la lógica del form.
 */
export default function InputNumerico({
  nombre,
  value,
  minimo,
  onCambio,
  ...rest
}: InputNumericoProps) {
  const [texto, setTexto] = useState(() =>
    parsearNumeroInput(String(value ?? "")) === 0
      ? ""
      : formatearNumeroInput(String(value ?? "")),
  );

  // Sincroniza el texto cuando el valor externo cambia (p.ej. al cargar un
  // anuncio en edición), sin pisar lo que el usuario está escribiendo.
  const valorActual = parsearNumeroInput(String(value ?? ""));
  if (parsearNumeroInput(texto) !== valorActual) {
    setTexto(valorActual === 0 ? "" : formatearNumeroInput(String(valorActual)));
  }

  const manejarCambio = (e: React.ChangeEvent<HTMLInputElement>) => {
    const nuevoTexto = formatearNumeroInput(e.target.value);
    const numero = parsearNumeroInput(nuevoTexto);
    if (minimo !== undefined && numero < minimo) return;

    setTexto(nuevoTexto);

    const eventoSimulado = {
      target: { name: nombre, value: numero === 0 ? "" : numero.toString() },
    } as unknown as React.ChangeEvent<HTMLInputElement>;
    onCambio(eventoSimulado);
  };

  return (
    <input
      {...rest}
      type="text"
      inputMode="decimal"
      name={nombre}
      value={texto}
      onChange={manejarCambio}
    />
  );
}