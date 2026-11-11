// AC-13 (punto 5): validación antes de enviar, con el mensaje junto al campo.
//
// Los formularios ya usan las reglas de HTML (required, type="email", min, max, step).
// Aquí se cambia la burbuja del navegador por un mensaje en español debajo de cada campo:
// se escucha el evento "invalid" de todo el documento, se guarda el texto en el atributo
// data-error del <div className="campo"> y el CSS lo muestra (.campo[data-error]::after).
// Se usa un atributo y no un elemento nuevo para no tocar los nodos que maneja React.

export function mensajeDeValidacion(campo) {
  const v = campo.validity;
  if (v.valueMissing) return campo.tagName === "SELECT" ? "Elegí una opción." : "Este campo es obligatorio.";
  if (v.typeMismatch && campo.type === "email") return "Escribí un correo válido, por ejemplo nombre@correo.com.";
  if (v.badInput) return "Escribí un número válido.";
  if (v.rangeUnderflow) return `El valor mínimo es ${campo.min}.`;
  if (v.rangeOverflow) return `El valor máximo es ${campo.max}.`;
  if (v.stepMismatch) return "Usá como máximo dos decimales.";
  if (v.tooShort) return `Escribí al menos ${campo.minLength} caracteres.`;
  return campo.validationMessage;
}

function contenedor(campo) {
  return campo.closest?.(".campo");
}

export function instalarValidacionDeCampos(documento = document) {
  let primero = true;

  documento.addEventListener("invalid", (evento) => {
    const campo = evento.target;
    const caja = contenedor(campo);
    if (!caja) return;
    evento.preventDefault(); // sin la burbuja del navegador
    caja.dataset.error = mensajeDeValidacion(campo);
    campo.setAttribute("aria-invalid", "true");
    // el foco va al primer campo con error de este intento de envío
    if (primero) {
      primero = false;
      campo.focus();
      queueMicrotask(() => { primero = true; });
    }
  }, true);

  const revisar = (evento) => {
    const campo = evento.target;
    const caja = contenedor(campo);
    if (!caja || !("validity" in campo) || caja.dataset.error === undefined) return;
    if (campo.validity.valid) {
      delete caja.dataset.error;
      campo.removeAttribute("aria-invalid");
    } else {
      caja.dataset.error = mensajeDeValidacion(campo);
    }
  };
  documento.addEventListener("input", revisar, true);
  documento.addEventListener("change", revisar, true);
}
