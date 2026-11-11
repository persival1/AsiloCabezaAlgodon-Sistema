// AC-01: convención única para mostrar el saldo de cargos médicos de un familiar.
//   saldo = cargos médicos - pagos ligados a una visita (la cuota mensual no cuenta)
//   positivo = el familiar debe · 0 = al día · negativo = pagó de más (saldo a favor)
// Nunca se muestra un signo menos: se dice con palabras qué significa el número.
export function describirSaldo(saldo) {
  const valor = Number(saldo) || 0;
  if (valor > 0) return { texto: `Q${valor.toFixed(2)} por pagar`, clase: "saldo-debe" };
  if (valor < 0) return { texto: `Q${(-valor).toFixed(2)} a favor`, clase: "saldo-favor" };
  return { texto: "Al día", clase: "saldo-al-dia" };
}
