# Matriz de actores, roles y permisos (AC-09 y AC-10)

Respuesta a las acciones AC-09 y AC-10 de la solicitud SQA-ACM-01 (10/11/2026). Pendiente de aprobación de SQA y del docente.

## 1. Origen de la diferencia «ocho actores, seis roles»

El análisis que revisó SQA era el documento «Nueva Vida», un trabajo de Análisis de Sistemas I que no describe este sistema. Ese documento tenía ocho actores, 64 casos de uso y 22 tablas. El análisis vigente es la **Fase de Análisis de Cabeza de Algodón (06/11/2026)**, que describe los 21 casos de uso y las 13 tablas (14 desde AC-07) que sí están implementados.

## 2. Correspondencia entre actores y roles

| Actor (análisis «Nueva Vida») | Rol implementado | Decisión | Justificación con el enunciado |
|---|---|---|---|
| Administrador | Administrador | Implementado | — |
| Médico General | MedicoGeneral | Implementado | «El asilo cuenta con un médico general quien es el primero que lo evalúa» |
| Especialista | Especialista | Implementado | «Médicos con distintas especialidades» de la fundación |
| Coordinador de Agenda (Fundación) | Integrado en Administrador | Integrado (supuesto 6) | El enunciado habla de «un módulo que se le proporcionará a la fundación», pero no define un usuario distinto. La fundación asigna médico y horario con el rol Administrador |
| Enfermero(a) | Enfermero | Implementado | «Asignar el enfermero(a) que acompañará al paciente» |
| Personal de Laboratorio | Laboratorio | Implementado | «El laboratorio podrá tener acceso a la visita médica» |
| Personal de Farmacia | Farmacia | Implementado | «Luego se puede pasar a farmacia a recoger el medicamento» |
| Familiar del Residente | No es usuario del sistema | **Exclusión propuesta** | El enunciado solo pide «enviar un correo electrónico al familiar». En ningún momento indica que el familiar entre al sistema. Recibe la información por correo (CU-11) |
| Director | No existe | **Exclusión propuesta** | El enunciado no menciona un director. Los informes los genera la administración |

Las dos exclusiones necesitan la aprobación del docente o de SQA (acción AC-12).

## 3. Permisos por rol (lo que valida la API)

`✔` = permitido. Todo lo demás responde 403 (con sesión) o 401 (sin sesión). La misma tabla está automatizada en `PermisosPorRolTests` (282 casos desde AC-08) y en `pruebas/probar-permisos.ps1`.

| Función | Adm. | Méd. general | Especialista | Enfermero | Laboratorio | Farmacia |
|---|:-:|:-:|:-:|:-:|:-:|:-:|
| Ver lista de internos | ✔ | ✔ | Solo sus pacientes | | | |
| Ver ficha médica | ✔ | ✔ | Solo sus pacientes | | | |
| Ver cuota y saldo del familiar en la ficha | ✔ | | | | | |
| Registrar interno | ✔ | ✔ | | | | |
| Editar datos del interno (AC-08) | ✔ (con cuota) | ✔ (sin cuota) | | | | |
| Dar de baja o reactivar un interno (AC-08) | ✔ | | | | | |
| Agregar, corregir o quitar psicopatología o medicina de cajón | ✔ | ✔ | Solo sus pacientes | | | |
| Ver solicitudes | Todas | Las que creó | Las asignadas a él | Las que acompaña | | |
| Crear solicitud | | ✔ | | | | |
| Corregir solicitud pendiente (AC-08) | | Las suyas | | | | |
| Cancelar solicitud (AC-08) | Pendiente o con cita | Las suyas, pendientes | | | | |
| Asignar o reprogramar especialista, enfermero(a) y horario | ✔ | | | | | |
| Ver visitas médicas | Todas | ✔ | Las suyas | | | |
| Registrar visita | | | Solo sus citas asignadas | | | |
| Ver exámenes y registrar resultado | ✔ (ver) | | | | ✔ | |
| Ver prescripciones y despachar | ✔ (ver) | | | | | ✔ |
| Donaciones, gastos y cobros | ✔ | | | | | |
| Pagos a la fundación | ✔ | | | | | |
| Informes financieros (1, 3, 4, 5 y conciliación) | ✔ | | | | | |
| Informes clínicos (2, 6 y 7) | ✔ | ✔ | Solo sus pacientes | | | |
| Usuarios: ver, crear, editar, desactivar y asignar contraseña temporal (AC-08, AC-05) | ✔ | | | | | |
| Cambiar la contraseña propia (AC-05) | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Estado de pago de cada visita | ✔ | | | | | |

**«Solo sus pacientes»:** internos que tienen al menos una solicitud asignada a ese especialista. Se aplica en la API (`PuedeVerInternoQuery`), no solo ocultando botones.

## 4. Cambios hechos con esta matriz

| Cambio | Antes | Ahora |
|---|---|---|
| Informes 2, 6 y 7 | Solo Administrador | Administrador, médico general y especialista (este, solo de sus pacientes). El menú «Reportes» aparece a los médicos solo con esos tres informes |
| Lista de internos para el especialista | Todos los internos | Solo sus pacientes |
| Ficha de un interno ajeno, pedida por el especialista | 200 | 403 |
| Cuota y saldo del familiar en la ficha | Visibles para los tres roles médicos | Solo el Administrador; los roles clínicos no reciben datos financieros |
| Laboratorio y farmacia | Sin acceso a la ficha | Igual: siguen sin acceso a la ficha completa |

## 5. Pruebas

- `PermisosPorRolTests`: 204 casos al cerrar AC-10 (282 después de AC-13 y AC-08), incluidos el interno propio (1) y el ajeno (2) del especialista.
- `La_ficha_solo_muestra_finanzas_al_administrador`: el Administrador recibe la cuota; el médico general y el especialista reciben `null`.
- `El_especialista_solo_lista_a_sus_pacientes`.
- `dotnet test`: 293 de 293.
