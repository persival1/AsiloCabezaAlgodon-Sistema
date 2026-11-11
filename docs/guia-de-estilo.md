# Guía breve de estilo de la interfaz (AC-13, punto 4)

Reglas que siguen todas las pantallas. Los valores están en `AsiloCabezaAlgodon.Web/src/index.css`, definidos una sola vez como variables (`:root`) para el modo claro y el oscuro.

## 1. Tipografía

| Uso | Fuente | Tamaño |
|---|---|---|
| Títulos de pantalla (`h1`) y de sección (`h2`) | Fraunces (`--fuente-titulo`) | 24 px y 15 px |
| Texto, tablas, botones y formularios | Plus Jakarta Sans (`--fuente-texto`) | 13 a 14 px |
| Etiquetas de tabla, grupos del menú y períodos | Plus Jakarta Sans, en mayúsculas | 11 a 12 px |
| Valor de un indicador | Fraunces | 30 px |

## 2. Colores y su significado

| Variable | Significado | Ejemplos |
|---|---|---|
| `--color-primario` | Acción principal y enlaces | «Guardar», «Abrir →», «Ver todo» |
| `--color-alerta` | Algo espera a alguien | Estado Pendiente; borde de un indicador con pendientes |
| `--color-exito` | Terminado | Atendida, Pagada, Despachado, Resultado registrado |
| `--color-info` | En curso, o saldo a favor | Asignada; saldo «A favor» |
| `--color-peligro` | Error, deuda o cancelación | Mensaje junto al campo; saldo «Por pagar»; Cancelada |
| `--color-texto-suave` | Información secundaria | Subtítulos, notas, período del indicador |

No se usan colores fuera de estas variables, así el modo oscuro funciona igual en todas las pantallas.

## 3. Estados (leyenda)

El componente `Badge` es el único que pinta estados. Siempre se muestra con texto, nunca solo con color.

| Estado | Color | Dónde aparece |
|---|---|---|
| Pendiente | Alerta (ámbar) | Solicitudes sin asignar, exámenes sin resultado, prescripciones sin despachar |
| Asignada | Info (azul) | Solicitudes con especialista y horario |
| Atendida | Éxito (verde) | Solicitudes con la visita registrada |
| Cancelada | Peligro (rojo) | Solicitudes canceladas |
| Pendiente de pago | Alerta (ámbar) | Visitas con cargos sin pagar completos (solo Administrador) |
| Pagada | Éxito (verde) | Visitas pagadas completas |
| Despachado / Resultado registrado | Éxito (verde) | Prescripciones y exámenes terminados |
| Sin cargos | Neutro (gris) | Visitas sin costo |

**Saldo del familiar** (AC-01): positivo = **Por pagar** (rojo), 0 = **Al día** (gris), negativo = **A favor** (azul). La leyenda está arriba de la tabla de Cobros.

## 4. Espaciado y forma

- Bordes redondeados: 6 px (botones y campos), 10 px (tarjetas y tablas), 18 px (banner de bienvenida).
- Separación entre tarjetas: 16 px. Entre secciones: 28 px. Margen de la pantalla: 32 px en escritorio, 24 px en tableta y 16 px en celular.
- Sombra suave (`--sombra-sm`) en tarjetas; sombra media solo en el banner.

## 5. Componentes que se repiten

| Componente | Regla |
|---|---|
| Encabezado de pantalla | `h1` + subtítulo de una línea; el botón principal a la derecha |
| Indicadores del inicio | Período arriba, valor, qué mide y «Abrir →». Toda la tarjeta es el enlace. Borde ámbar si hay algo pendiente |
| Listas | Arriba: «Buscar», «Estado» y rango de fechas. Abajo: total y paginación de 10 en 10. Mensaje distinto si la lista está vacía o si la búsqueda no encontró nada |
| Formularios | Asterisco rojo en los campos obligatorios y la nota «Los campos con * son obligatorios». El error se muestra debajo del campo, en español, antes de enviar. El error del servidor se muestra arriba del botón |
| Menú | Inicio, y luego grupos por área: Atención médica, Caja, Reportes (Laboratorio y Farmacia tienen su propio grupo) |

## 6. Tamaños de pantalla

| Ancho | Comportamiento |
|---|---|
| Más de 1100 px (escritorio) | Menú lateral de 240 px |
| 769 a 1100 px (tableta horizontal) | Menú lateral de 200 px |
| Hasta 768 px (tableta vertical y celular) | El menú pasa arriba; los campos se apilan; las tablas se deslizan de lado |
