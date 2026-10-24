# Pruebas: conciliación de saldos y cierre de sesión por token inválido

Fecha de ejecución: 19/10/2026. Antes de modificar datos se hizo un respaldo verificado de la base (`asilo_cabeza_algodon_20261019_121834.bak`).

## 1. Conciliación de saldos (observación SQA #4 / O-01)

**Qué se agregó:** el reporte `GET /api/Reportes/conciliacion-saldos` (solo Administrador, solo lectura). Recalcula el saldo de cada familiar a partir de sus movimientos (costos médicos menos cobros) y lo compara con el saldo guardado. Además, el script `base-de-datos/conciliar-saldos.sql` corrige los saldos que no coinciden, dentro de una transacción.

**Nota sobre las cifras:** el documento de la Fase de Desarrollo reportaba un saldo de −Q385.00 contra −Q359.50 esperado. Después se despachó un medicamento más en las pruebas (Diclofenaco, Q10.00), y ese cargo **sí** se sumó correctamente. Por eso las cifras de hoy son distintas, pero la diferencia es la misma: Q25.50 del Ibuprofeno.

### Antes de corregir (respuesta del reporte)

| Familiar | Cargos | Cobros | Saldo recalculado | Saldo guardado | Diferencia | ¿Cuadra? |
|---|---:|---:|---:|---:|---:|---|
| Carlos López (interno María López) | Q300.50 | Q650.00 | −Q349.50 | −Q375.00 | −Q25.50 | No |

Cargos = consultas Q150.00 + exámenes Q75.00 + medicamentos Q75.50 (Omeprazol Q40.00, Ibuprofeno Q25.50, Diclofenaco Q10.00).

### Corrección (`conciliar-saldos.sql`)

```
--- ANTES de la corrección ---
IdFamiliar  SaldoGuardado  SaldoCorrecto  Diferencia
1           -375.00        -349.50        -25.50
Familiares corregidos: 1
--- DESPUÉS de la corrección ---
IdFamiliar  SaldoGuardado  SaldoCorrecto  Resultado
1           -349.50        -349.50        CUADRA
```

### Después de corregir (respuesta del reporte)

| Familiar | Saldo recalculado | Saldo guardado | Diferencia | ¿Cuadra? |
|---|---:|---:|---:|---|
| Carlos López | −Q349.50 | −Q349.50 | Q0.00 | Sí |

### Permisos del reporte nuevo

| Actor | Esperado | Obtenido | Resultado |
|---|---|---|---|
| Administrador | 200 | 200 | OK |
| MedicoGeneral | 403 | 403 | OK |
| Especialista | 403 | 403 | OK |
| Enfermero | 403 | 403 | OK |
| Laboratorio | 403 | 403 | OK |
| Farmacia | 403 | 403 | OK |
| Sin sesión | 401 | 401 | OK |

## 2. Cierre de sesión cuando el token ya no sirve (hallazgo de la revisión)

**Qué pasaba:** si el token vencía (8 horas) o estaba firmado con otra llave (por ejemplo, después de cambiar la llave del JWT), la API respondía 401 pero la pantalla se quedaba con errores en segundo plano, sin avisar.

**Qué se cambió:** `api.js` detecta las respuestas 401, borra la sesión y vuelve al login, donde se muestra el aviso «Tu sesión expiró o ya no es válida». El login mismo se excluye, para que una contraseña incorrecta siga mostrando su propio mensaje.

**Prueba automática en el navegador (4 casos):**

| Caso | Resultado esperado | Resultado |
|---|---|---|
| Contraseña incorrecta | Se queda en el login y muestra «Usuario o contraseña incorrectos.» | OK |
| Token inválido al abrir `/dashboard` | Vuelve al login, muestra el aviso y borra el token | OK |
| Recargar el login después del aviso | El aviso ya no aparece (se muestra una sola vez) | OK |
| Sesión válida | Entra al dashboard y se queda ahí | OK |

## 3. Regresión de permisos

Después de estos cambios se volvió a ejecutar `pruebas/probar-permisos.ps1`: **142 de 142 pruebas correctas** (ver `resultados-permisos.md`).
