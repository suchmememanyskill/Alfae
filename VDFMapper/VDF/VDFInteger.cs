using System.Text;

namespace VDFMapper.VDF
{
    public class VDFInteger : VDFBaseType
    {
        public VDFInteger(VDFStream stream)
        {
            Type = VDFType.Integer;
            Integer = stream.ReadInteger();
        }

        public override void Write(BinaryWriter writer, string key)
        {
            writer.Write((byte)Type);
            writer.Write(Encoding.UTF8.GetBytes(key));
            writer.Write((byte)0);
            writer.Write(Integer);
        }

        public VDFInteger(uint value)
        {
            Type = VDFType.Integer;
            Integer = value;
        }
    }
}
