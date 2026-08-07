import { useNavigate, useParams } from "react-router-dom";
import GestorImagenes from "../components/GestorImagenes";
import FormularioVehiculo from "../components/FormularioVehiculo";
import { useFormularioVehiculo } from "../hooks/useFormularioVehiculo";
import Spinner from "../components/Spinner";

export default function EditarVehiculo() {
  const navigate = useNavigate();
  const { id } = useParams();
  
  const {
    formData,
    kilometraje,
    setKilometraje,
    accesoriosTexto,
    setAccesoriosTexto,
    mostrarTransmisionPersonalizada,
    transmisionPersonalizada,
    setTransmisionPersonalizada,
    archivos,
    fotosGuardadas,
    cargando,
    handleChange,
    handleImageChange,
    handleEliminarArchivo,
    handleEliminarFotoGuardada,
    guardar
  } = useFormularioVehiculo(true);

  if (cargando && !formData.marca) {
    return <Spinner />;
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    const totalImagenes = archivos.length + fotosGuardadas.length;
    if (totalImagenes < 5) {
      return;
    }

    const arrayAccesorios = accesoriosTexto
      .split(",")
      .map((item) => item.trim())
      .filter((item) => item.length > 0);

    const transmisionFinal =
      formData.transmision === "Otra"
        ? transmisionPersonalizada.trim()
        : formData.transmision;

    const kilometrajeNumero = kilometraje.trim() === "" ? 0 : Number(kilometraje);

    const payload = {
      ...formData,
      kilometraje: kilometrajeNumero,
      transmision: transmisionFinal,
      accesorios: arrayAccesorios,
    };

    guardar(payload);
  };

  return (
    <div className="mx-auto max-w-4xl rounded-lg border border-gray-200 bg-white p-8 shadow-sm">
      <h2 className="mb-6 text-2xl font-bold text-gray-900">
        Editar Vehículo (ID: {id})
      </h2>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* FORMULARIO DE TEXTOS E INPUTS AISLADO */}
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

        {/* GESTOR DE IMÁGENES AISLADO */}
        <GestorImagenes
          archivos={archivos}
          fotosGuardadas={fotosGuardadas}
          onImageChange={handleImageChange}
          onEliminarArchivo={handleEliminarArchivo}
          onEliminarFotoGuardada={handleEliminarFotoGuardada}
        />

        {/* BOTONES DE ACCIÓN */}
        <div className="flex justify-end gap-4 border-t border-gray-100 pt-6">
          <button
            type="button"
            onClick={() => navigate("/dashboard/mis-anuncios")}
            className="rounded-md border border-gray-300 px-6 py-2.5 font-medium text-gray-700 transition-colors hover:bg-gray-50"
          >
            Cancelar
          </button>

          <button
            type="submit"
            disabled={cargando}
            className={`rounded-md bg-blue-600 px-6 py-2.5 font-medium text-white transition-colors ${
              cargando ? "cursor-not-allowed opacity-50" : "hover:bg-blue-700"
            }`}
          >
            {cargando ? "Actualizando datos..." : "Actualizar vehículo"}
          </button>
        </div>
      </form>
    </div>
  );
}