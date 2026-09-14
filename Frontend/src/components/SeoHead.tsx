import { useEffect } from "react";

interface SeoProps {
  titulo: string;
  descripcion: string;
  imagen?: string;
  url?: string;
  tipo?: "website" | "article";
  jsonLd?: Record<string, unknown>;
}

export default function SeoHead({
  titulo,
  descripcion,
  imagen,
  url,
  tipo = "website",
  jsonLd,
}: SeoProps) {
  useEffect(() => {
    document.title = titulo ? `${titulo} | AutoMarket RD` : "AutoMarket RD";

    const setMeta = (name: string, content: string) => {
      let el = document.querySelector(`meta[property="${name}"], meta[name="${name}"]`) as HTMLMetaElement | null;
      if (!el) {
        el = document.createElement("meta");
        if (name.startsWith("og:") || name.startsWith("article:")) {
          el.setAttribute("property", name);
        } else {
          el.setAttribute("name", name);
        }
        document.head.appendChild(el);
      }
      el.setAttribute("content", content);
    };

    setMeta("description", descripcion);
    setMeta("og:title", titulo);
    setMeta("og:description", descripcion);
    setMeta("og:type", tipo);
    if (imagen) setMeta("og:image", imagen);
    if (url) setMeta("og:url", url);

    setMeta("twitter:card", imagen ? "summary_large_image" : "summary");
    setMeta("twitter:title", titulo);
    setMeta("twitter:description", descripcion);
    if (imagen) setMeta("twitter:image", imagen);

    // JSON-LD
    const existingScript = document.getElementById("seo-jsonld");
    if (existingScript) existingScript.remove();

    if (jsonLd) {
      const script = document.createElement("script");
      script.id = "seo-jsonld";
      script.type = "application/ld+json";
      script.textContent = JSON.stringify(jsonLd);
      document.head.appendChild(script);
    }

    return () => {
      const script = document.getElementById("seo-jsonld");
      if (script) script.remove();
    };
  }, [titulo, descripcion, imagen, url, tipo, jsonLd]);

  return null;
}
