using System;
using System.Collections.Generic;
using System.Text;

using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Infrastructure.Seguridad
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hashear(string password) => BCrypt.Net.BCrypt.HashPassword(password);
        public bool Verificar(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
    }
}