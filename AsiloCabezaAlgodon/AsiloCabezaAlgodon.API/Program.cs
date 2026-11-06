using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Infrastructure.Persistencia;
using AsiloCabezaAlgodon.Infrastructure.Seguridad;
using AsiloCabezaAlgodon.Infrastructure.Servicios;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// CORRECCIÓN (observación SQA #1 - configuración de seguridad):
// La llave con la que se firman los tokens JWT ya NO está escrita en appsettings.json.
//  - En desarrollo se toma de appsettings.Development.json (llave de prueba).
//  - En un servidor real se debe entregar por una variable de entorno llamada
//    Jwt__Key (con dos guiones bajos), así nunca se guarda dentro del repositorio.
// Si no hay llave, o es demasiado corta, la aplicación se niega a arrancar. Es mejor
// fallar al inicio con un mensaje claro que arrancar con una llave insegura o vacía.
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Falta la llave del JWT (Jwt:Key) o tiene menos de 32 caracteres. " +
        "Configurala con la variable de entorno Jwt__Key. " +
        "Ver appsettings.Production.example.json.");
}

// CORREGIDO (observación SQA #8 - despliegue): el origen del frontend que puede llamar a
// la API ya no está fijo en el código. Se lee de la configuración (Cors:Origenes), así en un
// servidor se cambia con un archivo o una variable de entorno (Cors__Origenes__0) sin
// recompilar. Si no está configurado, se usa el del entorno local de desarrollo.
var origenesPermitidos = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>();
if (origenesPermitidos == null || origenesPermitidos.Length == 0)
    origenesPermitidos = new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins(origenesPermitidos)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// La base de Azure (oferta gratis) se pausa sola cuando nadie la usa y tarda en despertar.
// Mientras despierta responde «no disponible» (error 40613) y la API devolvía 500 en la
// primera petición. Con EnableRetryOnFailure, Entity Framework reintenta unas veces antes de
// rendirse. Las transacciones (UnidadDeTrabajo, semana 4) se abren dentro de
// Database.CreateExecutionStrategy(), porque así lo exige este modo.
builder.Services.AddDbContext<BDConexion>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(maxRetryCount: 6, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AsiloCabezaAlgodon.Application.Autenticacion.Comandos.LoginCommand).Assembly));

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IInternoRepository, InternoRepository>();
builder.Services.AddScoped<ISolicitudRepository, SolicitudRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IVisitaMedicaRepository, VisitaMedicaRepository>();
builder.Services.AddScoped<ICajaRepository, CajaRepository>();
builder.Services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Plan de pruebas SQA, semana 4: límite de intentos de inicio de sesión. Sin límite, alguien
// podría probar miles de contraseñas por minuto. Cada dirección IP puede intentar 10 veces
// por minuto; el intento 11 recibe 429 hasta que pase el minuto. (10 y no menos, para que el
// script de permisos, que inicia sesión con los 6 roles seguidos, siga funcionando.)
builder.Services.AddRateLimiter(opciones =>
{
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opciones.AddPolicy("login", contexto => RateLimitPartition.GetFixedWindowLimiter(
        contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    opciones.OnRejected = async (contexto, token) =>
        await contexto.HttpContext.Response.WriteAsJsonAsync(
            new { mensaje = "Demasiados intentos de inicio de sesión. Esperá un minuto e intentá de nuevo." }, token);
});
// Plan de pruebas SQA, caso ROB-05: si llega JSON mal escrito o texto en un campo numérico,
// .NET respondía 400 con un mensaje en inglés que mostraba nombres de clases internas
// (por ejemplo "could not be converted to ...CrearCobroCommand"). Ahora responde 400 con el
// mismo formato que los demás errores: { mensaje } en español y sin detalles del código.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opciones =>
        opciones.InvalidModelStateResponseFactory = _ => new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
            new { mensaje = "Los datos enviados no tienen el formato correcto. Revisá los campos e intentá de nuevo." }));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Plan de pruebas SQA, semana 2: cuando un handler rechaza una operación lanza una de las
// excepciones de Application/Comun/Excepciones.cs. Aquí se convierten en la respuesta HTTP
// correcta, con un mensaje que el frontend puede mostrar: { "mensaje": "..." }.
//  - DatosInvalidosException -> 400 (por ejemplo, un monto negativo)
//  - ConflictoException      -> 409 (por ejemplo, despachar dos veces la misma prescripción)
//  - AccesoDenegadoException -> 403 (por ejemplo, la visita de una solicitud de otro especialista)
// Cualquier otro error sigue siendo un 500 sin detalles internos.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (DatosInvalidosException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { mensaje = ex.Message });
    }
    catch (ConflictoException ex)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await context.Response.WriteAsJsonAsync(new { mensaje = ex.Message });
    }
    catch (AccesoDenegadoException ex)
    {
        // Semana 3 (AUT-08): por ejemplo, registrar la visita de una solicitud de otro especialista.
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new { mensaje = ex.Message });
    }
});

// Archivos del frontend (React ya compilado). En el servidor de Azure, la carpeta wwwroot
// contiene el resultado de "npm run build", así la misma aplicación entrega la página y la
// API en una sola dirección. En desarrollo esa carpeta no existe y estas líneas no hacen
// nada (el frontend corre aparte con "npm run dev").
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("PermitirFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

// Las rutas de React (/dashboard, /dashboard/internos, ...) no son archivos reales: si el
// navegador pide una de ellas (por ejemplo al recargar la página), se entrega index.html y
// React decide qué pantalla mostrar. Las rutas que empiezan con /api no entran aquí: si no
// existen, responden 404 como cualquier API.
app.MapFallback("/api/{**ruta}", () => Results.NotFound());
app.MapFallbackToFile("index.html");

app.Run();

// Permite que las pruebas de integración (WebApplicationFactory<Program>) levanten esta API.
public partial class Program { }