using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS
{
    public class Bms_XXX_MarkeRGeniusIntegration_Mapping
    {
        public int Id { get; set; }
        public string LogoBranch { get; set; } = "";
        public string PosBranch { get; set; } = "";
        public string Ip { get; set; } = "";
        public static explicit operator Bms_XXX_MarkeRGeniusIntegration_Mapping(DataRow v)
        {
            return new Bms_XXX_MarkeRGeniusIntegration_Mapping()
            {
                LogoBranch = v["LogoBranch"].ToString(),
                PosBranch = v["PosBranch"].ToString(),
                Ip = v["Ip"].ToString()
            };
        }
    }
}
