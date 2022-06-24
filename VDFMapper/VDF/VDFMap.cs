using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VDFMapper.VDF
{
    public class VDFMap : VDFBaseType
    {
        private static Random random = new Random();

        public VDFMap(VDFStream stream)
        {
            Type = VDFType.MapStart;
            Map = new Dictionary<string, VDFBaseType>();

            while (true)
            {
                byte op = stream.ReadByte();

                if (op == (byte)VDFType.MapEnd)
                    break;

                string key = stream.ReadString();

                VDFBaseType value;
                if (op == (byte)VDFType.MapStart)
                    value = new VDFMap(stream);
                else if (op == (byte)VDFType.Integer)
                    value = new VDFInteger(stream);
                else if (op == (byte)VDFType.String)
                    value = new VDFString(stream);
                else
                    throw new Exception("Unknown opcode");

                Map.Add(key, value);
            }
        }

        public VDFMap()
        {
            Type = VDFType.MapStart;
            Map = new Dictionary<string, VDFBaseType>();
        }

        public void FillWithDefaultShortcutEntry()
        {
            Map.Add("appid", new VDFInteger((uint)random.Next()));
            Map.Add("appName", new VDFString("appName"));
            Map.Add("exe", new VDFString(""));
            Map.Add("StartDir", new VDFString("."));
            Map.Add("icon", new VDFString(""));
            Map.Add("ShortcutPath", new VDFString(""));
            Map.Add("LaunchOptions", new VDFString(""));
            Map.Add("isHidden", new VDFInteger(0));
            Map.Add("AllowDesktopConfig", new VDFInteger(1));
            Map.Add("AllowOverlay", new VDFInteger(1));
            Map.Add("openvr", new VDFInteger(0));
            Map.Add("Devkit", new VDFInteger(0));
            Map.Add("DevkitGameID", new VDFString("0"));
            Map.Add("DevkitOverrideAppID", new VDFInteger(0));
            Map.Add("LastPlayTime", new VDFInteger(0));
            Map.Add("tags", new VDFMap());
        }

        public void RemoveFromArray(int idx)
        {
            Map.Remove(idx.ToString());

            Dictionary<string, VDFBaseType> newMap = new Dictionary<string, VDFBaseType>();

            int i = 0;
            foreach (KeyValuePair<string, VDFBaseType> kv in Map)
            {
                newMap.Add(i.ToString(), kv.Value);
                i++;
            }

            Map = newMap;
        }

        public override void Write(BinaryWriter writer, string key)
        {
            if (key != null)
            {
                writer.Write((byte)Type);
                writer.Write(Encoding.UTF8.GetBytes(key));
                writer.Write((byte)0);
            }
            
            foreach (KeyValuePair<String, VDFBaseType> keyValue in Map)
            {
                keyValue.Value.Write(writer, keyValue.Key);
            }

            writer.Write((byte)VDFType.MapEnd);
        }

        public VDFBaseType GetValue(string key)
        {
            if (Map.ContainsKey(key))
                return Map[key];

            string temp = char.ToUpper(key[0]) + key.Substring(1);

            if (Map.ContainsKey(temp))
                return Map[temp];

            return null;
        }
        public int GetSize() => Map.Count;
    }
}
