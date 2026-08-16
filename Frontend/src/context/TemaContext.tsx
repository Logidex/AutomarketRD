import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from "react";

type Tema = "claro" | "oscuro";

interface TemaContextValue {
  tema: Tema;
  alternarTema: () => void;
}

const STORAGE_KEY = "automarket-tema";

const TemaContext = createContext<TemaContextValue | undefined>(undefined);

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [tema, setTema] = useState<Tema>(() => {
    try {
      const guardado = localStorage.getItem(STORAGE_KEY);
      if (guardado === "claro" || guardado === "oscuro") return guardado;
    } catch {
      // localStorage no disponible
    }
    return "oscuro";
  });

  useEffect(() => {
    document.documentElement.classList.toggle("dark", tema === "oscuro");
    try {
      localStorage.setItem(STORAGE_KEY, tema);
    } catch {
      // localStorage no disponible
    }
  }, [tema]);

  const alternarTema = useCallback(() => {
    setTema((t) => (t === "oscuro" ? "claro" : "oscuro"));
  }, []);

  return (
    <TemaContext.Provider value={{ tema, alternarTema }}>
      {children}
    </TemaContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useTema(): TemaContextValue {
  const ctx = useContext(TemaContext);
  if (!ctx) throw new Error("useTema debe usarse dentro de ThemeProvider");
  return ctx;
}