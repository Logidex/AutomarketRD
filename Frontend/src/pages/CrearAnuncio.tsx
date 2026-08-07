import GestorImagenes from "../components/GestorImagenes";
import FormularioVehiculo from "../components/FormularioVehiculo";
import { useFormularioVehiculo } from "../hooks/useFormularioVehiculo";

export default function CrearAnuncio() {
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
  } = useFormularioVehiculo(false);

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
    <div className="mx-auto max-w-4xl p-8">
      <h2 className="mb-6 text-2xl font-bold">Crear Nuevo Anuncio</h2>
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
        <button type="submit" disabled={cargando} className="bg-blue-600 text-white px-6 py-2 rounded">
          {cargando ? "Creando..." : "Publicar Anuncio"}
        </button>
      </form>
    </div>
  );
}
