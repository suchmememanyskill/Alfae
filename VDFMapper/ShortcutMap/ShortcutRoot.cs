using VDFMapper.VDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDFMapper.ShortcutMap
{
    public class ShortcutRoot
    {
        public VDFMap root { get; private set; }
        private VDFMap GetShortcutMap() => root.GetValue("shortcuts").ToMap();

        public ShortcutRoot(VDFMap root)
        {
            this.root = root;
        }

        public int GetSize() => GetShortcutMap().GetSize();

        public ShortcutEntry GetEntry(int entry) => new ShortcutEntry(GetShortcutMap().ToMap().GetValue(entry.ToString()).ToMap());

        public ShortcutEntry AddEntry()
        {
            VDFMap entry = new VDFMap();
            entry.FillWithDefaultShortcutEntry();

            GetShortcutMap().Map.Add(GetSize().ToString(), entry);
            return new ShortcutEntry(entry);
        }

        public void RemoveEntry(int idx)
        {
            GetShortcutMap().RemoveFromArray(idx);
        }
    }
}
