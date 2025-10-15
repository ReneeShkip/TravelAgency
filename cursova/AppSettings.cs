using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace cursova
{
    public static class AppSettings
    {
        public static string GetConnectionString()
        {
            return "server=localhost;user=root;database=travel_agency;port=3306;password=root1;";

        }
    }
}
