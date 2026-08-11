import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from "react";

const MAX_VEHICULOS = 4;
const CLAVE_LOCAL_STORAGE = "automarket.comparador";

interface ComparadorContextValue {
  seleccionados: number[];
  esSeleccionado: (id: number) => boolean;
  toggle: (id: number) => void;
  reemplazar: (ids: number[]) => void;
  limpiar: () => void;
  maxVehiculos: number;
}

const ComparadorContext = createContext<ComparadorContextValue | null>(null);

function leerSeleccionGuardada(): number[] {
  try {
    const crudo = localStorage.getItem(CLAVE_LOCAL_STORAGE);
    if (!crudo) return [];
    const ids = JSON.parse(crudo);
    if (!Array.isArray(ids)) return [];
    return ids
      .map(Number)
      .filter(Number.isFinite)
      .slice(0, MAX_VEHICULOS);
  } catch {
    return [];
  }
}

export function ComparadorProvider({ children }: { children: ReactNode }) {
  const [seleccionados, setSeleccionados] = useState<number[]>(
    leerSeleccionGuardada,
  );

  useEffect(() => {
    try {
      localStorage.setItem(CLAVE_LOCAL_STORAGE, JSON.stringify(seleccionados));
    } catch {
      // Si el almacenamiento falla (modo incógnito, cuota llena) la app sigue funcionando.
    }
  }, [seleccionados]);

  const esSeleccionado = useCallback(
    (id: number) => seleccionados.includes(id),
    [seleccionados],
  );

  const toggle = useCallback((id: number) => {
    setSeleccionados((prev) => {
      if (prev.includes(id)) return prev.filter((x) => x !== id);
      if (prev.length >= MAX_VEHICULOS) return prev;
      return [...prev, id];
    });
  }, []);

  const reemplazar = useCallback((ids: number[]) => {
    setSeleccionados(
      Array.from(new Set(ids))
        .map(Number)
        .filter(Number.isFinite)
        .slice(0, MAX_VEHICULOS),
    );
  }, []);

  const limpiar = useCallback(() => setSeleccionados([]), []);

  return (
    <ComparadorContext.Provider
      value={{ seleccionados, esSeleccionado, toggle, reemplazar, limpiar, maxVehiculos: MAX_VEHICULOS }}
    >
      {children}
    </ComparadorContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useComparador(): ComparadorContextValue {
  const contexto = useContext(ComparadorContext);
  if (contexto == null) {
    throw new Error("useComparador debe usarse dentro de <ComparadorProvider>.");
  }
  return contexto;
}
