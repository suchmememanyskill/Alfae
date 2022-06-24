using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VDFMapper.VDF
{
    public abstract class VDFBaseType
    {
        public VDFType Type { get; set; }
        public uint Integer { get; set; }
        public string Text { get; set; }
        public Dictionary<String, VDFBaseType> Map { get; protected set; }

        public abstract void Write(BinaryWriter writer, string key);

        public VDFMap ToMap()
        {
            if (GetType() == typeof(VDFMap))
            {
                return (VDFMap)this;
            }

            return null;
        }
    }
}
