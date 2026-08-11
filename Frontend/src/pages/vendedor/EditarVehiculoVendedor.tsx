import GestorImagenes from "../../components/GestorImagenes";
import FormularioVehiculo from "../../components/FormularioVehiculo";
import { useFormularioVehiculo } from "../../hooks/useFormularioVehiculo";
import { useNavigate } from "react-router-dom";

export default function EditarVehiculoVendedor() {
  const navigate = useNavigate();

  const {
    formData, kilometraje, setKilometraje, accesoriosTexto, setAccesoriosTexto,
    mostrarTransmisionPersonalizada, transmisionPersonalizada, setTransmisionPersonalizada,
    archivos, fotosGuardadas, handleChange, handleImageChange,
    handleEliminarArchivo, handleEliminarFotoGuardada, guardar, submitting
  } = useFormularioVehiculo(true, "/vendedor");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const payload = {
      ...formData,
      kilometraje: Number(kilometraje),
      transmision: mostrarTransmisionPersonalizada ? transmisionPersonalizada : formData.transmision,
      accesorios: accesoriosTexto.split(",").map(a => a.trim()).filter(a => a)
    };
    guardar(payload);
  };

  return (
    <div>
      <h1 className="mb-1 text-lg font-semibold text-gray-900">
        Editar mi vehículo
      </h1>
      <p className="mb-6 text-sm text-gray-500">
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
        />
        <GestorImagenes
          archivos={archivos}
          fotosGuardadas={fotosGuardadas}
          onImageChange={handleImageChange}
          onEliminarArchivo={handleEliminarArchivo}
          onEliminarFotoGuardada={handleEliminarFotoGuardada}
        />
        <div className="flex justify-end gap-4 pt-2">
          <button
            type="button"
            onClick={() => navigate("/vendedor")}
            className="rounded-md border border-gray-300 px-5 py-2.5 text-sm font-semibold text-gray-700 transition-colors hover:bg-gray-100"
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