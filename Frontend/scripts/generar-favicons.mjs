// Genera favicon.ico (16/32/48), favicon-196.png y apple-touch-icon.png
// a partir de public/favicon.svg, usando Chromium de Playwright para el
// render (sin dependencias adicionales).
//
// Uso:  node scripts/generar-favicons.mjs   (desde Frontend/)
//
// Google Search solo asocia favicons .ico/.png (>=48px) en la raíz del
// dominio; los SVG suelen ignorarlos en los resultados de búsqueda.

import { readFile, writeFile } from "node:fs/promises";
import { fileURLToPath } from "node:url";
import { chromium } from "@playwright/test";

const publico = fileURLToPath(new URL("../public/", import.meta.url));

const svg = await readFile(new URL("../public/favicon.svg", import.meta.url), "utf8");

async function renderizarPng(browser, lado) {
  const page = await browser.newPage({
    viewport: { width: lado, height: lado },
    deviceScaleFactor: 1,
  });
  const html = `<!doctype html><html><body style="margin:0;padding:0;background:transparent">
    <div style="width:${lado}px;height:${lado}px">${svg.replace("<svg ", `<svg style="width:100%;height:100%" `)}</div>
  </body></html>`;
  await page.setContent(html, { waitUntil: "networkidle" });
  const buffer = await page.screenshot({
    omitBackground: true,
    clip: { x: 0, y: 0, width: lado, height: lado },
  });
  await page.close();
  return buffer;
}

// Contenedor ICO con entradas PNG (válido desde Windows Vista).
function armarIco(pngs) {
  const cabecera = Buffer.alloc(6);
  cabecera.writeUInt16LE(0, 0); // reservado
  cabecera.writeUInt16LE(1, 2); // tipo icono
  cabecera.writeUInt16LE(pngs.length, 4);

  const entradas = [];
  let offset = 6 + 16 * pngs.length;
  for (const { lado, png } of pngs) {
    const e = Buffer.alloc(16);
    e.writeUInt8(lado >= 256 ? 0 : lado, 0); // ancho
    e.writeUInt8(lado >= 256 ? 0 : lado, 1); // alto
    e.writeUInt8(0, 2); // paleta
    e.writeUInt8(0, 3); // reservado
    e.writeUInt16LE(1, 4); // planos de color
    e.writeUInt16LE(32, 6); // bits por pixel
    e.writeUInt32LE(png.length, 8); // tamaño
    e.writeUInt32LE(offset, 12); // offset absoluto
    entradas.push({ e, png });
    offset += png.length;
  }

  return Buffer.concat([
    cabecera,
    ...entradas.map(({ e }) => e),
    ...entradas.map(({ png }) => png),
  ]);
}

const browser = await chromium.launch();

const png16 = await renderizarPng(browser, 16);
const png32 = await renderizarPng(browser, 32);
const png48 = await renderizarPng(browser, 48);
const png180 = await renderizarPng(browser, 180);
const png196 = await renderizarPng(browser, 196);

await browser.close();

await writeFile(new URL("favicon.ico", `file://${publico.split("\\").join("/")}`), armarIco([
  { lado: 16, png: png16 },
  { lado: 32, png: png32 },
  { lado: 48, png: png48 },
]));
await writeFile(new URL("apple-touch-icon.png", `file://${publico.split("\\").join("/")}`), png180);
await writeFile(new URL("favicon-196.png", `file://${publico.split("\\").join("/")}`), png196);

console.log("Favicons generados en public/: favicon.ico (16/32/48), apple-touch-icon.png (180), favicon-196.png (196)");
