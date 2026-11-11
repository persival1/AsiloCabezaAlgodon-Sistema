using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Application.Internos.Comandos;
using AsiloCabezaAlgodon.Application.Solicitudes.Comandos;
using AsiloCabezaAlgodon.Application.Usuarios.Comandos;
using AsiloCabezaAlgodon.Domain.Entidad;
using NSubstitute;

namespace AsiloCabezaAlgodon.Tests
{
    // AC-08 (operaciones del cuadro 3.5 de SQA-ACM-01) y AC-05 (contraseñas): cada operación
    // con su caso válido y sus validaciones. Los permisos por rol están en PermisosPorRolTests.
    public class OperacionesAc08Tests
    {
        private readonly IInternoRepository _internos = Substitute.For<IInternoRepository>();
        private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();
        private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
        private readonly IEmailService _correo = Substitute.For<IEmailService>();
        private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();

        private static Interno Interno(int id = 1, string estado = "activo") => new()
        {
            IdInterno = id, Nombres = "Rosa", Apellidos = "Méndez", Estado = estado,
            FechaNacimiento = new DateTime(1945, 5, 1), FechaIngreso = new DateTime(2025, 6, 2), MotivoIngreso = "x",
            Familiar = new Familiar { Nombres = "Ana", Apellidos = "Méndez", Correo = "ana@correo.test", CuotaMensual = 500 }
        };

        private static EditarInternoCommand Edicion(int id = 1, decimal? cuota = 800, bool admin = true, string correo = "nuevo@correo.test") => new(
            id, "Rosa Elvira", "Méndez", new DateTime(1945, 5, 1), new DateTime(2025, 6, 2), "Vive sola",
            "Ana", "Méndez", "Hija", "5555-1234", correo, cuota, admin);

        public OperacionesAc08Tests()
        {
            _hasher.Hashear(Arg.Any<string>()).Returns(l => "hash:" + l.Arg<string>());
            _hasher.Verificar(Arg.Any<string>(), Arg.Any<string>()).Returns(l => l.ArgAt<string>(1) == "hash:" + l.ArgAt<string>(0));
            _usuarios.ObtenerRolAsync(Arg.Any<string>()).Returns(l => new Rol { IdRol = 7, Nombre = l.Arg<string>() });
            _usuarios.CrearAsync(Arg.Any<Usuario>()).Returns(l => { var u = l.Arg<Usuario>(); u.IdUsuario = 50; return u; });
        }

        // ---- CU-006: editar interno ----

        [Fact]
        public async Task Editar_interno_cambia_datos_y_la_cuota_si_es_Administrador()
        {
            var interno = Interno();
            _internos.ObtenerPorIdAsync(1).Returns(interno);
            var ok = await new EditarInternoCommandHandler(_internos).Handle(Edicion(), CancellationToken.None);

            Assert.True(ok);
            Assert.Equal("Rosa Elvira", interno.Nombres);
            Assert.Equal("nuevo@correo.test", interno.Familiar!.Correo);
            Assert.Equal(800, interno.Familiar.CuotaMensual);
            await _internos.Received(1).GuardarCambiosAsync();
        }

        [Fact]
        public async Task Editar_interno_sin_ser_Administrador_no_toca_la_cuota()
        {
            var interno = Interno();
            _internos.ObtenerPorIdAsync(1).Returns(interno);
            await new EditarInternoCommandHandler(_internos).Handle(Edicion(admin: false), CancellationToken.None);
            Assert.Equal(500, interno.Familiar!.CuotaMensual);
        }

        [Fact]
        public async Task Editar_interno_con_correo_invalido_se_rechaza()
        {
            _internos.ObtenerPorIdAsync(1).Returns(Interno());
            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                new EditarInternoCommandHandler(_internos).Handle(Edicion(correo: "no-es-correo"), CancellationToken.None));
            await _internos.DidNotReceive().GuardarCambiosAsync();
        }

        [Fact]
        public async Task Editar_interno_para_que_quede_igual_a_otro_se_rechaza()
        {
            _internos.ObtenerPorIdAsync(1).Returns(Interno());
            _internos.ExisteDuplicadoAsync("Rosa Elvira", "Méndez", Arg.Any<DateTime>(), 1).Returns(true);
            await Assert.ThrowsAsync<ConflictoException>(() =>
                new EditarInternoCommandHandler(_internos).Handle(Edicion(), CancellationToken.None));
        }

        [Fact]
        public async Task Registrar_un_interno_duplicado_se_rechaza()
        {
            _internos.ExisteDuplicadoAsync("Rosa", "Méndez", Arg.Any<DateTime>(), null).Returns(true);
            var comando = new CrearInternoCommand("Rosa", "Méndez", new DateTime(1945, 5, 1), new DateTime(2025, 6, 2), "x",
                "Ana", "Méndez", "Hija", "1", "ana@correo.test", 0);
            await Assert.ThrowsAsync<ConflictoException>(() => new CrearInternoCommandHandler(_internos).Handle(comando, CancellationToken.None));
            await _internos.DidNotReceive().CrearAsync(Arg.Any<Interno>());
        }

        // ---- CU-007: alta y baja ----

        [Fact]
        public async Task Baja_de_un_interno_sin_solicitudes_abiertas()
        {
            var interno = Interno();
            _internos.ObtenerPorIdAsync(1).Returns(interno);
            _solicitudes.ObtenerTodasAsync().Returns(new List<Solicitud> { new() { IdInterno = 1, Estado = "Atendida" } });

            Assert.True(await new CambiarEstadoInternoCommandHandler(_internos, _solicitudes).Handle(new(1, "baja"), CancellationToken.None));
            Assert.Equal("baja", interno.Estado);
        }

        [Fact]
        public async Task Baja_con_una_cita_asignada_se_rechaza()
        {
            _internos.ObtenerPorIdAsync(1).Returns(Interno());
            _solicitudes.ObtenerTodasAsync().Returns(new List<Solicitud> { new() { IdInterno = 1, Estado = "Asignada" } });
            var error = await Assert.ThrowsAsync<ConflictoException>(() =>
                new CambiarEstadoInternoCommandHandler(_internos, _solicitudes).Handle(new(1, "baja"), CancellationToken.None));
            Assert.Contains("Cancelalas", error.Message);
        }

        [Fact]
        public async Task Estado_desconocido_se_rechaza_y_alta_reactiva()
        {
            var interno = Interno(estado: "baja");
            _internos.ObtenerPorIdAsync(1).Returns(interno);
            var handler = new CambiarEstadoInternoCommandHandler(_internos, _solicitudes);
            await Assert.ThrowsAsync<DatosInvalidosException>(() => handler.Handle(new(1, "borrado"), CancellationToken.None));
            Assert.True(await handler.Handle(new(1, "activo"), CancellationToken.None));
            Assert.Equal("activo", interno.Estado);
        }

        [Fact]
        public async Task A_un_interno_de_baja_no_se_le_crean_solicitudes()
        {
            _internos.ObtenerPorIdAsync(1).Returns(Interno(estado: "baja"));
            await Assert.ThrowsAsync<ConflictoException>(() =>
                new CrearSolicitudCommandHandler(_solicitudes, _internos).Handle(new CrearSolicitudCommand(1, "Control", null, 2), CancellationToken.None));
        }

        // ---- CU-009 y CU-010: corregir la ficha ----

        [Fact]
        public async Task Corregir_una_psicopatologia_de_otro_interno_no_se_permite()
        {
            _internos.ObtenerPsicopatologiaAsync(5).Returns(new Psicopatologia { IdPsicopatologia = 5, IdInterno = 2, Nombre = "Ansiedad" });
            var handler = new CorregirFichaCommandHandler(_internos);
            Assert.False(await handler.Handle(new EditarPsicopatologiaCommand(1, 5, "Depresión", DateTime.Today), CancellationToken.None));
            Assert.False(await handler.Handle(new EliminarPsicopatologiaCommand(1, 5), CancellationToken.None));
            await _internos.DidNotReceive().EliminarPsicopatologiaAsync(Arg.Any<Psicopatologia>());
        }

        [Fact]
        public async Task Corregir_y_quitar_una_medicina_de_cajon()
        {
            var m = new MedicamentoCajon { IdMedicamentoCajon = 3, IdInterno = 1, Nombre = "Paracetamol", Dosis = "500 mg", Frecuencia = "c/8h" };
            _internos.ObtenerMedicamentoCajonAsync(3).Returns(m);
            var handler = new CorregirFichaCommandHandler(_internos);

            Assert.True(await handler.Handle(new EditarMedicamentoCajonCommand(1, 3, "Paracetamol", "1 g", "c/12h"), CancellationToken.None));
            Assert.Equal("1 g", m.Dosis);
            await Assert.ThrowsAsync<DatosInvalidosException>(() => handler.Handle(new EditarMedicamentoCajonCommand(1, 3, "Paracetamol", " ", "c/12h"), CancellationToken.None));
            Assert.True(await handler.Handle(new EliminarMedicamentoCajonCommand(1, 3), CancellationToken.None));
            await _internos.Received(1).EliminarMedicamentoCajonAsync(m);
        }

        // ---- CU-013, CU-014 y CU-019: editar y cancelar solicitudes ----

        private Solicitud Solicitud(string estado, int idMedico = 2) => new()
        {
            IdSolicitud = 9, Estado = estado, IdMedicoGeneral = idMedico, Motivo = "Control",
            FechaHoraCita = new DateTime(2026, 12, 1, 10, 0, 0), Interno = Interno()
        };

        [Fact]
        public async Task El_medico_edita_su_solicitud_pendiente()
        {
            var s = Solicitud("Pendiente");
            _solicitudes.ObtenerPorIdAsync(9).Returns(s);
            Assert.True(await new EditarYCancelarSolicitudCommandHandler(_solicitudes, _correo).Handle(new EditarSolicitudCommand(9, "Dolor de rodilla", "Traumatología", 2), CancellationToken.None));
            Assert.Equal("Dolor de rodilla", s.Motivo);
        }

        [Fact]
        public async Task Editar_una_solicitud_ajena_o_ya_asignada_se_rechaza()
        {
            var handler = new EditarYCancelarSolicitudCommandHandler(_solicitudes, _correo);
            _solicitudes.ObtenerPorIdAsync(9).Returns(Solicitud("Pendiente", idMedico: 3));
            await Assert.ThrowsAsync<AccesoDenegadoException>(() => handler.Handle(new EditarSolicitudCommand(9, "x", null, 2), CancellationToken.None));
            _solicitudes.ObtenerPorIdAsync(9).Returns(Solicitud("Asignada"));
            await Assert.ThrowsAsync<ConflictoException>(() => handler.Handle(new EditarSolicitudCommand(9, "x", null, 2), CancellationToken.None));
        }

        [Fact]
        public async Task El_medico_no_cancela_una_cita_ya_asignada_pero_la_fundacion_si_y_avisa_al_familiar()
        {
            var handler = new EditarYCancelarSolicitudCommandHandler(_solicitudes, _correo);
            var s = Solicitud("Asignada");
            _solicitudes.ObtenerPorIdAsync(9).Returns(s);
            _correo.EnviarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            await Assert.ThrowsAsync<ConflictoException>(() => handler.Handle(new CancelarSolicitudCommand(9, 2, "MedicoGeneral"), CancellationToken.None));
            var resultado = await handler.Handle(new CancelarSolicitudCommand(9, 1, "Administrador"), CancellationToken.None);

            Assert.Equal("Cancelada", s.Estado);
            Assert.True(resultado!.CorreoEnviado);
            await _correo.Received(1).EnviarAsync("ana@correo.test", Arg.Is<string>(a => a.Contains("cancelada")), Arg.Any<string>());
        }

        [Fact]
        public async Task Una_solicitud_atendida_no_se_cancela()
        {
            _solicitudes.ObtenerPorIdAsync(9).Returns(Solicitud("Atendida"));
            await Assert.ThrowsAsync<ConflictoException>(() =>
                new EditarYCancelarSolicitudCommandHandler(_solicitudes, _correo).Handle(new CancelarSolicitudCommand(9, 1, "Administrador"), CancellationToken.None));
        }

        // ---- CU-004: usuarios ----

        private UsuariosCommandHandler Usuarios() => new(_usuarios, _hasher);

        [Fact]
        public async Task Crear_usuario_guarda_la_clave_cifrada_y_en_minusculas_el_usuario()
        {
            var id = await Usuarios().Handle(new CrearUsuarioCommand(" Lab.Dos ", "Laboratorista Dos", "lab2@correo.test", "Laboratorio", null, "clave1234"), CancellationToken.None);
            Assert.Equal(50, id);
            await _usuarios.Received(1).CrearAsync(Arg.Is<Usuario>(u => u.NombreUsuario == "lab.dos" && u.PasswordHash == "hash:clave1234" && u.IdRol == 7));
        }

        [Theory]
        [InlineData("ab", "Laboratorio", null, "clave1234")]          // usuario muy corto
        [InlineData("nuevo", "Director", null, "clave1234")]          // rol que no existe
        [InlineData("nuevo", "Especialista", null, "clave1234")]      // especialista sin especialidad
        [InlineData("nuevo", "Laboratorio", null, "corta1")]          // contraseña corta
        [InlineData("nuevo", "Laboratorio", null, "solamenteletras")] // contraseña sin números
        public async Task Crear_usuario_con_datos_invalidos_se_rechaza(string usuario, string rol, string? especialidad, string clave)
        {
            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                Usuarios().Handle(new CrearUsuarioCommand(usuario, "Nombre", "x@correo.test", rol, especialidad, clave), CancellationToken.None));
            await _usuarios.DidNotReceive().CrearAsync(Arg.Any<Usuario>());
        }

        [Fact]
        public async Task Crear_un_usuario_que_ya_existe_se_rechaza()
        {
            _usuarios.ObtenerPorNombreUsuarioAsync("farmacia").Returns(new Usuario());
            await Assert.ThrowsAsync<ConflictoException>(() =>
                Usuarios().Handle(new CrearUsuarioCommand("farmacia", "Otra", "x@correo.test", "Farmacia", null, "clave1234"), CancellationToken.None));
        }

        [Fact]
        public async Task No_se_desactiva_al_ultimo_administrador_ni_a_uno_mismo()
        {
            _usuarios.ObtenerPorIdAsync(1).Returns(new Usuario { IdUsuario = 1, Estado = "activo", Rol = new Rol { Nombre = "Administrador" } });
            _usuarios.ObtenerPorIdAsync(2).Returns(new Usuario { IdUsuario = 2, Estado = "activo", Rol = new Rol { Nombre = "Administrador" } });
            _usuarios.ContarActivosConRolAsync("Administrador").Returns(1);

            await Assert.ThrowsAsync<ConflictoException>(() => Usuarios().Handle(new CambiarEstadoUsuarioCommand(1, "inactivo", 1), CancellationToken.None));
            await Assert.ThrowsAsync<ConflictoException>(() => Usuarios().Handle(new CambiarEstadoUsuarioCommand(2, "inactivo", 1), CancellationToken.None));
        }

        [Fact]
        public async Task Desactivar_y_editar_un_usuario()
        {
            var u = new Usuario { IdUsuario = 4, Estado = "activo", NombreCompleto = "Viejo", Rol = new Rol { Nombre = "Enfermero" } };
            _usuarios.ObtenerPorIdAsync(4).Returns(u);

            Assert.True(await Usuarios().Handle(new EditarUsuarioCommand(4, "Dra. Nueva", "n@correo.test", "Especialista", "Cardiología", 1), CancellationToken.None));
            Assert.Equal("Cardiología", u.Especialidad);
            Assert.True(await Usuarios().Handle(new CambiarEstadoUsuarioCommand(4, "inactivo", 1), CancellationToken.None));
            Assert.Equal("inactivo", u.Estado);
        }

        // ---- AC-05: contraseñas ----

        [Fact]
        public async Task Cambiar_la_clave_pide_la_actual_correcta()
        {
            var u = new Usuario { IdUsuario = 4, Estado = "activo", PasswordHash = "hash:vieja1234", Rol = new Rol { Nombre = "Farmacia" } };
            _usuarios.ObtenerPorIdAsync(4).Returns(u);

            await Assert.ThrowsAsync<DatosInvalidosException>(() => Usuarios().Handle(new CambiarClaveCommand(4, "equivocada1", "nueva12345"), CancellationToken.None));
            await Assert.ThrowsAsync<DatosInvalidosException>(() => Usuarios().Handle(new CambiarClaveCommand(4, "vieja1234", "vieja1234"), CancellationToken.None));
            Assert.True(await Usuarios().Handle(new CambiarClaveCommand(4, "vieja1234", "nueva12345"), CancellationToken.None));
            Assert.Equal("hash:nueva12345", u.PasswordHash);
        }

        [Fact]
        public async Task El_administrador_restablece_una_clave_temporal_valida()
        {
            var u = new Usuario { IdUsuario = 4, PasswordHash = "hash:olvidada1" };
            _usuarios.ObtenerPorIdAsync(4).Returns(u);
            await Assert.ThrowsAsync<DatosInvalidosException>(() => Usuarios().Handle(new RestablecerClaveCommand(4, "123"), CancellationToken.None));
            Assert.True(await Usuarios().Handle(new RestablecerClaveCommand(4, "temporal2026"), CancellationToken.None));
            Assert.Equal("hash:temporal2026", u.PasswordHash);
            Assert.False(await Usuarios().Handle(new RestablecerClaveCommand(999, "temporal2026"), CancellationToken.None));
        }
    }
}
