import { useRef, useEffect } from "react";

interface Blob {
  x: number;
  y: number;
  r: number;
  vx: number;
  vy: number;
  hue: number;
}

interface Props {
  className?: string;
}

export default function MeshGradientCanvas({ className = "" }: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    let animId = 0;
    let w = 0;
    let h = 0;

    const blobs: Blob[] = [
      { x: 0.2, y: 0.3, r: 0.32, vx: 0.0003, vy: 0.0002, hue: 220 },
      { x: 0.75, y: 0.65, r: 0.28, vx: -0.0002, vy: 0.0003, hue: 190 },
      { x: 0.5, y: 0.5, r: 0.25, vx: 0.00015, vy: -0.00025, hue: 260 },
    ];

    const resize = () => {
      const dpr = Math.min(window.devicePixelRatio || 1, 2);
      const rect = canvas.getBoundingClientRect();
      w = rect.width;
      h = rect.height;
      canvas.width = w * dpr;
      canvas.height = h * dpr;
      ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    };

    const draw = () => {
      ctx.clearRect(0, 0, w, h);

      for (const b of blobs) {
        b.x += b.vx;
        b.y += b.vy;
        if (b.x < -0.1 || b.x > 1.1) b.vx *= -1;
        if (b.y < -0.1 || b.y > 1.1) b.vy *= -1;

        const grad = ctx.createRadialGradient(
          b.x * w,
          b.y * h,
          0,
          b.x * w,
          b.y * h,
          b.r * Math.max(w, h),
        );
        grad.addColorStop(0, `hsla(${b.hue}, 70%, 60%, 0.18)`);
        grad.addColorStop(0.6, `hsla(${b.hue}, 60%, 55%, 0.06)`);
        grad.addColorStop(1, "transparent");

        ctx.fillStyle = grad;
        ctx.fillRect(0, 0, w, h);
      }

      animId = requestAnimationFrame(draw);
    };

    resize();
    draw();
    window.addEventListener("resize", resize);

    return () => {
      cancelAnimationFrame(animId);
      window.removeEventListener("resize", resize);
    };
  }, []);

  return (
    <canvas
      ref={canvasRef}
      className={`pointer-events-none absolute inset-0 h-full w-full ${className}`}
      aria-hidden="true"
    />
  );
}
