declare global {
  interface Window {
    adsbygoogle?: unknown[];
  }
}

let cargaPromise: Promise<boolean> | null = null;

/** Carga el script de AdSense una sola vez (singleton) y devuelve si se pudo
 *  cargar. Devuelve `false` (sin lanzar error) si el CDN está bloqueado
 *  (ad-blocker, proxy) para no romper la página. */
export function cargarScriptAdsense(cliente: string): Promise<boolean> {
  if (!cargaPromise) {
    cargaPromise = new Promise<boolean>((resolve) => {
      try {
        const yaCargado = Array.from(
          document.querySelectorAll("script"),
        ).some((s) => s.src.includes("pagead2.googlesyndication.com"));
        if (yaCargado) {
          resolve(true);
          return;
        }

        const script = document.createElement("script");
        script.async = true;
        script.src = `https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=${cliente}`;
        script.crossOrigin = "anonymous";
        script.onload = () => resolve(true);
        script.onerror = () => resolve(false);
        document.head.appendChild(script);
      } catch {
        resolve(false);
      }
    });
  }
  return cargaPromise;
}