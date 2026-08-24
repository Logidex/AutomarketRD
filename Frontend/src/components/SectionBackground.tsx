interface SectionBackgroundProps {
  variant: "hero" | "gallery" | "search" | "recent" | "cta";
  className?: string;
  children?: React.ReactNode;
}

interface VariantConfig {
  base: string;
  gradient?: string;
  orbs: readonly string[];
}

const VARIANT_CONFIG: Record<SectionBackgroundProps["variant"], VariantConfig> = {
  hero: {
    base: "bg-[linear-gradient(135deg,var(--am-page)_0%,var(--am-surface)_55%,var(--am-brand-soft)_100%)]",
    orbs: [
      "absolute -left-32 top-0 h-72 w-72 rounded-full bg-brand/10 blur-3xl home-orbita-azul",
      "absolute -right-32 bottom-0 h-80 w-80 rounded-full bg-brand/10 blur-3xl home-orbita-cian",
    ],
  },
  gallery: {
    base: "bg-[linear-gradient(135deg,var(--am-page)_0%,var(--am-brand-soft)_45%,var(--am-surface)_100%)]",
    orbs: [
      "absolute -left-36 top-0 h-[420px] w-[420px] rounded-full bg-brand/20 blur-3xl home-orbita-azul",
      "absolute -right-36 bottom-20 h-[480px] w-[480px] rounded-full bg-brand/20 blur-3xl home-orbita-cian",
      "absolute left-[42%] top-1/2 h-72 w-72 -translate-y-1/2 rounded-full bg-brand/10 blur-3xl home-orbita-indigo",
    ],
  },
  search: {
    base: "bg-slate-950 dark:bg-slate-950",
    gradient: "bg-[radial-gradient(circle_at_12%_18%,var(--am-brand)/40,transparent_34%),radial-gradient(circle_at_85%_80%,var(--am-brand-hover)/30,transparent_36%),linear-gradient(135deg,#0f172a_0%,#172554_52%,#082f49_100%)] dark:bg-[radial-gradient(circle_at_12%_18%,var(--am-brand)/50,transparent_34%),radial-gradient(circle_at_85%_80%,var(--am-brand-hover)/35,transparent_36%),linear-gradient(135deg,var(--am-page)_0%,var(--am-surface)_52%,var(--am-brand-soft)_100%)]",
    orbs: [
      "absolute -left-28 top-0 h-96 w-96 rounded-full bg-brand/30 blur-3xl home-orbita-azul",
      "absolute -right-28 bottom-0 h-96 w-96 rounded-full bg-brand-hover/20 blur-3xl home-orbita-cian",
    ],
  },
  recent: {
    base: "bg-[linear-gradient(135deg,var(--am-surface)_0%,var(--am-page)_55%,var(--am-brand-soft)_100%)]",
    orbs: [
      "absolute -right-28 top-12 h-80 w-80 rounded-full bg-brand/10 blur-3xl home-orbita-indigo",
      "absolute -left-24 bottom-0 h-72 w-72 rounded-full bg-brand-hover/8 blur-3xl home-orbita-cian",
    ],
  },
  cta: {
    base: "bg-[linear-gradient(135deg,var(--am-brand-soft)_0%,var(--am-page)_55%,var(--am-surface)_100%)]",
    orbs: [
      "absolute -right-20 top-0 h-80 w-80 rounded-full bg-brand/10 blur-3xl home-orbita-azul",
    ],
  },
};

export default function SectionBackground({ variant, className = "", children }: SectionBackgroundProps) {
  const config = VARIANT_CONFIG[variant];

  return (
    <>
      <div className={`pointer-events-none absolute inset-0 ${config.base} ${config.gradient || ""}`} />
      {config.orbs.map((orb, i) => (
        <div key={i} className={`pointer-events-none ${orb}`} />
      ))}
      <div className={`relative ${className}`}>
        {children}
      </div>
    </>
  );
}