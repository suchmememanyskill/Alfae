using Force.Crc32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDFMapper.VDF;

namespace VDFMapper.ShortcutMap
{
    public class ShortcutEntry
    {
        public VDFMap Raw { get; private set; }

        public uint AppId { get => ReadInt("appid"); set => WriteInt("appid", value); }
        public string AppName { get => ReadString("appName"); set => WriteString("appName", value); }
        public string Exe { get => ReadString("exe"); set => WriteString("exe", value); }
        public string StartDir { get => ReadString("StartDir"); set => WriteString("StartDir", value); }
        public string Icon { get => ReadString("icon"); set => WriteString("icon", value); }
        public string ShortcutPath { get => ReadString("ShortcutPath"); set => WriteString("ShortcutPath", value); }
        public string LaunchOptions { get => ReadString("LaunchOptions"); set => WriteString("LaunchOptions", value); }
        public uint IsHidden { get => ReadInt("IsHidden"); set => WriteInt("IsHidden", value); }
        public uint AllowDesktopConfig { get => ReadInt("AllowDesktopConfig"); set => WriteInt("AllowDesktopConfig", value); }
        public uint AllowOverlay { get => ReadInt("AllowOverlay"); set => WriteInt("AllowOverlay", value); }
        public uint Openvr { get => ReadInt("openvr"); set => WriteInt("openvr", value); }
        public uint Devkit { get => ReadInt("Devkit"); set => WriteInt("Devkit", value); }
        public string DevkitGameID { get => ReadString("DevkitGameID"); set => WriteString("DevkitGameID", value); }
        public uint DevkitOverrideAppID { get => ReadInt("DevkitOverrideAppID"); set => WriteInt("DevkitOverrideAppID", value); }
        public uint LastPlayTime { get => ReadInt("LastPlayTime"); set => WriteInt("LastPlayTime", value); }

        public ShortcutEntry(VDFMap raw)
        {
            Raw = raw;

            if (raw == null)
                throw new Exception("Shortcut entry is null!");
        }

        public int GetTagsSize() => Raw.GetValue("tags").ToMap().GetSize();
        public string GetTag(int idx) => Raw.GetValue("tags").ToMap().GetValue(idx.ToString()).Text;
        public void SetTag(int idx, string value) => Raw.GetValue("tags").ToMap().GetValue(idx.ToString()).Text = value;
        public void AddTag(string value) => Raw.GetValue("tags").ToMap().Map.Add(GetTagsSize().ToString(), new VDFString(value));
        public void RemoveTag(int idx) => Raw.GetValue("tags").ToMap().RemoveFromArray(idx);
        public int GetTagIndex(string value)
        {
            for (int i = 0; i < GetTagsSize(); i++)
            {
                if (GetTag(i) == value)
                    return i;
            }

            return -1;
        }

        private uint ReadInt(string key) => Raw.GetValue(key).Integer;
        private void WriteInt(string key, uint value) => Raw.GetValue(key).Integer = value;

        private string ReadString(string key) => Raw.GetValue(key)?.Text ?? null;
        private void WriteString(string key, string value) => Raw.GetValue(key).Text = value;

        public static uint GenerateSteamGridAppId(string appName, string appTarget)
        {
            byte[] nameTargetBytes = Encoding.UTF8.GetBytes(appTarget + appName + "");
            uint crc = Crc32Algorithm.Compute(nameTargetBytes);
            uint gameId = crc | 0x80000000;

            return gameId;
        }
    }
}
