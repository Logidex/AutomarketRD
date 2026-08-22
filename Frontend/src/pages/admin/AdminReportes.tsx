import { useState } from "react";
import { Link } from "react-router-dom";
import Swal from "sweetalert2";
import {
  FaBan,
  FaCheckCircle,
  FaExternalLinkAlt,
  FaFlag,
} from "react-icons/fa";
import type { ReporteAdmin, MotivoReporte } from "../../services/admin.service";
import Spinner from "../../components/Spinner";
import { formatearFecha } from "../../utils/fecha";
import { urlImagen } from "../../utils/imagen";
import {
  useAdminReportes,
  useDescartarReporte,
  useResolverReporte,
  type EstadoReporte,
} from "../../hooks/useAdmin";

const MOTIVO_LABEL: Record<MotivoReporte, string> = {
  ContenidoInapropiado: "Contenido inapropiado (+18)",
  FraudeEstafa: "Fraude o estafa",
  InformacionFalsa: "Información falsa",
  Duplicado: "Anuncio duplicado",
  Otro: "Otro motivo",
};

const MOTIVO_CLASES: Record<MotivoReporte, string> = {
  ContenidoInapropiado: "bg-red-100 text-red-700",
  FraudeEstafa: "bg-orange-100 text-orange-700",
  InformacionFalsa: "bg-amber-100 text-amber-700",
  Duplicado: "bg-blue-50 text-blue-600",
  Otro: "bg-surface-2 text-ink-2",
};

const FILTROS: EstadoReporte[] = ["Pendiente", "Resuelto", "Descartado"];

const PLACEHOLDER_FOTO = "/sin-foto.svg";

export default function AdminReportes() {
  const [filtro, setFiltro] = useState<EstadoReporte>("Pendiente");
  const { data: reportes = [], isLoading } = useAdminReportes(filtro);
  const descartar = useDescartarReporte();
  const resolver = useResolverReporte();

  const gestionando = descartar.isPending || resolver.isPending;

  const pedirConfirmacion = async (
    reporte: ReporteAdmin,
    accion: "descartar" | "resolver",
  ): Promise<boolean> => {
    const esResolver = accion === "resolver";

    const resultado = await Swal.fire({
      icon: esResolver ? "warning" : "question",
      title: esResolver ? "¿Eliminar el anuncio?" : "¿Descartar el reporte?",
      html: esResolver
        ? `Se <strong>eliminará definitivamente</strong> el anuncio
           <strong>${reporte.anuncioTitulo}</strong> junto con sus fotos.<br/>
           Esta acción no se puede deshacer.`
        : `El reporte del anuncio <strong>${reporte.anuncioTitulo}</strong> quedará como descartado.`,
      showCancelButton: true,
      confirmButtonText: esResolver ? "Sí, eliminar anuncio" : "Sí, descartar",
      cancelButtonText: "Cancelar",
      confirmButtonColor: esResolver ? "#dc2626" : "#3b82f6",
    });

    return resultado.isConfirmed;
  };

  const manejarAccion = async (
    reporte: ReporteAdmin,
    accion: "descartar" | "resolver",
  ) => {
    if (!(await pedirConfirmacion(reporte, accion))) return;

    try {
      if (accion === "descartar") {
        await descartar.mutateAsync(reporte.id);
        await Swal.fire({
          icon: "success",
          title: "Reporte descartado",
          timer: 1500,
          showConfirmButton: false,
        });
      } else {
        await resolver.mutateAsync(reporte.id);
        await Swal.fire({
          icon: "success",
          title: "Anuncio eliminado y reporte resuelto",
          timer: 1800,
          showConfirmButton: false,
        });
      }
    } catch (err) {
      const mensaje =
        (err as { message?: string })?.message ?? "Ocurrió un error inesperado.";
      await Swal.fire({ icon: "error", title: "Error", text: mensaje });
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="flex items-center gap-2 text-2xl font-bold text-ink">
            <FaFlag className="text-violet-600" />
            Reportes de anuncios
          </h1>
          <p className="mt-1 text-sm text-ink-3">
            Revisa los anuncios marcados por los usuarios y toma una decisión.
          </p>
        </div>

        <div className="flex gap-2">
          {FILTROS.map((f) => (
            <button
              key={f}
              type="button"
              onClick={() => setFiltro(f)}
              className={`rounded-full px-3 py-1.5 text-xs font-semibold transition-colors ${
                filtro === f
                  ? "bg-violet-600 text-white"
                  : "border border-line bg-surface text-ink-2 hover:bg-hover"
              }`}
            >
              {f}
            </button>
          ))}
        </div>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      ) : reportes.length === 0 ? (
        <div className="rounded-xl border border-line bg-surface p-10 text-center text-ink-3">
          No hay reportes en estado <strong>{filtro}</strong>. 🎉
        </div>
      ) : (
        <ul className="space-y-4">
          {reportes.map((reporte) => (
            <li
              key={reporte.id}
              className="flex flex-col gap-4 rounded-xl border border-line bg-surface p-4 sm:flex-row"
            >
              {/* Miniatura */}
              <Link
                to={`/anuncio/${reporte.anuncioId}`}
                className="h-32 w-full shrink-0 overflow-hidden rounded-lg border border-line sm:w-48"
              >
                <img
                  src={
                    reporte.anuncioFotoPrincipal
                      ? urlImagen(reporte.anuncioFotoPrincipal)
                      : PLACEHOLDER_FOTO
                  }
                  alt={reporte.anuncioTitulo}
                  loading="lazy"
                  className="h-full w-full object-cover"
                />
              </Link>

              {/* Info */}
              <div className="min-w-0 flex-1 space-y-2">
                <div className="flex flex-wrap items-center gap-2">
                  <span
                    className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold ${
                      MOTIVO_CLASES[reporte.motivo]
                    }`}
                  >
                    {MOTIVO_LABEL[reporte.motivo]}
                  </span>
                  <span className="rounded-full bg-surface-2 px-2.5 py-0.5 text-xs text-ink-2">
                    {reporte.estado}
                  </span>
                  <span className="text-xs text-ink-3">
                    {formatearFecha(reporte.fechaCreacionUtc)}
                  </span>
                </div>

                <Link
                  to={`/anuncio/${reporte.anuncioId}`}
                  className="block truncate font-semibold text-ink hover:text-blue-600"
                >
                  {reporte.anuncioTitulo} ·{" "}
                  <span className="text-sm font-normal text-ink-2">
                    {reporte.anuncioMoneda} ${reporte.anuncioPrecio.toLocaleString("es-DO")}
                  </span>
                </Link>

                {reporte.detalle && (
                  <p className="text-sm italic text-ink-2">“{reporte.detalle}”</p>
                )}

                <p className="text-xs text-ink-3">
                  Estado del anuncio: {reporte.anuncioEstado} · IP: {reporte.ipReportante}
                </p>
              </div>

              {/* Acciones */}
              {reporte.estado === "Pendiente" && (
                <div className="flex shrink-0 flex-row gap-2 sm:flex-col sm:justify-center">
                  <Link
                    to={`/anuncio/${reporte.anuncioId}`}
                    className="flex items-center justify-center gap-2 rounded-lg border border-line px-3 py-2 text-xs font-semibold text-ink-2 transition-colors hover:border-blue-400 hover:text-blue-600"
                  >
                    <FaExternalLinkAlt /> Ver
                  </Link>
                  <button
                    type="button"
                    disabled={gestionando}
                    onClick={() => manejarAccion(reporte, "descartar")}
                    className="flex items-center justify-center gap-2 rounded-lg border border-line px-3 py-2 text-xs font-semibold text-ink-2 transition-colors hover:bg-hover disabled:opacity-50"
                  >
                    <FaBan /> Descartar
                  </button>
                  <button
                    type="button"
                    disabled={gestionando}
                    onClick={() => manejarAccion(reporte, "resolver")}
                    className="flex items-center justify-center gap-2 rounded-lg bg-red-600 px-3 py-2 text-xs font-semibold text-white transition-colors hover:bg-red-700 disabled:opacity-50"
                  >
                    <FaCheckCircle /> Eliminar anuncio
                  </button>
                </div>
              )}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
