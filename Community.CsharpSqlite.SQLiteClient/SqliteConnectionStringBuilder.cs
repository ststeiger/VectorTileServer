//
// Community.CsharpSqlite.SQLiteClient.SqliteConnectionStringBuilder.cs
//
// Author(s):
//   Sureshkumar T (tsureshkumar@novell.com)
//   Marek Habersack (grendello@gmail.com)
//   Stewart Adcock (stewart.adcock@medit.fr)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2007 Marek Habersack
// Copyright (C) 2012 MEDIT SA (http://medit-pharma.com)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Text;

namespace Community.CsharpSqlite.SQLiteClient
{


    // Fixes benchmark-compilation 
    /// <summary>
    ///     Controls the synchronization mode for writes to the database.
    /// </summary>
    /// <remarks>
    ///     Modes numbers and descriptions are from https://www.sqlite.org/pragma.html#pragma_synchronous.
    ///     Note that they are not identical to <see cref="System.Data.SQLite.SynchronizationModes" /> or else we
    ///     may have re-used that.
    /// </remarks>
    public enum SynchronizationModes
    {
        /// <summary>
        ///     With synchronous OFF (0), SQLite continues without syncing as soon as it has handed data off to the operating system.
        ///     If the application running SQLite crashes, the data will be safe, but the database might become corrupted if the
        ///     operating system crashes or the computer loses power before that data has been written to the disk surface.
        ///     On the other hand, commits can be orders of magnitude faster with synchronous OFF.
        /// </summary>
        Off = 0,

        /// <summary>
        ///     When synchronous is NORMAL (1), the SQLite database engine will still sync at the most critical moments,
        ///     but less often than in FULL mode. There is a very small (though non-zero) chance that a power failure at just the
        ///     wrong time could corrupt the database in NORMAL mode. But in practice, you are more likely to suffer a catastrophic
        ///     disk failure or some other unrecoverable hardware fault. Many applications choose NORMAL when in WAL mode.
        /// </summary>
        Normal = 1,

        /// <summary>
        ///     When synchronous is FULL (2), the SQLite database engine will use the xSync method of the VFS to ensure that all
        ///     content is safely written to the disk surface prior to continuing. This ensures that an operating system crash or power
        ///     failure will not corrupt the database. FULL synchronous is very safe, but it is also slower. FULL is the most commonly
        ///     used synchronous setting when not in WAL mode.
        /// </summary>
        Full = 2,

        /// <summary>
        ///     EXTRA synchronous is like FULL with the addition that the directory containing a rollback journal is synced after
        ///     that journal is unlinked to commit a transaction in DELETE mode. EXTRA provides additional durability if the commit
        ///     is followed closely by a power loss.
        /// </summary>
        Extra = 3
    }


    public sealed class SqliteConnectionStringBuilder : DbConnectionStringBuilder
    {
        private const string DEF_URI = null;
        private const Int32 DEF_MODE = 0644;
        private const Int32 DEF_VERSION = 3;
        private const Encoding DEF_ENCODING = null;
        private const Int32 DEF_BUSYTIMEOUT = 0;
        private const bool DEF_READONLY = false;
        private const bool DEF_FAILIFMISSING = false;



        public SynchronizationModes SyncMode; // Fixes benchmark-compilation 
        public int PageSize; // Fixes benchmark-compilation 




        #region // Fields
        private string _uri;
        private Int32 _mode;
        private Int32 _version;
        private Encoding _encoding;
        private Int32 _busy_timeout;
        private bool _readonly;
        private bool _failIfMissing;

        private static Dictionary<string, string> _keywords; // for mapping duplicate keywords
        #endregion // Fields

        #region Constructors
        public SqliteConnectionStringBuilder() : this(String.Empty)
        {
        }

        public SqliteConnectionStringBuilder(string connectionString)
        {
            Init();
            base.ConnectionString = connectionString;
        }

        static SqliteConnectionStringBuilder()
        {
            _keywords = new Dictionary<string, string>();
            _keywords["URI"] = "Uri";
            _keywords["DATA SOURCE"] = "Data Source";
            _keywords["DATASOURCE"] = "Data Source";
            _keywords["URI"] = "Data Source";
            _keywords["MODE"] = "Mode";
            _keywords["VERSION"] = "Version";
            _keywords["BUSY TIMEOUT"] = "Busy Timeout";
            _keywords["BUSYTIMEOUT"] = "Busy Timeout";
            _keywords["ENCODING"] = "Encoding";
            _keywords["READ ONLY"] = "Read Only";
            _keywords["READONLY"] = "Read Only";
            _keywords["FAILIFMISSING"] = "FailIfMissing";
        }
        #endregion // Constructors

        #region Properties
        public string DataSource
        {
            get { return _uri; }
            set
            {
                base["Data Source"] = value;
                _uri = value;
            }
        }

        public string Uri
        {
            get { return _uri; }
            set
            {
                base["Data Source"] = value;
                _uri = value;
            }
        }

        public Int32 Mode
        {
            get { return _mode; }
            set
            {
                base["Mode"] = value;
                _mode = value;
            }
        }

        public Int32 Version
        {
            get { return _version; }
            set
            {
                base["Version"] = value;
                _version = value;
            }
        }

        public Int32 BusyTimeout
        {
            get { return _busy_timeout; }
            set
            {
                base["Busy Timeout"] = value;
                _busy_timeout = value;
            }
        }

        public Encoding Encoding
        {
            get { return _encoding; }
            set
            {
                base["Encoding"] = value;
                _encoding = value;
            }
        }

        public override bool IsFixedSize
        {
            get { return true; }
        }

        public bool ReadOnly
        {
            get { return _readonly; }
            set
            {
                base["Read Only"] = value;
                _readonly = value;
            }
        }

        public bool FailIfMissing
        {
            get { return _failIfMissing; }
            set
            {
                base["FailIfMissing"] = value;
                _failIfMissing = value;
            }
        }

        public override object this[string keyword]
        {
            get
            {
                string mapped = MapKeyword(keyword);
                return base[mapped];
            }
            set { SetValue(keyword, value); }
        }

        public override ICollection Keys
        {
            get { return base.Keys; }
        }

        public override ICollection Values
        {
            get { return base.Values; }
        }
        #endregion // Properties

        #region Methods
        private void Init()
        {
            _uri = DEF_URI;
            _mode = DEF_MODE;
            _version = DEF_VERSION;
            _encoding = DEF_ENCODING;
            _busy_timeout = DEF_BUSYTIMEOUT;
            _readonly = DEF_READONLY;
            _failIfMissing = DEF_FAILIFMISSING;
        }

        public override void Clear()
        {
            base.Clear();
            Init();
        }

        public override bool ContainsKey(string keyword)
        {
            keyword = keyword.ToUpper().Trim();
            if (_keywords.ContainsKey(keyword))
                return base.ContainsKey(_keywords[keyword]);
            return false;
        }

        public override bool Remove(string keyword)
        {
            if (!ContainsKey(keyword))
                return false;
            this[keyword] = null;
            return true;
        }

        public override bool TryGetValue(string keyword, out object value)
        {
            if (!ContainsKey(keyword))
            {
                value = String.Empty;
                return false;
            }
            return base.TryGetValue(_keywords[keyword.ToUpper().Trim()], out value);
        }

        #endregion // Methods

        #region Private Methods
        private string MapKeyword(string keyword)
        {
            keyword = keyword.ToUpper().Trim();
            if (!_keywords.ContainsKey(keyword))
                throw new ArgumentException("Keyword not supported :" + keyword);
            return _keywords[keyword];
        }

        private void SetValue(string key, object value)
        {
            if (key == null)
                throw new ArgumentNullException("key cannot be null!");

            string mappedKey = MapKeyword(key);

            switch (mappedKey.ToUpper(CultureInfo.InvariantCulture).Trim())
            {
                case "DATA SOURCE":
                    if (value == null)
                    {
                        _uri = DEF_URI;
                        base.Remove(mappedKey);
                    }
                    else
                        this.Uri = value.ToString();
                    break;

                case "MODE":
                    if (value == null)
                    {
                        _mode = DEF_MODE;
                        base.Remove(mappedKey);
                    }
                    else
                        this.Mode = ConvertToInt32(value);
                    break;

                case "VERSION":
                    if (value == null)
                    {
                        _version = DEF_MODE;
                        base.Remove(mappedKey);
                    }
                    else
                        this.Version = ConvertToInt32(value);
                    break;

                case "BUSY TIMEOUT":
                    if (value == null)
                    {
                        _busy_timeout = DEF_BUSYTIMEOUT;
                        base.Remove(mappedKey);
                    }
                    else
                        this.BusyTimeout = ConvertToInt32(value);
                    break;

                case "ENCODING":
                    if (value == null)
                    {
                        _encoding = DEF_ENCODING;
                        base.Remove(mappedKey);
                    }
                    else if (value is string)
                    {
                        this.Encoding = Encoding.GetEncoding((string)value);
                    }
                    else
                        throw new ArgumentException("Cannot set encoding from a non-string argument");
                    break;

                case "READ ONLY":
                    if (value == null)
                    {
                        _readonly = DEF_READONLY;
                        base.Remove(mappedKey);
                    }
                    else
                        this.ReadOnly = ConvertToBoolean(value);
                    break;

                case "FAILIFMISSING":
                    if (value == null)
                    {
                        _failIfMissing = DEF_FAILIFMISSING;
                        base.Remove(mappedKey);
                    }
                    else
                        this.FailIfMissing = ConvertToBoolean(value);
                    break;

                default:
                    throw new ArgumentException("Keyword not supported :" + key);
            }
        }

        private static int ConvertToInt32(object value)
        {
            return Int32.Parse(value.ToString(), CultureInfo.InvariantCulture);
        }

        private static bool ConvertToBoolean(object value)
        {
            if (value == null)
                throw new ArgumentNullException("null value cannot be converted to boolean");
            string upper = value.ToString().ToUpper().Trim();
            if (upper == "YES" || upper == "TRUE")
                return true;
            if (upper == "NO" || upper == "FALSE")
                return false;
            throw new ArgumentException(String.Format("Invalid boolean value: {0}", value.ToString()));
        }
        #endregion // Private Methods
    }
}