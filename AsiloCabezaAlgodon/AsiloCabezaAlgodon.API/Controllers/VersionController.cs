using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsiloCabezaAlgodon.API.Controllers
{
    // AC-15: versión y commit de la API publicada. Es público a propósito: no expone datos,
    // solo permite comprobar que el código evaluado por SQA es el mismo que está en Azure.
    // El commit lo agrega el script de despliegue con -p:SourceRevisionId=<commit>.
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public IActionResult Obtener()
        {
            var informativa = typeof(VersionController).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "sin versión";
            var partes = informativa.Split('+', 2);
            return Ok(new { version = partes[0], commit = partes.Length > 1 ? partes[1] : "local" });
        }
    }
}
