import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from "react";

export type EstadoConsentimiento = "pendiente" | "aceptado" | "rechazado";

interface ConsentContextValue {
  estado: EstadoConsentimiento;
  aceptar: () => void;
  rechazar: () => void;
}

const STORAGE_KEY = "automarket-consentimiento-anuncios";

const ConsentContext = createContext<ConsentContextValue | undefined>(
  undefined,
);

function leerEstadoGuardado(): EstadoConsentimiento {
  try {
    const guardado = localStorage.getItem(STORAGE_KEY);
    if (guardado === "aceptado" || guardado === "rechazado") return guardado;
  } catch {
    // localStorage no disponible
  }
  return "pendiente";
}

export function ConsentProvider({ children }: { children: ReactNode }) {
  const [estado, setEstado] = useState<EstadoConsentimiento>(
    leerEstadoGuardado,
  );

  useEffect(() => {
    try {
      if (estado === "aceptado" || estado === "rechazado") {
        localStorage.setItem(STORAGE_KEY, estado);
      } else {
        localStorage.removeItem(STORAGE_KEY);
      }
    } catch {
      // localStorage no disponible
    }
  }, [estado]);

  const aceptar = useCallback(() => setEstado("aceptado"), []);
  const rechazar = useCallback(() => setEstado("rechazado"), []);

  return (
    <ConsentContext.Provider value={{ estado, aceptar, rechazar }}>
      {children}
    </ConsentContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useConsent(): ConsentContextValue {
  const ctx = useContext(ConsentContext);
  if (!ctx) throw new Error("useConsent debe usarse dentro de ConsentProvider");
  return ctx;
}