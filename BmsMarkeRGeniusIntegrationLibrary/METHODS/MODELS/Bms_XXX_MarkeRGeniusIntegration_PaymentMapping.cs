using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS
{
    public class Bms_XXX_MarkeRGeniusIntegration_PaymentMapping
    {
        public int Id { get; set; }
        public int Branch { get; set; }
        public string Saleman { get; set; } = "";
        public string IntegrationCode { get; set; } = "";
        public string LogoFicheType { get; set; } = ""; //CH KK, CH KK Iade, CH Tahsilat, CH Odeme, CH Borc, CH Alacak, Kasa Tahsilat, Kasa Odeme, Cek Girisi
        public string Currency { get; set; } = "";
        public string BankOrKsCode { get; set; } = "";
        public static explicit operator Bms_XXX_MarkeRGeniusIntegration_PaymentMapping(DataRow v)
        {
            return new Bms_XXX_MarkeRGeniusIntegration_PaymentMapping()
            {
                Id = Convert.ToInt32(v["Id"]),
                Branch = Convert.ToInt32(v["Branch"]),
                Saleman = v["Saleman"].ToString(),
                IntegrationCode = v["IntegrationCode"].ToString(),
                LogoFicheType = v["LogoFicheType"].ToString(),
                Currency = v["Currency"].ToString(),
                BankOrKsCode = v["BankOrKsCode"].ToString()
            };
        }
    }
}
