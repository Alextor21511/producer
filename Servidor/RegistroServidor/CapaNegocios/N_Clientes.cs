using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using CapaDatos;

namespace CapaNegocios
{
    public class N_Clientes
    {
        private readonly D_Clientes clientes = new D_Clientes();

        public DataTable MostrarDatos()
        {
            return clientes.MostrarRegistros();
        }
    }
}
