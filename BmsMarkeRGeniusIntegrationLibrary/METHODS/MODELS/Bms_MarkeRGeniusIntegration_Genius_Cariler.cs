using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS
{
    public class GENIUS3_CUSTOMER
    {
        public long ID { get; set; }
        public string CODE { get; set; }
        public string NAME { get; set; }
        public float DISCOUNT_LIMIT { get; set; } = 0;
        public float DISCOUNT_PERCENT { get; set; } = 0;
        public short DISCOUNT_FLAG { get; set; } = 0;
        public short CK_STOCK_PRICE_NO { get; set; } = 0;
        public decimal BONUS { get; set; } = 0;
        //public string TC_IDENTITY_NO { get; set; }
        public int FK_STORE_TYPE { get; set; } = 0;
        public string PARAM1 { get; set; }
        //public string PARAM2 { get; set; }
        public DateTime CREATE_DATE { get; set; }
        public int FK_USER_CREATE { get; set; } = 0;
        public DateTime MODIFY_DATE { get; set; }
        public int FK_USER_MODIFY { get; set; } = 0;
        public int FK_STORE_LU { get; set; } = 0;
        public int FK_STORE_LS { get; set; } = 0;
        public int UPDATESEQ { get; set; } = 1;
    }

    public class GENIUS3_CUSTOMER_EXTENSION
    {
        public long ID { get; set; }
        public long FK_CUSTOMER { get; set; }
        public string CUST_PARAMETER { get; set; }
        public string TAX_OFFICE { get; set; } = "";
        public string TAX_NUMBER { get; set; } = "";
        public string SSK_NO { get; set; } = "";
        public string ID_FATHER_NAME { get; set; } = "";
        public string ID_MOTHER_NAME { get; set; } = "";
        public string MAIDEN_NAME { get; set; } = "";
        public DateTime ID_DATE_OF_BIRTH { get; set; }
        public string ID_PLACE_OF_BIRTH { get; set; } = "";
        public Int16 SEX { get; set; } = 0;
        public Int16 MARITAL_STATUS { get; set; } = 0;
        public int FK_LOCATION_HOME { get; set; } = 1;
        public int FK_LOCATION_WORK { get; set; } = 1;
        public int FK_LOCATION_LETTER { get; set; } = 1;
        public string ADDRESS_HOME { get; set; } = "";
        public string ADDRESS_WORK { get; set; } = "";
        public string ADDRESS_LETTER { get; set; } = "";
        public string URL { get; set; } = "";
        public int FK_BANK { get; set; } = 0;
        public int FK_ACADEMY { get; set; } = 0;
        public int FK_JOB { get; set; } = 0;
        public string ID_GIVEN_PLACE { get; set; } = "";
        public int ID_FK_CITY { get; set; } = 1;
        public string ID_TOWN { get; set; } = "";
        public string ID_VILLAGE { get; set; } = "";
        public string ID_REGISTER_NO { get; set; } = "";
        public string ID_CILT_NO { get; set; } = "";
        public string ID_AILESIRA_NO { get; set; } = "";
        public string ID_SIRA_NO { get; set; } = "";
        public DateTime ID_GIVEN_DATE { get; set; }
        public string ID_SERIAL_NO { get; set; } = "";
        public string NOTES { get; set; } = "";
        public string CUSTOMER_MESSAGE_1 { get; set; } = "";
        public string CUSTOMER_MESSAGE_2 { get; set; } = "";
        public string HOME_TOWN { get; set; } = "";
        public int FK_HOME_CITY { get; set; } = 0;
        public string HOME_ZIP { get; set; } = "";
        public string WORK_TOWN { get; set; } = "";
        public int FK_WORK_CITY { get; set; } = 0;
        public string WORK_ZIP { get; set; } = "";
        public string LETTER_TOWN { get; set; } = "";
        public int FK_LETTER_CITY { get; set; } = 0;
        public string LETTER_ZIP { get; set; } = "";
        public string PHONE_1 { get; set; } = "";
        public string PHONE_2 { get; set; } = "";
        public string FAX_1 { get; set; } = "";
        public string CELL_PHONE { get; set; } = "";
        public DateTime CREATE_DATE { get; set; }
        public int FK_USER_CREATE { get; set; } = 0;
        public DateTime MODIFY_DATE { get; set; }
        public int FK_USER_MODIFY { get; set; } = 0;
        public int FK_STORE_LU { get; set; } = 0;
        public int FK_STORE_LS { get; set; } = 0;
        public int UPDATESEQ { get; set; } = 1;
    }
    public class GENIUS3_CARD
    {
        public long ID { get; set; }
        public string CODE { get; set; }
        public string PASSWORD { get; set; }
        public long FK_CUSTOMER { get; set; }
        public DateTime EXPIRE_DATE { get; set; }
        public DateTime GIVEN_DATE { get; set; }
        public short STATUS { get; set; } = 2;
        public decimal BONUS { get; set; } = 0;
        public short CAMPAIGN_PROCESS_TYPE { get; set; } = 1;
        public DateTime CREATE_DATE { get; set; }
        public int FK_USER_CREATE { get; set; } = 0;
        public DateTime MODIFY_DATE { get; set; }
        public int FK_USER_MODIFY { get; set; } = 0;
        public int FK_STORE_LU { get; set; } = 0;
        public int FK_STORE_LS { get; set; } = 0;
        public int UPDATESEQ { get; set; } = 1; 
        public bool IS_REDEEMER { get; set; } = true;
    }
}
