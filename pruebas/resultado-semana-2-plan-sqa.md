# Semana 2 del plan de pruebas de SQA

**Fecha:** 26 de octubre de 2026
**Plan de referencia:** «Plan de pruebas para la implementación y validación del sistema», SQA, 25/10/2026.
**Alcance:** lo que el cronograma asigna al desarrollador en la semana 2: corregir los cargos duplicados y validar los datos en el servidor (casos LAB, FAR, FIN-05 a FIN-07, RES-02 y RES-03), con pruebas unitarias del backend (fase 2 de automatización).

## Resumen

| Caso | Problema antes | Ahora | Evidencia |
|---|---|---|---|
| FAR-05 | Despachar dos veces cargaba dos veces el costo al familiar | Solo se despacha si está Pendiente; si no, **409** y el saldo no cambia | xUnit + script |
| LAB-06 | Registrar dos veces el resultado cargaba dos veces el costo | Solo si está Pendiente; si no, **409** | xUnit + script |
| FAR-03, LAB-03 | Costos negativos aceptados | **400**, con mensaje | xUnit + script |
| FIN-05 | Cobros, donaciones y gastos negativos aceptados. Un cobro negativo **aumentaba** la deuda | **400**: negativos, cero, más de dos decimales, montos mayores que Q99,999,999.99 y texto | xUnit + script |
| FIN-06 | Un cobro se podía ligar a la visita de otro interno | **400**: la visita debe ser del interno de ese familiar | xUnit + script |
| RES-02, RES-03 | La API aceptaba un interno sin nombre, correos inválidos, fechas imposibles y cuotas negativas | **400**, con mensaje | xUnit + script |
| FIN-07 | (Doble clic en «Registrar») | Ya estaba cubierto: los botones se desactivan mientras se guarda (`disabled={guardando}`) | Revisión del código; no se probó con un doble clic real para no crear datos |

## Cómo se corrigió

**Dos tipos de error** (`Application/Comun/Excepciones.cs`):
- `DatosInvalidosException`: el dato no es válido → la API responde **400**.
- `ConflictoException`: el dato es válido, pero choca con el estado actual (por ejemplo, ya está despachada) → **409**.

**Reglas en un solo lugar** (`Application/Comun/Validar.cs`): `Monto`, `Texto`, `Correo` y `FechaNoFutura`. Las reglas de montos son la propuesta del plan (apartados 8.1 y 8.2) y quedaron como **supuesto 8** en `docs/supuestos-para-aprobacion.md`, pendiente de aprobar:
- Nunca negativos; máximo Q99,999,999.99 (lo que cabe en `decimal(10,2)`); máximo dos decimales.
- Cobros, donaciones y gastos deben ser mayores que cero.
- Los costos médicos aceptan Q0.00 (algo sin costo, como una medicina donada), que no se carga al familiar.

**Handlers modificados:** `DespacharPrescripcion`, `RegistrarResultado`, `CrearCobro`, `CrearDonacion`, `CrearGasto`, `CrearInterno` y `RegistrarVisita` (en esta solo los datos; las reglas de estado y de especialista asignado son de la semana 3).

**Repositorio de caja:** dos consultas nuevas, `ExisteFamiliarAsync` y `VisitaEsDelFamiliarAsync`, para el caso FIN-06.

**`Program.cs`:** un middleware convierte las dos excepciones en 400 o 409 con el cuerpo `{ "mensaje": "..." }`. Cualquier otro error sigue siendo un 500 sin detalles internos.

**Frontend:** `api.js` tiene la función `mensajeDeError`, y las pantallas Prescripciones, Exámenes, Cobros, Donaciones, Gastos, Internos y Citas muestran el motivo que da el servidor. Antes, Prescripciones, Exámenes, Donaciones y Gastos no mostraban nada si fallaba. Los campos de monto tienen `min`, pero la validación real es la del servidor.

**De paso:** se quitaron tres `using` duplicados que generaban advertencias al compilar (`BDConexion.cs`, `JwtTokenService.cs`). La solución compila con **0 advertencias**.

## Pruebas unitarias del backend (fase 2)

Proyecto nuevo `AsiloCabezaAlgodon.Tests` (xUnit + NSubstitute), agregado a la solución.

| Archivo | Qué prueba |
|---|---|
| `ValidarTests.cs` | Valores límite de montos (−0.01, 0, 0.01, 99,999,999.99, 100,000,000, 10.555), textos vacíos, correos y fechas |
| `DespacharPrescripcionCommandHandlerTests.cs` | FAR-05 (repetido no carga), FAR-03 (costo negativo), FAR-02 (despacho normal), ID inexistente |
| `RegistrarResultadoCommandHandlerTests.cs` | LAB-06, LAB-03 y LAB-02 |
| `CajaCommandHandlerTests.cs` | FIN-05 (cobros, donaciones y gastos), FIN-06, familiar inexistente y cobro válido |
| `CrearInternoCommandHandlerTests.cs` | RES-02 y RES-03 (7 casos inválidos) y RES-01 |
| `RegistrarVisitaCommandHandlerTests.cs` | Costo de consulta negativo y prescripción sin nombre (FAR-03) |

Las pruebas de despacho siguen la propuesta del **Anexo B** de SQA, con un cambio: en lugar de devolver `false`, el handler lanza una excepción, para que la API pueda decir por qué rechazó la operación. Lo que se comprueba es lo mismo: no se carga el costo y no se guarda nada.

**Antes y después**, como pide el plan (una prueba que falla antes y pasa después):

| Momento | Resultado de `dotnet test` |
|---|---|
| Con el código anterior (pruebas escritas primero) | **21 con error**, 22 superadas, 43 en total |
| Después de corregir | **43 superadas**, 0 con error |

## Pruebas contra la API (integración)

Script nuevo `pruebas/probar-validaciones.ps1`: envía 20 peticiones inválidas con la cuenta de Administrador y compara el código HTTP. Al final verifica que **nada cambió** (cantidad de internos, cobros, donaciones y gastos, saldo de cada familiar y conciliación).

Resultado contra la API local: **20 de 20 correctas** y datos sin cambios. Detalle en `pruebas/resultados-validaciones.md`.

> El script solo debe ejecutarse en el entorno local o de pruebas: si alguna validación fallara, el dato inválido sí se guardaría.

## Regresión

| Prueba | Resultado |
|---|---|
| Permisos por rol (`probar-permisos.ps1`, local) | 142 de 142 correctas |
| Frontend: `npm test` / `npm run lint` / `npm run build` | 8 de 8 / 0 problemas / compila |
| Backend: `dotnet build` | 0 advertencias, 0 errores |
| Navegador: interno con fecha de nacimiento futura | La pantalla muestra «La fecha de nacimiento no puede ser una fecha futura.» y no se guarda |

## Pendiente

- **Aprobar el supuesto 8** (reglas de montos).
- **Semana 3:** visita solo del especialista asignado y solo sobre una solicitud Asignada (AUT-08, EXP-07), validación de la asignación (EXP-06) y correo real.
- **Semana 4 (ROB-03):** el despacho y el resultado de laboratorio guardan el cambio de estado y el cargo al familiar en dos pasos. Si el segundo falla, quedarían a medias. Conviene hacerlos en una sola transacción.
- Publicar en Azure cuando estén listas las semanas 1 y 2 (acordado con el desarrollador).
