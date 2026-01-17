using BmsPosMiaEntegrasyon_LIBRARY;
using BmsPosMiaEntegrasyon_LIBRARY.METHODS.CONVERTER;
using BmsPosMiaEntegrasyon_LIBRARY.MODELS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BmsPosMiaEntegrasyon_LogoTS
{
    internal class Program
    {
        static CONFIG CFG;
        static List<BM_XXX_OrionFicheInvoiceHeaders> OHS = new List<BM_XXX_OrionFicheInvoiceHeaders>();
        static List<BM_XXX_PosMiaPayments> OHP = new List<BM_XXX_PosMiaPayments>();
        static CONVERTDATATOMODELS CDM = new CONVERTDATATOMODELS();
        [STAThread]
        static void Main(string[] args)
        {
            HELPER.LOGYAZ("SERVICE STARTED!", null);
            CFG = CONFIG_HELPER.GET_CONFIG();
            if (CFG == null)
            {
                Console.WriteLine("CONFIG ERROR.");
                Console.ReadLine();
                return;
            }
            try
            {
                Console.WriteLine("SyncSalesFromOrionToBM");
                try { SyncSalesFromOrionToBM(); } catch { }
                Console.WriteLine("SyncPaymentsFromOrionToBM");
                try { SyncPaymentsFromOrionToBM(); } catch { }
                string strLogin = LOGO_LOGIN(CFG.LOBJECTDEFAULTUSERNAME, CFG.LOBJECTDEFAULTPASSWORD);
                if (strLogin != "") throw new Exception(strLogin);
                Console.WriteLine("SyncSalesFromBMToLogo");
                try { SyncSalesFromBMToLogo(); } catch { }
                Console.WriteLine("SyncPaymentsFromBMToLogo");
                try { SyncPaymentsFromBMToLogo(); } catch { }
                LOGO_LOGOUT();
            }
            catch (Exception ex) { HELPER.LOGYAZ("HATA!", ex); }
            finally { try { LOGO_LOGOUT(); } catch { } }
            HELPER.LOGYAZ("SERVICE FINISHED!", null);
        }
        #region getFromOrion 
        private static void SyncSalesFromOrionToBM()
        {
            BmsPosMiaEntegrasyon_LIBRARY.METHODS.SQLCOMMANDS.SQLINSERTCOMMANDS SIC = new BmsPosMiaEntegrasyon_LIBRARY.METHODS.SQLCOMMANDS.SQLINSERTCOMMANDS();
            SqlConnection CON;
            SqlTransaction TRANSACTION;
            SqlCommand COM;
            string CONSTR_LG = $@"Data Source={CFG.LGDBSERVER};Initial Catalog={CFG.LGDBDATABASE};User Id={CFG.LGDBUSERNAME};Password={CFG.LGDBPASSWORD};MultipleActiveResultSets=True;";
            string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

            DataTable DT = HELPER.SqlSelectLogo("SELECT * FROM BM_" + CFG.FIRMNR + "_OrionLogoMapping order by OutletNo");
            List<BM_XXX_OrionLogoMapping> OLM = CDM.BM_XXX_OrionLogoMapping_CONVERT_FROM_DATATABLE(DT);
            List<BM_XXX_OrionFicheInvoiceHeaders> UnionedListOLM = new List<BM_XXX_OrionFicheInvoiceHeaders>();
            foreach (var itemm in OLM)
            {
                DataSet ds = new DataSet();
                ds = HELPER.GetDataSetWithManuelSqlConnection($@"EXECUTE [dbo].[IntSalesByCheckDetails] '{yesterday}', '{yesterday}'", itemm.SqlServer, itemm.SqlUsername, itemm.SqlPassword, itemm.SqlDatabaseName);
                DataTable DTFicheInvoiceHeader = ds.Tables[0];
                DataTable DTFicheInvoiceDetail = ds.Tables[1];
                DataTable DTPayments = ds.Tables[2];
                OHS = BMS_XXX_OrionFicheInvoice(DTFicheInvoiceHeader, DTFicheInvoiceDetail, DTPayments);
                CON = new SqlConnection(CONSTR_LG);
                try { if (CON.State != ConnectionState.Open) CON.Open(); } catch { }
                TRANSACTION = CON.BeginTransaction();
                try
                {
                    foreach (var item in OHS)
                    {
                        try
                        {
                            int LOGICALREF = 0;
                            COM = SIC.BM_XXX_OrionFicheInvoiceHeaders_INSERT(item, true, false, CFG.FIRMNR);
                            COM.Transaction = TRANSACTION;
                            COM.Connection = CON;
                            LOGICALREF = int.Parse(COM.ExecuteScalar().ToString());
                            #region SALES_DETAILS
                            foreach (var SALES_DETAILS in item.OrionFicheInvoiceDetails)
                            {
                                COM = SIC.BM_XXX_OrionFicheInvoiceDetails_INSERT(SALES_DETAILS, true, false, CFG.FIRMNR);
                                COM.Transaction = TRANSACTION;
                                COM.Connection = CON;
                                COM.ExecuteNonQuery();
                            }
                            #endregion
                            TRANSACTION.Commit();
                        }
                        catch { }
                    }
                }
                catch (Exception E) { try { TRANSACTION.Rollback(); } catch (Exception EE) { HELPER.LOGYAZ("SyncSalesFromOrionToBM Outlet=" + itemm.OutletNo.ToString() + "-" + itemm.OutletName, EE); } }
                finally
                {
                    try { if (CON.State != ConnectionState.Closed) CON.Close(); }
                    catch (Exception EE) { HELPER.LOGYAZ("SyncSalesFromOrionToBM Outlet=" + itemm.OutletNo.ToString() + "-" + itemm.OutletName, EE); }
                    try { TRANSACTION.Dispose(); } catch (Exception EE) { HELPER.LOGYAZ("SyncSalesFromOrionToBM Outlet=" + itemm.OutletNo.ToString() + "-" + itemm.OutletName, EE); }
                }
            }
        }
        private static void SyncPaymentsFromOrionToBM()
        {
            BmsPosMiaEntegrasyon_LIBRARY.METHODS.SQLCOMMANDS.SQLINSERTCOMMANDS SIC = new BmsPosMiaEntegrasyon_LIBRARY.METHODS.SQLCOMMANDS.SQLINSERTCOMMANDS();
            SqlConnection CON;
            SqlTransaction TRANSACTION;
            SqlCommand COM;
            string CONSTR_LG = $@"Data Source={CFG.LGDBSERVER};Initial Catalog={CFG.LGDBDATABASE};User Id={CFG.LGDBUSERNAME};Password={CFG.LGDBPASSWORD};MultipleActiveResultSets=True;";
            string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

            DataTable DT = HELPER.SqlSelectLogo("SELECT * FROM BM_" + CFG.FIRMNR + "_OrionLogoMapping order by OutletNo");
            List<BM_XXX_OrionLogoMapping> OLM = CDM.BM_XXX_OrionLogoMapping_CONVERT_FROM_DATATABLE(DT);
            List<BM_XXX_PosMiaPayments> UnionedListOLM = new List<BM_XXX_PosMiaPayments>();
            foreach (var itemm in OLM)
            {

                DataSet ds = new DataSet();
                ds = HELPER.GetDataSetWithManuelSqlConnection($@"EXECUTE [dbo].[IntSalesByCheckDetails] '{yesterday}', '{yesterday}'", itemm.SqlServer, itemm.SqlUsername, itemm.SqlPassword, itemm.SqlDatabaseName);
                BM_XXX_OrionFicheInvoiceHeaders OFIH = CDM.BM_XXX_OrionFicheInvoiceHeaders_CONVERT_FROM_DATAROW(ds.Tables[0].Rows[0]);
                DataTable DTPayments = ds.Tables[2];
                OHP = CDM.BM_XXX_OrionPayments_CONVERT_FROM_DATATABLE(DTPayments, OFIH.OutletNo);
                CON = new SqlConnection(CONSTR_LG);
                try { if (CON.State != ConnectionState.Open) CON.Open(); } catch { }
                TRANSACTION = CON.BeginTransaction();
                try
                {
                    foreach (var item in OHP)
                    {
                        try
                        {
                            int LOGICALREF = 0;
                            COM = SIC.BM_XXX_OrionPayments_INSERT(item, true, false, CFG.FIRMNR);
                            COM.Transaction = TRANSACTION;
                            COM.Connection = CON;
                            LOGICALREF = int.Parse(COM.ExecuteScalar().ToString());
                            TRANSACTION.Commit();
                        }
                        catch { }
                    }
                }
                catch (Exception E)
                {
                    try { TRANSACTION.Rollback(); } catch (Exception EE) { HELPER.LOGYAZ("SyncPaymentsFromOrionToBM Outlet=" + itemm.OutletNo.ToString() + "-" + itemm.OutletName, EE); }
                }
                finally
                {
                    try
                    {
                        if (CON.State != ConnectionState.Closed)
                        {
                            CON.Close();
                        }
                    }
                    catch (Exception EE) { HELPER.LOGYAZ("SyncPaymentsFromOrionToBM Outlet=" + itemm.OutletNo.ToString() + "-" + itemm.OutletName, EE); }
                    try { TRANSACTION.Dispose(); } catch (Exception EE) { HELPER.LOGYAZ("SyncPaymentsFromOrionToBM Outlet=" + itemm.OutletNo.ToString() + "-" + itemm.OutletName, EE); }
                }
            }
        }
        #endregion

        private static void SyncPaymentsFromBMToLogo()
        {
            #region GetSalesFromBM
            DataTable DTPayments = HELPER.SqlSelectLogo("SELECT * FROM BM_" + CFG.FIRMNR + "_OrionPayments WHERE ISNULL(TSTATUS,0)=0");
            OHP = CDM.BM_XXX_OrionPayments_CONVERT_FROM_DATATABLE(DTPayments, 0);
            #endregion
            #region InsertPaymentsToLogo
            if (OHS.Count == 0) return;

            foreach (var itemPayments in OHP)
            {
                if (itemPayments.PaymentBackOfficeCode == "1" /*PaymentName=NAKİT*/) KasasPaymentToLogo(itemPayments);
                else if (itemPayments.PaymentBackOfficeCode == "2" /*PaymentName=KREDI KARTI*/) CCPaymentToLogo(itemPayments);
                else continue;
            }
            #endregion
        }

        private static void CCPaymentToLogo(BM_XXX_PosMiaPayments BOL)
        {
            try
            {
                int LOGICALREF = 0;
                string CLCARDCODE = getCustomerCodeFromMapping(BOL.OutletNo);
                string WAREHOUSE = getWareHouseNrFromMapping(BOL.OutletNo);
                string BRANCH = getBranchNrFromMapping(BOL.OutletNo);
                string DIVISION = getDivisionNrFromMapping(BOL.OutletNo);
                string BANKACCOUNT = getBankAccountFromMapping(BOL.OutletNo);
                string CLCARDDEFINITION = getCustomerName(CLCARDCODE);
                UnityObjects.Data F = NewObjectData(UnityObjects.DataObjectType.doARAPVoucher);
                F.New();
                F.DataFields.FieldByName("AUXIL_CODE").Value = "BMS";
                F.DataFields.FieldByName("NUMBER").Value = BOL.UniqueCheckId.ToString();
                F.DataFields.FieldByName("DATE").Value = BOL.BusinessDate;
                F.DataFields.FieldByName("TYPE").Value = 70;
                F.DataFields.FieldByName("DIVISION").Value = BRANCH;
                F.DataFields.FieldByName("DEPARTMENT").Value = DIVISION;
                F.DataFields.FieldByName("TOTAL_CREDIT").Value = BOL.Price;
                F.DataFields.FieldByName("CURRSEL_TOTALS").Value = 1;
                F.DataFields.FieldByName("ARP_CODE").Value = CLCARDCODE;
                UnityObjects.Lines TL = F.DataFields.FieldByName("TRANSACTIONS").Lines;
                TL.AppendLine();
                TL[TL.Count - 1].FieldByName("ARP_CODE").Value = CLCARDCODE;
                TL[TL.Count - 1].FieldByName("TRANNO").Value = BOL.UniqueCheckId.ToString();
                TL[TL.Count - 1].FieldByName("CREDIT").Value = BOL.Price;
                TL[TL.Count - 1].FieldByName("TC_XRATE").Value = 1;
                TL[TL.Count - 1].FieldByName("TC_AMOUNT").Value = BOL.Price;
                TL[TL.Count - 1].FieldByName("BNLN_TC_XRATE").Value = 1;
                TL[TL.Count - 1].FieldByName("BNLN_TC_AMOUNT").Value = BOL.Price;
                TL[TL.Count - 1].FieldByName("BANKACC_CODE").Value = BANKACCOUNT;
                if (!F.Post())
                    throw new Exception(GetLastError(F));
                LOGICALREF = Convert.ToInt32(F.DataFields.DBFieldByName("LOGICALREF").Value);
                F.Read(LOGICALREF);
                F.Post();
                HELPER.SqlExecute("UPDATE BM_" + CFG.FIRMNR + "_OrionPayments SET TSTATUS = '1' , LogoLRef='" + LOGICALREF + "' ,LogoInsertDate=GETDATE()  WHERE LOGICALREF= " + BOL.LOGICALREF);
            }
            catch (Exception E)
            {
                HELPER.SqlExecute("UPDATE BM_" + CFG.FIRMNR + "_OrionPayments SET TSTATUS = '0' , ErrorMessage=LEFT('" + E.Message + "',254) WHERE LOGICALREF= " + BOL.LOGICALREF);
            }
        }

        private static void KasasPaymentToLogo(BM_XXX_PosMiaPayments BOL)
        {
            try
            {
                int LOGICALREF = 0;
                string CLCARDCODE = getCustomerCodeFromMapping(BOL.OutletNo);
                string WAREHOUSE = getWareHouseNrFromMapping(BOL.OutletNo);
                string BRANCH = getBranchNrFromMapping(BOL.OutletNo);
                string DIVISION = getDivisionNrFromMapping(BOL.OutletNo);
                string KSCODE = getKasaKoduFromMapping(BOL.OutletNo);
                string CLCARDDEFINITION = getCustomerName(CLCARDCODE);
                UnityObjects.Data F = NewObjectData(UnityObjects.DataObjectType.doSafeDepositTrans);
                F.New();
                F.DataFields.FieldByName("AUXIL_CODE").Value = "BMS";
                F.DataFields.FieldByName("TYPE").Value = 11;
                F.DataFields.FieldByName("SD_CODE").Value = KSCODE;
                F.DataFields.FieldByName("DATE").Value = BOL.BusinessDate;
                F.DataFields.FieldByName("DIVISION").Value = BRANCH;
                F.DataFields.FieldByName("DEPARTMENT").Value = DIVISION;
                F.DataFields.FieldByName("NUMBER").Value = BOL.UniqueCheckId.ToString();
                F.DataFields.FieldByName("MASTER_TITLE").Value = CLCARDDEFINITION;
                F.DataFields.FieldByName("AMOUNT").Value = BOL.Price;
                F.DataFields.FieldByName("TC_XRATE").Value = 1;
                F.DataFields.FieldByName("TC_AMOUNT").Value = BOL.Price;
                F.DataFields.FieldByName("DOC_NUMBER").Value = BOL.CheckNumber.ToString();
                F.DataFields.FieldByName("DOC_DATE").Value = BOL.BusinessDate;
                UnityObjects.Lines TL = F.DataFields.FieldByName("ATTACHMENT_ARP").Lines;
                TL.AppendLine();
                TL[TL.Count - 1].FieldByName("AUXIL_CODE").Value = "BMS";
                TL[TL.Count - 1].FieldByName("ARP_CODE").Value = CLCARDCODE;
                TL[TL.Count - 1].FieldByName("TRANNO").Value = BOL.UniqueCheckId.ToString();
                TL[TL.Count - 1].FieldByName("CREDIT").Value = BOL.Price;
                TL[TL.Count - 1].FieldByName("TC_XRATE").Value = 1;
                TL[TL.Count - 1].FieldByName("TC_AMOUNT").Value = BOL.Price;
                TL[TL.Count - 1].FieldByName("DOC_DATE").Value = BOL.BusinessDate;
                if (!F.Post())
                    throw new Exception(GetLastError(F));
                LOGICALREF = Convert.ToInt32(F.DataFields.DBFieldByName("LOGICALREF").Value);
                F.Read(LOGICALREF);
                F.Post();
                HELPER.SqlExecute("UPDATE BM_" + CFG.FIRMNR + "_OrionPayments SET TSTATUS = '1' , LogoLRef='" + LOGICALREF + "' ,LogoInsertDate=GETDATE()  WHERE LOGICALREF= " + BOL.LOGICALREF);
            }
            catch (Exception E)
            {
                HELPER.SqlExecute("UPDATE BM_" + CFG.FIRMNR + "_OrionPayments SET TSTATUS = '0' , ErrorMessage=LEFT('" + E.Message + "',254) WHERE LOGICALREF= " + BOL.LOGICALREF);
            }
        }

        private static void SyncSalesFromBMToLogo()
        {
            #region GetSalesFromBM
            DataTable DTFicheHeaders = HELPER.SqlSelectLogo("SELECT * FROM BM_" + CFG.FIRMNR + "_OrionFicheInvoiceHeaders WHERE ISNULL(TSTATUS,0)=0");
            OHS = CDM.BM_XXX_OrionFicheInvoiceHeaders_CONVERT_FROM_DATATABLE(DTFicheHeaders);
            #endregion
            #region InsertSalesToLogo
            if (OHS.Count == 0) return;

            foreach (var OHHeader in OHS)
            {
                DataTable DTFicheDetails = HELPER.SqlSelectLogo($@"SELECT * FROM BM_{CFG.FIRMNR}_OrionFicheInvoiceDetails WHERE OutletNo={OHHeader.OutletNo} and UniqueCheckId={OHHeader.UniqueCheckId} and CheckNumber={OHHeader.CheckNumber}");
                List<BM_XXX_OrionFicheInvoiceDetails> OL = CDM.BM_XXX_OrionFicheInvoiceDetails_CONVERT_FROM_DATATABLE(DTFicheDetails, 0);
                InsertInvoiceFromBmToLogo(OHHeader, OL);
            }
            #endregion
        }
        #region Helpers  
        public static void LOGO_LOGOUT()
        {
            try
            {
                if (AppUnity != null && AppUnity.CompanyLoggedIn)
                {
                    AppUnity.CompanyLogout();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                if (AppUnity != null && AppUnity.LoggedIn) AppUnity.UserLogout();
                if (AppUnity != null && AppUnity.Connected) AppUnity.Disconnect();

                AppUnity = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch
            {

            }
        }

        public static string LOGO_LOGIN(string USERNAME, string PASSWORD)
        {
            String strresult = "";
            try
            {
                if (AppUnity != null && AppUnity.Connected)
                    LOGO_LOGOUT();
                AppUnity = new UnityObjects.UnityApplication();
                if (!AppUnity.Login(USERNAME, PASSWORD, Convert.ToInt32(CFG.FIRMNR), 1))
                    throw new Exception(AppUnity.GetLastErrorString());

                strresult = "";
            }
            catch (Exception exp)
            {
                HELPER.LOGYAZ("HATA", exp);
                strresult = exp.Message;
            }

            return strresult;
        }

        public static string GetLastError(UnityObjects.Data doMain)
        {
            string strResult = "";

            if (doMain.ErrorCode != 0) strResult = string.Format("({0}) {1}", doMain.ErrorCode, doMain.ErrorDesc);
            if (doMain.DBErrorDesc != "") strResult += "(" + doMain.DBErrorDesc + ")";
            if (doMain.ValidateErrors.Count > 0)
                for (int i = 0; i < doMain.ValidateErrors.Count; i++)
                {
                    strResult += "Hata: " + doMain.ValidateErrors[i].ID.ToString() + ".Detay:" + doMain.ValidateErrors[i].Error + ".";
                    strResult = System.String.Concat(strResult, (char)13, (char)10);
                }
            return strResult;
        }
        public static string[] SELECT_UNIT_CODE(string SKU)
        {
            try
            {
                int LOGO_ITEM_UNITSETREF = 0;
                try { LOGO_ITEM_UNITSETREF = int.Parse(HELPER.SqlSelectLogo("SELECT UNITSETREF FROM LG_" + CFG.FIRMNR + "_ITEMS WHERE CODE='" + SKU + "'").Rows[0][0].ToString()); } catch { };
                string[] UNIT = new string[3];
                DataRow R = HELPER.SqlSelectLogo(string.Format("SELECT CODE, CONVFACT1, CONVFACT2 FROM LG_{0}_UNITSETL WHERE MAINUNIT=1 AND  UNITSETREF = {1}", CFG.FIRMNR, LOGO_ITEM_UNITSETREF)).Rows[0];
                UNIT[0] = R["CODE"].ToString();
                UNIT[1] = R["CONVFACT1"].ToString();
                UNIT[2] = R["CONVFACT2"].ToString();
                return UNIT;
            }
            catch (Exception E)
            {
                HELPER.LOGYAZ("SELECT_UNIT_CODE", E);
                return null;
            }
        }

        static UnityObjects.UnityApplication AppUnity;

        public static UnityObjects.Data NewObjectData(UnityObjects.DataObjectType objecttype)
        {
            return AppUnity.NewDataObject(objecttype);
        }

        static void InsertInvoiceFromBmToLogo(BM_XXX_OrionFicheInvoiceHeaders B, List<BM_XXX_OrionFicheInvoiceDetails> BOL)
        {
            try
            {
                int LOGICALREF = 0;
                string CLCARDCODE = getCustomerCodeFromMapping(B.OutletNo);
                string WAREHOUSE = getWareHouseNrFromMapping(B.OutletNo);
                string BRANCH = getBranchNrFromMapping(B.OutletNo);
                UnityObjects.Data F = NewObjectData(UnityObjects.DataObjectType.doSalesOrderSlip);
                F.New();
                F.DataFields.FieldByName("NUMBER").Value = B.UniqueCheckId.ToString();
                F.DataFields.FieldByName("DATE").Value = B.BusinessDate;
                //F.DataFields.FieldByName("TIME").Value = I.FTIME; 
                F.DataFields.FieldByName("DOC_NUMBER").Value = B.CheckNumber.ToString();
                //F.DataFields.FieldByName("AUTH_CODE").Value = I.CYPHCODE;
                F.DataFields.FieldByName("ARP_CODE").Value = CLCARDCODE;
                F.DataFields.FieldByName("SOURCE_WH").Value = WAREHOUSE;
                F.DataFields.FieldByName("SOURCE_COST_GRP").Value = WAREHOUSE;
                F.DataFields.FieldByName("PRINT_COUNTER").Value = "1";
                F.DataFields.FieldByName("PRINT_DATE").Value = B.BusinessDate;
                F.DataFields.FieldByName("DIVISION").Value = BRANCH;
                F.DataFields.FieldByName("ORDER_STATUS").Value = "4";
                F.DataFields.FieldByName("CURRSEL_TOTAL").Value = "1";
                F.DataFields.FieldByName("TC_RATE").Value = "1";
                F.DataFields.FieldByName("AFFECT_RISK").Value = "1";
                F.DataFields.FieldByName("DEDUCTIONPART1").Value = "2";
                F.DataFields.FieldByName("DEDUCTIONPART2").Value = "3";
                F.DataFields.FieldByName("EINVOICE_PROFILEID").Value = "2";
                UnityObjects.Lines TL = F.DataFields.FieldByName("TRANSACTIONS").Lines;
                for (int i = 0; i < BOL.Count; i++)
                {
                    string[] UNIT = SELECT_UNIT_CODE(BOL[i].ItemNo.ToString());
                    if (UNIT == null) throw new Exception("ITEM UNIT NOT FOUND FOR ITEM:" + BOL[i].ItemNo + "-" + BOL[i].ItemName);
                    TL.AppendLine();
                    TL[TL.Count - 1].FieldByName("TYPE").Value = 0;
                    TL[TL.Count - 1].FieldByName("MASTER_CODE").Value = BOL[i].ItemNo;
                    TL[TL.Count - 1].FieldByName("QUANTITY").Value = BOL[i].Qty;
                    TL[TL.Count - 1].FieldByName("PRICE").Value = BOL[i].UnitPrice;
                    TL[TL.Count - 1].FieldByName("UNIT_CODE").Value = UNIT[0];
                    TL[TL.Count - 1].FieldByName("UNIT_CONV1").Value = UNIT[1];
                    TL[TL.Count - 1].FieldByName("UNIT_CONV2").Value = UNIT[2];
                    TL[TL.Count - 1].FieldByName("DUE_DATE").Value = B.BusinessDate;
                    TL[TL.Count - 1].FieldByName("SOURCE_WH").Value = 0;
                    TL[TL.Count - 1].FieldByName("SOURCE_COST_GRP").Value = 0;
                    TL[TL.Count - 1].FieldByName("DIVISION").Value = 0;
                    TL[TL.Count - 1].FieldByName("AFFECT_RISK").Value = "1";
                    TL[TL.Count - 1].FieldByName("ORG_DUE_DATE").Value = B.BusinessDate;
                    TL[TL.Count - 1].FieldByName("ORG_QUANTITY").Value = BOL[i].Qty;
                }
                if (!F.Post())
                    throw new Exception(GetLastError(F));
                LOGICALREF = Convert.ToInt32(F.DataFields.DBFieldByName("LOGICALREF").Value);
                F.Read(LOGICALREF);
                F.Post();
                HELPER.SqlExecute("UPDATE BM_" + CFG.FIRMNR + "_OrionFicheInvoiceHeaders SET TSTATUS = '1' , LogoLRef='" + LOGICALREF + "' ,LogoInsertDate=GETDATE()  WHERE LOGICALREF= " + B.LOGICALREF);
            }
            catch (Exception E)
            {
                HELPER.SqlExecute("UPDATE BM_" + CFG.FIRMNR + "_OrionFicheInvoiceHeaders SET TSTATUS = '0' , ErrorMessage=LEFT('" + E.Message + "',254) WHERE LOGICALREF= " + B.LOGICALREF);
            }
        }

        private static string getCustomerCodeFromMapping(int outletNo)
        {
            string result = "";
            try { result = HELPER.SqlSelectLogo("SELECT CariKodu FROM BM_" + CFG.FIRMNR + "_OrionLogoMapping WHERE OutletNo=" + outletNo).Rows[0][0].ToString(); } catch (Exception E) { HELPER.LOGYAZ("getCustomerCodeFromMapping", E); }
            return result;
        }
        private static string getCustomerName(string CUSTOMERCODE)
        {
            string result = "";
            try { result = HELPER.SqlSelectLogo("SELECT DEFINITION_ FROM LG_" + CFG.FIRMNR + "_CLCARD WHERE CODE='" + CUSTOMERCODE + "'").Rows[0][0].ToString(); } catch (Exception E) { HELPER.LOGYAZ("getCustomerName", E); }
            return result;
        }
        private static string getWareHouseNrFromMapping(int outletNo)
        {
            string result = "";
            try { result = HELPER.SqlSelectLogo("SELECT AmbarNr FROM BM_" + CFG.FIRMNR + "_OrionLogoMapping WHERE OutletNo=" + outletNo).Rows[0][0].ToString(); } catch (Exception E) { HELPER.LOGYAZ("getWareHouseNrFromMapping", E); }
            return result;
        }
        private static string getBranchNrFromMapping(int outletNo)
        {
            string result = "";
            try { result = HELPER.SqlSelectLogo("SELECT IsYeriNr FROM BM_" + CFG.FIRMNR + "_OrionLogoMapping WHERE OutletNo=" + outletNo).Rows[0][0].ToString(); } catch (Exception E) { HELPER.LOGYAZ("getBranchNrFromMapping", E); }
            return result;
        }
        private static string getDivisionNrFromMapping(int outletNo)
        {
            string result = "";
            try { result = HELPER.SqlSelectLogo("SELECT BolumNr FROM BM_" + CFG.FIRMNR + "_OrionLogoMapping WHERE OutletNo=" + outletNo).Rows[0][0].ToString(); } catch (Exception E) { HELPER.LOGYAZ("getDivisionNrFromMapping", E); }
            return result;
        }
        private static string getKasaKoduFromMapping(int outletNo)
        {
            string result = "";
            try { result = HELPER.SqlSelectLogo("SELECT KasaKodu FROM BM_" + CFG.FIRMNR + "_OrionLogoMapping WHERE OutletNo=" + outletNo).Rows[0][0].ToString(); } catch (Exception E) { HELPER.LOGYAZ("getKasaKoduFromMapping", E); }
            return result;
        }
        private static string getBankAccountFromMapping(int outletNo)
        {
            string result = "";
            try { result = HELPER.SqlSelectLogo("SELECT BankaHesapNo FROM BM_" + CFG.FIRMNR + "_OrionLogoMapping WHERE OutletNo=" + outletNo).Rows[0][0].ToString(); } catch (Exception E) { HELPER.LOGYAZ("getBankAccountFromMapping", E); }
            return result;
        }
        private static List<BM_XXX_OrionFicheInvoiceHeaders> BMS_XXX_OrionFicheInvoice(DataTable dTFicheInvoiceHeader, DataTable dTFicheInvoiceDetail, DataTable dTPayment)
        {
            List<BM_XXX_OrionFicheInvoiceHeaders> rl = new List<BM_XXX_OrionFicheInvoiceHeaders>();
            rl = CDM.BM_XXX_OrionFicheInvoiceHeaders_CONVERT_FROM_DATATABLE(dTFicheInvoiceHeader);
            List<BM_XXX_OrionFicheInvoiceDetails> rld = new List<BM_XXX_OrionFicheInvoiceDetails>();
            rld = CDM.BM_XXX_OrionFicheInvoiceDetails_CONVERT_FROM_DATATABLE(dTFicheInvoiceDetail, rl.FirstOrDefault().OutletNo);
            List<BM_XXX_PosMiaPayments> rlp = new List<BM_XXX_PosMiaPayments>();
            rlp = CDM.BM_XXX_OrionPayments_CONVERT_FROM_DATATABLE(dTPayment, rl.FirstOrDefault().OutletNo);
            foreach (var item in rl)
            {
                item.OrionFicheInvoiceDetails = rld.Where(x => x.UniqueCheckId == item.UniqueCheckId && x.CheckNumber == item.CheckNumber).ToList();
                item.OrionPayments = rlp.Where(x => x.UniqueCheckId == item.UniqueCheckId && x.CheckNumber == item.CheckNumber).ToList();
            }
            return rl;
        }

        #endregion
    }
}
