using AsiloCabezaAlgodon.Application.Comun;

namespace AsiloCabezaAlgodon.Tests
{
    // Valores límite de montos (plan de pruebas SQA, apartado 8.2, y caso FIN-05)
    public class ValidarTests
    {
        [Theory]
        [InlineData("0.01")]
        [InlineData("150.00")]
        [InlineData("99999999.99")]
        public void Monto_valido_se_acepta(string monto)
        {
            Validar.Monto(decimal.Parse(monto), "monto");
        }

        [Theory]
        [InlineData("-0.01")]
        [InlineData("-1")]
        [InlineData("0")]
        [InlineData("100000000.00")]
        [InlineData("10.555")]
        public void Monto_invalido_se_rechaza(string monto)
        {
            Assert.Throws<DatosInvalidosException>(() => Validar.Monto(decimal.Parse(monto), "monto"));
        }

        [Fact]
        public void Costo_medico_de_cero_se_acepta_cuando_se_permite()
        {
            Validar.Monto(0m, "costo", permiteCero: true);
        }

        [Fact]
        public void Costo_negativo_se_rechaza_aunque_se_permita_cero()
        {
            Assert.Throws<DatosInvalidosException>(() => Validar.Monto(-5m, "costo", permiteCero: true));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Texto_vacio_se_rechaza(string? texto)
        {
            Assert.Throws<DatosInvalidosException>(() => Validar.Texto(texto, "nombre"));
        }

        [Theory]
        [InlineData("familiar.ejemplo")]
        [InlineData("sin arroba.com")]
        public void Correo_con_formato_invalido_se_rechaza(string correo)
        {
            Assert.Throws<DatosInvalidosException>(() => Validar.Correo(correo, "correo"));
        }

        [Fact]
        public void Correo_valido_se_acepta()
        {
            Validar.Correo("familiar.a@ejemplo.test", "correo");
        }

        [Fact]
        public void Fecha_futura_se_rechaza_y_hoy_se_acepta()
        {
            Validar.FechaNoFutura(DateTime.Today, "fecha");
            Assert.Throws<DatosInvalidosException>(() => Validar.FechaNoFutura(DateTime.Today.AddDays(1), "fecha"));
        }
    }
}
