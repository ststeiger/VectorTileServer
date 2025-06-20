﻿//
// Community.CsharpSqlite.SQLiteClient.SqliteClientFactory.cs
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace SQLite.FullyManaged
{


    public class SqliteClientFactory
        : System.Data.Common.DbProviderFactory
    {
        public static SqliteClientFactory Instance = null;
        public static object lockStatic = new object();

        private SqliteClientFactory()
        {
        }

        static SqliteClientFactory()
        {
            lock (lockStatic)
            {
                if (Instance == null)
                    Instance = new SqliteClientFactory();
            }
        }

        public override bool CanCreateDataSourceEnumerator
        {
            get
            {
                return false;
            }
        }

        public override System.Data.Common.DbCommand CreateCommand()
        {
            return new SqliteCommand();
        }

        public override System.Data.Common.DbCommandBuilder CreateCommandBuilder()
        {
            return new SqliteCommandBuilder();
        }

        public override System.Data.Common.DbConnection CreateConnection()
        {
            return new SqliteConnection();
        }

        public override System.Data.Common.DbDataAdapter CreateDataAdapter()
        {
            return new SqliteDataAdapter();
        }

        public override System.Data.Common.DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return new SqliteDataSourceEnumerator();
        }

        public override System.Data.Common.DbParameter CreateParameter()
        {
            return new SqliteParameter();
        }

        public override System.Data.Common.DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new SqliteConnectionStringBuilder();
        }
    }

}
