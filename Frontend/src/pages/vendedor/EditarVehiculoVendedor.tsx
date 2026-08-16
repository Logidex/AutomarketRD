import GestorImagenes from "../../components/GestorImagenes";
import FormularioVehiculo from "../../components/FormularioVehiculo";
import { useFormularioVehiculo } from "../../hooks/useFormularioVehiculo";
import { useMaxFotosAnuncio } from "../../hooks/useSuscripcion";
import { useNavigate } from "react-router-dom";

export default function EditarVehiculoVendedor() {
  const navigate = useNavigate();
  const maxFotos = useMaxFotosAnuncio();

  const {
    formData, kilometraje, setKilometraje, accesoriosTexto, setAccesoriosTexto,
    mostrarTransmisionPersonalizada, transmisionPersonalizada, setTransmisionPersonalizada,
    archivos, fotosGuardadas, handleChange, handleImageChange,
    handleEliminarArchivo, handleEliminarFotoGuardada, guardar, submitting,
    publicarAlGuardar, setPublicarAlGuardar,
    destacarAlPublicar, setDestacarAlPublicar,
    fotoPrincipal, handleEstablecerPrincipal
  } = useFormularioVehiculo(true, "/vendedor", maxFotos);

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
    <div>
      <h1 className="mb-1 text-lg font-semibold text-ink">
        Editar mi vehículo
      </h1>
      <p className="mb-6 text-sm text-ink-3">
        Modifica los datos cuando quieras; los cambios se reflejan en la vitrina.
      </p>

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
        <div className="flex justify-end gap-4 pt-2">
          <button
            type="button"
            onClick={() => navigate("/vendedor")}
            className="rounded-md border border-line px-5 py-2.5 text-sm font-semibold text-ink-2 transition-colors hover:bg-surface-2"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={submitting}
            className="rounded-md bg-gray-800 px-5 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-gray-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {submitting ? "Guardando..." : "Guardar cambios"}
          </button>
        </div>
      </form>
    </div>
  );
}