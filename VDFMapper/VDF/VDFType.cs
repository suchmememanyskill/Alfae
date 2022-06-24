using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDFMapper.VDF
{
    public enum VDFType
    {
        MapStart = 0x00,
        MapEnd = 0x08,
        Integer = 0x02,
        String = 0x01,
    }
}
