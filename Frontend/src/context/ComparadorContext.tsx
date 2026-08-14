import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";

const MAX_VEHICULOS = 4;
const CLAVE_LOCAL_STORAGE = "automarket.comparador:v1";

interface ComparadorContextValue {
  seleccionados: number[];
  esSeleccionado: (id: number) => boolean;
  toggle: (id: number) => void;
  reemplazar: (ids: number[]) => void;
  limpiar: () => void;
  maxVehiculos: number;
}

const ComparadorContext = createContext<ComparadorContextValue | null>(null);

function normalizarIds(valores: unknown[]): number[] {
  const resultado: number[] = [];
  for (const valor of valores) {
    const numero = Number(valor);
    if (Number.isFinite(numero) && resultado.length < MAX_VEHICULOS) {
      resultado.push(numero);
    }
  }
  return resultado;
}

function leerSeleccionGuardada(): number[] {
  try {
    const crudo = localStorage.getItem(CLAVE_LOCAL_STORAGE);
    if (!crudo) return [];
    const ids = JSON.parse(crudo);
    if (!Array.isArray(ids)) return [];
    return normalizarIds(ids);
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
    setSeleccionados(normalizarIds(Array.from(new Set(ids))));
  }, []);

  const limpiar = useCallback(() => setSeleccionados([]), []);

  const value = useMemo(
    () => ({
      seleccionados,
      esSeleccionado,
      toggle,
      reemplazar,
      limpiar,
      maxVehiculos: MAX_VEHICULOS,
    }),
    [seleccionados, esSeleccionado, toggle, reemplazar, limpiar],
  );

  return (
    <ComparadorContext.Provider value={value}>
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
