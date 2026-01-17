using BmsMarkeRGeniusIntegrationLibrary;
using BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace BmsMarkeRGeniusIntegrationCfg.Logo2Genius.Transaction
{
    public partial class Frm_Cari : DevExpress.XtraEditors.XtraForm
    {
        CONFIG CFG;
        string BMTABLENAME = "Bms_XXX_MarkeRGeniusIntegration_Cariler";
        string VALUETABLE = "Bms_XXX_MarkeRGeniusIntegration_IbmKasa";
        List<Bms_XXX_MarkeRGeniusIntegration_Cariler> OList = new List<Bms_XXX_MarkeRGeniusIntegration_Cariler>();
        public Frm_Cari(string HEADERNAME)
        {
            InitializeComponent();
            this.Text = HEADERNAME;
            CFG = CONFIG_HELPER.GET_CONFIG();
            de_StartDate.DateTime = DateTime.Now;
            de_EndDate.DateTime = DateTime.Now;
            BMTABLENAME = BMTABLENAME.Replace("XXX", CFG.FIRMNR);
            VALUETABLE = VALUETABLE.Replace("XXX", CFG.FIRMNR);
            InitializeGLE();
            //InitializeData(null, null);
            // Genius aktif değilse mağaza seçim alanını gizle
            gb_Branch.Visible = CFG.ISGENIUSACTIVE == "1";
        }

        private void InitializeGLE()
        {
            gle_Value.Properties.DataSource = HELPER.SqlSelectLogo($@"SELECT * FROM {VALUETABLE}");
            gle_Value.Properties.DisplayMember = "LogoValue";
            gle_Value.Properties.ValueMember = "LogoValue";
            gle_Value.Properties.NullText = "Seçiniz";
            gle_Value.Properties.SearchMode = DevExpress.XtraEditors.Repository.GridLookUpSearchMode.AutoSearch;
            gle_Value.Properties.View.OptionsView.ShowAutoFilterRow = true;
        }

        private void InitializeData(object sender, EventArgs e)
        {
            string senderName = "";
            if (sender != null)
                senderName = ((DevExpress.XtraEditors.SimpleButton)sender).Text;
            SplashScreenManager.ShowForm(this, typeof(PROGRESSFORM), true, true, false);
            SplashScreenManager.Default.SetWaitFormCaption("LÜTFEN BEKLEYİN.");
            SplashScreenManager.Default.SetWaitFormDescription("");
            string wh = gle_Value.EditValue?.ToString() ?? "0";
            if (string.IsNullOrEmpty(wh)) wh = "0";
            if (!string.IsNullOrEmpty(CFG.DefaultBranchForGeniusSending))
                wh = CFG.DefaultBranchForGeniusSending;
            string sqlFormattedDateStart = de_StartDate.DateTime.ToString("yyyy-MM-dd");
            string sqlFormattedDateEnd = de_EndDate.DateTime.ToString("yyyy-MM-dd");
            string where = "";
            if (rb_ChangedClients.Checked)
            {
                where = $@" AND (TARIH>= '{sqlFormattedDateStart}' AND TARIH<= '{sqlFormattedDateEnd}') ";

            }
            OList = HELPER.DataTableToList<Bms_XXX_MarkeRGeniusIntegration_Cariler>(HELPER.SqlSelectLogo($@"SELECT * FROM {BMTABLENAME} WHERE 1=1 {where} "));
            grc_Malzeme.DataSource = OList;
            SplashScreenManager.CloseForm(false);
        }

        private void ExportToExcel(object sender, EventArgs e)
        {
            HELPER.DxExportGridToExcel(grv_Malzeme, true);
        }

        private void Frm_CRUDs_FormClosed(object sender, FormClosedEventArgs e)
        {
            grv_Malzeme.SaveLayoutToRegistry(string.Format(@"{0}\{1}", Application.StartupPath, this.GetType().Name));
        }

        private void grv_Invoices_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
            {
                e.Menu.Items.Add(new DXMenuItem("Excele Kaydet", ExportToExcel));
                e.Menu.Items.Add(new DXMenuItem("Güncelle", InitializeData));
            }
        }

        private void sb_GetFromMysql_Click(object sender, EventArgs e)
        {
            InitializeData(null, null);
        }

        private void sb_GetFromBms_Click(object sender, EventArgs e)
        {
            InitializeData(sender, null);
        }

        private void sb_Load_Click(object sender, EventArgs e)
        {
            // Genius aktifse mağaza zorunlu, değilse zorunlu değil
            if (CFG.ISGENIUSACTIVE == "1" && gle_Value.EditValue == null)
            {
                XtraMessageBox.Show("Lütfen Değer Seçiniz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            InitializeData(null, null);
        }

        private void gle_Value_EditValueChanged(object sender, EventArgs e)
        {
            string getPathFromSqlByValue = HELPER.SqlSelectLogo($@"SELECT * FROM {VALUETABLE} WHERE LogoValue = '{gle_Value.EditValue}'").Rows[0]["Path"].ToString();
            l_FilePath.Text = getPathFromSqlByValue + "\\";
        }

        private void sb_Save2Destination_Click(object sender, EventArgs e)
        {
            try {
                var SqlInfo = HELPER.SqlSelectLogo($@"SELECT top 1 SqlServer,SqlUsername,SqlPassword,SqlDatabase FROM {VALUETABLE} WHERE LogoValue = '{gle_Value.EditValue}'").Rows[0];
                string SqlServer = SqlInfo["SqlServer"].ToString();
                string SqlUsername = SqlInfo["SqlUsername"].ToString();
                string SqlPassword = SqlInfo["SqlPassword"].ToString();
                string SqlDatabase = SqlInfo["SqlDatabase"].ToString();
                SplashScreenManager.ShowForm(this, typeof(PROGRESSFORM), true, true, false);
                SplashScreenManager.Default.SetWaitFormCaption("LÜTFEN BEKLEYİN.");
                SplashScreenManager.Default.SetWaitFormDescription("");
                string LGCONSTR = string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3};MultipleActiveResultSets=True;", SqlServer, SqlDatabase, SqlUsername, SqlPassword);
                SqlConnection CON = new SqlConnection(LGCONSTR);
                SqlTransaction TRANSACTION = null;
                if (CON.State != ConnectionState.Open)
                    CON.Open();
                TRANSACTION = CON.BeginTransaction();
                var selectedRows = grv_Malzeme.GetSelectedRows();
                for (int i = 0; i < selectedRows.Length; i++)
                {
                    bool isUpdate = false;
                    Bms_XXX_MarkeRGeniusIntegration_Cariler item = (Bms_XXX_MarkeRGeniusIntegration_Cariler)grv_Malzeme.GetRow(selectedRows[i]);
                    //if (HELPER.SqlSelectLogo("SELECT * FROM " + BMTABLENAME + " WITH(NOLOCK) WHERE id = " + item.id).Rows.Count > 0)
                    //    continue;
                    SqlCommand com = null;
                    //check if exists
                    com = new SqlCommand($@"SELECT * FROM GENIUS3.CUSTOMER WHERE CODE = '{item.CODE}'", CON, TRANSACTION);
                    if (com.ExecuteScalar() != null)
                        isUpdate = true;

                    #region GENIUS3_CUSTOMER

                    GENIUS3_CUSTOMER GC = new GENIUS3_CUSTOMER();
                    GC.CODE = item.CODE;
                    GC.NAME = item.EXPLANATION;
                    GC.PARAM1 = item.CODE;
                    GC.CREATE_DATE = item.TARIH.Date;
                    GC.MODIFY_DATE = item.TARIH.Date;
                    GC.UPDATESEQ = 1;
                    long GENIUS3_CUSTOMER_ID = 10010000000001;
                    if (!isUpdate)
                    {
                        com = new SqlCommand($@"SELECT ISNULL(MAX(ID),0)+1 FROM GENIUS3.CUSTOMER ", CON, TRANSACTION);
                        try { GENIUS3_CUSTOMER_ID = Convert.ToInt64(com.ExecuteScalar()); } catch { }
                        if (GENIUS3_CUSTOMER_ID.ToString().Length < 2)
                            GENIUS3_CUSTOMER_ID = 10010000000001;
                        //insert to sql and get id
                        com = new SqlCommand($@"INSERT INTO GENIUS3.CUSTOMER (ID,CODE,NAME,PARAM1,CREATE_DATE,MODIFY_DATE) VALUES (@ID,@CODE,@NAME,@PARAM1,@CREATE_DATE,@MODIFY_DATE) SELECT SCOPE_IDENTITY()", CON, TRANSACTION);
                        com.Parameters.AddWithValue("@ID", GENIUS3_CUSTOMER_ID);
                    }
                    else //update
                    {
                        com = new SqlCommand($@"SELECT ID FROM GENIUS3.CUSTOMER WITH(NOLOCK) WHERE CODE = '{item.CODE}'", CON, TRANSACTION);
                        try { GENIUS3_CUSTOMER_ID = Convert.ToInt64(com.ExecuteScalar()); } catch { }
                        //insert to sql and get id
                        com = new SqlCommand($@"UPDATE GENIUS3.CUSTOMER SET NAME = @NAME,PARAM1 = @PARAM1,CREATE_DATE = @CREATE_DATE,MODIFY_DATE = @MODIFY_DATE WHERE CODE = @CODE", CON, TRANSACTION);
                    }
                    com.Parameters.AddWithValue("@CODE", GC.CODE);
                    com.Parameters.AddWithValue("@NAME", GC.NAME);
                    com.Parameters.AddWithValue("@PARAM1", GC.PARAM1);
                    com.Parameters.AddWithValue("@CREATE_DATE", GC.CREATE_DATE);
                    com.Parameters.AddWithValue("@MODIFY_DATE", GC.MODIFY_DATE);
                    com.ExecuteScalar();

                    #endregion

                    #region GENIUS3_CARD

                    GENIUS3_CARD G3C = new GENIUS3_CARD();
                    G3C.CODE = item.CODE;
                    G3C.PASSWORD = "1111";
                    G3C.FK_CUSTOMER = GENIUS3_CUSTOMER_ID;
                    G3C.EXPIRE_DATE = item.TARIH.Date;
                    G3C.GIVEN_DATE = item.TARIH.Date;
                    G3C.CREATE_DATE = item.TARIH.Date;
                    G3C.MODIFY_DATE = item.TARIH.Date;
                    long GENIUS3_CARD_ID = 10010000000001;
                    if (!isUpdate)
                    {
                        com = new SqlCommand($@"SELECT ISNULL(MAX(ID),0)+1 FROM GENIUS3.CARD ", CON, TRANSACTION);
                        try { GENIUS3_CARD_ID = Convert.ToInt64(com.ExecuteScalar()); } catch { }
                        if (GENIUS3_CARD_ID.ToString().Length < 2)
                            GENIUS3_CARD_ID = 10010000000001;
                        com = new SqlCommand($@"INSERT INTO GENIUS3.CARD (ID,CODE,PASSWORD,FK_CUSTOMER,EXPIRE_DATE,GIVEN_DATE,CREATE_DATE,MODIFY_DATE,STATUS,UPDATESEQ,IS_REDEEMER) VALUES (@ID,@CODE,@PASSWORD,@FK_CUSTOMER,@EXPIRE_DATE,@GIVEN_DATE,@CREATE_DATE,@MODIFY_DATE,@STATUS,@UPDATESEQ,@IS_REDEEMER) SELECT SCOPE_IDENTITY()", CON, TRANSACTION);
                        com.Parameters.AddWithValue("@ID", GENIUS3_CARD_ID);
                    }
                    else
                    {
                        com = new SqlCommand($@"SELECT ID FROM GENIUS3.CARD WHERE FK_CUSTOMER = {GENIUS3_CUSTOMER_ID}", CON, TRANSACTION);
                        try { GENIUS3_CARD_ID = Convert.ToInt64(com.ExecuteScalar()); } catch { }
                        com = new SqlCommand($@"UPDATE GENIUS3.CARD SET CODE = @CODE,PASSWORD = @PASSWORD,FK_CUSTOMER = @FK_CUSTOMER,EXPIRE_DATE = @EXPIRE_DATE,GIVEN_DATE = @GIVEN_DATE,CREATE_DATE = @CREATE_DATE,MODIFY_DATE = @MODIFY_DATE WHERE ID = @ID", CON, TRANSACTION);
                        com.Parameters.AddWithValue("@ID", GENIUS3_CARD_ID);
                    }
                    com.Parameters.AddWithValue("@CODE", G3C.CODE);
                    com.Parameters.AddWithValue("@PASSWORD", G3C.PASSWORD);
                    com.Parameters.AddWithValue("@FK_CUSTOMER", G3C.FK_CUSTOMER);
                    com.Parameters.AddWithValue("@EXPIRE_DATE", G3C.EXPIRE_DATE);
                    com.Parameters.AddWithValue("@GIVEN_DATE", G3C.GIVEN_DATE);
                    com.Parameters.AddWithValue("@CREATE_DATE", G3C.CREATE_DATE);
                    com.Parameters.AddWithValue("@MODIFY_DATE", G3C.MODIFY_DATE);
                    com.Parameters.AddWithValue("@STATUS", G3C.STATUS);
                    com.Parameters.AddWithValue("@UPDATESEQ", G3C.UPDATESEQ);
                    com.Parameters.AddWithValue("@IS_REDEEMER", G3C.IS_REDEEMER);
                    com.ExecuteScalar();

                    #endregion

                    #region GENIUS3_CUSTOMER_EXTENSION

                    GENIUS3_CUSTOMER_EXTENSION CE = new GENIUS3_CUSTOMER_EXTENSION();
                    CE.FK_CUSTOMER = GENIUS3_CUSTOMER_ID;
                    CE.CUST_PARAMETER = item.INDIRIM;
                    CE.ID_DATE_OF_BIRTH = item.TARIH;
                    CE.FK_LOCATION_HOME = 1;
                    CE.FK_LOCATION_WORK = 1;
                    CE.FK_LOCATION_LETTER = 1;
                    CE.ID_FK_CITY = 1;
                    CE.ID_GIVEN_DATE = item.TARIH.Date;
                    CE.PHONE_1 = item.TELNR;
                    CE.CREATE_DATE = item.TARIH.Date;
                    CE.MODIFY_DATE = item.TARIH.Date;
                    CE.UPDATESEQ = 1;
                    long GENIUS3_CUSTOMER_EXTENSION_ID = 10010000000001;
                    if (!isUpdate)
                    {
                        com = new SqlCommand($@"SELECT ISNULL(MAX(ID),0)+1 FROM GENIUS3.CUSTOMER_EXTENSION ", CON, TRANSACTION);
                        try { GENIUS3_CUSTOMER_EXTENSION_ID = Convert.ToInt64(com.ExecuteScalar()); } catch { }
                        if (GENIUS3_CUSTOMER_EXTENSION_ID.ToString().Length < 2)
                            GENIUS3_CUSTOMER_EXTENSION_ID = 10010000000001;
                        com = new SqlCommand($@"INSERT INTO GENIUS3.CUSTOMER_EXTENSION (ID,FK_CUSTOMER,CUST_PARAMETER,ID_DATE_OF_BIRTH,FK_LOCATION_HOME,FK_LOCATION_WORK,FK_LOCATION_LETTER,ID_FK_CITY,ID_GIVEN_DATE,PHONE_1,CREATE_DATE,MODIFY_DATE,UPDATESEQ) VALUES (@ID,@FK_CUSTOMER,@CUST_PARAMETER,@ID_DATE_OF_BIRTH,@FK_LOCATION_HOME,@FK_LOCATION_WORK,@FK_LOCATION_LETTER,@ID_FK_CITY,@ID_GIVEN_DATE,@PHONE_1,@CREATE_DATE,@MODIFY_DATE,@UPDATESEQ) SELECT SCOPE_IDENTITY()", CON, TRANSACTION);
                        com.Parameters.AddWithValue("@ID", GENIUS3_CUSTOMER_EXTENSION_ID);

                    }
                    else
                    {
                        com = new SqlCommand($@"SELECT ID FROM GENIUS3.CUSTOMER_EXTENSION WHERE FK_CUSTOMER = {GENIUS3_CUSTOMER_ID}", CON, TRANSACTION);
                        try { GENIUS3_CUSTOMER_EXTENSION_ID = Convert.ToInt64(com.ExecuteScalar()); } catch { }
                        com = new SqlCommand($@"UPDATE GENIUS3.CUSTOMER_EXTENSION SET FK_CUSTOMER = @FK_CUSTOMER,CUST_PARAMETER = @CUST_PARAMETER,ID_DATE_OF_BIRTH = @ID_DATE_OF_BIRTH,FK_LOCATION_HOME = @FK_LOCATION_HOME,FK_LOCATION_WORK = @FK_LOCATION_WORK,FK_LOCATION_LETTER = @FK_LOCATION_LETTER,ID_FK_CITY = @ID_FK_CITY,ID_GIVEN_DATE = @ID_GIVEN_DATE,PHONE_1 = @PHONE_1,CREATE_DATE = @CREATE_DATE,MODIFY_DATE = @MODIFY_DATE,UPDATESEQ = @UPDATESEQ WHERE ID = @ID", CON, TRANSACTION);
                        com.Parameters.AddWithValue("@ID", GENIUS3_CUSTOMER_EXTENSION_ID);
                    }
                    com.Parameters.AddWithValue("@FK_CUSTOMER", CE.FK_CUSTOMER);
                    com.Parameters.AddWithValue("@CUST_PARAMETER", CE.CUST_PARAMETER);
                    com.Parameters.AddWithValue("@ID_DATE_OF_BIRTH", CE.ID_DATE_OF_BIRTH);
                    com.Parameters.AddWithValue("@FK_LOCATION_HOME", CE.FK_LOCATION_HOME);
                    com.Parameters.AddWithValue("@FK_LOCATION_WORK", CE.FK_LOCATION_WORK);
                    com.Parameters.AddWithValue("@FK_LOCATION_LETTER", CE.FK_LOCATION_LETTER);
                    com.Parameters.AddWithValue("@ID_FK_CITY", CE.ID_FK_CITY);
                    com.Parameters.AddWithValue("@ID_GIVEN_DATE", CE.ID_GIVEN_DATE);
                    com.Parameters.AddWithValue("@PHONE_1", CE.PHONE_1);
                    com.Parameters.AddWithValue("@CREATE_DATE", CE.CREATE_DATE);
                    com.Parameters.AddWithValue("@MODIFY_DATE", CE.MODIFY_DATE);
                    com.Parameters.AddWithValue("@UPDATESEQ", CE.UPDATESEQ);
                    com.ExecuteScalar();

                    #endregion
                }
                TRANSACTION.Commit();
            }
            catch (Exception ex) {
                HELPER.LOGYAZ(ex.ToString(), null);
            }
            SplashScreenManager.CloseForm(false);
            InitializeData(null, null);
            XtraMessageBox.Show("Kayıt İşlemi Tamamlandı ", "İşlem Sonucu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}