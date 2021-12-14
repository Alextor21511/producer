using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.Data;
using System.Data.SqlClient;
namespace CapaDatos
{
    public class D_Clientes
    {
        private readonly SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["conexion"].ConnectionString);

        public DataTable MostrarRegistros()
        {
            DataTable resultado = new DataTable();
            SqlCommand command = new SqlCommand("insert into clientes (Usuario,Alias,Password) values ('Ruben', 'Rubyk', 'Hola')", connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();

            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(resultado);

            return resultado;
        }
    }
}
