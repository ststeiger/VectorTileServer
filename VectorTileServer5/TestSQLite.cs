
// #define USE_CLASS_FOR_SERIALIZATION 

namespace VectorTileServer5
{

    // Not necessary, ONE context can keep all 

    //[System.Text.Json.Serialization.JsonSerializable(typeof(System.Collections.Generic.List<ColumnInfo>))]
    //public partial class ColumnInfoJsonContext
    //    : System.Text.Json.Serialization.JsonSerializerContext
    //{ }


    public record ColumnInfo(
        [property: System.Text.Json.Serialization.JsonPropertyName("tableName")] 
        string TableName,

        [property: System.Text.Json.Serialization.JsonPropertyName("columnName")] 
        string ColumnName,

        [property: System.Text.Json.Serialization.JsonPropertyName("dataType")] 
        string DataType,

        [property: System.Text.Json.Serialization.JsonPropertyName("isNullable")] 
        bool IsNullable,

        [property: System.Text.Json.Serialization.JsonPropertyName("defaultValue")] 
        string DefaultValue,

        [property: System.Text.Json.Serialization.JsonPropertyName("isPrimaryKey")] 
        bool IsPrimaryKey
    );


    public class ColumnInfoClass
    {
        [System.Text.Json.Serialization.JsonPropertyName("tableName")]
        public string? TableName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("columnName")]
        public string? ColumnName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("dataType")]
        public string? DataType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("isNullable")]
        public bool? IsNullable { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("defaultValue")]
        public string? DefaultValue { get; set; } // SQLite stores default values as strings

        [System.Text.Json.Serialization.JsonPropertyName("isPrimaryKey")]
        public bool? IsPrimaryKey { get; set; }
    }


    public class TestSQLite 
    {
        

        public static async System.Threading.Tasks.Task Test()
        {
            // Initialize SQLitePCLRaw batteries. This ensures the native SQLite library
            // is properly loaded and configured, especially when using bundle packages
            // like SQLitePCLRaw.bundle_e_sqlite3 which Microsoft.Data.Sqlite depends on.
            // SQLitePCL.Batteries.Init(); // WTF ? no SQLitePCL


            Microsoft.Data.Sqlite.SqliteConnectionStringBuilder csb = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();
            csb.DataSource = @"D:\"+System.Environment.UserName + @"\Documents\Visual Studio 2022\gitlab\VectorTileServer\VectorTileServer\App_Data\COR_switzerland.mbtiles"; // is this the path ? 
            csb.Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly;
            csb.Cache = Microsoft.Data.Sqlite.SqliteCacheMode.Default;
            csb.ForeignKeys = false;
            csb.Pooling = true;
            csb.RecursiveTriggers = false;
            csb.DefaultTimeout = 30;

            

            // Use an in-memory database for this example.
            // For a file-based database, use "Data Source=yourdatabase.db"
            string connectionString = "Data Source=:memory:";
            connectionString = csb.ConnectionString;

#if USE_CLASS_FOR_SERIALIZATION
            System.Collections.Generic.List<ColumnInfoClass> allColumns = 
                new System.Collections.Generic.List<ColumnInfoClass>();
#else

            System.Collections.Generic.List<ColumnInfo> allColumns =
                new System.Collections.Generic.List<ColumnInfo>();

#endif 
            try
            {
                using (System.Data.Common.DbConnection connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString))
                {
                    if(connection.State != System.Data.ConnectionState.Open)
                        await connection.OpenAsync();
                    /*
                    // 1. Create a sample table for demonstration purposes
                    using (System.Data.Common.DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            FirstName TEXT NOT NULL,
                            LastName TEXT,
                            Email TEXT UNIQUE,
                            Age INTEGER DEFAULT 18
                        );

                        CREATE TABLE IF NOT EXISTS Products (
                            ProductId INTEGER PRIMARY KEY,
                            ProductName TEXT NOT NULL,
                            Price REAL DEFAULT 0.0,
                            Stock INTEGER
                        );
                    ";
                        command.ExecuteNonQuery();
                        System.Console.WriteLine("Sample tables created successfully.");
                    }
                    */


                    // 2. Get all table names from sqlite_master
                    using (System.Data.Common.DbCommand getTablesCommand = connection.CreateCommand())
                    {
                        getTablesCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
                        using (System.Data.Common.DbDataReader reader = getTablesCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string tableName = reader.GetString(0);
                                // tableName = "images";

                                System.Console.WriteLine($"Processing table: {tableName}");

                                // 3. For each table, get column information using PRAGMA table_info
                                using (System.Data.Common.DbCommand getColumnsCommand = connection.CreateCommand())
                                {
                                    getColumnsCommand.CommandText = $"PRAGMA table_info('{tableName}')";
                                    using (System.Data.Common.DbDataReader columnReader = getColumnsCommand.ExecuteReader())
                                    {
                                        while (columnReader.Read())
                                        {
                                            // PRAGMA table_info returns:
                                            // 0: cid (column ID)
                                            // 1: name (column name)
                                            // 2: type (data type)
                                            // 3: notnull (0 for nullable, 1 for not null)
                                            // 4: dflt_value (default value as string)
                                            // 5: pk (0 for not primary key, 1 for primary key)
#if USE_CLASS_FOR_SERIALIZATION
                                            ColumnInfoClass column = new ColumnInfoClass()
                                            {
                                                TableName = tableName,
                                                ColumnName = columnReader.GetString(1),
                                                DataType = columnReader.GetString(2),
                                                IsNullable = columnReader.GetInt32(3) == 0, // 0 means nullable
                                                DefaultValue = columnReader.IsDBNull(4) ? "<NULL>" : columnReader.GetString(4),
                                                IsPrimaryKey = columnReader.GetInt32(5) == 1 // 1 means primary key
                                            };
#else
                                            ColumnInfo column = new ColumnInfo(
                                                TableName: tableName,
                                                ColumnName: columnReader.GetString(1),
                                                DataType: columnReader.GetString(2),
                                                IsNullable: columnReader.GetInt32(3) == 0, // 0 means nullable
                                                DefaultValue: columnReader.IsDBNull(4) ? "<NULL>": columnReader.GetString(4),
                                                IsPrimaryKey: columnReader.GetInt32(5) == 1 // 1 means primary key
                                            );
#endif


                                            allColumns.Add(column);
                                        }
                                    }
                                }
                            }
                        }
                    }


                    if (connection.State != System.Data.ConnectionState.Closed)
                        await connection.CloseAsync();
                } // End using cnn 

                // 4. Serialize the list of ColumnInfo objects to JSON
                //System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions()
                //{ 
                //    WriteIndented = true

                //    // In contrast, when you call JsonSerializer.Serialize() manually,
                //    // you must explicitly pass those options or the generated context,
                //    // otherwise it won’t know about your source-generated metadata
                //    // and will throw the "Reflection-based serialization disabled" error.
                //    ,TypeInfoResolver = AppJsonSerializerContext.Default
                //};

                // The order of insertion matters — the first inserted resolver gets the highest priority.
                // options.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
                // options.TypeInfoResolverChain.Insert(0, ColumnInfoJsonContext.Default);


                // string jsonOutput = System.Text.Json.JsonSerializer.Serialize(allColumns, AppJsonSerializerContext.Default.ListColumnInfo);


                //#pragma warning disable IL3050 // AOT warning about RequiresDynamicCode
                //#pragma warning disable IL2026 // AOT warning about UnreferencedCodeAttribute 
                // string jsonOutput = System.Text.Json.JsonSerializer.Serialize(allColumns, options);
                //#pragma warning restore IL3050
                //#pragma warning restore IL2026

                // string jsonOutput = JsonSerializer.Serialize(
                //     allColumns
                //    , (new ColumnInfoJsonContext(options)).ListColumnInfo
                // );


                string jsonOutput = System.Text.Json.JsonSerializer.Serialize(allColumns, AppJsonSerializerContext.Pretty.ListColumnInfo);


                System.Console.WriteLine("\n--- SQLite Column Schema (JSON Output) ---");
                System.Console.WriteLine(jsonOutput);
                System.Console.WriteLine("----------------------------------------");
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex)
            {
                System.Console.WriteLine($"SQLite Error: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }

        } // End Sub 


    } // End Class Program 


} // End Namespace 
