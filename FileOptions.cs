using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ManageFiles__LastOne
{
    class FileOptions 
    {
        Connexion cr = new Connexion();
        public void RemoveFile(string FileName) // when a dir is deleted we must delete files inside it
        {
            cr.ToConnect();
            string query = "delete from files where Parent_id = @rn";
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@rn", FileName);
            cmd.Connection = cr.conn;
            cmd.ExecuteNonQuery();
        }
    }
}
