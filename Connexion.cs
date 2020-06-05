using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ManageFiles__LastOne
{
    class Connexion
    {
        public MySqlConnection conn = null;
        public void ToConnect()
        {
            string cs = @"server=localhost;userid=root;
            password=root;database=mybd";

            try
            {
                conn = new MySqlConnection(cs);
                conn.Open();


            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.Write("Connection error, {0}", ex.Message);

            }
        }
    }
}
