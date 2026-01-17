using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS
{
    public class Bms_XXX_MarkeRGeniusIntegration_Default
    {
        public int Id { get; set; }
        public string Description { get; set; } = "";
        public string Value { get; set; } = "";
        public static explicit operator Bms_XXX_MarkeRGeniusIntegration_Default(DataRow v)
        {
            return new Bms_XXX_MarkeRGeniusIntegration_Default()
            {
                Description = v["Description"].ToString(),
                Value = v["Value"].ToString(),
            };
        }
    }
}
