import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { listarInternos, crearInterno } from "../Servicios/internosServicio";
import { useAutenticacion } from "../contexto/AutenticacionContexto";
import Badge from "../componentes/Badge";

const VACIO = {
  nombres: "",
  apellidos: "",
  fechaNacimiento: "",
  fechaIngreso: "",
  motivoIngreso: "",
  nombreFamiliar: "",
  apellidoFamiliar: "",
  parentesco: "",
  telefonoFamiliar: "",
  correoFamiliar: "",
  cuotaMensual: "",
};

function Internos() {
  const { rol } = useAutenticacion();
  const puedeCrear = ["Administrador", "MedicoGeneral"].includes(rol);
  const [internos, setInternos] = useState([]);
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [form, setForm] = useState(VACIO);
  const [error, setError] = useState("");
  const [guardando, setGuardando] = useState(false);

  useEffect(() => {
    cargarInternos();
  }, []);

  async function cargarInternos() {
    const datos = await listarInternos();
    setInternos(datos);
  }

  function manejarCambio(campo, valor) {
    setForm((anterior) => ({ ...anterior, [campo]: valor }));
  }

  async function manejarEnvio(evento) {
    evento.preventDefault();
    setError("");
    setGuardando(true);

    try {
      await crearInterno({
        ...form,
        cuotaMensual: Number(form.cuotaMensual) || 0,
      });
      setForm(VACIO);
      setMostrarFormulario(false);
      await cargarInternos();
    } catch (err) {
      setError("No se pudo registrar el interno. Revisá los datos.");
    } finally {
      setGuardando(false);
    }
  }

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Internos</h1>
          <p className="subtitulo">Ficha básica de cada interno y su familiar responsable</p>
        </div>
        {puedeCrear && (
          <button className="boton-primario" onClick={() => setMostrarFormulario((v) => !v)}>
            {mostrarFormulario ? "Cancelar" : "Nuevo interno"}
          </button>
        )}
      </div>

      {mostrarFormulario && (
        <form onSubmit={manejarEnvio} className="tarjeta-formulario">
          <h2>Datos del interno</h2>
          <div className="fila-campos">
            <div className="campo">
              <label>Nombres</label>
              <input value={form.nombres} onChange={(e) => manejarCambio("nombres", e.target.value)} required />
            </div>
            <div className="campo">
              <label>Apellidos</label>
              <input value={form.apellidos} onChange={(e) => manejarCambio("apellidos", e.target.value)} required />
            </div>
          </div>
          <div className="fila-campos">
            <div className="campo">
              <label>Fecha de nacimiento</label>
              <input type="date" value={form.fechaNacimiento} onChange={(e) => manejarCambio("fechaNacimiento", e.target.value)} required />
            </div>
            <div className="campo">
              <label>Fecha de ingreso</label>
              <input type="date" value={form.fechaIngreso} onChange={(e) => manejarCambio("fechaIngreso", e.target.value)} required />
            </div>
          </div>
          <div className="campo">
            <label>Motivo de ingreso</label>
            <input value={form.motivoIngreso} onChange={(e) => manejarCambio("motivoIngreso", e.target.value)} required />
          </div>

          <h2>Familiar responsable</h2>
          <div className="fila-campos">
            <div className="campo">
              <label>Nombres</label>
              <input value={form.nombreFamiliar} onChange={(e) => manejarCambio("nombreFamiliar", e.target.value)} required />
            </div>
            <div className="campo">
              <label>Apellidos</label>
              <input value={form.apellidoFamiliar} onChange={(e) => manejarCambio("apellidoFamiliar", e.target.value)} required />
            </div>
          </div>
          <div className="fila-campos">
            <div className="campo">
              <label>Parentesco</label>
              <input value={form.parentesco} onChange={(e) => manejarCambio("parentesco", e.target.value)} required />
            </div>
            <div className="campo">
              <label>Teléfono</label>
              <input value={form.telefonoFamiliar} onChange={(e) => manejarCambio("telefonoFamiliar", e.target.value)} required />
            </div>
          </div>
          <div className="fila-campos">
            <div className="campo">
              <label>Correo (para notificaciones)</label>
              <input type="email" value={form.correoFamiliar} onChange={(e) => manejarCambio("correoFamiliar", e.target.value)} required />
            </div>
            <div className="campo">
              <label>Cuota mensual (Q)</label>
              <input type="number" step="0.01" value={form.cuotaMensual} onChange={(e) => manejarCambio("cuotaMensual", e.target.value)} required />
            </div>
          </div>

          {error && <p className="error">{error}</p>}

          <button type="submit" className="boton-primario" disabled={guardando}>
            {guardando ? "Guardando..." : "Guardar interno"}
          </button>
        </form>
      )}

      <table className="tabla">
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Fecha de ingreso</th>
            <th>Motivo</th>
            <th>Familiar</th>
            <th>Estado</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {internos.map((i) => (
            <tr key={i.idInterno}>
              <td>{i.nombres} {i.apellidos}</td>
              <td>{new Date(i.fechaIngreso).toLocaleDateString()}</td>
              <td>{i.motivoIngreso}</td>
              <td>{i.nombreFamiliar || "—"}</td>
              <td><Badge texto={i.estado} /></td>
              <td><Link to={`/dashboard/internos/${i.idInterno}`} className="boton-secundario" style={{ textDecoration: "none", display: "inline-block" }}>Ver ficha</Link></td>
            </tr>
          ))}
          {internos.length === 0 && (
            <tr>
              <td colSpan="6">No hay internos registrados todavía.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default Internos;
