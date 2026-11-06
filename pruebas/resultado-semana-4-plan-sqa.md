# Semana 4 del plan de pruebas de SQA (03/11/2026)

Trabajo hecho y probado **en local**. Todavía **no está publicado en Azure**.

## 1. Transacción en las operaciones compuestas (ROB-03)

**Problema:** registrar una visita hace tres guardados seguidos: la visita, el cargo al familiar y el cambio de estado de la solicitud. Despachar una prescripción y registrar un resultado de laboratorio hacen dos (el registro y el cargo). Si el servidor fallaba entre un guardado y otro, quedaban cambios a medias, por ejemplo una visita guardada con la solicitud todavía Asignada.

**Arreglo:**

- Nueva interfaz `IUnidadDeTrabajo` en Application, implementada en Infrastructure por `UnidadDeTrabajo`. Ejecuta una operación dentro de una transacción de base de datos: se guardan todos los cambios o ninguno.
- La transacción se abre dentro de `CreateExecutionStrategy()`, que es lo que exige el reintento automático de la base (`EnableRetryOnFailure`, agregado el 30/10).
- `RegistrarVisitaCommandHandler`, `DespacharPrescripcionCommandHandler` y `RegistrarResultadoCommandHandler` hacen todos sus guardados dentro de la transacción. Las validaciones siguen antes, fuera de ella.

**Pruebas:**

| Prueba | Tipo | Antes del arreglo | Después |
|---|---|---|---|
| `Si_falla_el_ultimo_guardado_no_queda_visita_ni_cargo_ni_cambio_de_estado` | Integración, SQL Server real | **Falla**: quedaba 1 visita guardada | Pasa: 0 visitas, saldo Q0, solicitud Asignada |
| `Sin_fallas_se_guarda_la_visita_el_cargo_y_el_estado` | Integración, SQL Server real | Pasa | Pasa: 1 visita, saldo Q150, solicitud Atendida |
| `..._se_guardan_dentro_de_la_transaccion` (visita, despacho, resultado) | Unitaria | — | Pasan: si la transacción no se ejecuta, no se guarda nada por fuera |

Las pruebas de integración usan una base aparte (`asilo_pruebas_integracion`, en SQL Express local) que se borra y se vuelve a crear en cada prueba; nunca tocan la base del sistema. La falla se simula con un repositorio de solicitudes que lanza un error en el último guardado, igual que pide el caso ROB-03.

**En vivo (API local):** la semana 3 completa pasó 11 de 11 con la visita dentro de la transacción. Un despacho (Q12.50) y un resultado de laboratorio (Q30.00) respondieron 200, y la conciliación de saldos cuadra.

## 2. Límite de intentos de inicio de sesión

**Problema:** se podían probar contraseñas sin límite.

**Arreglo:** se usa el limitador que trae ASP.NET Core (`AddRateLimiter`, política `login` en `AuthController.Login`). Cada dirección IP puede intentar **10 veces por minuto**; el intento 11 recibe **429** con el mensaje «Demasiados intentos de inicio de sesión. Esperá un minuto e intentá de nuevo.». La pantalla de login muestra ese aviso.

Se eligió 10 y no menos para que el script de permisos, que inicia sesión con los 6 roles seguidos, siga funcionando. Si se corren varios scripts seguidos, hay que esperar un minuto entre ellos.

**Pendiente de verificar en Azure:** que el servidor vea la IP real de cada usuario y no la del balanceador de Azure. Si viera la del balanceador, el límite sería compartido entre todos los usuarios.

## 3. Datos inesperados en la API (ROB-05)

**Problema encontrado:** con JSON mal escrito, texto en un campo numérico o un cuerpo vacío, la API respondía 400, pero con un mensaje en inglés que mostraba nombres de clases internas (por ejemplo, «could not be converted to AsiloCabezaAlgodon.Application.Caja.Cobros.CrearCobroCommand»).

**Arreglo:** `InvalidModelStateResponseFactory` en `Program.cs`. Ahora responde 400 con el mismo formato que los demás errores: `{ "mensaje": "Los datos enviados no tienen el formato correcto. Revisá los campos e intentá de nuevo." }`.

## 4. Pipeline de integración continua (Anexo C)

Archivo `.github/workflows/calidad.yml`. En cada push y pull request, GitHub compila y prueba en Linux:

- **Backend:** restore, build, `dotnet test` y paquetes vulnerables.
- **Frontend:** `npm ci`, lint, pruebas, build y `npm audit --omit=dev --audit-level=high`.

Diferencia con la propuesta del Anexo C: las pruebas de integración necesitan SQL Server, que no hay en el servidor de GitHub, así que se excluyen con `--filter "Tipo!=Integracion"`. Se corren en el equipo del desarrollador.

**Verificación:** cada paso se ejecutó en local con resultado correcto (70 pruebas sin las de integración, 0 paquetes vulnerables; lint 0, 8 de 8, build y audit 0). **En GitHub:** la primera ejecución fue el 06/11/2026, sobre el commit `4243429`, y salió en verde (backend y frontend).

**Falta (lo hace el dueño del repositorio en GitHub):** marcarlo como verificación obligatoria, en Settings → Branches → regla para `main` → «Require status checks to pass».

## 5. Usuario de base de datos con permisos mínimos

**Problema:** la aplicación en Azure se conecta con el administrador del servidor SQL, que puede borrar tablas.

**Preparado:** `base-de-datos/usuario-permisos-minimos.sql` crea el usuario `app_asilo` con solo lectura y escritura de datos (`db_datareader` y `db_datawriter`). Se revisó el código: la aplicación nunca cambia la estructura de la base mientras corre (no llama a `Migrate`, `EnsureCreated` ni ejecuta SQL propio), así que no necesita más permisos.

**No aplicado todavía:** hay que crearlo en la base de Azure y cambiar la cadena de conexión del App Service. Se hace junto con la publicación, con aprobación. No se pudo probar en local porque SQL Express solo acepta inicio de sesión de Windows.

## 6. Resultados

| Prueba | Resultado |
|---|---|
| `dotnet test` (todas) | 72 de 72 (67 anteriores + 3 unitarias + 2 de integración); 0 advertencias |
| `pruebas/probar-semana-4.ps1` (ROB-05 y límite de intentos) | 19 de 19 |
| Semana 3 | 11 de 11, conciliación cuadra |
| Permisos | 142 de 142 |
| Validaciones (semana 2) | 20 de 20, datos sin cambios |
| Frontend | lint 0, 8 de 8 |

## 7. Lo que no se hizo de este plan

- **Fase 3 del plan (pruebas de integración de la API con `WebApplicationFactory`):** no se hizo. Los permisos se siguen probando con el script de PowerShell (142 casos). Las pruebas de integración se empezaron por ROB-03, con los repositorios reales.
- **ROB-01, ROB-02, ROB-04 y ROB-06, y los casos REP y FIN de esta semana:** el plan los asigna al analista SQA.
