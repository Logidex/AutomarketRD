export function getUserIdFromToken(): number | null {
  // El JWT está en una cookie HttpOnly (no accesible desde JS).
  // El id se lee del usuario (metadatos no sensibles) guardado en localStorage.
  try {
    const crudo =
      localStorage.getItem("user:v1") ?? localStorage.getItem("user");
    if (!crudo) return null;

    const usuario = JSON.parse(crudo) as { usuarioId?: unknown };
    const parsed = Number(usuario?.usuarioId);
    return Number.isFinite(parsed) ? parsed : null;
  } catch {
    return null;
  }
}
