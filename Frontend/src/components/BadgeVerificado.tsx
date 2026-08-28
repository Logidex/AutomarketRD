import { FaCheckCircle } from "react-icons/fa";

interface BadgeVerificadoProps {
  className?: string;
  /** Solo el icono (para tarjetas pequeñas donde el texto no cabe) */
  compacto?: boolean;
}

export default function BadgeVerificado({ className = "", compacto = false }: BadgeVerificadoProps) {
  return (
    <span
      title="Dealer verificado: suscripción pagada y correo confirmado"
      className={`inline-flex items-center gap-1.5 rounded-full bg-green-600 font-bold text-white shadow ${
        compacto ? "px-2 py-1 text-[11px]" : "px-3 py-1 text-xs"
      } ${className}`}
    >
      <FaCheckCircle className="h-3.5 w-3.5" />
      {!compacto && "Dealer Verificado"}
    </span>
  );
}
