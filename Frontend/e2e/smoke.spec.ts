import { expect, test } from "@playwright/test";

// Smoke pack E2E contra el stack de docker-compose.e2e.yml.
// El seeder crea el admin (admin@e2e.local / AdminE2e_123) y los planes.

test.describe("Público", () => {
  test("Home renderiza la vitrina", async ({ page }) => {
    await page.goto("/");

    await expect(page).toHaveTitle(/AutoMarket/i);
    // La página pública carga su contenido sin pantalla blanca
    await expect(page.locator("body")).not.toHaveText("");
  });

  test("/vehiculos carga el catálogo sin errores", async ({ page }) => {
    await page.goto("/vehiculos");

    await expect(page.getByRole("heading")).toBeVisible();
  });

  test("/precios muestra los planes sembrados", async ({ page }) => {
    await page.goto("/precios");

    await expect(page.getByText(/gratis|básico|pro/i).first()).toBeVisible();
  });

  test("Ruta desconocida muestra la 404 con enlace al inicio", async ({ page }) => {
    await page.goto("/esta-ruta-no-existe");

    await expect(page.getByRole("heading", { name: "Página no encontrada" })).toBeVisible();

    const volver = page.getByRole("link", { name: "Volver al inicio" });
    await expect(volver).toBeVisible();
    await volver.click();
    await expect(page).toHaveURL(/\/$/);
  });
});

test.describe("Autenticación", () => {
  test("Login del admin sembrado entra al panel /admin", async ({ page }) => {
    await page.goto("/login");

    await page.locator("#loginEmail").fill("admin@e2e.local");
    await page.locator("#loginPassword").fill("AdminE2e_123");
    await page.getByRole("button", { name: "Iniciar Sesión" }).click();

    await expect(page).toHaveURL(/\/admin/, { timeout: 15_000 });
  });

  test("Registro de comprador termina en /login con mensaje de éxito", async ({ page }, testInfo) => {
    const email = `e2e-${Date.now()}-${testInfo.retry}@test.local`;

    await page.goto("/registro");

    await page.locator("#nombre").fill("E2E");
    await page.locator("#apellido").fill("Smoke");
    await page.locator("#email").fill(email);
    await page.locator("#password").fill("ClaveE2E_123");
    await page.locator("#confirmarPassword").fill("ClaveE2E_123");
    // El rol por defecto es Comprador; no requiere campos de agencia
    await page.getByRole("button", { name: "Registrarse" }).click();

    // Éxito: modal de confirmación (requiere clic en OK) y redirección al login
    await expect(page.getByText(/Registro exitoso/i)).toBeVisible({ timeout: 15_000 });
    await page.getByRole("button", { name: /^OK$/i }).click();
    await expect(page).toHaveURL(/\/login/, { timeout: 15_000 });

    // El usuario creado puede iniciar sesión de inmediato
    await page.locator("#loginEmail").fill(email);
    await page.locator("#loginPassword").fill("ClaveE2E_123");
    await page.getByRole("button", { name: "Iniciar Sesión" }).click();
    await expect(page).not.toHaveURL(/login/, { timeout: 15_000 });
  });
});
