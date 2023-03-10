using LogogramHelper.Classes;
using System;
using System.Collections.Generic;

namespace LogogramHelper
{
    [Serializable]
    public class LogosAction { 
        public string Name { get; set; }
        public uint IconID { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string? Duration { get; set; }
        public string? Cast { get; set; }
        public string? Recast { get; set; }
        public List<List<Recipe>> Recipes { get; set; }
        public List<uint> Roles { get; set; }
    }
}
