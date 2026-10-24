# Prueba de respaldo y recuperación de la base de datos

Respuesta a la observación 7 de SQA (pruebas de respaldo y recuperación).

- **Fecha de ejecución:** 15/10/2026
- **Servidor:** SQL Server 2022 Express (`localhost\SQLEXPRESS`)
- **Base probada:** `asilo_cabeza_algodon`
- **Script:** [`base-de-datos/respaldo-y-restauracion.ps1`](../base-de-datos/respaldo-y-restauracion.ps1), acción `Probar`
- **Comando:** `./respaldo-y-restauracion.ps1 -Accion Probar`

## Qué hace la prueba

1. **Respalda** la base real con `BACKUP DATABASE ... WITH CHECKSUM` (verifica la integridad de las páginas).
2. **Verifica** que el archivo `.bak` se pueda leer (`RESTORE VERIFYONLY`).
3. **Restaura** el respaldo en una base nueva con otro nombre (`asilo_cabeza_algodon_prueba_restauracion`), de modo que la base original no se toca.
4. **Compara** cuántas filas tiene cada tabla en la original y en la copia restaurada.
5. **Elimina** la base temporal. El archivo `.bak` se conserva.

## Resultado

| Tabla | Original | Copia restaurada | Resultado |
|---|---:|---:|---|
| __EFMigrationsHistory | 7 | 7 | OK |
| Cobros | 2 | 2 | OK |
| Donaciones | 1 | 1 | OK |
| ExamenesSolicitados | 2 | 2 | OK |
| Familiares | 1 | 1 | OK |
| Gastos | 1 | 1 | OK |
| Internos | 1 | 1 | OK |
| MedicamentosCajon | 1 | 1 | OK |
| Prescripciones | 3 | 3 | OK |
| Psicopatologias | 1 | 1 | OK |
| Roles | 6 | 6 | OK |
| Solicitudes | 3 | 3 | OK |
| Usuarios | 6 | 6 | OK |
| VisitasMedicas | 3 | 3 | OK |

**Conclusión:** la recuperación quedó verificada. Las 14 tablas (las 13 del sistema más la de migraciones) tienen las mismas filas en la copia restaurada. La base temporal se eliminó y la original no se modificó.

## Limitaciones (dichas con claridad)

- La prueba compara **cantidad de filas por tabla**, no el contenido campo por campo.
- Se probó en el mismo servidor. Una recuperación en **otro equipo** no se ha probado.
- Se hizo un solo respaldo completo. No hay respaldos automáticos programados ni respaldos incrementales.
- Falta definir una política de respaldo (cada cuánto, dónde se guardan las copias y cuántas se conservan).
