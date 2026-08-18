import { useNavigate, useParams } from "react-router-dom";
import GestorImagenes from "../components/GestorImagenes";
import FormularioVehiculo from "../components/FormularioVehiculo";
import { useFormularioVehiculo } from "../hooks/useFormularioVehiculo";
import { useMaxFotosAnuncio, usePermiteDestacarAnuncio } from "../hooks/useSuscripcion";

export default function EditarVehiculo() {
  const { id } = useParams();
  const navigate = useNavigate();
  const maxFotos = useMaxFotosAnuncio();
  const permiteDestacar = usePermiteDestacarAnuncio();

  const {
    formData, kilometraje, setKilometraje, accesoriosTexto, setAccesoriosTexto,
    mostrarTransmisionPersonalizada, transmisionPersonalizada, setTransmisionPersonalizada,
    archivos, fotosGuardadas, handleChange, handleImageChange,
    handleEliminarArchivo, handleEliminarFotoGuardada, guardar, submitting,
    publicarAlGuardar, setPublicarAlGuardar,
    destacarAlPublicar, setDestacarAlPublicar,
    fotoPrincipal, handleEstablecerPrincipal
  } = useFormularioVehiculo(true, "/dashboard/mis-anuncios", maxFotos, permiteDestacar);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const payload = {
      ...formData,
      kilometraje: Number(kilometraje),
      precioAnterior: formData.precioAnterior || null,
      transmision: mostrarTransmisionPersonalizada ? transmisionPersonalizada : formData.transmision,
      accesorios: accesoriosTexto.split(",").map(a => a.trim()).filter(a => a)
    };
    guardar(payload);
  };

  return (
    <div className="mx-auto max-w-4xl rounded-lg border border-line bg-surface p-8 shadow-sm">
      <h2 className="mb-6 text-2xl font-bold text-ink">
        Editar Vehículo (ID: {id})
      </h2>

      <form onSubmit={handleSubmit} className="space-y-6">
        <FormularioVehiculo
          formData={formData}
          kilometraje={kilometraje}
          accesoriosTexto={accesoriosTexto}
          mostrarTransmisionPersonalizada={mostrarTransmisionPersonalizada}
          transmisionPersonalizada={transmisionPersonalizada}
          onChange={handleChange}
          onKilometrajeChange={(e) => setKilometraje(e.target.value)}
          onAccesoriosChange={(e) => setAccesoriosTexto(e.target.value)}
          onTransmisionPersonalizadaChange={(e) => setTransmisionPersonalizada(e.target.value)}
          publicarAlGuardar={publicarAlGuardar}
          onPublicarAlGuardarChange={(e) => setPublicarAlGuardar(e.target.checked)}
          destacarAlPublicar={destacarAlPublicar}
          onDestacarAlPublicarChange={(e) => setDestacarAlPublicar(e.target.checked)}
          mostrarDestacado={permiteDestacar}
        />

        <GestorImagenes
          archivos={archivos}
          fotosGuardadas={fotosGuardadas}
          fotoPrincipal={fotoPrincipal}
          maxImagenes={maxFotos}
          onImageChange={handleImageChange}
          onEliminarArchivo={handleEliminarArchivo}
          onEliminarFotoGuardada={handleEliminarFotoGuardada}
          onEstablecerPrincipal={handleEstablecerPrincipal}
        />

        <div className="flex justify-end gap-4 border-t border-line pt-6">
          <button
            type="button"
            onClick={() => navigate("/dashboard")}
            className="rounded-md border border-line px-6 py-2.5 font-medium text-ink-2 transition-colors hover:bg-hover"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={submitting}
            className="rounded-md bg-blue-600 px-6 py-2.5 font-medium text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-blue-400"
          >
            {submitting ? "Guardando..." : "Actualizar vehículo"}
          </button>
        </div>
      </form>
    </div>
  );
}