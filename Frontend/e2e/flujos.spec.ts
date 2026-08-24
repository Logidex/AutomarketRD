import { expect, test, type Page } from "@playwright/test";

// Flujo completo de negocio: registro → publicar anuncio → comprar plan.
// El stack e2e (docker-compose.e2e.yml) corre la API en Development con
// credenciales dummy, por lo que PayPal y el almacenamiento de imágenes son
// simulados en el backend (FakePayPalService / AlmacenadorArchivosLocal):
// la "aprobación de pago" regresa directo a /pago-exitoso?token=FAKE-...

const PNG_1PX = Buffer.from(
  "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==",
  "base64",
);

const FOTOS = Array.from({ length: 5 }, (_, i) => ({
  name: `foto-${i + 1}.png`,
  mimeType: "image/png",
  buffer: PNG_1PX,
}));

async function aceptarModal(page: Page, texto: RegExp) {
  await expect(page.getByText(texto).first()).toBeVisible({ timeout: 20_000 });
  await page.getByRole("button", { name: /^OK$/i }).click();
}

async function iniciarSesion(page: Page, email: string, password: string) {
  await page.goto("/login");
  await page.locator("#loginEmail").fill(email);
  await page.locator("#loginPassword").fill(password);
  await page.getByRole("button", { name: "Iniciar Sesión" }).click();
  await expect(page).not.toHaveURL(/\/login/, { timeout: 15_000 });
}

test.describe("Flujo Vendedor: publicar vehículo", () => {
  // El wizard de 5 pasos + registro + login no cabe en el timeout global de 30s
  test.setTimeout(120_000);

  test("Registro → publicar con 5 fotos → visible en su panel", async ({ page }, testInfo) => {
    const email = `e2e-vendedor-${Date.now()}-${testInfo.retry}@test.local`;

    // 1. Registro como Vendedor (rol sin campos de agencia)
    await page.goto("/registro");
    await page.locator("#nombre").fill("Vendedor");
    await page.locator("#apellido").fill("E2E");
    await page.locator("#email").fill(email);
    await page.locator("#password").fill("ClaveE2E_123");
    await page.locator("#confirmarPassword").fill("ClaveE2E_123");
    await page.locator("#rol").selectOption("Vendedor");
    await page.getByRole("button", { name: "Registrarse" }).click();

    await aceptarModal(page, /Registro exitoso/i);
    await expect(page).toHaveURL(/\/login/, { timeout: 15_000 });

    // 2. Login y navegar al formulario de publicación
    await iniciarSesion(page, email, "ClaveE2E_123");
    await page.goto("/vendedor/publicar");
    await expect(page.getByRole("heading", { name: "Publicar mi vehículo" })).toBeVisible();

    // Paso 1: información básica
    await page.locator("#marca").fill("Toyota");
    await page.locator("#modelo").fill("Corolla");
    await page.locator("#version").fill("SE 2022");
    await page.locator("#precio").fill("850000");
    await page.locator("#kilometraje").fill("45000");
    await page.getByRole("button", { name: "Siguiente" }).click();

    // Paso 2: especificaciones
    await page.locator("#tipoVehiculo").selectOption({ index: 1 });
    await page.locator("#motor").fill("1.8L 4 cilindros");
    await page.locator("#traccion").selectOption("Delantera");
    await page.locator("#transmision").selectOption("Automatica");
    await page.locator("#combustible").selectOption({ index: 1 });
    await page.locator("#colorExterior").fill("Blanco");
    await page.locator("#colorInterior").fill("Negro");
    await page.getByRole("button", { name: "Siguiente" }).click();

    // Paso 3: detalles y ubicación
    await page.locator("#ubicacion").fill("Santo Domingo");
    await page.locator("#descripcion").fill("Anuncio creado por prueba E2E del flujo completo.");
    await page.getByRole("button", { name: "Siguiente" }).click();

    // Paso 4: fotos (mínimo 5) y publicación inmediata
    await page.locator("#fotosVehiculo").setInputFiles(FOTOS);
    await expect(page.getByText(/\b5\/\d+\b/)).toBeVisible();
    await page.locator("#publicarAlGuardar").check();

    // force evita el retry de Playwright: al avanzar de paso, el botón
    // "Siguiente" se reemplaza por el de submit en la misma posición y el
    // reintento puede aterrizar en él, disparando el guardado dos veces.
    await page.getByRole("button", { name: "Siguiente" }).click({ force: true });

    // Paso 5: revisar y guardar. Si la carrera igual disparó el guardado,
    // aceptamos cualquiera de los dos estados (paso 5 o éxito directo).
    await expect(
      page
        .getByRole("heading", { name: "Revisar y publicar" })
        .or(page.getByText(/Publicado correctamente/i))
    ).toBeVisible({ timeout: 10_000 });

    if (await page.getByRole("button", { name: "Guardar mi vehículo" }).isVisible().catch(() => false)) {
      await page.getByRole("button", { name: "Guardar mi vehículo" }).click();
    }

    // 3. Éxito: modal y redirección al panel del vendedor
    await aceptarModal(page, /Publicado correctamente/i);
    await expect(page).toHaveURL(/\/vendedor$/, { timeout: 20_000 });

    // 4. El anuncio aparece en el panel del vendedor
    await expect(page.getByText(/Corolla/i).first()).toBeVisible({ timeout: 15_000 });

    // 5. SEO: la vitrina enlaza con URL descriptiva (slug + id) y el
    //    formato antiguo /anuncio/<id> sigue funcionando
    await page.goto("/vehiculos");
    await page.locator("div.grid > div.group > button").first().click();
    await expect(page).toHaveURL(/\/anuncio\/[a-z0-9-]+-\d+$/);
    const idAnuncio = page.url().match(/(\d+)$/)?.[1];
    await page.goto(`/anuncio/${idAnuncio}`);
    await expect(page.getByText(/Corolla/i).first()).toBeVisible({ timeout: 15_000 });
  });
});

test.describe("Flujo Dealer: comprar plan", () => {
  test.setTimeout(120_000);

  test("Registro → comprar plan Pro (PayPal simulado) → suscripción activa", async ({ page }, testInfo) => {
    const email = `e2e-dealer-${Date.now()}-${testInfo.retry}@test.local`;

    // 1. Registro como Dealer (con datos de agencia); auto-login y va a /suscripcion
    await page.goto("/registro");
    await page.locator("#nombre").fill("Dealer");
    await page.locator("#apellido").fill("E2E");
    await page.locator("#email").fill(email);
    await page.locator("#password").fill("ClaveE2E_123");
    await page.locator("#confirmarPassword").fill("ClaveE2E_123");
    await page.locator("#telefonoPersonal").fill("809-555-1234");
    await page.locator("#rol").selectOption("Dealer");
    await page.locator("#nombreAgencia").fill("Agencia E2E");
    await page.locator("#agenciaRNC").fill("1-30-99999-9");
    await page.locator("#ubicacionAgencia").fill("Santo Domingo");
    await page.locator("#telefonoAgencia").fill("809-555-9999");
    await page.getByRole("button", { name: "Registrarse" }).click();

    await aceptarModal(page, /Registro exitoso/i);
    await expect(page).toHaveURL(/\/suscripcion/, { timeout: 15_000 });

    // 2. Elegir el plan Pro (marcado como "Más popular") en ciclo Mensual
    await page.goto("/precios");
    const tarjetaPro = page.locator("div.grid > div", { hasText: "Más popular" });
    await tarjetaPro.getByRole("button", { name: "Comprar Plan" }).click();

    // 3. PayPal simulado: regresa directo a la página de éxito con el token
    await expect(page).toHaveURL(/pago-exitoso\?token=FAKE-/, { timeout: 20_000 });
    await expect(page.getByText("¡Pago exitoso!")).toBeVisible({ timeout: 20_000 });

    // 4. Al panel: la suscripción quedó activa y el tour de bienvenida arranca
    await page.getByRole("link", { name: /Ir a mi Panel/i }).click();
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });
    await expect(
      page.getByText("¡Bienvenido a tu panel!")
    ).toBeVisible({ timeout: 10_000 });
  });
});

test.describe("Flujo Dealer: cupón de bienvenida", () => {
  test.setTimeout(120_000);

  test("Registro → aplicar cupón → Pro activa por 15 días", async ({ page }, testInfo) => {
    const email = `e2e-cupon-${Date.now()}-${testInfo.retry}@test.local`;

    // 1. Registro como Dealer; auto-login y aterrizaje en /suscripcion
    await page.goto("/registro");
    await page.locator("#nombre").fill("Cupon");
    await page.locator("#apellido").fill("E2E");
    await page.locator("#email").fill(email);
    await page.locator("#password").fill("ClaveE2E_123");
    await page.locator("#confirmarPassword").fill("ClaveE2E_123");
    await page.locator("#telefonoPersonal").fill("809-555-4321");
    await page.locator("#rol").selectOption("Dealer");
    await page.locator("#nombreAgencia").fill("Agencia Cupón E2E");
    await page.locator("#agenciaRNC").fill("1-30-88888-8");
    await page.locator("#ubicacionAgencia").fill("La Romana");
    await page.locator("#telefonoAgencia").fill("809-555-8888");
    await page.getByRole("button", { name: "Registrarse" }).click();

    await aceptarModal(page, /Registro exitoso/i);
    await expect(page).toHaveURL(/\/suscripcion/, { timeout: 15_000 });

    // 2. Aplicar el cupón de bienvenida (sembrado por el seeder)
    await page.locator("#codigoCupon").fill("pro15bienvenida");
    await page.getByRole("button", { name: "Aplicar cupón" }).click();

    await expect(
      page.getByRole("heading", { name: "¡Cupón aplicado!" })
    ).toBeVisible({ timeout: 20_000 });
    await page.getByRole("button", { name: /^OK$/i }).click();
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });

    // 3. La suscripción quedó en Pro con la vigencia del cupón (~15 días)
    await page.goto("/dashboard/suscripcion");
    await expect(page.getByText(/Plan Pro/).first()).toBeVisible({ timeout: 15_000 });
    await expect(page.getByText(/días restantes/).first()).toBeVisible();
  });
});

test.describe("Encuesta de satisfacción", () => {
  test.setTimeout(120_000);

  test("Comprador la ve en su 3ra visita y responde una vez", async ({ page }, testInfo) => {
    const email = `e2e-encuesta-${Date.now()}-${testInfo.retry}@test.local`;

    // Simular 3ra visita: el contador ya viene cargado antes de cargar la app
    await page.addInitScript(() => {
      localStorage.setItem("am-visitas", "3");
    });

    // 1. Registro de comprador (cualquier rol autenticado puede responder)
    await page.goto("/registro");
    await page.locator("#nombre").fill("Encuesta");
    await page.locator("#apellido").fill("E2E");
    await page.locator("#email").fill(email);
    await page.locator("#password").fill("ClaveE2E_123");
    await page.locator("#confirmarPassword").fill("ClaveE2E_123");
    await page.getByRole("button", { name: "Registrarse" }).click();
    await aceptarModal(page, /Registro exitoso/i);
    await expect(page).toHaveURL(/\/login/, { timeout: 15_000 });

    // 2. Login → el modal aparece solo (tras el delay de 2.5s)
    await iniciarSesion(page, email, "ClaveE2E_123");
    await expect(
      page.getByText("¿Cómo es tu experiencia en AutoMarket RD?")
    ).toBeVisible({ timeout: 15_000 });

    // 3. Responder: 5★ y 5★ en las escalas + comentario abierto
    await page.getByRole("button", { name: "5 de 5" }).nth(0).click();
    await page.getByRole("button", { name: "5 de 5" }).nth(1).click();
    await page.getByPlaceholder("Cuéntanos (opcional)").fill("Excelente plataforma, sigan así");
    await page.getByRole("button", { name: "Enviar respuestas" }).click();

    await expect(
      page.getByText("¡Gracias por tu opinión!")
    ).toBeVisible({ timeout: 15_000 });
  });

  test("Admin ve los resultados agregados", async ({ page }) => {
    // La respuesta la creó el test anterior (misma BD, ejecución secuencial)
    await page.goto("/login");
    await page.locator("#loginEmail").fill("admin@e2e.local");
    await page.locator("#loginPassword").fill("AdminE2e_123");
    await page.getByRole("button", { name: "Iniciar Sesión" }).click();
    await expect(page).toHaveURL(/\/admin/, { timeout: 15_000 });

    await page.goto("/admin/encuestas");
    await expect(
      page.getByText(/usuario ha respondido|usuarios han respondido/)
    ).toBeVisible({ timeout: 15_000 });
    await expect(
      page.getByText("Excelente plataforma, sigan así").first()
    ).toBeVisible();
  });

  test("Cerrada con X: se oculta en la sesión y reaparece en la próxima", async ({ page }, testInfo) => {
    const email = `e2e-descarte-${Date.now()}-${testInfo.retry}@test.local`;

    await page.addInitScript(() => {
      localStorage.setItem("am-visitas", "3");
    });

    // Registrar comprador y entrar
    await page.goto("/registro");
    await page.locator("#nombre").fill("Descarte");
    await page.locator("#apellido").fill("E2E");
    await page.locator("#email").fill(email);
    await page.locator("#password").fill("ClaveE2E_123");
    await page.locator("#confirmarPassword").fill("ClaveE2E_123");
    await page.getByRole("button", { name: "Registrarse" }).click();
    await aceptarModal(page, /Registro exitoso/i);
    await expect(page).toHaveURL(/\/login/, { timeout: 15_000 });
    await iniciarSesion(page, email, "ClaveE2E_123");

    // El modal aparece; cerrarlo con la X
    await expect(
      page.getByText("¿Cómo es tu experiencia en AutoMarket RD?")
    ).toBeVisible({ timeout: 15_000 });
    await page.getByRole("button", { name: "Cerrar encuesta" }).click();
    await expect(
      page.getByText("¿Cómo es tu experiencia en AutoMarket RD?")
    ).toBeHidden();

    // Misma sesión: sigue oculta incluso recargando (pasado el delay de 2.5s)
    await page.reload();
    await page.waitForTimeout(3200);
    await expect(
      page.getByText("¿Cómo es tu experiencia en AutoMarket RD?")
    ).toBeHidden({ timeout: 1_000 });

    // Nueva sesión (sessionStorage limpio): reaparece porque no la respondió
    await page.evaluate(() => sessionStorage.clear());
    await page.reload();
    await expect(
      page.getByText("¿Cómo es tu experiencia en AutoMarket RD?")
    ).toBeVisible({ timeout: 15_000 });
  });
});
