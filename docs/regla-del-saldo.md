# Regla del saldo del familiar (AC-01)

Respuesta a la acción AC-01 de la solicitud SQA-ACM-01 (10/11/2026). Pendiente de validación de SQA.

## Regla

El saldo del familiar es **lo que debe por cargos médicos** de su interno:

```
saldo = cargos médicos − pagos ligados a una visita
```

- **Cargos médicos:** costo de la consulta, de cada examen con resultado y de cada medicamento despachado. Un costo de Q0.00 (por ejemplo, una medicina donada) no genera cargo.
- **Pagos ligados a una visita:** cobros con concepto «Cargo médico», registrados contra una visita del mismo interno.
- **La cuota mensual no cambia el saldo.** Paga la estadía en el asilo, no los cargos médicos. Se registra como cobro y aparece en el informe 5 (entradas), pero no se resta del saldo.

## Signo y cómo se muestra

| Valor | Significado | Cómo se muestra |
|---|---|---|
| Positivo | El familiar debe | «Q115.00 por pagar», en rojo |
| Cero | No debe nada | «Al día», en gris |
| Negativo | Pagó de más | «Q20.00 a favor», en azul |

En pantalla nunca aparece el signo menos. La pantalla Cobros muestra **una fila por familiar** (cargos, pagado y saldo) con la leyenda, y aparte la lista de cobros, sin repetir el saldo en cada fila.

## Validación nueva

Un pago ligado a una visita no puede ser mayor que lo que falta pagar de esa visita. El servidor responde 400 con el mensaje «El monto supera lo que falta pagar de esa visita (Qx)».

## Por qué cambió

Antes, todos los cobros (también la cuota mensual) bajaban el saldo. Por eso el familiar del ejemplo de SQA aparecía con «Q-385.00» en verde: la cuota de Q500.00 se restaba de los cargos médicos (Q265.00) aunque no los pagaba, y la pantalla no explicaba el signo.

## Ajuste de los datos existentes

Los saldos guardados se recalculan con `base-de-datos/conciliar-saldos.sql`, que muestra el antes y el después y corrige dentro de una transacción solo los familiares que no cuadran. Evidencia en local:

- `pruebas/evidencia-ac/ac-01-ajuste-asilo_capturas.txt`: 3 familiares corregidos; por ejemplo, el familiar 1 pasa de −Q385.00 a Q115.00 (cargos Q265.00 − pago Q150.00).
- `pruebas/evidencia-ac/ac-01-ajuste-asilo_cabeza_algodon.txt`: 1 familiar corregido.

En Azure el script se ejecuta al publicar esta versión, con respaldo previo, y su salida se guarda como evidencia.

## Pruebas

| Prueba | Tipo | Resultado |
|---|---|---|
| `La_cuota_mensual_no_cambia_el_saldo_de_cargos_medicos` | Integración (SQL Server) | Aprobada |
| `Un_pago_ligado_a_la_visita_baja_el_saldo_y_lo_pendiente` | Integración | Aprobada: pendiente Q265.00 → Q115.00 |
| `La_conciliacion_cuadra_con_cuota_y_pago_parcial` | Integración | Aprobada: cargos Q265.00, pagado Q150.00, saldo Q115.00 |
| `Cobro_mayor_a_lo_pendiente_de_la_visita_se_rechaza` | Unitaria | Aprobada |
| `Cobro_igual_a_lo_pendiente_se_acepta` | Unitaria | Aprobada |
| Suite completa (`dotnet test`) | Todas | 220 de 220 |

## Relación con los supuestos

Esta regla concreta los supuestos 3 (descuento ya aplicado en el costo) y 4 («cancelada» = pagada) de `docs/supuestos-para-aprobacion.md`. Queda sujeta a la decisión de esos supuestos (AC-12).
