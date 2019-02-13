
namespace VectorTileServer
{



    public class FontData
    {
        protected System.Collections.Generic.Dictionary<string, string> m_headers;

        public System.IO.Stream File;
        public string ContentType;


        public System.Collections.Generic.Dictionary<string, string> Headers
        {
            get
            {
                return this.m_headers;
            }
            set
            {
                const string CONTENT_TYPE = "content-type";

                System.Collections.Generic.Dictionary<string, string> cisHeaders =
                    new System.Collections.Generic.Dictionary<string, string>(value, System.StringComparer.OrdinalIgnoreCase);

                if (cisHeaders.ContainsKey(CONTENT_TYPE))
                {
                    this.ContentType = cisHeaders[CONTENT_TYPE];
                    cisHeaders.Remove(CONTENT_TYPE);
                }
                else
                    this.ContentType = "application/octet-stream";

                this.m_headers = cisHeaders;
            }
        }


        public FontData()
        { }

    }


} // End Namespace 

