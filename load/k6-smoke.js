import http from "k6/http";
import { check } from "k6";

// Verificación rápida (1 VU, 30s) de que el objetivo responde antes de
// una corrida completa. Útil como sanity check post-despliegue.
//   k6 run -e BASE=http://localhost:8081 load/k6-smoke.js

export const options = {
  vus: 1,
  duration: "30s",
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<500"],
  },
};

const BASE = __ENV.BASE || "http://localhost:8081";

export default function () {
  const planes = http.get(`${BASE}/api/planes`);
  check(planes, { "planes 200": (r) => r.status === 200 });

  const buscar = http.get(`${BASE}/api/anuncios/buscar?pagina=1&tamanoPagina=12`);
  check(buscar, { "buscar 200": (r) => r.status === 200 });
}
