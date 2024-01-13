
namespace libWebAppBasics.Database
{


    public interface IConnectionFactory
    {
        bool CheckConnection { get; }
        string ConnectionString { get; }
        string ConnectionStringWithoutPassword { get; }

        System.Data.Common.DbConnection Connection { get; }
        System.Data.Common.DbConnection ClosedConnection { get; }

    } // End interface IConnectionFactory 


    public class ConnectionFactory
        : IConnectionFactory
    {
        protected string m_cs;
        protected System.Data.Common.DbProviderFactory m_factory;


        private static System.Data.Common.DbProviderFactory GetFactory(System.Type type)
        {
            if (type != null && System.Reflection.IntrospectionExtensions.GetTypeInfo(type)
                .IsSubclassOf(typeof(System.Data.Common.DbProviderFactory)))
            {
                // Provider factories are singletons with Instance field having the sole instance
                System.Reflection.FieldInfo field = System.Reflection.IntrospectionExtensions.GetTypeInfo(type).GetField("Instance"
                    , System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
                );

                if (field != null)
                {
                    return (System.Data.Common.DbProviderFactory)field.GetValue(null);
                    //return field.GetValue(null) as DbProviderFactory;
                } // End if (field != null)

            } // End if (type != null && type.IsSubclassOf(typeof(System.Data.Common.DbProviderFactory)))

            throw new System.Exception("DataProvider is missing!");
            //throw new System.Configuration.ConfigurationException("DataProvider is missing!");
        } // End Function GetFactory



        public ConnectionFactory(string connectionString, System.Data.Common.DbProviderFactory factory)
        {
            this.m_cs = connectionString;
            this.m_factory = factory;
        }


        public ConnectionFactory(string connectionString, System.Type factory)
            : this(connectionString, GetFactory(factory))
        { }


        public ConnectionFactory(string connectionString)
            : this(connectionString, typeof(System.Data.SQLite.SQLiteFactory))
        { }


        public ConnectionFactory() : this(null)
        { }


        public string ConnectionString
        {
            get
            {
                if (this.m_cs != null)
                    return this.m_cs;

                this.m_cs = "";
                return this.m_cs;
            }
        } // End Property ConnectionString 


        public string ConnectionStringWithoutPassword
        {
            get
            {
                System.Data.Common.DbConnectionStringBuilder dbc = new System.Data.Common.DbConnectionStringBuilder();
                dbc.ConnectionString = this.ConnectionString;

                if (dbc.ContainsKey("password") && !string.IsNullOrEmpty(System.Convert.ToString(dbc["Password"])))
                    dbc["Password"] = new string('*', 8);

                return dbc.ConnectionString;
            }
        } // End Property ConnectionStringWithoutPassword 


        protected System.Data.Common.DbConnection GetConnection(bool opened)
        {
            System.Data.Common.DbConnection cn = this.m_factory.CreateConnection();
            cn.ConnectionString = this.ConnectionString;

            if (opened && cn.State != System.Data.ConnectionState.Open)
                cn.Open();

            return cn;
        } // End Function GetConnection 


        public System.Data.Common.DbConnection Connection
        {
            get { return this.GetConnection(true); }
        }


        public System.Data.Common.DbConnection ClosedConnection
        {
            get { return this.GetConnection(false); }
        }


        public bool CheckConnection
        {
            get
            {
                try
                {
                    using (System.Data.Common.DbConnection connection = this.ClosedConnection)
                    {
                        if (connection.State != System.Data.ConnectionState.Open)
                            connection.Open();

                        if (connection.State != System.Data.ConnectionState.Closed)
                            connection.Close();
                    }
                }
                catch (System.Exception)
                {
                    return false;
                }

                return true;
            } // End Get 

        } // End Property CheckConnection 


    } // End Class ConnectionFactory 


} // End Namespace LdapService
