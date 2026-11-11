# Pagos a la fundación (AC-07)

Respuesta a la acción AC-07 de la solicitud SQA-ACM-01 (10/11/2026). Atiende los requerimientos 13 y 21 del enunciado («registrar el costo total que se le debe a la fundación» y «reporte de pagos a la fundación») y reemplaza el supuesto 7, que decía que esta parte no estaba construida. Pendiente de validación de SQA.

## Qué se agregó

| Parte | Detalle |
|---|---|
| Tabla nueva | `PagosFundacion` (fecha, monto, número de comprobante y descripción). Migración `AgregarPagosFundacion`; script para una base existente: `base-de-datos/migracion-ac-07-pagos-fundacion.sql` (solo crea la tabla, no toca datos) |
| API | `GET /api/PagosFundacion` y `POST /api/PagosFundacion`, solo para Administrador |
| Pantalla | «Pagos a la fundación» en el menú del Administrador: tarjetas con lo adeudado, lo pagado y lo pendiente; formulario y lista de pagos |
| Informe 4 | Ahora muestra, para el periodo elegido, lo adeudado por consultas, laboratorio y farmacia, lo pagado y la lista de pagos; y el saldo con la fundación hasta hoy |

## Regla

```
pendiente con la fundación = cargos médicos acumulados − pagos registrados
```

Los cargos médicos son los mismos que se cargan a los familiares (consultas, exámenes y medicamentos con costo, ya con el descuento de la fundación).

Validaciones en el servidor: el monto debe ser mayor que cero, con máximo dos decimales; el número de comprobante es obligatorio; y **no se puede pagar más de lo pendiente** (400 con el mensaje «El monto supera lo que se le debe a la fundación (Qx)»).

## Pruebas

| Prueba | Tipo | Resultado |
|---|---|---|
| `Un_pago_valido_se_registra` | Unitaria | Aprobada |
| `No_se_puede_pagar_mas_de_lo_adeudado` | Unitaria | Aprobada: el mensaje indica Q395.00 pendientes |
| `Un_monto_cero_o_negativo_se_rechaza` (0 y −10) | Unitaria | Aprobada |
| `El_comprobante_es_obligatorio` | Unitaria | Aprobada |
| `El_informe_4_separa_lo_pagado_de_lo_pendiente` | Unitaria | Aprobada: adeudado Q595.00, pagado Q200.00, pendiente Q395.00 |
| Permisos de `/PagosFundacion` (13 casos: lectura con los 6 roles y sin sesión, escritura con los 5 roles sin permiso y sin sesión) | Integración | Aprobados |
| Suite completa (`dotnet test`) | Todas | 239 de 239 |
| Recorrido en el navegador (base de demostración) | Manual con evidencia | Pendiente antes Q745.00; un pago de Q99,999.00 se rechaza con el mensaje del servidor; un pago de Q200.00 deja el pendiente en Q545.00; el informe 4 lo refleja |

Capturas en `pruebas/evidencia-ac/`: `ac-07-pagos-antes.png`, `ac-07-pago-de-mas-rechazado.png`, `ac-07-pagos-despues.png` y `ac-07-informe-4.png`.

## En Azure

Para publicar esta versión hay que aplicar antes `migracion-ac-07-pagos-fundacion.sql` en la base de Azure, con respaldo previo. Se hace junto con la publicación y con autorización.
