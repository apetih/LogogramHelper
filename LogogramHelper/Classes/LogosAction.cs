using LogogramHelper.Classes;
using System;
using System.Collections.Generic;

namespace LogogramHelper
{
    [Serializable]
    public class LogosAction { 
        public uint Id { get; set; }
        public uint IconID { get; set; }
        public string? Duration { get; set; }
        public string? Cast { get; set; }
        public string? Recast { get; set; }
        public List<List<Recipe>> Recipes { get; set; }
        public List<uint> Roles { get; set; }
    }
}
