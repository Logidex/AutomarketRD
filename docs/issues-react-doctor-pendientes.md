# Issues React Doctor pendientes — falsos positivos documentados

Los 9 issues restantes de `react-doctor` (score 67/100) se dividen en:

- **7 falsos positivos** documentados abajo (no requieren cambio de código; la propia
  documentación de cada regla recomienda *no* modificar el código cuando no hay cache
  afectada o cuando reordenar rompería el patrón intencional).
- **2 issues de seguridad** (`auth-token-in-web-storage`) que requieren migración a
  cookie HttpOnly — ver `docs/migrar-token-httpOnly.md`.

Cada sección está formateada para pegarse directamente como issue de GitHub en
`Logidex/AutomarketRDSpn`.

---

## 1. `react-doctor/query-mutation-missing-invalidation` ×6

**Regla:** `query-mutation-missing-invalidation` — P2, categoría TanStack Query.
La receta dice textualmente: *"Keep the mutation unchanged when no query cache is
affected or synchronization is deliberately owned elsewhere, and document that
ownership rather than adding a broad invalidation."*

**Confianza:** alta — ninguna de las 6 mutaciones invalida una query cacheada porque
no hay query asociada.

| # | Archivo:línea | Mutación | Por qué no aplica invalidación |
|---|---|---|---|
| 1 | `src/hooks/useComparador.ts:18` | `useBuscarComparador` | El resultado se guarda en **estado local** del componente Comparador, no en la cache de TanStack Query. No hay queryKey afectada. |
| 2 | `src/hooks/useContacto.ts:5` | `useEnviarContacto` | Formulario de contacto: POST sin efectos sobre ninguna query cacheada. |
| 3 | `src/hooks/useSuscripcion.ts:48` | `useGenerarLinkPago` | Devuelve la URL de pago para redirigir. La sincronización de cache ocurre en `useConfirmarPago` (invalida `suscripcion` y `dashboard-resumen`). |
| 4 | `src/hooks/useUsuario.ts:26` | `useCambiarPassword` | El cambio de contraseña no altera `usuario-cuenta`; no hay datos cacheados que invalidar. |
| 5 | `src/hooks/useUsuario.ts:33` | `useConfirmarCambioPassword` | Ídem anterior: confirma el código, sin cache asociada. |
| 6 | `src/hooks/useUsuario.ts:39` | `useSolicitarCambioEmail` | Solo envía código de verificación; el cambio real se sincroniza en `useConfirmarCambioEmail` (invalida `usuario-cuenta`). |

**Impacto:** nulo. Añadir `invalidateQueries` genérico sería código muerto que
invalidaría caches innecesariamente y penalizaría el rendimiento.

**Fix propuesto:** ninguno (documentar y cerrar). Si en el futuro alguna de estas
mutaciones alimenta una query cacheada, agregar `onSuccess` con el queryKey exacto.

---

## 2. `react-doctor/async-defer-await`

**Archivo:** `src/hooks/useFormularioVehiculo.ts:61`

**Patrón:**
```ts
useEffect(() => {
  if (isEditMode && id) {
    let activo = true;
    const cargarAnuncio = async () => {
      setLoading(true);
      try {
        if (!activo) return;                                   // guard pre-await (línea 60)
        const datos = await anuncioService.obtenerPorId(id);   // línea 61
        if (!activo) return;                                   // guard post-await (línea 62)
        setFormData(...);
      } ...
```

**Por qué es falso positivo:** el guard `if (!activo) return` *posterior* al await es
intencional: previene `setState` después del desmontaje del componente (patrón
async-effect). Mover el guard antes del await anularía la protección contra
`setState` tras unmount. La receta de la regla exige *"preserve the sequence"*
cuando el guard debe evaluarse después del await por su semántica de limpieza.

**Impacto:** nulo. El patrón ya tiene el guard pre-await en la línea 60.

**Fix propuesto:** ninguno (documentar y cerrar). Alternativa futura si se refactoriza:
usar un hook de datos (TanStack Query) para la carga en modo edición.

---

## Seguimiento

- [ ] Crear issues en GitHub `Logidex/AutomarketRDSpn` con las secciones 1 y 2.
- [ ] Cerrar los issues tras la migración a cookie HttpOnly (ver
  `docs/migrar-token-httpOnly.md`) si corresponde.
