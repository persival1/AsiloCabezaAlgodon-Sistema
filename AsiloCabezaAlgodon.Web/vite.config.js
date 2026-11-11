import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'
import { execSync } from 'node:child_process'
import { readFileSync } from 'node:fs'
import process from 'node:process'

// AC-15: la versión (package.json) y el commit quedan dentro del frontend compilado, para
// comprobar que lo publicado es exactamente el código revisado. El script de despliegue
// pasa VITE_COMMIT; si no viene, se le pregunta a git; si git no está, queda "local".
const paquete = JSON.parse(readFileSync(new URL('./package.json', import.meta.url), 'utf8'))
function commitActual() {
  if (process.env.VITE_COMMIT) return process.env.VITE_COMMIT
  try {
    return execSync('git rev-parse --short HEAD', { stdio: ['ignore', 'pipe', 'ignore'] }).toString().trim()
  } catch {
    return 'local'
  }
}
process.env.VITE_VERSION = paquete.version
process.env.VITE_COMMIT = commitActual()

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
})
