using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryIntegration.Model
{
    public class InstalledGameList
    { 
        public Dictionary<string, InstalledGame> Games { get; set; }
        public List<InstalledGame> GetGamesAsList() => Games.Select((x) => x.Value).ToList();
    }
}
