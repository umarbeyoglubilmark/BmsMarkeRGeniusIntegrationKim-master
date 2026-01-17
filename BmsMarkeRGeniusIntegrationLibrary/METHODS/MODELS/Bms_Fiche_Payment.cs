using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS
{
    public class Bms_Fiche_Payment
    {
        public int LOGO_BRANCH { get; set; }
        public string LOGO_FICHE_TYPE { get; set; } = "";
        public string LOGO_CURRENCY { get; set; } = "";
        public string LOGO_BANK_OR_KS_CODE { get; set; } = "";
        public Int64 STORE { get; set; } = 0;
        public Int64 POS { get; set; } = 0;
        public string FTYPE { get; set; } = "";
        public string SALESMAN { get; set; } = "";
        public DateTime DATE_ { get; set; }
        public string CUSTOMER_CODE { get; set; } = "";
        public string CUSTOMER_NAME { get; set; } = "";
        public string DOCUMENT_NO { get; set; } = "";
        public Int64 PAYMENT_TYPE { get; set; } = 0;     
        public string PAYMENT_TYPE_DESCRIPTION { get; set; } = "";
        public string SERIAL_NO { get; set; } = "";
        public decimal PAYMENT_TOTAL { get; set; } = 0;
        public static explicit operator Bms_Fiche_Payment(DataRow v)
        {
            Bms_Fiche_Payment bms_Fiche_Detail = new Bms_Fiche_Payment();
            bms_Fiche_Detail.LOGO_BRANCH = Convert.ToInt32(v["LOGO_BRANCH"]);
            bms_Fiche_Detail.LOGO_FICHE_TYPE = v["LOGO_FICHE_TYPE"].ToString();
            bms_Fiche_Detail.LOGO_CURRENCY = v["LOGO_CURRENCY"].ToString();
            bms_Fiche_Detail.LOGO_BANK_OR_KS_CODE = v["LOGO_BANK_OR_KS_CODE"].ToString();
            bms_Fiche_Detail.STORE = Convert.ToInt64(v["STORE"]);
            bms_Fiche_Detail.POS = Convert.ToInt64(v["POS"]);
            bms_Fiche_Detail.SALESMAN = v["SALESMAN"].ToString();
            bms_Fiche_Detail.DATE_ = Convert.ToDateTime(v["DATE_"]);
            bms_Fiche_Detail.CUSTOMER_CODE = v["CUSTOMER_CODE"].ToString();
            bms_Fiche_Detail.CUSTOMER_NAME = v["CUSTOMER_NAME"].ToString();
            bms_Fiche_Detail.DOCUMENT_NO = v["DOCUMENT_NO"].ToString();     
            bms_Fiche_Detail.PAYMENT_TYPE = Convert.ToInt64(v["PAYMENT_TYPE"]);
            bms_Fiche_Detail.PAYMENT_TYPE_DESCRIPTION = v["PAYMENT_TYPE_DESCRIPTION"].ToString();
            bms_Fiche_Detail.SERIAL_NO = v["SERIAL_NO"].ToString();
            bms_Fiche_Detail.PAYMENT_TOTAL = Convert.ToDecimal(v["PAYMENT_TOTAL"]);
            return bms_Fiche_Detail;
        }
    }
}
