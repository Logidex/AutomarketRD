import GestorImagenes from "../../components/GestorImagenes";
import FormularioVehiculo from "../../components/FormularioVehiculo";
import { useFormularioVehiculo } from "../../hooks/useFormularioVehiculo";
import { useMaxFotosAnuncio } from "../../hooks/useSuscripcion";

export default function PublicarVehiculoVendedor() {
  const maxFotos = useMaxFotosAnuncio();

  const {
    formData, kilometraje, setKilometraje, accesoriosTexto, setAccesoriosTexto,
    mostrarTransmisionPersonalizada, transmisionPersonalizada, setTransmisionPersonalizada,
    archivos, fotosGuardadas, handleChange, handleImageChange,
    handleEliminarArchivo, handleEliminarFotoGuardada, guardar, submitting,
    publicarAlGuardar, setPublicarAlGuardar,
    destacarAlPublicar, setDestacarAlPublicar,
    fotoPrincipal, handleEstablecerPrincipal
  } = useFormularioVehiculo(false, "/vendedor", maxFotos, false);

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
        Publicar mi vehículo
      </h1>
      <p className="mb-6 text-sm text-ink-3">
        Completa los datos de tu vehículo. Recuerda que solo puedes publicar un
        anuncio con tu cuenta de vendedor.
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
          mostrarDestacado={false}
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
        <button
          type="submit"
          disabled={submitting}
          className="rounded-md bg-gray-800 px-5 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-gray-700 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {submitting ? "Guardando..." : "Guardar mi vehículo"}
        </button>
      </form>
    </div>
  );
}