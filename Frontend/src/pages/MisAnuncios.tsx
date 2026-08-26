import { useEffect, useState } from "react";
import { motion } from "motion/react";
import Swal from "sweetalert2";
import AnuncioCard from "../components/AnuncioCard";
import Spinner from "../components/Spinner";
import { dashboardService, type DashboardResumen } from "../services/dashboard.service";
import type { AnuncioListado } from "../types/anuncio.types";
import { getUserIdFromToken } from "../utils/jwt.util";
import { Link, useNavigate } from "react-router-dom";
import { FaCar, FaPlusCircle } from "react-icons/fa";
import {
  useMisAnuncios,
  usePublicarAnuncio,
  useCambiarEstadoAnuncio,
  useEliminarAnuncio,
  useDestacarAnuncio,
  useQuitarDestacadoAnuncio,
} from "../hooks/useAnuncios";

export default function MisAnuncios() {
  const [resumen, setResumen] = useState<DashboardResumen | null>(null);
  const usuarioId = getUserIdFromToken();
  const navigate = useNavigate();

  const {
    data: paged,
    isLoading,
    invalidate,
  } = useMisAnuncios(usuarioId ?? 0, usuarioId !== null);

  const publicar = usePublicarAnuncio();
  const cambiarEstado = useCambiarEstadoAnuncio();
  const eliminar = useEliminarAnuncio();
  const destacar = useDestacarAnuncio();
  const quitarDestacado = useQuitarDestacadoAnuncio();

  const anuncios = paged?.items ?? ([] as AnuncioListado[]);

  useEffect(() => {
    if (usuarioId === null) return;

    let activo = true;
    const cargarResumen = async () => {
      try {
        const resumen = await dashboardService.obtenerResumen();
        if (activo) setResumen(resumen);
      } catch {
        // El banner de uso del plan es opcional; no bloquea la lista.
      }
    };

    cargarResumen();
    return () => {
      activo = false;
    };
  }, [usuarioId]);

  if (usuarioId === null) {
    return (
      <div className="mx-auto max-w-6xl p-6">
        <p className="text-ink-3">No se pudo identificar al usuario.</p>
      </div>
    );
  }

  if (isLoading) {
    return <Spinner />;
  }

  const recargarResumen = async () => {
    try {
      setResumen(await dashboardService.obtenerResumen());
    } catch {
      // Banner opcional: si falla, se conserva el valor anterior.
    }
  };

  const handlePublicar = async (id: number) => {
    try {
      await publicar.mutateAsync(id);
      await invalidate();
      await recargarResumen();

      Swal.fire({
        title: "Publicado",
        text: "El anuncio fue publicado correctamente.",
        icon: "success",
        confirmButtonColor: "#2563eb",
      });
    } catch (error) {
      console.error(error);
      Swal.fire({
        title: "Error",
        text: error instanceof Error ? error.message : "No se pudo publicar el anuncio.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    }
  };

  const handleCambiarEstado = async (id: number, nuevoEstado: string) => {
    try {
      const result = await Swal.fire({
        title: "¿Cambiar estado?",
        text: `Vas a cambiar el anuncio a "${nuevoEstado}".`,
        icon: "question",
        showCancelButton: true,
        confirmButtonText: "Sí, cambiar",
        cancelButtonText: "Cancelar",
        confirmButtonColor: "#2563eb",
        cancelButtonColor: "#6b7280",
      });

      if (!result.isConfirmed) return;

      await cambiarEstado.mutateAsync({ id, estado: nuevoEstado });
      await invalidate();
      await recargarResumen();

      Swal.fire({
        title: "Actualizado",
        text: "El estado del anuncio se actualizó correctamente.",
        icon: "success",
        confirmButtonColor: "#2563eb",
      });
    } catch (error) {
      console.error(error);
      Swal.fire({
        title: "Error",
        text: error instanceof Error ? error.message : "No se pudo cambiar el estado del anuncio.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    }
  };

  const handleEliminar = async (id: number) => {
    const result = await Swal.fire({
      title: "¿Eliminar anuncio?",
      text: "Esta acción eliminará el anuncio y sus fotos. No se puede deshacer.",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Sí, eliminar",
      cancelButtonText: "Cancelar",
      confirmButtonColor: "#ef4444",
      cancelButtonColor: "#6b7280",
    });

    if (!result.isConfirmed) return;

    try {
      await eliminar.mutateAsync(id);
      await invalidate();
      await recargarResumen();

      Swal.fire({
        title: "Eliminado",
        text: "El anuncio fue eliminado correctamente.",
        icon: "success",
        confirmButtonColor: "#2563eb",
      });
    } catch (error) {
      console.error(error);
      Swal.fire({
        title: "Error",
        text: error instanceof Error ? error.message : "No se pudo eliminar el anuncio.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    }
  };

  const handleDestacar = async (id: number) => {
    if (resumen && (resumen.cuotaDestacados ?? 0) <= 0) {
      const result = await Swal.fire({
        title: "Destacar anuncios",
        text: "Tu plan actual no incluye anuncios destacados. Mejora tu suscripción para destacar tus vehículos en la página principal.",
        icon: "info",
        showCancelButton: true,
        confirmButtonText: "Ver planes",
        cancelButtonText: "Cancelar",
        confirmButtonColor: "#f59e0b",
        cancelButtonColor: "#6b7280",
      });

      if (result.isConfirmed) navigate("/dashboard/suscripcion");
      return;
    }

    const result = await Swal.fire({
      title: "¿Destacar anuncio?",
      text: "El anuncio aparecerá en la sección de destacados de la página principal.",
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "Sí, destacar",
      cancelButtonText: "Cancelar",
      confirmButtonColor: "#f59e0b",
      cancelButtonColor: "#6b7280",
    });

    if (!result.isConfirmed) return;

    try {
      await destacar.mutateAsync(id);
      await invalidate();
      await recargarResumen();

      Swal.fire({
        title: "Destacado",
        text: "El anuncio ahora es destacado.",
        icon: "success",
        confirmButtonColor: "#2563eb",
      });
    } catch (error) {
      console.error(error);
      Swal.fire({
        title: "No se pudo destacar",
        text: error instanceof Error ? error.message : "Verifica tu plan de suscripción.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    }
  };

  const handleQuitarDestacado = async (id: number) => {
    const result = await Swal.fire({
      title: "¿Quitar destacado?",
      text: "El anuncio dejará de aparecer en la sección de destacados.",
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "Sí, quitar",
      cancelButtonText: "Cancelar",
      confirmButtonColor: "#ef4444",
      cancelButtonColor: "#6b7280",
    });

    if (!result.isConfirmed) return;

    try {
      await quitarDestacado.mutateAsync(id);
      await invalidate();
      await recargarResumen();

      Swal.fire({
        title: "Actualizado",
        text: "El anuncio ya no es destacado.",
        icon: "success",
        confirmButtonColor: "#2563eb",
      });
    } catch (error) {
      console.error(error);
      Swal.fire({
        title: "Error",
        text: error instanceof Error ? error.message : "No se pudo quitar el destacado.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    }
  };

  const activosEnVitrina = resumen?.anunciosActivos ?? 0;
  const limitePlan = resumen?.limiteAnuncios ?? 0;
  const disponibles = Math.max(0, limitePlan - activosEnVitrina);

  const cuotaDestacados = resumen?.cuotaDestacados ?? 0;
  const destacadosActivos = resumen?.destacadosActivos ?? 0;
  const destacadosDisponibles = Math.max(0, cuotaDestacados - destacadosActivos);

  return (
    <div className="mx-auto max-w-6xl p-6">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-ink">Mis Anuncios</h1>

        {anuncios.length > 0 && (
          <Link
            to="/dashboard/publicar"
            className="flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 font-semibold text-white transition-colors hover:bg-blue-700"
          >
            <FaPlusCircle />
            Crear anuncio
          </Link>
        )}
      </div>

      {resumen && limitePlan > 0 && (
        <div
          className={`mb-6 flex flex-wrap items-center justify-between gap-4 rounded-xl border p-4 ${
            disponibles === 0 ? "border-red-200 bg-red-50" : "border-line bg-surface"
          }`}
        >
          <div className="min-w-0">
            <p
              className={`text-sm font-semibold ${
                disponibles === 0 ? "text-red-700" : "text-ink"
              }`}
            >
              Uso del plan {resumen.planActual}
            </p>
            <p
              className={`text-sm ${
                disponibles === 0 ? "text-red-600" : "text-ink-3"
              }`}
            >
              {disponibles === 0
                ? `Alcanzaste el límite de tu plan (${limitePlan} anuncios). Cambia de plan para seguir publicando.`
                : `Estás usando ${activosEnVitrina} de ${limitePlan} anuncios de tu plan (${disponibles} disponibles).`}
            </p>
          </div>

          <div className="flex items-center gap-3">
            <span className="text-xs font-semibold text-ink-3">
              {activosEnVitrina}/{limitePlan}
            </span>
            <div className="h-2 w-40 overflow-hidden rounded-full bg-surface-2">
              <div
                className={`h-full rounded-full ${
                  disponibles === 0
                    ? "bg-red-500"
                    : disponibles === 1
                      ? "bg-amber-500"
                      : "bg-blue-500"
                }`}
                style={{
                  width: `${Math.min(100, (activosEnVitrina / limitePlan) * 100)}%`,
                }}
              />
            </div>
          </div>
        </div>
      )}

      {resumen && cuotaDestacados > 0 && (
        <div className={`mb-6 flex flex-wrap items-center justify-between gap-4 rounded-xl border p-4 ${
          destacadosDisponibles === 0 ? "border-amber-200 bg-amber-50" : "border-transparent bg-surface"
        }`}>
          <div className="min-w-0">
            <p className="text-sm font-semibold text-ink">
              Destacados del plan
            </p>
            <p className={`text-sm ${destacadosDisponibles === 0 ? "text-amber-700" : "text-ink-3"}`}>
              {destacadosDisponibles === 0
                ? "Llegaste al límite de anuncios destacados de tu plan. Quita uno para destacar otro."
                : `Estás usando ${destacadosActivos} de ${cuotaDestacados} anuncios destacados de tu plan (${destacadosDisponibles} disponibles).`}
            </p>
          </div>

          <div className="flex items-center gap-3">
            <span className="text-xs font-semibold text-ink-3">
              {destacadosActivos}/{cuotaDestacados}
            </span>
            <div className="h-2 w-40 overflow-hidden rounded-full bg-surface-2">
              <div
                className={`h-full rounded-full ${destacadosDisponibles === 0 ? "bg-amber-500" : "bg-yellow-500"}`}
                style={{
                  width: `${Math.min(100, (destacadosActivos / cuotaDestacados) * 100)}%`,
                }}
              />
            </div>
          </div>
        </div>
      )}

      {anuncios.length === 0 ? (
        <motion.div
          className="flex min-h-[420px] flex-col items-center justify-center rounded-2xl border border-line bg-surface px-6 text-center shadow-sm"
          initial={{ scale: 0.92, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ duration: 0.4, type: "spring", stiffness: 180 }}
        >
          <div className="mb-5 flex h-20 w-20 items-center justify-center rounded-full bg-blue-50">
            <FaCar className="text-4xl text-blue-600" />
          </div>

          <h2 className="mb-2 text-2xl font-bold text-ink">
            Todavía no tienes anuncios
          </h2>

          <p className="mb-6 max-w-md text-ink-3">
            Aún no has publicado ningún vehículo. Comienza agregando tu primer
            anuncio para mostrarlo en AutoMarket RD.
          </p>

          <Link
            to="/dashboard/publicar"
            className="flex items-center gap-2 rounded-lg bg-blue-600 px-6 py-3 font-semibold text-white transition-colors hover:bg-blue-700"
          >
            <FaPlusCircle />
            Publicar vehículo
          </Link>
        </motion.div>
      ) : (
        <motion.ul
          className="space-y-4"
          initial="hidden"
          animate="visible"
          variants={{
            hidden: {},
            visible: { transition: { staggerChildren: 0.06 } },
          }}
        >
          {anuncios.map((anuncio) => (
            <motion.li
              key={anuncio.id}
              variants={{ hidden: { opacity: 0, y: 12 }, visible: { opacity: 1, y: 0 } }}
              transition={{ duration: 0.3 }}
            >
              <AnuncioCard
                anuncio={anuncio}
                onPublicar={handlePublicar}
                onCambiarEstado={handleCambiarEstado}
                onEliminar={handleEliminar}
                onDestacar={handleDestacar}
                onQuitarDestacado={handleQuitarDestacado}
              />
            </motion.li>
          ))}
        </motion.ul>
      )}
    </div>
  );
}
