import FormularioVehiculoWizard from "../components/FormularioVehiculoWizard";
import { useFormularioVehiculo } from "../hooks/useFormularioVehiculo";
import { useMaxFotosAnuncio, usePermiteDestacarAnuncio } from "../hooks/useSuscripcion";

export default function CrearAnuncio() {
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
  } = useFormularioVehiculo(false, "/dashboard/mis-anuncios", maxFotos, permiteDestacar);

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
    <div className="mx-auto max-w-4xl p-8">
      <h2 className="mb-6 text-2xl font-bold">Crear Nuevo Anuncio</h2>
      <FormularioVehiculoWizard
        formData={formData}
        kilometraje={kilometraje}
        accesoriosTexto={accesoriosTexto}
        mostrarTransmisionPersonalizada={mostrarTransmisionPersonalizada}
        transmisionPersonalizada={transmisionPersonalizada}
        archivos={archivos}
        fotosGuardadas={fotosGuardadas}
        fotoPrincipal={fotoPrincipal}
        maxImagenes={maxFotos}
        publicarAlGuardar={publicarAlGuardar}
        onPublicarAlGuardarChange={(e) => setPublicarAlGuardar(e.target.checked)}
        destacarAlPublicar={destacarAlPublicar}
        onDestacarAlPublicarChange={(e) => setDestacarAlPublicar(e.target.checked)}
        mostrarDestacado={permiteDestacar}
        onChange={handleChange}
        onKilometrajeChange={(e) => setKilometraje(e.target.value)}
        onAccesoriosChange={(e) => setAccesoriosTexto(e.target.value)}
        onTransmisionPersonalizadaChange={(e) => setTransmisionPersonalizada(e.target.value)}
        onImageChange={handleImageChange}
        onEliminarArchivo={handleEliminarArchivo}
        onEliminarFotoGuardada={handleEliminarFotoGuardada}
        onEstablecerPrincipal={handleEstablecerPrincipal}
        submitting={submitting}
        tituloBoton="Publicar Anuncio"
        onSubmit={handleSubmit}
      />
    </div>
  );
}
