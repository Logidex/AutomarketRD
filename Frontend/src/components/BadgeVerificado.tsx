import { FaCheckCircle } from "react-icons/fa";

export default function BadgeVerificado({ className = "" }: { className?: string }) {
  return (
    <span
      title="Dealer verificado: suscripción pagada y correo confirmado"
      className={`inline-flex items-center gap-1.5 rounded-full bg-green-600 px-3 py-1 text-xs font-bold text-white shadow ${className}`}
    >
      <FaCheckCircle className="h-3.5 w-3.5" />
      Dealer Verificado
    </span>
  );
}
