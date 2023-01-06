using System.Text;

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
            List<byte> text = new();
            while (true)
            {
                byte c = ReadByte();
                if (c == 0) break;
                text.Add(c);
            }

            return Encoding.UTF8.GetString(text.ToArray());
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
