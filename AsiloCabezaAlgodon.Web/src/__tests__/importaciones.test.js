// PU-FE-08: compatibilidad con sistemas de archivos que distinguen mayúsculas (Linux)
// Prueba escrita por SQA (plan de pruebas, Anexo A) e incorporada al repositorio.
// Windows no distingue "servicios" de "Servicios", pero Linux sí: esta prueba lo detecta.
import { describe, it, expect } from "vitest";
import fs from "node:fs";
import path from "node:path";

const raiz = path.resolve(__dirname, "..");
const archivos = [];
(function recorrer(dir) {
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    const p = path.join(dir, e.name);
    if (e.isDirectory() && e.name !== "__tests__") recorrer(p);
    else if (/\.(js|jsx)$/.test(e.name)) archivos.push(p);
  }
})(raiz);

function existeConMayusculasExactas(ruta) {
  const partes = path.relative(raiz, ruta).split(path.sep);
  let actual = raiz;
  for (const parte of partes) {
    if (!fs.existsSync(actual) || !fs.readdirSync(actual).includes(parte)) return false;
    actual = path.join(actual, parte);
  }
  return true;
}

describe("Importaciones relativas", () => {
  it("PU-FE-08: toda importación relativa coincide exactamente (incluidas mayúsculas) con un archivo", () => {
    const fallas = [];
    for (const archivo of archivos) {
      const texto = fs.readFileSync(archivo, "utf8");
      for (const m of texto.matchAll(/from\s+["'](\.{1,2}\/[^"']+)["']/g)) {
        const base = path.resolve(path.dirname(archivo), m[1]);
        const candidatos = [base, base + ".js", base + ".jsx"];
        if (!candidatos.some(existeConMayusculasExactas)) fallas.push(`${path.relative(raiz, archivo)} -> ${m[1]}`);
      }
    }
    expect(fallas).toEqual([]);
  });
});
