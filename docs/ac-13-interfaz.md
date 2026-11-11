# AC-13: mejoras de la interfaz (sección 5 de SQA-ACM-01)

- **Rama:** `ac-13-interfaz`
- **Commit del código:** `509e457`
- **Entorno de las capturas:** local, base `asilo_capturas` (datos ficticios), 10/11/2026.
- **Capturas:** `pruebas/evidencia-ac/ac-13/`. Los nombres siguen el formato `momento-resolución-pantalla-rol.png`. Las de «antes» son del commit `ec1c413` (AC-10, justo antes de AC-13). Las de escritorio dicen «commit a915aa0» porque ese servidor de Vite se había iniciado antes y no se reinició. Las de tableta y celular se tomaron después, levantando `ec1c413` en una carpeta aparte, y dicen el commit correcto.

## Los diez puntos

| N° | Aspecto | Qué se hizo | Evidencia |
|---|---|---|---|
| 1 | Pantalla principal | Cada rol ve lo que tiene que atender. Cada número dice su período («Al día de hoy» o «En noviembre de 2026») y es un enlace a su módulo. Debajo hay una lista con lo pendiente, empezando por lo más antiguo | `antes-escritorio-inicio-*.png` y `despues-escritorio-inicio-*.png` de los seis roles |
| 2 | Indicadores | Solo los del cuadro de SQA (tabla de abajo). Se quitaron «Internos registrados» y los totales históricos, que no tienen requisito. El borde ámbar marca los que tienen pendientes | `__tests__/indicadores.test.js` (5 pruebas); valores comparados con la base (abajo) |
| 3 | Módulos y menús | Inicio, y luego grupos por área: Atención médica, Caja y Reportes (Laboratorio y Farmacia, su propio grupo). Para el especialista, «Internos» se llama «Mis pacientes», porque solo ve los suyos (AC-04) | Menú visible en todas las capturas `despues-*` |
| 4 | Consistencia visual | Guía de estilo con tipografía, colores, estados, espaciados y componentes. Las listas, filtros, indicadores y formularios nuevos usan los mismos componentes | `docs/guia-de-estilo.md` |
| 5 | Formularios y validaciones | Asterisco rojo en los campos obligatorios y la nota «Los campos con * son obligatorios». Antes de enviar, el error sale debajo del campo, en español, y el foco va al primer campo con error. Al corregir el dato, el mensaje desaparece. Aplica a todos los formularios | `despues-formulario-invalido.png`; `__tests__/validacionCampos.test.js` (4 pruebas) |
| 6 | Tablas, filtros y búsquedas | Internos, solicitudes, visitas y cobros tienen búsqueda (sin importar tildes), filtro por estado, rango de fechas y páginas de 10. Las visitas pasaron de tarjetas largas a una tabla con «Ver detalle» | `despues-filtro-*.png`, `despues-visitas-detalle.png`; `__tests__/tabla.test.js` (8 pruebas) |
| 7 | Estados visuales | Leyenda de estados y la convención del saldo (Por pagar, Al día, A favor) | `docs/guia-de-estilo.md` sección 3; `ac-01-cobros-despues.png` |
| 8 | Funciones por rol | El menú de cada rol coincide con la matriz de AC-09, y la API lo valida igual: 218 casos | `e2e/navegacion.spec.js`; `docs/matriz-actores-y-roles.md`; `PermisosPorRolTests` |
| 9 | Navegación | Prueba automática por rol: abre cada opción del menú, revisa que quede marcada, usa el botón «atrás» del navegador y entra a un módulo desde un indicador, sin errores de JavaScript | `e2e/navegacion.spec.js`: 6 de 6 |
| 10 | Adaptabilidad | Inicio de los seis roles en escritorio (1366 × 768), tableta (768 × 1024) y celular (390 × 844), antes y después | `antes-*` y `despues-*` en las tres resoluciones; `e2e/celular.spec.js` |

## Indicadores y su requisito

| Rol | Indicador | Período | Requisito | Valor en la captura | Valor en la base |
|---|---|---|---|---|---|
| Administrador | Solicitudes por asignar | Al día de hoy | 6 | 1 | 1 solicitud en estado Pendiente |
| Administrador | Visitas pendientes de pago | Al día de hoy | 14 | 2 | 2 visitas con cargos mayores que lo pagado |
| Administrador | Adeudado a la fundación | Al día de hoy | 13 | Q545.00 | Igual que el informe 4 (AC-07) |
| Administrador | Donaciones recibidas | Mes en curso | 15 | Q15800.00 | `SUM(Monto)` de noviembre = 15,800.00 |
| Administrador | Cobros a familiares | Mes en curso | 15 | Q2030.00 | `SUM(Monto)` de noviembre = 2,030.00 |
| Médico general | Mis solicitudes sin asignar, citas programadas | Al día de hoy | 4 y 6 | — | Solo cuenta sus solicitudes (lo filtra la API) |
| Especialista | Citas asignadas por atender | Al día de hoy | 5 a 7 | 0 | Sus citas en estado Asignada |
| Enfermero(a) | Acompañamientos asignados | Al día de hoy | 5 a 7 | — | Sus solicitudes en estado Asignada |
| Laboratorio | Exámenes pendientes de resultado | Al día de hoy | 10 | — | Exámenes en estado Pendiente |
| Farmacia | Prescripciones pendientes de despacho | Al día de hoy | 12 | — | Prescripciones en estado Pendiente |

## Pruebas de búsqueda y filtrado (base `asilo_capturas`)

| Pantalla | Acción | Resultado esperado (consulta a la base) | Resultado observado |
|---|---|---|---|
| Solicitudes | Buscar «mendez» (sin tilde) | 2 solicitudes de Rosa Elvira Méndez | «2 de 5 solicitudes» |
| Solicitudes | Estado = Atendida | 3 | «3 de 5 solicitudes» |
| Cobros | Tipo = Pago de visita médica | 2 de 5 cobros tienen visita | «2 de 5 cobros» |
| Internos (formulario) | Guardar vacío con un correo mal escrito | 11 mensajes junto a los campos, ninguno se guarda | 11 mensajes; al corregir el correo quedan 10 |

## Cambio de seguridad que salió al revisar las visitas (AC-04)

Al pasar las visitas a tabla se encontraron dos problemas en `GET /api/VisitasMedicas`, y ya están corregidos:

1. Con `?idInterno=` el especialista podía ver el historial de un interno que no es su paciente. Ahora recibe 403. Hay casos nuevos en la matriz, que suma 218.
2. El estado de pago y lo pagado de cada visita llegaban a los roles clínicos. Ahora llegan en `null`, y la columna «Pago» solo la ve el Administrador. Prueba: `El_estado_de_pago_de_las_visitas_solo_lo_ve_el_administrador`.

## Resultado de las pruebas

- `dotnet test`: 310 de 310.
- `npm test` (Vitest): 25 de 25.
- `npx playwright test e2e/navegacion.spec.js`: 6 de 6.
- `npm run lint` y `npm run build`: sin errores.

Pendiente: la validación final de SQA (punto 10 del criterio de aceptación).
