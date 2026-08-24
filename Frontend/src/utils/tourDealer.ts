import { driver } from "driver.js";
import "driver.js/dist/driver.css";

/**
 * Tour interactivo de bienvenida para dealers (driver.js).
 * Recorre el menú del panel y enfatiza "Mi Perfil": los dealers que completan
 * su perfil (logo, horarios, descripción) generan más confianza en la vitrina.
 *
 * Se lanza una sola vez por usuario (flag en localStorage) y solo en escritorio:
 * en móvil la sidebar está oculta y el tour no tiene elementos que señalar.
 */
export function iniciarTourDealer(usuarioId: number): void {
  const clave = `tour-dealer-visto-${usuarioId}`;

  if (typeof window === "undefined") return;
  if (window.innerWidth < 1024) return; // sidebar oculta en móvil
  if (localStorage.getItem(clave)) return;

  localStorage.setItem(clave, "1");

  const driverObj = driver({
    showProgress: true,
    progressText: "{{current}} de {{total}}",
    nextBtnText: "Siguiente",
    prevBtnText: "Anterior",
    doneBtnText: "¡Listo!",
    steps: [
      {
        popover: {
          title: "¡Bienvenido a tu panel!",
          description:
            "Un recorrido rápido de 1 minuto por las herramientas de tu agencia.",
        },
      },
      {
        element: "aside nav a[href='/dashboard']",
        popover: {
          title: "Resumen",
          description:
            "El pulso de tu agencia: anuncios activos, contactos recibidos y uso de tu plan.",
          side: "right",
          align: "start",
        },
      },
      {
        element: "aside nav a[href='/dashboard/publicar']",
        popover: {
          title: "Publicar Vehículo",
          description:
            "Crea anuncios con fotos, precio en RD$/USD y toda la ficha del vehículo.",
          side: "right",
          align: "start",
        },
      },
      {
        element: "aside nav a[href='/dashboard/mis-anuncios']",
        popover: {
          title: "Mi Inventario",
          description:
            "Edita, publica, destaca o elimina tus vehículos desde un solo lugar.",
          side: "right",
          align: "start",
        },
      },
      {
        element: "aside nav a[href='/dashboard/leads']",
        popover: {
          title: "Leads",
          description:
            "Aquí llegan los compradores interesados en tus vehículos. ¡Respóndelos rápido!",
          side: "right",
          align: "start",
        },
      },
      {
        element: "aside nav a[href='/dashboard/mi-perfil']",
        popover: {
          title: "Mi Perfil ⭐",
          description:
            "El paso que casi todos se saltan: agrega logo, horarios y descripción de tu agencia. Un perfil completo genera confianza y vende más.",
          side: "right",
          align: "start",
        },
      },
      {
        element: "aside nav a[href='/dashboard/suscripcion']",
        popover: {
          title: "Suscripción",
          description:
            "Tu plan actual, historial de pagos y upgrades cuando necesites más alcance.",
          side: "right",
          align: "start",
        },
      },
      {
        popover: {
          title: "Último consejo ⭐",
          description:
            "Antes de publicar, ve a <strong>Mi Perfil</strong> y completa los datos de tu agencia: los perfiles completos inspiran confianza y atraen más contactos. ¡A vender!",
        },
      },
    ],
  });

  driverObj.drive();
}
