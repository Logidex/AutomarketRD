import { useNavigate, useParams } from "react-router-dom";
import FormularioVehiculoWizard from "../components/FormularioVehiculoWizard";
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
        tituloBoton="Actualizar vehículo"
        onCancelar={() => navigate("/dashboard")}
        onSubmit={handleSubmit}
      />
    </div>
  );
}