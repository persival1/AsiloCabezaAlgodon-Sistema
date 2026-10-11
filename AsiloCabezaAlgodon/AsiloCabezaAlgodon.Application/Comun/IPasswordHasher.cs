using System;
using System.Collections.Generic;
using System.Text;

namespace AsiloCabezaAlgodon.Application.Comun.Interfaces
{
    public interface IPasswordHasher
    {
        string Hashear(string password);
        bool Verificar(string password, string hash);
    }
}