//
// Community.CsharpSqlite.SQLiteClient.SqliteCommandBuilder.cs
//
// Author(s): Tim Coleman (tim@timcoleman.com)
//            Marek Habersack (grendello@gmail.com)
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2007 Marek Habersack
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
    public sealed class SqliteCommandBuilder 
        : System.Data.Common.DbCommandBuilder
    {
        static readonly string clause1 = "({0} = 1 AND {1} IS NULL)";
        static readonly string clause2 = "({0} = {1})";

        System.Data.DataTable _schemaTable;
        SqliteDataAdapter _dataAdapter;
        SqliteCommand _insertCommand;
        SqliteCommand _updateCommand;
        SqliteCommand _deleteCommand;
        bool _disposed;
        string _quotePrefix = "'";
        string _quoteSuffix = "'";
        string _tableName;
        SqliteRowUpdatingEventHandler rowUpdatingHandler;

        public new System.Data.Common.DbDataAdapter DataAdapter
        {
            get { return _dataAdapter; }
            set
            {
                if (_dataAdapter != null)
                    _dataAdapter.RowUpdating -= new SqliteRowUpdatingEventHandler(RowUpdatingHandler);
                _dataAdapter = value as SqliteDataAdapter;
                if (_dataAdapter != null)
                    _dataAdapter.RowUpdating += new SqliteRowUpdatingEventHandler(RowUpdatingHandler);
            }
        }

        public override string QuotePrefix
        {
            get { return _quotePrefix; }

            set
            {
                if (_schemaTable != null)
                    throw new System.InvalidOperationException("The QuotePrefix and QuoteSuffix properties cannot be changed once an Insert, Update or Delete commands have been generated.");
                _quotePrefix = value;
            }
        }

        public override string QuoteSuffix
        {
            get { return _quoteSuffix; }

            set
            {
                if (_schemaTable != null)
                    throw new System.InvalidOperationException("The QuotePrefix and QuoteSuffix properties cannot be changed once an Insert, Update or Delete commands have been generated.");
                _quoteSuffix = value;
            }
        }

        private SqliteCommand SourceCommand
        {
            get
            {
                if (_dataAdapter != null)
                    return _dataAdapter.SelectCommand as SqliteCommand;
                return null;
            }
        }

        private string QuotedTableName
        {
            get { return GetQuotedString(_tableName); }
        }

        public new SqliteCommand GetDeleteCommand()
        {
            BuildCache(true);
            if (_deleteCommand == null)
                return CreateDeleteCommand(false);
            return _deleteCommand;
        }

        public new SqliteCommand GetInsertCommand()
        {
            BuildCache(true);
            if (_insertCommand == null)
                return CreateInsertCommand(false);
            return _insertCommand;
        }

        public new SqliteCommand GetUpdateCommand()
        {
            BuildCache(true);
            if (_updateCommand == null)
                return CreateUpdateCommand(false);
            return _updateCommand;
        }

        public override void RefreshSchema()
        {
            // FIXME: "Figure out what else needs to be cleaned up when we refresh."
            _tableName = string.Empty;
            _schemaTable = null;
            CreateNewCommand(ref _deleteCommand);
            CreateNewCommand(ref _updateCommand);
            CreateNewCommand(ref _insertCommand);
        }

        protected override void SetRowUpdatingHandler(
            System.Data.Common.DbDataAdapter adapter
        )
        {
            if (!(adapter is SqliteDataAdapter))
            {
                throw new System.InvalidOperationException("Adapter needs to be a SqliteDataAdapter");
            }
            rowUpdatingHandler = new SqliteRowUpdatingEventHandler(RowUpdatingHandler);
            ((SqliteDataAdapter)adapter).RowUpdating += rowUpdatingHandler;
        }

        protected override void ApplyParameterInfo(
            System.Data.Common.DbParameter dbParameter,
            System.Data.DataRow row,
            System.Data.StatementType statementType,
            bool whereClause
        )
        {
            // Nothing to do here
        }

        protected override string GetParameterName(int position)
        {
            return string.Format("?p{0}", position);
        }

        protected override string GetParameterName(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new System.ArgumentException("parameterName cannot be null or empty");
            if (parameterName[0] == '?')
                return parameterName;
            return string.Format("?{0}", parameterName);
        }


        protected override string GetParameterPlaceholder(int position)
        {
            return string.Format("?p{0}", position);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_insertCommand != null)
                        _insertCommand.Dispose();
                    if (_deleteCommand != null)
                        _deleteCommand.Dispose();
                    if (_updateCommand != null)
                        _updateCommand.Dispose();
                    if (_schemaTable != null)
                        _schemaTable.Dispose();
                }
                _disposed = true;
            }
        }

        private void BuildCache(bool closeConnection)
        {
            SqliteCommand sourceCommand = SourceCommand;
            if (sourceCommand == null)
                throw new System.InvalidOperationException("The DataAdapter.SelectCommand property needs to be initialized.");
            SqliteConnection connection = sourceCommand.Connection as SqliteConnection;
            if (connection == null)
                throw new System.InvalidOperationException("The DataAdapter.SelectCommand.Connection property needs to be initialized.");

            if (_schemaTable == null)
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    closeConnection = false;
                else
                    connection.Open();

                SqliteDataReader reader = sourceCommand.ExecuteReader(
                    System.Data.CommandBehavior.SchemaOnly |
                    System.Data.CommandBehavior.KeyInfo
                );

                _schemaTable = reader.GetSchemaTable();
                reader.Close();
                if (closeConnection)
                    connection.Close();
                BuildInformation(_schemaTable);
            }
        }

        private void BuildInformation(System.Data.DataTable schemaTable)
        {
            _tableName = string.Empty;
            foreach (System.Data.DataRow schemaRow in schemaTable.Rows)
            {
                if (schemaRow.IsNull("BaseTableName") ||
                    (string)schemaRow["BaseTableName"] == string.Empty)
                    continue;

                if (_tableName == string.Empty)
                    _tableName = (string)schemaRow["BaseTableName"];
                else if (_tableName != (string)schemaRow["BaseTableName"])
                    throw new System.InvalidOperationException("Dynamic SQL generation is not supported against multiple base tables.");
            }
            if (_tableName == string.Empty)
                throw new System.InvalidOperationException("Dynamic SQL generation is not supported with no base table.");
            _schemaTable = schemaTable;
        }

        private SqliteCommand CreateInsertCommand(bool option)
        {
            if (QuotedTableName == string.Empty)
                return null;

            CreateNewCommand(ref _insertCommand);

            string command = string.Format("INSERT INTO {0}", QuotedTableName);
            string sql;
            System.Text.StringBuilder columns = new System.Text.StringBuilder();
            System.Text.StringBuilder values = new System.Text.StringBuilder();

            int parmIndex = 1;
            foreach (System.Data.DataRow schemaRow in _schemaTable.Rows)
            {
                if (!IncludedInInsert(schemaRow))
                    continue;

                if (parmIndex > 1)
                {
                    columns.Append(", ");
                    values.Append(", ");
                }

                SqliteParameter parameter = null;
                if (option)
                {
                    parameter = _insertCommand.Parameters.Add(CreateParameter(schemaRow));
                }
                else
                {
                    parameter = _insertCommand.Parameters.Add(CreateParameter(parmIndex++, schemaRow));
                }
                parameter.SourceVersion = System.Data.DataRowVersion.Current;
                columns.Append(GetQuotedString(parameter.SourceColumn));
                values.Append(parameter.ParameterName);
            }

            sql = string.Format("{0} ({1}) VALUES ({2})", command, columns.ToString(), values.ToString());
            _insertCommand.CommandText = sql;
            return _insertCommand;
        }

        private SqliteCommand CreateDeleteCommand(bool option)
        {
            // If no table was found, then we can't do an delete
            if (QuotedTableName == string.Empty)
                return null;

            CreateNewCommand(ref _deleteCommand);

            string command = string.Format("DELETE FROM {0}", QuotedTableName);
            System.Text.StringBuilder whereClause = new System.Text.StringBuilder();
            bool keyFound = false;
            int parmIndex = 1;

            foreach (System.Data.DataRow schemaRow in _schemaTable.Rows)
            {
                if ((bool)schemaRow["IsExpression"] == true)
                    continue;
                if (!IncludedInWhereClause(schemaRow))
                    continue;

                if (whereClause.Length > 0)
                    whereClause.Append(" AND ");

                bool isKey = (bool)schemaRow["IsKey"];
                SqliteParameter parameter = null;

                if (isKey)
                    keyFound = true;

                bool allowNull = (bool)schemaRow["AllowDBNull"];
                if (!isKey && allowNull)
                {
                    if (option)
                    {
                        parameter = _deleteCommand.Parameters.Add(
                            string.Format("@{0}", schemaRow["BaseColumnName"]), System.Data.DbType.Int32);
                    }
                    else
                    {
                        parameter = _deleteCommand.Parameters.Add(
                            string.Format("@p{0}", parmIndex++), System.Data.DbType.Int32);
                    }
                    string sourceColumnName = (string)schemaRow["BaseColumnName"];
                    parameter.Value = 1;

                    whereClause.Append("(");
                    whereClause.Append(string.Format(clause1, parameter.ParameterName,
                                       GetQuotedString(sourceColumnName)));
                    whereClause.Append(" OR ");
                }

                if (option)
                {
                    parameter = _deleteCommand.Parameters.Add(CreateParameter(schemaRow));
                }
                else
                {
                    parameter = _deleteCommand.Parameters.Add(CreateParameter(parmIndex++, schemaRow));
                }
                parameter.SourceVersion = System.Data.DataRowVersion.Original;

                whereClause.Append(string.Format(clause2, GetQuotedString(parameter.SourceColumn),
                                   parameter.ParameterName));

                if (!isKey && allowNull)
                    whereClause.Append(")");
            }
            if (!keyFound)
                throw new System.InvalidOperationException("Dynamic SQL generation for the DeleteCommand is not supported against a SelectCommand that does not return any key column information.");

            string sql = string.Format("{0} WHERE ({1})", command, whereClause.ToString());
            _deleteCommand.CommandText = sql;
            return _deleteCommand;
        }

        private SqliteCommand CreateUpdateCommand(bool option)
        {
            if (QuotedTableName == string.Empty)
                return null;

            CreateNewCommand(ref _updateCommand);

            string command = string.Format("UPDATE {0} SET ", QuotedTableName);
            System.Text.StringBuilder columns = new System.Text.StringBuilder();
            System.Text.StringBuilder whereClause = new System.Text.StringBuilder();
            int parmIndex = 1;
            bool keyFound = false;

            foreach (System.Data.DataRow schemaRow in _schemaTable.Rows)
            {
                if (!IncludedInUpdate(schemaRow))
                    continue;
                if (columns.Length > 0)
                    columns.Append(", ");

                SqliteParameter parameter = null;
                if (option)
                {
                    parameter = _updateCommand.Parameters.Add(CreateParameter(schemaRow));
                }
                else
                {
                    parameter = _updateCommand.Parameters.Add(CreateParameter(parmIndex++, schemaRow));
                }
                parameter.SourceVersion = System.Data.DataRowVersion.Current;

                columns.Append(string.Format("{0} = {1}", GetQuotedString(parameter.SourceColumn),
                                   parameter.ParameterName));
            }

            foreach (System.Data.DataRow schemaRow in _schemaTable.Rows)
            {
                if ((bool)schemaRow["IsExpression"] == true)
                    continue;

                if (!IncludedInWhereClause(schemaRow))
                    continue;

                if (whereClause.Length > 0)
                    whereClause.Append(" AND ");

                bool isKey = (bool)schemaRow["IsKey"];
                SqliteParameter parameter = null;

                if (isKey)
                    keyFound = true;

                bool allowNull = (bool)schemaRow["AllowDBNull"];
                if (!isKey && allowNull)
                {
                    if (option)
                    {
                        parameter = _updateCommand.Parameters.Add(
                            string.Format("@{0}", schemaRow["BaseColumnName"]), System.Data.SqlDbType.Int);
                    }
                    else
                    {
                        parameter = _updateCommand.Parameters.Add(
                            string.Format("@p{0}", parmIndex++), System.Data.SqlDbType.Int);
                    }
                    parameter.Value = 1;
                    whereClause.Append("(");
                    whereClause.Append(string.Format(clause1, parameter.ParameterName,
                                       GetQuotedString((string)schemaRow["BaseColumnName"])));
                    whereClause.Append(" OR ");
                }

                if (option)
                {
                    parameter = _updateCommand.Parameters.Add(CreateParameter(schemaRow));
                }
                else
                {
                    parameter = _updateCommand.Parameters.Add(CreateParameter(parmIndex++, schemaRow));
                }
                parameter.SourceVersion = System.Data.DataRowVersion.Original;
                whereClause.Append(string.Format(clause2, GetQuotedString(parameter.SourceColumn),
                                   parameter.ParameterName));

                if (!isKey && allowNull)
                    whereClause.Append(")");
            }
            if (!keyFound)
                throw new System.InvalidOperationException("Dynamic SQL generation for the UpdateCommand is not supported against a SelectCommand that does not return any key column information.");

            string sql = string.Format("{0}{1} WHERE ({2})", command, columns.ToString(), whereClause.ToString());
            _updateCommand.CommandText = sql;
            return _updateCommand;
        }

        private void CreateNewCommand(ref SqliteCommand command)
        {
            SqliteCommand sourceCommand = SourceCommand;
            if (command == null)
            {
                command = sourceCommand.Connection.CreateCommand() as SqliteCommand;
                command.CommandTimeout = sourceCommand.CommandTimeout;
                command.Transaction = sourceCommand.Transaction;
            }
            command.CommandType = System.Data.CommandType.Text;
            command.UpdatedRowSource = System.Data.UpdateRowSource.None;
            command.Parameters.Clear();
        }

        private bool IncludedInWhereClause(System.Data.DataRow schemaRow)
        {
            if ((bool)schemaRow["IsLong"])
                return false;
            return true;
        }

        private bool IncludedInInsert(System.Data.DataRow schemaRow)
        {
            // not all of the below are supported by Sqlite, but we leave them here anyway, since some day Sqlite may
            // support some of them.
            if (!schemaRow.IsNull("IsAutoIncrement") && (bool)schemaRow["IsAutoIncrement"])
                return false;
            if (!schemaRow.IsNull("IsHidden") && (bool)schemaRow["IsHidden"])
                return false;
            if (!schemaRow.IsNull("IsExpression") && (bool)schemaRow["IsExpression"])
                return false;
            if (!schemaRow.IsNull("IsRowVersion") && (bool)schemaRow["IsRowVersion"])
                return false;
            if (!schemaRow.IsNull("IsReadOnly") && (bool)schemaRow["IsReadOnly"])
                return false;
            return true;
        }

        private bool IncludedInUpdate(System.Data.DataRow schemaRow)
        {
            // not all of the below are supported by Sqlite, but we leave them here anyway, since some day Sqlite may
            // support some of them.
            if (!schemaRow.IsNull("IsAutoIncrement") && (bool)schemaRow["IsAutoIncrement"])
                return false;
            if (!schemaRow.IsNull("IsHidden") && (bool)schemaRow["IsHidden"])
                return false;
            if (!schemaRow.IsNull("IsRowVersion") && (bool)schemaRow["IsRowVersion"])
                return false;
            if (!schemaRow.IsNull("IsExpression") && (bool)schemaRow["IsExpression"])
                return false;
            if (!schemaRow.IsNull("IsReadOnly") && (bool)schemaRow["IsReadOnly"])
                return false;

            return true;
        }

        private SqliteParameter CreateParameter(System.Data.DataRow schemaRow)
        {
            string sourceColumn = (string)schemaRow["BaseColumnName"];
            string name = string.Format("@{0}", sourceColumn);
            System.Data.DbType dbType = (System.Data.DbType)schemaRow["ProviderType"];
            int size = (int)schemaRow["ColumnSize"];

            return new SqliteParameter(name, dbType, size, sourceColumn);
        }

        private SqliteParameter CreateParameter(int parmIndex, System.Data.DataRow schemaRow)
        {
            string name = string.Format("@p{0}", parmIndex);
            string sourceColumn = (string)schemaRow["BaseColumnName"];
            System.Data.DbType dbType = (System.Data.DbType)schemaRow["ProviderType"];
            int size = (int)schemaRow["ColumnSize"];

            return new SqliteParameter(name, dbType, size, sourceColumn);
        }

        private string GetQuotedString(string value)
        {
            if (value == string.Empty || value == null)
                return value;
            if (string.IsNullOrEmpty(_quotePrefix) && string.IsNullOrEmpty(_quoteSuffix))
                return value;
            return string.Format("{0}{1}{2}", _quotePrefix, value, _quoteSuffix);
        }

        private void RowUpdatingHandler(
            object sender, 
            System.Data.Common.RowUpdatingEventArgs args
        )
        {
            if (args.Command != null)
                return;
            try
            {
                switch (args.StatementType)
                {
                    case System.Data.StatementType.Insert:
                        args.Command = GetInsertCommand();
                        break;
                    case System.Data.StatementType.Update:
                        args.Command = GetUpdateCommand();
                        break;
                    case System.Data.StatementType.Delete:
                        args.Command = GetDeleteCommand();
                        break;
                }
            }
            catch (System.Exception e)
            {
                args.Errors = e;
                args.Status = System.Data.UpdateStatus.ErrorsOccurred;
            }
        }
    }
}
