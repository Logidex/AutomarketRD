import { FaMoon, FaSun } from "react-icons/fa";
import { useTema } from "../context/TemaContext";

export default function BotonTema({ className = "" }: { className?: string }) {
  const { tema, alternarTema } = useTema();
  const oscuro = tema === "oscuro";

  return (
    <button
      type="button"
      onClick={alternarTema}
      aria-label={oscuro ? "Cambiar a tema claro" : "Cambiar a tema oscuro"}
      title={oscuro ? "Cambiar a tema claro" : "Cambiar a tema oscuro"}
      className={`inline-flex h-9 w-9 items-center justify-center rounded-full border border-line text-ink-2 transition-colors hover:border-blue-500 hover:text-blue-500 ${className}`}
    >
      {oscuro ? <FaSun /> : <FaMoon />}
    </button>
  );
}