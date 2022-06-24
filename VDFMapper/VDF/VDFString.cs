using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VDFMapper.VDF
{
    public class VDFString : VDFBaseType
    {
        public VDFString(VDFStream stream)
        {
            Type = VDFType.String;
            Text = stream.ReadString();
        }

        public VDFString(string text)
        {
            Type = VDFType.String;
            Text = text;
        }

        public override void Write(BinaryWriter writer, string key)
        {
            writer.Write((byte)Type);
            writer.Write(Encoding.UTF8.GetBytes(key));
            writer.Write((byte)0);
            writer.Write(Encoding.UTF8.GetBytes(Text));
            writer.Write((byte)0);
        }
    }
}