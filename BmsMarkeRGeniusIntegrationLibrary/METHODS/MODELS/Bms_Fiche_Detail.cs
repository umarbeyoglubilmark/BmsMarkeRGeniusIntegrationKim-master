using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS
{
    public class Bms_Fiche_Detail
    {
        public DateTime DATE_ { get; set; }
        public int BRANCH { get; set; }
        public Int64 POS { get; set; }
        public string FTYPE { get; set; }
        public string SALESMAN { get; set; }
        public string ITEMCODE { get; set; }
        public string ITEMNAME { get; set; }
        public string ITEMUNIT { get; set; }
        public double QUANTITY { get; set; }
        public decimal PRICE { get; set; }
        public decimal LINETOTAL { get; set; }
        public decimal LINE_DISCOUNT_TOTAL { get; set; }
        public decimal DISCOUNT_TOTAL { get; set; }
        public decimal INDIRECT_DISCOUNT_TOTAL { get; set; }
        public decimal CAMPAIGN_DISCOUNT { get; set; }
        public decimal CHEQUE_DISCOUNT { get; set; }
        public static explicit operator Bms_Fiche_Detail(DataRow v)
        {
            Bms_Fiche_Detail bms_Fiche_Detail = new Bms_Fiche_Detail();
            bms_Fiche_Detail.DATE_ = Convert.ToDateTime(v["DATE_"]);
            bms_Fiche_Detail.BRANCH = Convert.ToInt32(v["BRANCH"]);
            bms_Fiche_Detail.POS = Convert.ToInt64(v["POS"]);
            bms_Fiche_Detail.FTYPE = v["FTYPE"].ToString();
            bms_Fiche_Detail.SALESMAN = v["SALESMAN"].ToString();
            bms_Fiche_Detail.ITEMCODE = v["ITEMCODE"].ToString();
            bms_Fiche_Detail.ITEMNAME = v["ITEMNAME"].ToString();
            bms_Fiche_Detail.ITEMUNIT = v["ITEMUNIT"].ToString();
            bms_Fiche_Detail.QUANTITY = Convert.ToDouble(v["QUANTITY"]);
            bms_Fiche_Detail.PRICE = Convert.ToDecimal(v["PRICE"]);
            bms_Fiche_Detail.LINETOTAL = Convert.ToDecimal(v["LINETOTAL"]);
            bms_Fiche_Detail.LINE_DISCOUNT_TOTAL = Convert.ToDecimal(v["LINE_DISCOUNT_TOTAL"]);
            bms_Fiche_Detail.DISCOUNT_TOTAL = Convert.ToDecimal(v["DISCOUNT_TOTAL"]);
            bms_Fiche_Detail.INDIRECT_DISCOUNT_TOTAL = Convert.ToDecimal(v["INDIRECT_DISCOUNT_TOTAL"]);
            bms_Fiche_Detail.CAMPAIGN_DISCOUNT = Convert.ToDecimal(v["CAMPAIGN_DISCOUNT"]);
            bms_Fiche_Detail.CHEQUE_DISCOUNT = Convert.ToDecimal(v["CHEQUE_DISCOUNT"]);
            return bms_Fiche_Detail;
        }
    }
}
