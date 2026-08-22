import { chromium } from "@playwright/test";
import { mkdirSync } from "node:fs";

const BASE = process.env.BASE ?? "http://localhost:5173";
const DIR = "C:/Users/erick/AppData/Local/Temp/opencode/fase4-shots";
mkdirSync(DIR, { recursive: true });

const browser = await chromium.launch();
const ctx = await browser.newContext({ viewport: { width: 375, height: 812 } });
const page = await ctx.newPage();

// 1. Home pública
await page.goto(BASE + "/");
await page.waitForTimeout(800);
await page.screenshot({ path: DIR + "/01-home-375.png" });

// 2. Vehículos
await page.goto(BASE + "/vehiculos");
await page.waitForTimeout(600);
await page.screenshot({ path: DIR + "/02-vehiculos-375.png" });

// 3. Registrar dealer de prueba vía API
const email = `movil-${Date.now()}@test.local`;
const reg = await page.request.post(BASE + "/api/auth/registrar", {
  data: {
    nombre: "Movil",
    apellido: "Test",
    email,
    password: "ClaveMovil123",
    rol: "Dealer",
    telefonoPersonal: "8090000000",
    nombreAgencia: "Agencia Movil",
    agenciaRNC: "123456789",
    ubicacionAgencia: "Santo Domingo",
    telefonoAgencia: "8090000001",
  },
});
console.log("registro:", reg.status());

// 4. Login por UI -> /dashboard
await page.goto(BASE + "/login");
await page.locator("#loginEmail").fill(email);
await page.locator("#loginPassword").fill("ClaveMovil123");
await page.getByRole("button", { name: "Iniciar Sesión" }).click();
await page.waitForURL(/dashboard/, { timeout: 15000 });
await page.waitForTimeout(700);
await page.screenshot({ path: DIR + "/03-dashboard-cerrado.png" });

// 5. Drawer abierto
await page.click('[aria-label="Abrir menú"]');
await page.waitForTimeout(350);
await page.screenshot({ path: DIR + "/04-drawer-abierto.png" });

// 6. Navegar a Mi Inventario desde el drawer (debe cerrarse solo)
await page.getByRole("link", { name: "Mi Inventario" }).click();
await page.waitForTimeout(700);
await page.screenshot({ path: DIR + "/05-inventario-tras-navegar.png" });

// 7. Publicar vehículo (formulario wizard en móvil)
await page.click('[aria-label="Abrir menú"]');
await page.waitForTimeout(300);
await page.getByRole("link", { name: "Publicar Vehículo" }).click();
await page.waitForTimeout(800);
await page.screenshot({ path: DIR + "/06-publicar-375.png" });

await browser.close();
console.log("screenshots listos en", DIR);
