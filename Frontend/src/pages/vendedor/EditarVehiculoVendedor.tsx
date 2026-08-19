import FormularioVehiculoWizard from "../../components/FormularioVehiculoWizard";
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
  } = useFormularioVehiculo(true, "/vendedor", maxFotos, false);

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
        mostrarDestacado={false}
        onChange={handleChange}
        onKilometrajeChange={(e) => setKilometraje(e.target.value)}
        onAccesoriosChange={(e) => setAccesoriosTexto(e.target.value)}
        onTransmisionPersonalizadaChange={(e) => setTransmisionPersonalizada(e.target.value)}
        onImageChange={handleImageChange}
        onEliminarArchivo={handleEliminarArchivo}
        onEliminarFotoGuardada={handleEliminarFotoGuardada}
        onEstablecerPrincipal={handleEstablecerPrincipal}
        submitting={submitting}
        tituloBoton="Guardar cambios"
        onCancelar={() => navigate("/vendedor")}
        onSubmit={handleSubmit}
      />
    </div>
  );
}