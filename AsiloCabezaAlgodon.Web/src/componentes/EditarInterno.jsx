import { fechaCorta } from "../utilidades/tabla";
import { useState } from "react";
import { editarInterno } from "../Servicios/internosServicio";
import { mensajeDeError } from "../Servicios/api";

// AC-08, CU-006: formulario para corregir los datos del interno y de su familiar.
// La cuota mensual solo aparece si el usuario la puede ver (Administrador).
const soloFecha = (f) => (f ? String(f).slice(0, 10) : "");
// AC-16: fecha local (toISOString da la fecha UTC: después de las 18:00 en Guatemala ya es "mañana")
const HOY = fechaCorta(new Date());

function EditarInterno({ interno, onGuardado, onCancelar }) {
  const fam = interno.familiar || {};
  const veCuota = fam.cuotaMensual != null;
  const [form, setForm] = useState({
    nombres: interno.nombres,
    apellidos: interno.apellidos,
    fechaNacimiento: soloFecha(interno.fechaNacimiento),
    fechaIngreso: soloFecha(interno.fechaIngreso),
    motivoIngreso: interno.motivoIngreso,
    nombreFamiliar: fam.nombres || "",
    apellidoFamiliar: fam.apellidos || "",
    parentesco: fam.parentesco || "",
    telefonoFamiliar: fam.telefono || "",
    correoFamiliar: fam.correo || "",
    cuotaMensual: veCuota ? String(fam.cuotaMensual) : "",
  });
  const [error, setError] = useState("");
  const [guardando, setGuardando] = useState(false);
  const cambiar = (campo, valor) => setForm((f) => ({ ...f, [campo]: valor }));

  async function guardar(evento) {
    evento.preventDefault();
    setError("");
    setGuardando(true);
    try {
      await editarInterno(interno.idInterno, { ...form, cuotaMensual: veCuota ? Number(form.cuotaMensual) : null });
      onGuardado();
    } catch (err) {
      setError(mensajeDeError(err, "No se pudieron guardar los cambios."));
    } finally {
      setGuardando(false);
    }
  }

  const campo = (id, etiqueta, props = {}) => (
    <div className="campo">
      <label htmlFor={`ei-${id}`}>{etiqueta}</label>
      <input id={`ei-${id}`} value={form[id]} onChange={(e) => cambiar(id, e.target.value)} required {...props} />
    </div>
  );

  return (
    <form onSubmit={guardar} className="tarjeta-formulario">
      <h2>Editar datos del interno</h2>
      <div className="fila-campos">{campo("nombres", "Nombres")}{campo("apellidos", "Apellidos")}</div>
      <div className="fila-campos">
        {campo("fechaNacimiento", "Fecha de nacimiento", { type: "date", max: HOY })}
        {campo("fechaIngreso", "Fecha de ingreso", { type: "date", min: form.fechaNacimiento || undefined })}
      </div>
      {campo("motivoIngreso", "Motivo de ingreso")}
      <h2>Familiar responsable</h2>
      <div className="fila-campos">{campo("nombreFamiliar", "Nombres")}{campo("apellidoFamiliar", "Apellidos")}</div>
      <div className="fila-campos">{campo("parentesco", "Parentesco")}{campo("telefonoFamiliar", "Teléfono")}</div>
      <div className="fila-campos">
        {campo("correoFamiliar", "Correo (para notificaciones)", { type: "email" })}
        {veCuota && campo("cuotaMensual", "Cuota mensual (Q)", { type: "number", step: "0.01", min: "0" })}
      </div>
      {error && <p className="error">{error}</p>}
      <div style={{ display: "flex", gap: 10 }}>
        <button type="submit" className="boton-primario" disabled={guardando}>{guardando ? "Guardando..." : "Guardar cambios"}</button>
        <button type="button" className="boton-secundario" onClick={onCancelar}>Cancelar</button>
      </div>
    </form>
  );
}

export default EditarInterno;
