interface SectionBackgroundProps {
  variant: "hero" | "gallery" | "search" | "recent" | "cta";
  className?: string;
  children?: React.ReactNode;
}

interface VariantConfig {
  base: string;
  gradient?: string;
  orbs: readonly string[];
  aurora?: boolean;
}

const VARIANT_CONFIG: Record<
  SectionBackgroundProps["variant"],
  VariantConfig
> = {
  hero: {
    base:
      "bg-[linear-gradient(135deg,var(--am-page)_0%,var(--am-surface)_55%,var(--am-brand-soft)_100%)]",
    orbs: [
      "absolute -left-32 top-0 h-72 w-72 rounded-full bg-brand/10 blur-3xl home-orbita-azul",
      "absolute -right-32 bottom-0 h-80 w-80 rounded-full bg-brand/10 blur-3xl home-orbita-cian",
    ],
  },

  gallery: {
    base:
      "bg-[linear-gradient(135deg,var(--am-page)_0%,var(--am-brand-soft)_45%,var(--am-surface)_100%)]",
    aurora: true,
    orbs: [
      "absolute -left-36 top-0 h-[420px] w-[420px] rounded-full bg-brand/20 blur-3xl home-orbita-azul",
      "absolute -right-36 bottom-20 h-[480px] w-[480px] rounded-full bg-brand/20 blur-3xl home-orbita-cian",
      "absolute left-[42%] top-1/2 h-72 w-72 -translate-y-1/2 rounded-full bg-brand/10 blur-3xl home-orbita-indigo",
    ],
  },

  search: {
    base: "bg-page dark:bg-slate-950",
    gradient:
      "bg-[radial-gradient(circle_at_12%_18%,var(--am-brand-soft),transparent_42%),radial-gradient(circle_at_85%_80%,rgba(6,182,212,0.10),transparent_40%),linear-gradient(160deg,var(--am-page)_0%,var(--am-surface)_100%)] dark:bg-[radial-gradient(circle_at_12%_18%,var(--am-brand)/50,transparent_34%),radial-gradient(circle_at_85%_80%,var(--am-brand-hover)/35,transparent_36%),linear-gradient(135deg,var(--am-page)_0%,var(--am-surface)_52%,var(--am-brand-soft)_100%)]",
    aurora: true,
    orbs: [
      "absolute -left-28 top-0 h-96 w-96 rounded-full bg-brand/20 blur-3xl home-orbita-azul",
      "absolute -right-28 bottom-0 h-96 w-96 rounded-full bg-brand-hover/15 blur-3xl home-orbita-cian",
    ],
  },

  recent: {
    base:
      "bg-[linear-gradient(135deg,var(--am-surface)_0%,var(--am-page)_55%,var(--am-brand-soft)_100%)]",
    aurora: true,
    orbs: [
      "absolute -right-28 top-12 h-80 w-80 rounded-full bg-brand/10 blur-3xl home-orbita-indigo",
      "absolute -left-24 bottom-0 h-72 w-72 rounded-full bg-brand-hover/8 blur-3xl home-orbita-cian",
    ],
  },

  cta: {
    base:
      "bg-[linear-gradient(135deg,var(--am-brand-soft)_0%,var(--am-page)_55%,var(--am-surface)_100%)]",
    aurora: true,
    orbs: [
      "absolute -right-20 top-0 h-80 w-80 rounded-full bg-brand/10 blur-3xl home-orbita-azul",
    ],
  },
};

export default function SectionBackground({
  variant,
  className = "",
  children,
}: SectionBackgroundProps) {
  const config = VARIANT_CONFIG[variant];

  return (
    <section className="relative isolate overflow-hidden">
      <div
        className={`pointer-events-none absolute inset-0 z-0 ${config.base} ${
          config.gradient ?? ""
        }`}
        aria-hidden="true"
      />

      {config.aurora && (
        <div
          className="home-aurora pointer-events-none absolute inset-0 z-0"
          aria-hidden="true"
        />
      )}

      {config.orbs.map((orb, index) => (
        <div
          key={index}
          className={`pointer-events-none z-0 ${orb}`}
          aria-hidden="true"
        />
      ))}

      <div className={`relative z-10 ${className}`}>
        {children}
      </div>
    </section>
  );
}