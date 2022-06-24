using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VDFMapper.VDF
{
    public class VDFStream
    {
        private BinaryReader reader;

        public VDFStream(string path)
        {
            reader = new BinaryReader(new FileStream(path, FileMode.Open));
        }

        public void Close() => reader.Close();

        public string ReadString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (true)
            {
                char c = (char)reader.ReadByte();
                if (c == '\0') break;
                stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }

        public uint ReadInteger()
        {
            return reader.ReadUInt32();
        }

        public byte ReadByte()
        {
            return reader.ReadByte();
        }
    }
}
