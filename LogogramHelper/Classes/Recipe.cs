using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogogramHelper.Classes
{
    [Serializable]
    public class Recipe
    {
        public int LogogramID { get; set; }
        public int Quantity { get; set; }

    }
}
