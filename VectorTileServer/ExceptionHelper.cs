
namespace VectorTileServer
{


    public static class ExceptionHelper
    {


        public static string JsonizeError(System.Exception? ex)
        {
            string ret = "{}";

            if (ex == null) 
                return ret;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                JsonizeError(ex, ms);
                ret = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            } // End Using ms 

            return ret;
        } // End Sub JsonizeError 


        public static void JsonizeError(System.Exception? ex, System.IO.StreamWriter sw)
        {
            JsonizeError(ex, sw.BaseStream);
        } // End Sub JsonizeError 


        public static void JsonizeError(System.Exception? ex, System.IO.Stream ms)
        {
            using (System.Text.Json.Utf8JsonWriter jw = new System.Text.Json.Utf8JsonWriter(ms, new System.Text.Json.JsonWriterOptions
            {
                Indented = true,
                SkipValidation = false
            }))
            {
                if (ex == null)
                { 
                    jw.WriteRawValue("{}");
                    jw.Flush();
                    return;
                } // End if (ex == null) 

                System.Exception? thisError = ex;
                int objectCount = 0;

                while (thisError != null)
                {
                    jw.WriteStartObject();
                    objectCount++;

                    jw.WriteString("message", thisError.Message);

                    if (thisError.StackTrace != null)
                        jw.WriteString("stackTrace", thisError.StackTrace);

                    if (thisError.Source != null)
                        jw.WriteString("source", thisError.Source);

                    jw.WriteString("name", thisError.GetType().FullName);
                    jw.WriteNumber("hResult", thisError.HResult);

                    if (thisError.Data != null && thisError.Data.Keys.Count > 0)
                    {
                        jw.WritePropertyName("data");
                        jw.WriteStartObject();
                        foreach (System.Collections.DictionaryEntry entry in thisError.Data)
                        {
                            jw.WriteString(entry.Key?.ToString() ?? "", entry.Value?.ToString());
                        }
                        jw.WriteEndObject();
                    }

                    if (thisError.HelpLink != null)
                        jw.WriteString("helpLink", thisError.HelpLink);

                    if (thisError.InnerException != null)
                        jw.WritePropertyName("innerException");

                    thisError = thisError.InnerException;
                } // Whend 

                for (int i = 0; i < objectCount; ++i)
                    jw.WriteEndObject();

                jw.Flush();
            } // End Using jw 

        } // End Sub JsonizeError 


        public static string StringifyError(System.Exception ex)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine(System.Environment.NewLine);
            sb.AppendLine(System.Environment.NewLine);

            System.Exception? thisError = ex;
            while (thisError != null)
            {
                sb.AppendLine(thisError.GetType().FullName);
                sb.AppendLine(thisError.Source);
                sb.AppendLine(thisError.Message);
                // sb.AppendLine(thisError.HResult);
                sb.AppendLine(thisError.StackTrace);

                if (thisError.InnerException != null)
                {
                    sb.AppendLine(System.Environment.NewLine);
                    sb.AppendLine("Inner Exception:");
                } // End if (thisError.InnerException != null) 

                thisError = thisError.InnerException;
            } // Whend 

            sb.AppendLine(System.Environment.NewLine);
            sb.AppendLine(System.Environment.NewLine);

            return sb.ToString();
        } // End Sub DisplayError 


        public static void DisplayError(System.Exception ex)
        {
            System.Console.WriteLine(System.Environment.NewLine);
            System.Console.WriteLine(System.Environment.NewLine);

            System.Exception? thisError = ex;
            while (thisError != null)
            {
                System.Console.WriteLine(thisError.GetType().FullName);
                System.Console.WriteLine(thisError.Source);
                System.Console.WriteLine(thisError.Message);
                // System.Console.WriteLine(thisError.HResult);
                System.Console.WriteLine(thisError.StackTrace);

                if (thisError.InnerException != null)
                {
                    System.Console.WriteLine(System.Environment.NewLine);
                    System.Console.WriteLine("Inner Exception:");
                } // End if (thisError.InnerException != null) 

                thisError = thisError.InnerException;
            } // Whend 

            System.Console.WriteLine(System.Environment.NewLine);
            System.Console.WriteLine(System.Environment.NewLine);
        } // End Sub DisplayError 


    } // End Class Tools 


} // End Namespace 
