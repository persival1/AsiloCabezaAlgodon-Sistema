# Resultados: semana 4, datos inesperados (ROB-05) y límite de intentos de inicio de sesión

Fecha: 05/11/2026 23:24 · API: https://localhost:7022/api

| Caso | Esperado | Obtenido | Resultado | Detalle |
|---|---|---|---|---|
| ROB-05 Cobros: JSON mal escrito | 400 | 400 | OK | Los datos enviados no tienen el formato correcto. Revisá los campos e intentá de nuevo. |
| ROB-05 Cobros: texto en el monto | 400 | 400 | OK | Los datos enviados no tienen el formato correcto. Revisá los campos e intentá de nuevo. |
| ROB-05 Cobros: cuerpo vacío | 400 | 400 | OK | Los datos enviados no tienen el formato correcto. Revisá los campos e intentá de nuevo. |
| ROB-05 Internos: JSON mal escrito | 400 | 400 | OK | Los datos enviados no tienen el formato correcto. Revisá los campos e intentá de nuevo. |
| ROB-05 Internos: texto en la fecha | 400 | 400 | OK | Los datos enviados no tienen el formato correcto. Revisá los campos e intentá de nuevo. |
| ROB-05 Internos: cuerpo vacío | 400 | 400 | OK | Los datos enviados no tienen el formato correcto. Revisá los campos e intentá de nuevo. |
| Intento 1 con usuario inexistente | 401 | 401 | OK |  |
| Intento 2 con usuario inexistente | 401 | 401 | OK |  |
| Intento 3 con usuario inexistente | 401 | 401 | OK |  |
| Intento 4 con usuario inexistente | 401 | 401 | OK |  |
| Intento 5 con usuario inexistente | 401 | 401 | OK |  |
| Intento 6 con usuario inexistente | 401 | 401 | OK |  |
| Intento 7 con usuario inexistente | 401 | 401 | OK |  |
| Intento 8 con usuario inexistente | 401 | 401 | OK |  |
| Intento 9 con usuario inexistente | 401 | 401 | OK |  |
| Intento 10 con usuario inexistente | 401 | 401 | OK |  |
| Intento 11 (pasó el límite) | 429 | 429 | OK | Demasiados intentos de inicio de sesión. Esperá un minuto e intentá de nuevo. |
| Login correcto durante el bloqueo | 429 | 429 | OK | el límite es por IP |
| Login correcto después de un minuto | 200 | 200 | OK |  |

**Total: 19 · Correctas: 19 · Fallidas: 0**
