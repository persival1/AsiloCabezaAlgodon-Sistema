import { describirSaldo } from "../utilidades/saldo";

// Muestra un saldo con la convención de AC-01 (por pagar, al día o a favor).
export function Saldo({ valor }) {
  const { texto, clase } = describirSaldo(valor);
  return <span className={clase}>{texto}</span>;
}
