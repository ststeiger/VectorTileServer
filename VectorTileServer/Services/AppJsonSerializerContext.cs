#if AOT

namespace VectorTileServer
{


    [System.Text.Json.Serialization.JsonSerializable(typeof(Todo[]))]
    [System.Text.Json.Serialization.JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
    internal partial class AppJsonSerializerContext
        : System.Text.Json.Serialization.JsonSerializerContext
    {



        public static AppJsonSerializerContext InitPrettyContext()
        {
            // 4. Serialize the list of ColumnInfo objects to JSON
            System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true

                // In contrast, when you call JsonSerializer.Serialize() manually,
                // you must explicitly pass those options or the generated context,
                // otherwise it won’t know about your source-generated metadata
                // and will throw the "Reflection-based serialization disabled" error.
                // , TypeInfoResolver = AppJsonSerializerContext.Default
            };

            return new AppJsonSerializerContext(options);
        }


        public static AppJsonSerializerContext Pretty = InitPrettyContext();

        // Singleton is fine. 
        //public static AppJsonSerializerContext Pretty2
        //{
        //    get
        //    {
        //        return InitPrettyContext();
        //    }
        //}


    }

}

#endif
