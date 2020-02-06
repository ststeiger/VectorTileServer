
namespace TestSQLite
{


    public static class DbDataReaderExtensions
    {


        private static System.IO.Stream GetStreamOld(this System.Data.Common.DbDataReader reader, int ordinal)
        {
            using (System.IO.MemoryStream bufferStream = new System.IO.MemoryStream())
            {
                long bytesRead = 0;
                long bytesReadTotal = 0;
                byte[] buffer = new byte[4096];

                do
                {
                    bytesRead = reader.GetBytes(ordinal, bytesReadTotal, buffer, 0, buffer.Length);
                    bufferStream.Write(buffer, 0, (int)bytesRead);
                    bytesReadTotal += bytesRead;
                } while (bytesRead > 0);

                buffer = null;

                return new System.IO.MemoryStream(bufferStream.ToArray(), false);
            }
        } // End Function GetStreamOld



        public static System.Data.Common.DbCommand AddParameter(this System.Data.Common.DbCommand command, string name, object value, System.Data.DbType type, int length)
        {
            System.Data.Common.DbParameter param = command.CreateParameter();
            param.ParameterName = name;
            param.DbType = type;
            param.Size = length;
            param.Value = value;
            command.Parameters.Add(param);

            return command;
        }


        public static System.IO.Stream GetStream(this System.Data.Common.DbDataReader reader, int ordinal)
        {
            System.IO.MemoryStream bufferStream = new System.IO.MemoryStream();

            long bytesRead = 0;
            long bytesReadTotal = 0;
            byte[] buffer = new byte[4096];
            do
            {
                bytesRead = reader.GetBytes(ordinal, bytesReadTotal, buffer, 0, buffer.Length);
                bufferStream.Write(buffer, 0, (int)bytesRead);
                bytesReadTotal += bytesRead;
            } while (bytesRead > 0);

            buffer = null;
            bufferStream.Position = 0;
            return bufferStream;
        } // End Function GetStream


    }


}