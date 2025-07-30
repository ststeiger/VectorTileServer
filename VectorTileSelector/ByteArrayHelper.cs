
namespace VectorTileSelector
{


    internal class ByteArrayHelper
    {

        private static readonly uint[] _lookup32 = CreateLookup32();



        private static uint[] CreateLookup32()
        {
            uint[] result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        } // End Function CreateLookup32 


        internal static string ByteArrayToHex(byte[] bytes)
        {
            uint[] lookup32 = _lookup32;
            char[] result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                uint val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            } // Next i 

            return new string(result);
        } // End Function ByteArrayToHex 


        // Same as Byte-Array to hex 
        private static string ByteToHexBitFiddle(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        } // End Function ByteToHexBitFiddle 


        private static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        } // End Function GetHexVal 


        public static byte[] StringToByteArray(string hex)
        {
            if (hex.StartsWith("0x", System.StringComparison.InvariantCultureIgnoreCase))
                hex = hex.Substring(2);

            if (hex.Length % 2 == 1)
                throw new System.Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        } // End Function StringToByteArray 


    } // End Class ByteArrayHelper 


} // End Namespace SQLScriptGenerator 
