
// https://www.codeproject.com/Tips/823670/Csharp-Light-and-Fast-CSV-Parser
namespace VectorTileSelector
{


    public static class CsvParser
    {
        private static System.Tuple<T, System.Collections.Generic.IEnumerable<T>> 
            HeadAndTail<T>(this System.Collections.Generic.IEnumerable<T> source)
        {
            if (source == null)
                throw new System.ArgumentNullException("source");
            System.Collections.Generic.IEnumerator<T> en = source.GetEnumerator();
            en.MoveNext();
            return System.Tuple.Create(en.Current, EnumerateTail(en));
        }

        private static System.Collections.Generic.IEnumerable<T> EnumerateTail<T>(System.Collections.Generic.IEnumerator<T> en)
        {
            while (en.MoveNext()) yield return en.Current;
        }

        public static System.Collections.Generic.IEnumerable<
            System.Collections.Generic.IList<string>
            > Parse(string content, char delimiter, char qualifier)
        {
            using (System.IO.StringReader reader = new System.IO.StringReader(content))
                return Parse(reader, delimiter, qualifier);
        }

        public static System.Tuple<
            System.Collections.Generic.IList<string>, 
            System.Collections.Generic.IEnumerable<
                System.Collections.Generic.IList<string>
                >
            > ParseHeadAndTail(System.IO.TextReader reader, char delimiter, char qualifier)
        {
            return HeadAndTail(Parse(reader, delimiter, qualifier));
        }


        public static System.Collections.Generic.List<
            System.Collections.Generic.List<string>
            > ParseSimple(string content, char delimiter, char qualifier)
        {
            using (System.IO.StringReader reader = new System.IO.StringReader(content))
                return ParseSimple(reader, delimiter, qualifier);
        }

       
        public static System.Collections.Generic.List<
            System.Collections.Generic.List<string>
        > ParseFileSimple(string filePath, char delimiter, char qualifier)
        {
            System.Collections.Generic.List<System.Collections.Generic.List<string>> result = null;

            using (System.IO.TextReader reader = new System.IO.StreamReader(filePath))
            {
                result = ParseSimple(reader, delimiter, qualifier);
                // Do something with result...
            }

            return result;
        }

        public static System.Collections.Generic.List<
            System.Collections.Generic.List<string>
            > ParseSimple(System.IO.TextReader reader, char delimiter, char qualifier)
        {
            System.Collections.Generic.List<System.Collections.Generic.List<string>> ls = 
                new System.Collections.Generic.List<System.Collections.Generic.List<string>>();

            bool inQuote = false;
            System.Collections.Generic.List<string> record = 
                new System.Collections.Generic.List<string>();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            while (reader.Peek() != -1)
            {
                char readChar = (char)reader.Read();

                if (readChar == '\n' || (readChar == '\r' && (char)reader.Peek() == '\n'))
                {
                    // If it's a \r\n combo consume the \n part and throw it away.
                    if (readChar == '\r')
                        reader.Read();

                    if (inQuote)
                    {
                        if (readChar == '\r')
                            sb.Append('\r');
                        sb.Append('\n');
                    }
                    else
                    {
                        if (record.Count > 0 || sb.Length > 0)
                        {
                            record.Add(sb.ToString());
                            sb.Clear();
                        }

                        if (record.Count > 0)
                        {
                            // yield return record;
                            ls.Add(record);
                        }


                        record = new System.Collections.Generic.List<string>(record.Count);
                    }
                }
                else if (sb.Length == 0 && !inQuote)
                {
                    if (readChar == qualifier)
                        inQuote = true;
                    else if (readChar == delimiter)
                    {
                        record.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (char.IsWhiteSpace(readChar))
                    {
                        // Ignore leading whitespace
                    }
                    else
                        sb.Append(readChar);
                }
                else if (readChar == delimiter)
                {
                    if (inQuote)
                        sb.Append(delimiter);
                    else
                    {
                        record.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else if (readChar == qualifier)
                {
                    if (inQuote)
                    {
                        if ((char)reader.Peek() == qualifier)
                        {
                            reader.Read();
                            sb.Append(qualifier);
                        }
                        else
                            inQuote = false;
                    }
                    else
                        sb.Append(readChar);
                }
                else
                    sb.Append(readChar);
            }

            if (record.Count > 0 || sb.Length > 0)
                record.Add(sb.ToString());

            if (record.Count > 0)
            {
                // yield return record;
                ls.Add(record);
            }

            return ls;
        }


        public static System.Collections.Generic.IEnumerable<
            System.Collections.Generic.IList<string>
            > Parse(System.IO.TextReader reader, char delimiter, char qualifier)
        {
            bool inQuote = false;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            System.Collections.Generic.List<string> record =
                new System.Collections.Generic.List<string>();

            while (reader.Peek() != -1)
            {
                char readChar = (char)reader.Read();

                if (readChar == '\n' || (readChar == '\r' && (char)reader.Peek() == '\n'))
                {
                    // If it's a \r\n combo consume the \n part and throw it away.
                    if (readChar == '\r')
                        reader.Read();

                    if (inQuote)
                    {
                        if (readChar == '\r')
                            sb.Append('\r');
                        sb.Append('\n');
                    }
                    else
                    {
                        if (record.Count > 0 || sb.Length > 0)
                        {
                            record.Add(sb.ToString());
                            sb.Clear();
                        }

                        if (record.Count > 0)
                            yield return record;

                        record = new System.Collections.Generic.List<string>(record.Count);
                    }
                }
                else if (sb.Length == 0 && !inQuote)
                {
                    if (readChar == qualifier)
                        inQuote = true;
                    else if (readChar == delimiter)
                    {
                        record.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (char.IsWhiteSpace(readChar))
                    {
                        // Ignore leading whitespace
                    }
                    else
                        sb.Append(readChar);
                }
                else if (readChar == delimiter)
                {
                    if (inQuote)
                        sb.Append(delimiter);
                    else
                    {
                        record.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else if (readChar == qualifier)
                {
                    if (inQuote)
                    {
                        if ((char)reader.Peek() == qualifier)
                        {
                            reader.Read();
                            sb.Append(qualifier);
                        }
                        else
                            inQuote = false;
                    }
                    else
                        sb.Append(readChar);
                }
                else
                    sb.Append(readChar);
            }

            if (record.Count > 0 || sb.Length > 0)
                record.Add(sb.ToString());

            if (record.Count > 0)
                yield return record;
        }
    }

}
