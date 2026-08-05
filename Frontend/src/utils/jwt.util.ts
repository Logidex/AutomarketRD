export function getUserIdFromToken(): number | null {
  const token = localStorage.getItem("token");
  if (!token) return null;

  try {
    const payload = JSON.parse(atob(token.split(".")[1]));

    const rawId =
      payload.nameid ??
      payload.sub ??
      payload.userId ??
      payload.userid ??
      payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];

    const parsed = Number(rawId);
    return Number.isFinite(parsed) ? parsed : null;
  } catch {
    return null;
  }
}