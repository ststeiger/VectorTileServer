
namespace VectorTileSelector
{
    class Db
    {
        public static string ProjectDirectory
        {
            get
            {
                string bd = System.AppDomain.CurrentDomain.BaseDirectory;
                bd = System.IO.Path.Combine(bd, "..", "..", "..");
                bd = System.IO.Path.GetFullPath(bd);

                return bd;
            }
        }


        public static string HtmlDirectory
        {
            get
            {
                string bd = System.AppDomain.CurrentDomain.BaseDirectory;
                bd = System.IO.Path.Combine(ProjectDirectory, "Results", "html");
                bd = System.IO.Path.GetFullPath(bd);

                return bd;
            }
        }

        public static string KmlDirectory
        {
            get
            {
                string bd = System.AppDomain.CurrentDomain.BaseDirectory;
                bd = System.IO.Path.Combine(ProjectDirectory, "Results", "kml");
                bd = System.IO.Path.GetFullPath(bd);

                return bd;
            }
        }


        public static string TileSizeDirectory
        {
            get
            {
                string bd = System.AppDomain.CurrentDomain.BaseDirectory;
                bd = System.IO.Path.Combine(ProjectDirectory, "Results", "tile_size");
                bd = System.IO.Path.GetFullPath(bd);

                return bd;
            }
        }

        public static string SqlDirectory
        {
            get
            {
                string bd = System.AppDomain.CurrentDomain.BaseDirectory;
                bd = System.IO.Path.Combine(ProjectDirectory, "Results", "sql");
                bd = System.IO.Path.GetFullPath(bd);

                return bd;
            }
        }


        private static string GetMsCs()
        {
            // Create SqlConnectionStringBuilder instance
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder();

            // Set the data source to the machine name
            builder.DataSource = System.Environment.MachineName;
            builder.InitialCatalog = "openmaptiles";

            // Set integrated security to true
            builder.IntegratedSecurity = true;
            builder.Encrypt = false;

            // Build the connection string
            string connectionString = builder.ConnectionString;

            return connectionString;
        } // End Function GetMsCs 


        private static string GetPgCs()
        {
            // Create SqlConnectionStringBuilder instance
            Npgsql.NpgsqlConnectionStringBuilder builder = new Npgsql.NpgsqlConnectionStringBuilder();

            // Set the data source to the machine name
            builder.Host = "localhost"; // System.Environment.MachineName;
            builder.Port = 5432;
            builder.Database = "openmaptiles";

            // Set integrated security to true
            // builder.IntegratedSecurity = false;
            builder.Username = "postgres";
            builder.Password = "TOP_SECRET";

            // Build the connection string
            string connectionString = builder.ConnectionString;

            return connectionString;
        } // End Function GetPgCs 


        public static System.Data.Common.DbConnection Connection
        {
            get
            {
                System.Data.Common.DbConnection dbc = new Microsoft.Data.SqlClient.SqlConnection(GetMsCs());
                dbc.ConnectionString = GetMsCs();
                // System.Data.Common.DbConnection dbc = new Npgsql.NpgsqlConnection(GetPgCs())
                // dbc.ConnectionString = GetPgCs();
                return dbc;
            }
        } // End Property Connection 



    }
}
