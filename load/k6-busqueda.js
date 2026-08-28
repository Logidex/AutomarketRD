import http from "k6/http";
import { check, group } from "k6";

// Baseline de carga para la API pública de AutoMarketRD.
// Uso (en el VM, contra localhost, con RateLimiting__GlobalPermitLimit elevado):
//   k6 run -e BASE=http://localhost:8081 load/k6-busqueda.js

export const options = {
  scenarios: {
    busqueda: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        { duration: "1m", target: 20 },
        { duration: "2m", target: 50 },
        { duration: "2m", target: 50 },
        { duration: "30s", target: 0 },
      ],
      gracefulRampDown: "15s",
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],
    "http_req_duration{endpoint:buscar}": ["p(95)<1500"],
    "http_req_duration{endpoint:planes}": ["p(95)<800"],
  },
};

const BASE = __ENV.BASE || "http://localhost:8081";
const PAGINAS = [1, 2, 3];

export default function () {
  group("busqueda publica", () => {
    const pagina = PAGINAS[Math.floor(Math.random() * PAGINAS.length)];
    const res = http.get(
      `${BASE}/api/anuncios/buscar?pagina=${pagina}&tamanoPagina=12`,
      { tags: { endpoint: "buscar" } }
    );

    check(res, {
      "status 200": (r) => r.status === 200,
      "cuerpo con anuncios": (r) => r.body && r.body.length > 10,
    });
  });

  group("catalogo de planes", () => {
    const res = http.get(`${BASE}/api/planes`, {
      tags: { endpoint: "planes" },
    });

    check(res, {
      "status 200": (r) => r.status === 200,
    });
  });
}
