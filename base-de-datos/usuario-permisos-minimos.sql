-- -----------------------------------------------------------------------------
-- Plan de pruebas SQA, semana 4: usuario de base de datos con permisos mínimos
--
-- Hoy la aplicación en Azure se conecta con el ADMINISTRADOR del servidor SQL, que puede
-- borrar tablas o la base completa. Si alguien robara esa cadena de conexión, podría hacerlo.
-- Este usuario solo puede LEER y ESCRIBIR datos: no puede crear, cambiar ni borrar tablas.
-- La aplicación no necesita más: nunca cambia la estructura de la base mientras corre
-- (no llama a Migrate, EnsureCreated ni ejecuta SQL propio).
--
-- El script despliegue/aplicar-usuario-minimo.ps1 hace todos estos pasos solo (con firewall
-- temporal, clave al azar guardada en la carpeta de secretos y confirmación antes de cambiar
-- la conexión). Pasos a mano, si se prefiere:
-- Cómo usarlo (Azure SQL):
--   1. Conectarse a la base asilo_cabeza_algodon (NO a master) con el administrador.
--   2. Reemplazar <CLAVE_GENERADA> por una contraseña larga generada al azar.
--      La clave va SOLO a la carpeta de secretos y a la configuración de Azure, nunca al repositorio.
--   3. Ejecutar la PARTE 1.
--   4. Conectarse COMO app_asilo y ejecutar la PARTE 2 para comprobar los permisos.
--   5. Cambiar la cadena de conexión DefaultConnection del App Service a User ID=app_asilo.
--   6. Verificar el sitio (11 comprobaciones y 142 permisos).
-- Los cambios de estructura futuros (migraciones) se siguen haciendo con el administrador.
-- -----------------------------------------------------------------------------

-- PARTE 1 (como administrador)
CREATE USER app_asilo WITH PASSWORD = '<CLAVE_GENERADA>';
ALTER ROLE db_datareader ADD MEMBER app_asilo;
ALTER ROLE db_datawriter ADD MEMBER app_asilo;

-- PARTE 2 (conectado como app_asilo): lo primero debe funcionar y lo segundo debe fallar
-- SELECT COUNT(*) AS internos FROM Internos;            -- esperado: funciona
-- CREATE TABLE prueba_permisos (x INT);                  -- esperado: error de permiso denegado
