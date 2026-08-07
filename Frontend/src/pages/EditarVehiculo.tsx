import { useParams } from "react-router-dom";
import GestorImagenes from "../components/GestorImagenes";
import FormularioVehiculo from "../components/FormularioVehiculo";
import { useFormularioVehiculo } from "../hooks/useFormularioVehiculo";

export default function EditarVehiculo() {
  const { id } = useParams();
  
  const {
    formData, kilometraje, setKilometraje, accesoriosTexto, setAccesoriosTexto,
    mostrarTransmisionPersonalizada, transmisionPersonalizada, setTransmisionPersonalizada,
    archivos, fotosGuardadas, handleChange, handleImageChange, 
    handleEliminarArchivo, handleEliminarFotoGuardada, guardar
  } = useFormularioVehiculo(true);

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
    <div className="mx-auto max-w-4xl rounded-lg border border-gray-200 bg-white p-8 shadow-sm">
      <h2 className="mb-6 text-2xl font-bold text-gray-900">
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
        />

        <GestorImagenes
          archivos={archivos}
          fotosGuardadas={fotosGuardadas}
          onImageChange={handleImageChange}
          onEliminarArchivo={handleEliminarArchivo}
          onEliminarFotoGuardada={handleEliminarFotoGuardada}
        />

        <div className="flex justify-end gap-4 border-t border-gray-100 pt-6">
          <button
            type="submit"
            className="rounded-md bg-blue-600 px-6 py-2.5 font-medium text-white transition-colors hover:bg-blue-700"
          >
            Actualizar vehículo
          </button>
        </div>
      </form>
    </div>
  );
}