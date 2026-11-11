# AC-02: tokens falsificados contra Azure (10/11/2026, versión 1.0.0-rc2, commit 675011f)

Endpoint: GET /api/Usuarios (solo Administrador). Las llaves publicadas se leyeron del historial de Git y no se muestran.

```
OK    401  Firmado con la llave publicada en Git n.º 1
OK    401  Firmado con la llave publicada en Git n.º 2
OK    401  Firmado con una llave inventada
OK    401  Sin firma (alg none)
OK    401  Vencido (llave inventada, exp en el pasado)
OK    401  Texto que no es un token
OK    401  Sin token
RESULTADO: todos rechazados
```
