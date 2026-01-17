using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS
{
    public class Bms_Fiche_Header
    {
        public string FICHE_ID { get; set; } = "";
        public DateTime DATE_ { get; set; }
        public int BRANCH { get; set; } = 0;
        public Int64 POS { get; set; } = 0;
        public string FTYPE { get; set; } = "";
        public string DOCUMENT_NO { get; set; } = "";
        public string CUSTOMER_CODE { get; set; } = "";
        public string CUSTOMER_NAME { get; set; } = "";
        public static explicit operator Bms_Fiche_Header(DataRow v)
        {
            Bms_Fiche_Header bms_Fiche_Header = new Bms_Fiche_Header();
            try { bms_Fiche_Header.FICHE_ID = v["FICHE_ID"].ToString(); } catch { }
            try { bms_Fiche_Header.DATE_ = Convert.ToDateTime(v["DATE_"]); } catch { }
            try { bms_Fiche_Header.BRANCH = Convert.ToInt32(v["BRANCH"]); } catch { }
            try { bms_Fiche_Header.POS = Convert.ToInt64(v["POS"]); } catch { }
            try { bms_Fiche_Header.FTYPE = v["FTYPE"].ToString(); } catch { }
            try { bms_Fiche_Header.DOCUMENT_NO = v["DOCUMENT_NO"].ToString(); } catch { }
            try { bms_Fiche_Header.CUSTOMER_CODE = v["CUSTOMER_CODE"].ToString(); } catch { }
            try { bms_Fiche_Header.CUSTOMER_NAME = v["CUSTOMER_NAME"].ToString(); } catch { }
            return bms_Fiche_Header;
        }
    }
}
