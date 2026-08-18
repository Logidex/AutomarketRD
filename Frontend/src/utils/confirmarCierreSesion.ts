import Swal from "sweetalert2";

/** Muestra la confirmación antes de cerrar sesión. Devuelve true si el usuario confirma. */
export const confirmarCierreSesion = async (): Promise<boolean> => {
  const resultado = await Swal.fire({
    icon: "warning",
    title: "¿Cerrar sesión?",
    text: "Vas a salir de tu cuenta. ¿Quieres continuar?",
    showCancelButton: true,
    confirmButtonColor: "#dc2626",
    confirmButtonText: "Sí, salir",
    cancelButtonText: "Cancelar",
  });
  return resultado.isConfirmed;
};