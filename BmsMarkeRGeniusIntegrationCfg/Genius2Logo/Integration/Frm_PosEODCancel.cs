using BmsMarkeRGeniusIntegrationCfg;
using BmsMarkeRGeniusIntegrationLibrary;
using BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraPrinting;
using DevExpress.XtraSplashScreen;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static DevExpress.Xpo.Helpers.CommandChannelHelper;

namespace Integration.BmsMarkeRGeniusIntegrationCfg.Genius2Logo.Integration
{
    public partial class Frm_PosEODCancel : DevExpress.XtraEditors.XtraForm
    {
        CONFIG CFG;
        string TABLENAME = "BMSF_XXX_MarkeRGenius_Sales";
        public Frm_PosEODCancel(string HEADERNAME)
        {
            InitializeComponent();
            this.Text = HEADERNAME;
            CFG = CONFIG_HELPER.GET_CONFIG();
            TABLENAME = TABLENAME.Replace("XXX", CFG.FIRMNR);
            de_Date.DateTime = DateTime.Now.Date;
            //de_DateStart.DateTime = new DateTime(2023, 12, 26);
            //de_DateEnd.DateTime = new DateTime(2023, 12, 26);
            loadLookupEdits();
            // NCR ve Genius buton görünürlükleri
            simpleButton1.Visible = CFG.ISNCRACTIVE == "1";  // NCR Sil
            sb_Cancel.Visible = CFG.ISGENIUSACTIVE == "1";  // Sil (Genius)
        }

        private void loadLookupEdits()
        {
            //le_InvoiceClient
            le_InvoiceClient.Properties.DataSource = HELPER.SqlSelectLogo($@"SELECT NR,NAME FROM BMS_{CFG.FIRMNR}_MarkeRGenius_InvoiceClient ORDER BY NR");
            le_InvoiceClient.Properties.ValueMember = "NR";
            le_InvoiceClient.Properties.DisplayMember = "NAME";
            le_InvoiceClient.Properties.PopulateColumns();

            //le_ReturnClient
            le_ReturnClient.Properties.DataSource = HELPER.SqlSelectLogo($@"SELECT NR,NAME FROM BMS_{CFG.FIRMNR}_MarkeRGenius_ReturnClient ORDER BY NR");
            le_ReturnClient.Properties.ValueMember = "NR";
            le_ReturnClient.Properties.DisplayMember = "NAME";
            le_ReturnClient.Properties.PopulateColumns();

            //le_Branch
            le_Branch.Properties.DataSource = HELPER.SqlSelectLogo($@"SELECT NR,NAME FROM BMS_{CFG.FIRMNR}_MarkeRGenius_Branch ORDER BY NR");
            le_Branch.Properties.ValueMember = "NR";
            le_Branch.Properties.DisplayMember = "NAME";
            le_Branch.Properties.PopulateColumns();
            //le_Pos
            le_Pos.Properties.DataSource = HELPER.SqlSelectLogo($@"SELECT NR FROM BMS_{CFG.FIRMNR}_MarkeRGenius_GeniusPos ORDER BY NR");
            le_Pos.Properties.ValueMember = "NR";
            le_Pos.Properties.DisplayMember = "NR";
            le_Pos.Properties.PopulateColumns();



            //object valueOfIc = HELPER.SqlSelectLogo($@"SELECT TOP 1 NR FROM BMS_{CFG.FIRMNR}_MarkeRGenius_InvoiceClient ORDER BY NR").Rows[0][0].ToString();
            //le_InvoiceClient.ItemIndex = le_InvoiceClient.Properties.GetDataSourceRowIndex("NR", valueOfIc);
            //object valueOfRc = HELPER.SqlSelectLogo($@"SELECT TOP 1 NR FROM BMS_{CFG.FIRMNR}_MarkeRGenius_ReturnClient ORDER BY NR").Rows[0][0].ToString();
            //le_ReturnClient.ItemIndex = le_ReturnClient.Properties.GetDataSourceRowIndex("NR", valueOfRc);
            //object valueOfB = HELPER.SqlSelectLogo($@"SELECT TOP 1 NR FROM BMS_{CFG.FIRMNR}_MarkeRGenius_Branch ORDER BY NR").Rows[0][0].ToString();
            //le_Branch.ItemIndex = le_Branch.Properties.GetDataSourceRowIndex("NR", valueOfB);
        }

        private void InitializeData(object sender, EventArgs e)
        {
            List<Bms_Errors> errorList = new List<Bms_Errors>();
            SplashScreenManager.ShowForm(this, typeof(PROGRESSFORM), true, true, false);
            SplashScreenManager.Default.SetWaitFormCaption("LÜTFEN BEKLEYİN.");
            SplashScreenManager.Default.SetWaitFormDescription("");

            string sqlFormattedDate = de_Date.DateTime.ToString("MM/dd/yyyy") + " 00:00:00";

            string wherePos = "";
            if (ce_AllPos.Checked)
                wherePos = $@"(SELECT DISTINCT CAST(GP.NR AS VARCHAR) AS NR FROM BMS_{CFG.FIRMNR}_MarkeRGenius_GeniusPos GP WITH(NOLOCK) )";
            else
                wherePos = le_Pos.EditValue.ToString();

            string sqlQueryInvoiceHeader = $@"SELECT II.LOGICALREF FROM LG_{CFG.FIRMNR}_01_INVOICE II  WITH(NOLOCK) WHERE II.TIME_=0 AND II.POSTRANSFERINFO=1 AND II.CYPHCODE='BMS' AND II.DATE_ = '{sqlFormattedDate}' AND II.BRANCH = {le_Branch.EditValue} /*AND II.DOCODE IN ( {wherePos} )*/";

            bool isThereAnyInvoice = HELPER.SqlSelectLogo(sqlQueryInvoiceHeader).Rows.Count > 0;

            string sqlQueryPayments = $@"SELECT LOGICALREF FROM LG_{CFG.FIRMNR}_01_CSROLL WITH(NOLOCK)  WHERE CYPHCODE='BMS' AND DATE_ = '{sqlFormattedDate}' AND BRANCH = {le_Branch.EditValue} AND SPECODE IN ({wherePos})
UNION ALL
SELECT LOGICALREF FROM LG_{CFG.FIRMNR}_01_CLFICHE WITH(NOLOCK)  WHERE CYPHCODE='BMS' AND DATE_ = '{sqlFormattedDate}' AND BRANCH = {le_Branch.EditValue} AND SPECCODE IN ({wherePos})
UNION ALL
SELECT LOGICALREF FROM LG_{CFG.FIRMNR}_01_KSLINES  WITH(NOLOCK) WHERE CYPHCODE='BMS' AND DATE_ = '{sqlFormattedDate}' AND BRANCH = {le_Branch.EditValue} AND SPECODE IN ({wherePos})";

            bool isThereAnyPayment = HELPER.SqlSelectLogo(sqlQueryPayments).Rows.Count > 0;

            string sqlQueryDebtClose = $@"SELECT 
	                                        BRANCH,DATE_ 
                                        FROM  
	                                        LG_{CFG.FIRMNR}_01_INVOICE
                                        WHERE 
	                                        Cyphcode='BMS' and CAPIBLOCK_CREATEDBY=(SELECT CU.NR FROM L_CAPIUSER CU WHERE CU.NAME='LOGO') and 
	                                        TRCODE IN (2,7) AND SPECODE IN ({wherePos}) AND DATE_ = '{sqlFormattedDate}' AND LOGICALREF IN 
	                                        (SELECT FICHEREF FROM  LG_{CFG.FIRMNR}_01_PAYTRANS WHERE MODULENR=4 AND  TRCODE in (2,7) AND PAID<>0)";

            bool isThereAnyDebtClose = HELPER.SqlSelectLogo(sqlQueryDebtClose).Rows.Count > 0 ? true : false;

            if (isThereAnyInvoice == false && isThereAnyPayment == false)
            {
                XtraMessageBox.Show("Seçilen tarihte işlem bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SplashScreenManager.CloseForm(false);
                return;
            }

            if (isThereAnyDebtClose == true)
            {
                if (ce_DontRollbackDebtClose.Checked)
                {
                    XtraMessageBox.Show("Seçilen tarihte kapatılmış borç bulunmaktadır. İşlem yapılamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    SplashScreenManager.CloseForm(false);
                    return;
                }
            }

            try
            {
                string strLogin = HELPER.LOGO_LOGIN(CFG.LOBJECTDEFAULTUSERNAME, CFG.LOBJECTDEFAULTPASSWORD, int.Parse(CFG.FIRMNR));
                if (strLogin != "") throw new Exception(strLogin);
                #region Borc Takipleri Geri Alma İşlemi
                if (!ce_DontRollbackDebtClose.Checked)
                {
                    string sqlQueryRevertDebtCloseInvoice = $@"SELECT 
                                            ISNULL(PAYTRANS.LOGICALREF,0) LOGICALREF
                                        FROM 
                                            LG_{CFG.FIRMNR}_01_INVOICE INVOICE WITH(NOLOCK) LEFT JOIN  
                                            LG_{CFG.FIRMNR}_01_PAYTRANS PAYTRANS WITH(NOLOCK) 
                                            ON INVOICE.LOGICALREF=PAYTRANS.FICHEREF AND PAYTRANS.MODULENR=4 AND  PAYTRANS.TRCODE in (2,7) AND PAYTRANS.PAID<>0
                                        WHERE INVOICE.DATE_='{sqlFormattedDate}' AND
                                            INVOICE.Cyphcode='BMS' and INVOICE.CAPIBLOCK_CREATEDBY=(SELECT CU.NR FROM L_CAPIUSER CU WHERE CU.NAME='LOGO') and 
                                            INVOICE.TRCODE IN (2,7) AND INVOICE.SPECODE IN ({wherePos})";
                    string sqlQueryRevertDebtCloseCSROLL = $@"SELECT 
                                            ISNULL(PAYTRANS.LOGICALREF,0) LOGICALREF
                                        FROM 
                                            LG_{CFG.FIRMNR}_01_CSROLL CSROLL WITH(NOLOCK) LEFT JOIN  
                                            LG_{CFG.FIRMNR}_01_PAYTRANS PAYTRANS WITH(NOLOCK) 
                                            ON CSROLL.LOGICALREF=PAYTRANS.FICHEREF AND PAYTRANS.MODULENR=6 AND  PAYTRANS.TRCODE in (1) AND PAYTRANS.PAID<>0
                                        WHERE CSROLL.DATE_='{sqlFormattedDate}' AND
                                            CSROLL.Cyphcode='BMS' and CSROLL.CAPIBLOCK_CREATEDBY=(SELECT CU.NR FROM L_CAPIUSER CU WHERE CU.NAME='LOGO') and 
                                            CSROLL.TRCODE IN (1) AND CSROLL.SPECODE IN ({wherePos})";
                    string sqlQueryRevertDebtCloseKS = $@"SELECT 
                                            ISNULL(PAYTRANS.LOGICALREF,0) LOGICALREF
                                        FROM 
                                            LG_{CFG.FIRMNR}_01_KSLINES KSLINES WITH(NOLOCK) LEFT JOIN  
                                            LG_{CFG.FIRMNR}_01_PAYTRANS PAYTRANS WITH(NOLOCK) 
                                            ON KSLINES.LOGICALREF=PAYTRANS.FICHEREF AND PAYTRANS.MODULENR=10 AND  PAYTRANS.TRCODE in (1) AND PAYTRANS.PAID<>0
                                        WHERE KSLINES.DATE_='{sqlFormattedDate}' AND
                                            KSLINES.Cyphcode='BMS' and KSLINES.CAPIBLOCK_CREATEDBY=(SELECT CU.NR FROM L_CAPIUSER CU WHERE CU.NAME='LOGO') and 
                                            KSLINES.TRCODE IN (11) AND KSLINES.SPECODE IN ({wherePos})";

                    string sqlQueryRevertDebtClose = $@"SELECT * FROM( {sqlQueryRevertDebtCloseInvoice} UNION ALL {sqlQueryRevertDebtCloseCSROLL} UNION ALL {sqlQueryRevertDebtCloseKS} ) AS TF WHERE LOGICALREF>0";

                    DataTable fhl5 = HELPER.SqlSelectLogo(sqlQueryRevertDebtClose);
                    if (fhl5.Rows.Count > 0)
                    {
                        foreach (DataRow item in fhl5.Rows)
                        {
                            double percantage = Math.Round((double)fhl5.Rows.IndexOf(item) / fhl5.Rows.Count, 2);
                            SplashScreenManager.Default.SetWaitFormDescription($"Borç Takipleri Geri Alınıyor... {percantage * 100} %");
                            string result = "";
                            int REF = int.Parse(item["LOGICALREF"].ToString());
                            result = HELPER.rollBackDebtClose(REF);

                            if (result != "ok")
                            {
                                errorList.Add(new Bms_Errors()
                                {
                                    BRANCH = int.Parse(le_Branch.EditValue.ToString()),
                                    POS = int.Parse(le_Pos.EditValue.ToString()),
                                    FTYPE = "RollbackDebtClose",
                                    DATE_ = de_Date.DateTime,
                                    ERRORMESSAGE = result
                                });
                            }
                        }
                    }
                }
                #endregion
                if (isThereAnyInvoice == true)
                {
                    #region Sadece Carili Satışlar Silme İşlemi
                    if (ce_OnlySalesWithCustomer.Checked)
                    {
                        //get clientref values from le_InvoiceClient and le_ReturnClient
                        string clientRefs = "";
                        if (le_InvoiceClient.EditValue != null)
                        {
                            clientRefs += HELPER.SqlSelectLogo($@"SELECT LOGICALREF FROM LG_{CFG.FIRMNR}_CLCARD  WITH(NOLOCK) WHERE CODE = '{le_InvoiceClient.EditValue}'").Rows[0][0].ToString();
                        }
                        if (le_ReturnClient.EditValue != null)
                        {
                            clientRefs += "," + HELPER.SqlSelectLogo($@"SELECT LOGICALREF FROM LG_{CFG.FIRMNR}_CLCARD  WITH(NOLOCK) WHERE CODE = '{le_ReturnClient.EditValue}'").Rows[0][0].ToString();
                        }
                        sqlQueryInvoiceHeader = sqlQueryInvoiceHeader + " AND II.CLIENTREF NOT IN (" + clientRefs + ")";
                    }
                    #endregion


                    #region Satışları Silme İşlemi
                    if (ce_OnlyPayments.Checked == false)
                    {
                        DataTable fhl = HELPER.SqlSelectLogo(sqlQueryInvoiceHeader);
                        try
                        {
                            HELPER.SqlExecute($@"DELETE FROM LG_{CFG.FIRMNR}_01_STLINE WHERE (BILLED=1 AND INVOICEREF>0) AND INVOICEREF IN ( {sqlQueryInvoiceHeader} )");
                        }
                        catch
                        { }

                        foreach (DataRow item in fhl.Rows)
                        {
                            double percantage = Math.Round((double)fhl.Rows.IndexOf(item) / fhl.Rows.Count, 2);
                            SplashScreenManager.Default.SetWaitFormDescription($"Faturalar Siliniyor... {percantage * 100} %");
                            string result = "";
                            int REF = int.Parse(item["LOGICALREF"].ToString());
                            result = HELPER.deleteInvoice(REF, CFG.FIRMNR);

                            if (result != "ok")
                            {
                                errorList.Add(new Bms_Errors()
                                {
                                    BRANCH = int.Parse(le_Branch.EditValue.ToString()),
                                    POS = int.Parse(le_Pos.EditValue.ToString()),
                                    FTYPE = "",
                                    DATE_ = de_Date.DateTime,
                                    ERRORMESSAGE = result
                                });
                            }
                        }
                    }
                    #endregion
                }
                #region Sadece Tahsilatları Silme İşlemi
                //else if (ce_OnlyPayments.Checked == true)
                {
                    if (isThereAnyPayment == true)
                        DeletePayments(errorList, sqlFormattedDate, wherePos);
                }
                #endregion
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HELPER.LOGO_LOGOUT();
            }

            if (errorList.Count > 0)
            {
                string errorText = "İşlem Hatalarla Tamamlandı.";
                XtraMessageBox.Show(errorText, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                FRM_Errors frm = new FRM_Errors(errorList);
                frm.Show();
            }
            else
                XtraMessageBox.Show("İşlem tamamlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SplashScreenManager.CloseForm(false);
        }
      
        private void DeletePayments(List<Bms_Errors> errorList, string sqlFormattedDate, string wherePos)
        {
            #region Çek Silme İşlemi
            string sqlQuery = $@"SELECT LOGICALREF FROM LG_{CFG.FIRMNR}_01_CSROLL WITH(NOLOCK)  WHERE CYPHCODE='BMS' AND DATE_ = '{sqlFormattedDate}' AND BRANCH = {le_Branch.EditValue} AND SPECODE IN ({wherePos}) ";
            DataTable fhl2 = HELPER.SqlSelectLogo(sqlQuery);
            foreach (DataRow item in fhl2.Rows)
            {
                double percantage =  Math.Round((double)fhl2.Rows.IndexOf(item) / fhl2.Rows.Count, 2);
                SplashScreenManager.Default.SetWaitFormDescription($"Tahsilat-Çekler Siliniyor... {percantage * 100} %");
                string result = "";
                int REF = int.Parse(item["LOGICALREF"].ToString());
                result = HELPER.deleteCheque(REF, CFG.FIRMNR);

                if (result != "ok")
                {
                    errorList.Add(new Bms_Errors()
                    {
                        BRANCH = int.Parse(le_Branch.EditValue.ToString()),
                        POS = int.Parse(le_Pos.EditValue.ToString()),
                        FTYPE = "Payment:Cheque",
                        DATE_ = de_Date.DateTime,
                        ERRORMESSAGE = result
                    });
                }
            }
            #endregion

            #region Kredi Kartı + Kredi Kartı İade + CH Borc + CH Alacak Fişi Silme İşlemi
            string sqlQuery2 = $@"SELECT LOGICALREF FROM LG_{CFG.FIRMNR}_01_CLFICHE WITH(NOLOCK)  WHERE CYPHCODE='BMS' AND DATE_ = '{sqlFormattedDate}' AND BRANCH = {le_Branch.EditValue} AND SPECCODE IN ({wherePos}) ";
            DataTable fhl3 = HELPER.SqlSelectLogo(sqlQuery2);
            foreach (DataRow item in fhl3.Rows)
            {
                double percantage = Math.Round((double)fhl3.Rows.IndexOf(item) / fhl3.Rows.Count, 2);
                SplashScreenManager.Default.SetWaitFormDescription($"Tahsilat-Kredi Kartı Fişleri Siliniyor... {percantage * 100} %");
                string result = "";
                int REF = int.Parse(item["LOGICALREF"].ToString());
                result = HELPER.deleteCLFiche(REF, CFG.FIRMNR);

                if (result != "ok")
                {
                    errorList.Add(new Bms_Errors()
                    {
                        BRANCH = int.Parse(le_Branch.EditValue.ToString()),
                        POS = int.Parse(le_Pos.EditValue.ToString()),
                        FTYPE = "Payment:Kredi Kartı + Kredi Kartı İade + CH Borc + CH Alacak",
                        DATE_ = de_Date.DateTime,
                        ERRORMESSAGE = result
                    });
                }
            }
            #endregion

            #region Kasa Tahsilat + Kasa Ödeme
            string sqlQuery3 = $@"SELECT LOGICALREF FROM LG_{CFG.FIRMNR}_01_KSLINES  WITH(NOLOCK) WHERE CYPHCODE='BMS' AND DATE_ = '{sqlFormattedDate}' AND BRANCH = {le_Branch.EditValue} AND SPECODE IN ({wherePos}) ";
            DataTable fhl4 = HELPER.SqlSelectLogo(sqlQuery3);
            foreach (DataRow item in fhl4.Rows)
            {
                double percantage = Math.Round((double)fhl4.Rows.IndexOf(item) / fhl4.Rows.Count, 2);
                SplashScreenManager.Default.SetWaitFormDescription($"Tahsilat-Kasa Fişleri Siliniyor... {percantage * 100} %");
                string result = "";
                int REF = int.Parse(item["LOGICALREF"].ToString());
                result = HELPER.deleteKsLines(REF, CFG.FIRMNR);

                if (result != "ok")
                {
                    errorList.Add(new Bms_Errors()
                    {
                        BRANCH = int.Parse(le_Branch.EditValue.ToString()),
                        POS = int.Parse(le_Pos.EditValue.ToString()),
                        FTYPE = "Payment:Kasa Tahsilat + Kasa Ödeme",
                        DATE_ = de_Date.DateTime,
                        ERRORMESSAGE = result
                    });
                }
            }
            #endregion
        }

        private void sb_Cancel_Click(object sender, EventArgs e)
        {
            if (le_Branch.EditValue == null || le_InvoiceClient.EditValue == null || le_ReturnClient.EditValue == null)
            {
                XtraMessageBox.Show("Lütfen tüm alanları doldurunuz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            InitializeData(null, null);
        }

        List<string> willBeDeletedFicheReferences = new List<string>();

        private void sb_SaveToBm_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ce_AllPos_CheckedChanged(object sender, EventArgs e)
        {
            if (ce_AllPos.Checked)
                le_Pos.Enabled = false;
            else
                le_Pos.Enabled = true;
        }

        private void ce_OnlySalesWithCustomer_CheckedChanged(object sender, EventArgs e)
        {
            if (ce_OnlySalesWithCustomer.Checked)
                ce_OnlyPayments.Enabled = false;
            else
                ce_OnlyPayments.Enabled = true;
        }

        private void ce_OnlyPayments_CheckedChanged(object sender, EventArgs e)
        {
            if (ce_OnlyPayments.Checked)
                ce_OnlySalesWithCustomer.Enabled = false;
            else
                ce_OnlySalesWithCustomer.Enabled = true;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            try
            {
                string sqlFormattedDate = de_Date.DateTime.ToString("MM/dd/yyyy") + " 00:00:00";
                string sqlQuery = $@"DELETE FROM LG_126_01_INVOICE WHERE CYPHCODE = 'BMS-NCR' AND DATE_ = '{sqlFormattedDate}'";
                string sqlQuery2 = $@"DELETE FROM LG_126_01_STFICHE WHERE CYPHCODE = 'BMS-NCR' AND DATE_ = '{sqlFormattedDate}'";
                SplashScreenManager.ShowForm(this, typeof(PROGRESSFORM), true, true, false);
                SplashScreenManager.Default.SetWaitFormCaption("LÜTFEN BEKLEYİN.");
                SplashScreenManager.Default.SetWaitFormDescription("");
                HELPER.SqlDeleteCommand(sqlQuery, false, null);
                HELPER.SqlDeleteCommand(sqlQuery2, false, null);
                XtraMessageBox.Show("İşlem tamamlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HELPER.LOGO_LOGOUT();
            }

     
            
                
        }
    }
}