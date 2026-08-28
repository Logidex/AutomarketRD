// Captura de pantalla del tour de dealer contra el stack E2E local.
// Uso: node scripts/captura-tour.mjs  (desde Frontend/, stack e2e levantado)
import { chromium } from "@playwright/test";

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1280, height: 800 } });

const email = `tour-${Date.now()}@test.local`;
await page.goto("http://localhost:5173/registro");
await page.locator("#nombre").fill("Tour");
await page.locator("#apellido").fill("Captura");
await page.locator("#email").fill(email);
await page.locator("#password").fill("ClaveE2E_123");
await page.locator("#confirmarPassword").fill("ClaveE2E_123");
await page.locator("#telefonoPersonal").fill("809-555-0000");
await page.locator("#rol").selectOption("Dealer");
await page.locator("#nombreAgencia").fill("Agencia Tour");
await page.locator("#agenciaRNC").fill("1-30-77777-7");
await page.locator("#ubicacionAgencia").fill("Santiago");
await page.locator("#telefonoAgencia").fill("809-555-7777");
await page.getByRole("button", { name: "Registrarse" }).click();
await page.getByText(/Registro exitoso/i).waitFor({ timeout: 15000 });
await page.getByRole("button", { name: /^OK$/i }).click();
await page.waitForURL(/\/suscripcion/, { timeout: 15000 });

// Ir al panel: el tour arranca solo
await page.goto("http://localhost:5173/dashboard");
await page.getByText("¡Bienvenido a tu panel!").waitFor({ timeout: 10000 });
await page.waitForTimeout(600);
await page.screenshot({ path: process.env.TEMP + "\\opencode\\tour-1.png" });

// Avanzar hasta el paso de Mi Perfil (paso 6)
for (let i = 0; i < 5; i++) {
  await page.getByRole("button", { name: "Siguiente" }).click();
  await page.waitForTimeout(400);
}
await page.screenshot({ path: process.env.TEMP + "\\opencode\\tour-2.png" });

await browser.close();
console.log("Capturas guardadas: tour-1.png, tour-2.png");
