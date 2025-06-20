﻿//
// Community.CsharpSqlite.SQLiteClient.SqliteDataReader.cs
//
// Provides a means of reading a forward-only stream of rows from a Sqlite 
// database file.
//
// Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//            Everaldo Canuto  <everaldo_canuto@yahoo.com.br>
//	          Joshua Tauberer <tauberer@for.net>
//
// Copyright (C) 2002  Vladimir Vukicevic
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

    using SQLite.FullyManaged.Engine;

    public class SqliteDataReader 
        : System.Data.Common.DbDataReader, 
        System.Data.IDataReader, 
        System.IDisposable, 
        System.Data.IDataRecord
    {

        private SqliteCommand command;
        private System.Collections.Generic.List<object[]> rows;
        private string[] columns;
        private System.Collections.Generic.Dictionary<string, object> column_names_sens, column_names_insens;
        private int current_row;
        private bool closed;
        private bool reading;
        private int records_affected;
        private string[] decltypes;


        internal SqliteDataReader(SqliteCommand cmd, Sqlite3.Vdbe pVm, int version)
        {
            command = cmd;
            rows = new System.Collections.Generic.List<object[]>();
            column_names_sens = new System.Collections.Generic.Dictionary<string, object>();
            column_names_insens = new System.Collections.Generic.Dictionary<string, object>(System.StringComparer.OrdinalIgnoreCase);
            closed = false;
            current_row = -1;
            reading = true;
            ReadpVm(pVm, version, cmd);
            ReadingDone();
        }


        public override int Depth
        {
            get { return 0; }
        }

        public override int FieldCount
        {
            get { return columns.Length; }
        }

        public override object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }

        public override object this[int i]
        {
            get { return GetValue(i); }
        }

        public override bool IsClosed
        {
            get { return closed; }
        }

        public override int RecordsAffected
        {
            get { return records_affected; }
        }


        internal void ReadpVm(Sqlite3.Vdbe pVm, int version, SqliteCommand cmd)
        {
            int pN;
            System.IntPtr pazValue;
            System.IntPtr pazColName;
            bool first = true;

            int[] declmode = null;

            while (true)
            {
                bool hasdata = cmd.ExecuteStatement(pVm, out pN, out pazValue, out pazColName);

                // For the first row, get the column information
                if (first)
                {
                    first = false;

                    if (version == 3)
                    {
                        // A decltype might be null if the type is unknown to sqlite.
                        decltypes = new string[pN];
                        declmode = new int[pN]; // 1 == integer, 2 == datetime
                        for (int i = 0; i < pN; i++)
                        {
                            string decl = Sqlite3.sqlite3_column_decltype(pVm, i);
                            if (decl != null)
                            {
                                decltypes[i] = decl.ToLower(
#if !SQLITE_WINRT
              System.Globalization.CultureInfo.InvariantCulture
#endif
);
                                if (decltypes[i] == "int" || decltypes[i] == "integer")
                                    declmode[i] = 1;
                                else if (decltypes[i] == "date" || decltypes[i] == "datetime")
                                    declmode[i] = 2;
                            }
                        }
                    }

                    columns = new string[pN];
                    for (int i = 0; i < pN; i++)
                    {
                        string colName;
                        //if (version == 2) {
                        //	IntPtr fieldPtr = Marshal.ReadIntPtr (pazColName, i*IntPtr.Size);
                        //	colName = Sqlite.HeapToString (fieldPtr, ((SqliteConnection)cmd.Connection).Encoding);
                        //} else {
                        colName = Sqlite3.sqlite3_column_name(pVm, i);
                        //}
                        columns[i] = colName;
                        column_names_sens[colName] = i;
                        column_names_insens[colName] = i;
                    }
                }

                if (!hasdata) break;

                object[] data_row = new object[pN];
                for (int i = 0; i < pN; i++)
                {
                    /*
                    if (version == 2) {
						IntPtr fieldPtr = Marshal.ReadIntPtr (pazValue, i*IntPtr.Size);
						data_row[i] = Sqlite.HeapToString (fieldPtr, ((SqliteConnection)cmd.Connection).Encoding);
					} else {
                    */
                    switch (Sqlite3.sqlite3_column_type(pVm, i))
                    {
                        case 1:
                            long val = Sqlite3.sqlite3_column_int64(pVm, i);

                            // If the column was declared as an 'int' or 'integer', let's play
                            // nice and return an int (version 3 only).
                            if (declmode[i] == 1 && val >= int.MinValue && val <= int.MaxValue)
                                data_row[i] = (int)val;

#if !SQLITE_WINRT
                            // Or if it was declared a date or datetime, do the reverse of what we
                            // do for DateTime parameters.
                            else if (declmode[i] == 2)
                                data_row[i] = System.DateTime.FromFileTime(val);
#endif
                            else
                                data_row[i] = val;

                            break;
                        case 2:
                            data_row[i] = Sqlite3.sqlite3_column_double(pVm, i);
                            break;
                        case 3:
                            data_row[i] = Sqlite3.sqlite3_column_text(pVm, i);

                            // If the column was declared as a 'date' or 'datetime', let's play
                            // nice and return a DateTime (version 3 only).
                            if (declmode[i] == 2)
                                if (data_row[i] == null) data_row[i] = null;
                                else data_row[i] = System.DateTime.Parse((string)data_row[i], System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case 4:
                            //int blobbytes = Sqlite3.sqlite3_column_bytes16 (pVm, i);
                            byte[] blob = Sqlite3.sqlite3_column_blob(pVm, i);
                            //byte[] blob = new byte[blobbytes];
                            //Marshal.Copy (blobptr, blob, 0, blobbytes);
                            data_row[i] = blob;
                            break;
                        case 5:
                            data_row[i] = null;
                            break;
                        default:
                            throw new System.Exception("FATAL: Unknown sqlite3_column_type");
                            //}
                    }
                }

                rows.Add(data_row);
            }
        }
        internal void ReadingDone()
        {
            records_affected = command.NumChanges();
            reading = false;
        }


        public override void Close()
        {
            closed = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Close();
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return new System.Data.Common.DbEnumerator(this);
        }


#if !(SQLITE_SILVERLIGHT || SQLITE_WINRT)
        public override System.Data.DataTable GetSchemaTable()
        {
            System.Data.DataTable dataTableSchema = new System.Data.DataTable();

            dataTableSchema.Columns.Add("ColumnName", typeof(string));
            dataTableSchema.Columns.Add("ColumnOrdinal", typeof(System.Int32));
            dataTableSchema.Columns.Add("ColumnSize", typeof(System.Int32));
            dataTableSchema.Columns.Add("NumericPrecision", typeof(System.Int32));
            dataTableSchema.Columns.Add("NumericScale", typeof(System.Int32));
            dataTableSchema.Columns.Add("IsUnique", typeof(bool));
            dataTableSchema.Columns.Add("IsKey", typeof(bool));
            dataTableSchema.Columns.Add("BaseCatalogName", typeof(string));
            dataTableSchema.Columns.Add("BaseColumnName", typeof(string));
            dataTableSchema.Columns.Add("BaseSchemaName", typeof(string));
            dataTableSchema.Columns.Add("BaseTableName", typeof(string));
            dataTableSchema.Columns.Add("DataType", typeof(System.Type));
            dataTableSchema.Columns.Add("AllowDBNull", typeof(bool));
            dataTableSchema.Columns.Add("ProviderType", typeof(System.Int32));
            dataTableSchema.Columns.Add("IsAliased", typeof(bool));
            dataTableSchema.Columns.Add("IsExpression", typeof(bool));
            dataTableSchema.Columns.Add("IsIdentity", typeof(bool));
            dataTableSchema.Columns.Add("IsAutoIncrement", typeof(bool));
            dataTableSchema.Columns.Add("IsRowVersion", typeof(bool));
            dataTableSchema.Columns.Add("IsHidden", typeof(bool));
            dataTableSchema.Columns.Add("IsLong", typeof(bool));
            dataTableSchema.Columns.Add("IsReadOnly", typeof(bool));

            dataTableSchema.BeginLoadData();
            for (int i = 0; i < this.FieldCount; i += 1)
            {

                System.Data.DataRow schemaRow = dataTableSchema.NewRow();

                schemaRow["ColumnName"] = columns[i];
                schemaRow["ColumnOrdinal"] = i;
                schemaRow["ColumnSize"] = 0;
                schemaRow["NumericPrecision"] = 0;
                schemaRow["NumericScale"] = 0;
                schemaRow["IsUnique"] = false;
                schemaRow["IsKey"] = false;
                schemaRow["BaseCatalogName"] = "";
                schemaRow["BaseColumnName"] = columns[i];
                schemaRow["BaseSchemaName"] = "";
                schemaRow["BaseTableName"] = "";
                schemaRow["DataType"] = typeof(string);
                schemaRow["AllowDBNull"] = true;
                schemaRow["ProviderType"] = 0;
                schemaRow["IsAliased"] = false;
                schemaRow["IsExpression"] = false;
                schemaRow["IsIdentity"] = false;
                schemaRow["IsAutoIncrement"] = false;
                schemaRow["IsRowVersion"] = false;
                schemaRow["IsHidden"] = false;
                schemaRow["IsLong"] = false;
                schemaRow["IsReadOnly"] = false;

                dataTableSchema.Rows.Add(schemaRow);
                schemaRow.AcceptChanges();
            }
            dataTableSchema.EndLoadData();

            return dataTableSchema;
        }
#endif
        public override bool NextResult()
        {
            current_row++;

            return (current_row < rows.Count);
        }

        public override bool Read()
        {
            return NextResult();
        }


        public override bool GetBoolean(int i)
        {
            int result = System.Convert.ToInt32(((object[])rows[current_row])[i]);
            return System.Convert.ToBoolean(result);
        }

        public override byte GetByte(int i)
        {
            return System.Convert.ToByte(((object[])rows[current_row])[i]);
        }


        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            byte[] data = (byte[])(((object[])rows[current_row])[i]);
            if (buffer != null)
                System.Array.Copy(data, (int)fieldOffset, buffer, bufferOffset, length);
#if (SQLITE_SILVERLIGHT || WINDOWS_MOBILE || SQLITE_WINRT)
            return data.Length - fieldOffset;
#else
            return data.LongLength - fieldOffset;
#endif
        }


        public override System.IO.Stream GetStream(int ordinal)
        {
            long dataLength = this.GetBytes(ordinal, 0, null, 0, 0); // get the length 
            byte[] buffer = new byte[dataLength];
            this.GetBytes(ordinal, 0, buffer, 0, (int)dataLength); // get the data 

            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(buffer);
            return memoryStream;
        }


        public override char GetChar(int i)
        {
            return System.Convert.ToChar(((object[])rows[current_row])[i]);
        }

        public override long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            char[] data = (char[])(((object[])rows[current_row])[i]);
            if (buffer != null)
                System.Array.Copy(data, (int)fieldOffset, buffer, bufferOffset, length);
#if (SQLITE_SILVERLIGHT || WINDOWS_MOBILE || SQLITE_WINRT)
            return data.Length - fieldOffset;
#else
            return data.LongLength - fieldOffset;
#endif
        }

        public override string GetDataTypeName(int i)
        {
            if (decltypes != null && decltypes[i] != null)
                return decltypes[i];
            return "text"; // SQL Lite data type
        }

        public override System.DateTime GetDateTime(int i)
        {
            return System.Convert.ToDateTime(((object[])rows[current_row])[i]);
        }

        public override decimal GetDecimal(int i)
        {
            return System.Convert.ToDecimal(((object[])rows[current_row])[i]);
        }

        public override double GetDouble(int i)
        {
            return System.Convert.ToDouble(((object[])rows[current_row])[i]);
        }

        public override System.Type GetFieldType(int i)
        {
            int row = current_row;
            if (row == -1 && rows.Count == 0) return typeof(string);
            if (row == -1) row = 0;
            object element = ((object[])rows[row])[i];
            if (element != null)
                return element.GetType();
            else
                return typeof(string);

            // Note that the return value isn't guaranteed to
            // be the same as the rows are read if different
            // types of information are stored in the column.
        }

        public override float GetFloat(int i)
        {
            return System.Convert.ToSingle(((object[])rows[current_row])[i]);
        }

        public override System.Guid GetGuid(int i)
        {
            object value = GetValue(i);
            if (!(value is System.Guid))
            {
                if (value is System.DBNull)
                    throw new SqliteExecutionException("Column value must not be null");
                throw new System.InvalidCastException("Type is " + value.GetType().ToString());
            }
            return ((System.Guid)value);
        }

        public override short GetInt16(int i)
        {
            return System.Convert.ToInt16(((object[])rows[current_row])[i]);
        }

        public override int GetInt32(int i)
        {
            return System.Convert.ToInt32(((object[])rows[current_row])[i]);
        }

        public override long GetInt64(int i)
        {
            return System.Convert.ToInt64(((object[])rows[current_row])[i]);
        }

        public override string GetName(int i)
        {
            return columns[i];
        }

        public override int GetOrdinal(string name)
        {
            object v = column_names_sens.ContainsKey(name) ? column_names_sens[name] : null;
            if (v == null)
                v = column_names_insens.ContainsKey(name) ? column_names_insens[name] : null;
            if (v == null)
                throw new System.ArgumentException("Column does not exist.");
            return (int)v;
        }

        public override string GetString(int i)
        {
            if (((object[])rows[current_row])[i] != null)
                return (((object[])rows[current_row])[i]).ToString();
            else return null;
        }

        public override object GetValue(int i)
        {
            return ((object[])rows[current_row])[i];
        }

        public override int GetValues(object[] values)
        {
            int num_to_fill = System.Math.Min(values.Length, columns.Length);
            for (int i = 0; i < num_to_fill; i++)
            {
                if (((object[])rows[current_row])[i] != null)
                {
                    values[i] = ((object[])rows[current_row])[i];
                }
                else
                {
                    values[i] = System.DBNull.Value;
                }
            }
            return num_to_fill;
        }

        public override bool IsDBNull(int i)
        {
            return (((object[])rows[current_row])[i] == null);
        }

        public override bool HasRows
        {
            get { return rows.Count > 0; }
        }

        public override int VisibleFieldCount
        {
            get { return FieldCount; }
        }

    }
}
