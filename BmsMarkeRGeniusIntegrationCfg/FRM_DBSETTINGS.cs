using BmsMarkeRGeniusIntegrationLibrary;
using System;
using System.IO;
using System.Windows.Forms;

namespace BmsMarkeRGeniusIntegrationCfg
{
    public partial class FRM_DBSETTINGS : DevExpress.XtraEditors.XtraForm
    {
        CONFIG CFG;
        public FRM_DBSETTINGS()
        {
            InitializeComponent();
            CFG = CONFIG_HELPER.GET_CONFIG();
            INITIALIZE_VALUES();
            HELPER.disableResizingForm(this);
        }

        private void INITIALIZE_VALUES()
        {
            if (CFG != null)
            {
                try { te_LogoSqlUsername.Text = CFG.LGDBUSERNAME; } catch { }
                try { te_LogoSqPassword.Text = CFG.LGDBPASSWORD; } catch { }
                try { te_LogoSqlServer.Text = CFG.LGDBSERVER; } catch { }
                try { te_LogoSqlDb.Text = CFG.LGDBDATABASE; } catch { }
                try { te_LogoFirmnr.Text = CFG.FIRMNR; } catch { }
                try { te_LogoPeriodnr.Text = CFG.PERIOD; } catch { }
                try { rb_FirmBasedCurr.Checked = CFG.ISFIRMBASEDCURR == "1" ? true : false; } catch { }
                try { rb_NotFirmBasedCurr.Checked = rb_FirmBasedCurr.Checked == true ? false : true; } catch { }
                try { rb_DoDebtCloseYes.Checked = CFG.DODEBTCLOSE == "1" ? true : false; } catch { }
                try { te_LogoDefaultBranchForGeniusSending.Text = CFG.DefaultBranchForGeniusSending; } catch { }
                try { te_LogoUsername.Text = CFG.LOBJECTDEFAULTUSERNAME; } catch { }
                try { te_LogoPassword.Text = CFG.LOBJECTDEFAULTPASSWORD; } catch { }
                try { te_OtherUsername.Text = CFG.OTHERUSERNAME; } catch { }
                try { te_OtherPassword.Text = CFG.OTHERPASSWORD; } catch { }
                try { te_OtherServer.Text = CFG.OTHERSERVER; } catch { }
                try { te_OtherPort.Text = CFG.OTHERPORT; } catch { }
                try { te_OtherDb.Text = CFG.OTHERDATABASE; } catch { }
                try { te_ApiUrl.Text = CFG.APIURL; } catch { }
                try { te_ApiUsername.Text = CFG.APIUSERNAME; } catch { }
                try { te_ApiPassword.Text = CFG.APIPASSWORD; } catch { }
                try { cb_NCRDevrede.Checked = CFG.ISNCRACTIVE == "1" ? true : false; } catch { }
                try { cb_GeniusDevrede.Checked = CFG.ISGENIUSACTIVE == "1" ? true : false; } catch { }
                try { te_NcrBaseUrl.Text = CFG.NCRBASEURL; } catch { }
                try { te_NcrUsername.Text = CFG.NCRUSERNAME; } catch { }
                try { te_NcrPassword.Text = CFG.NCRPASSWORD; } catch { }
                try { rb_EncodingANSI.Checked = CFG.FILEENCODING == "ANSI"; rb_EncodingUTF8.Checked = CFG.FILEENCODING != "ANSI"; } catch { }
                try { te_ItemServiceBranches.Text = CFG.ITEM_SERVICE_BRANCHES; } catch { }
                try { te_GeniusApiPort.Text = CFG.GENIUSAPIPORT; } catch { }
            }
        }

        private void simpleButtonKAYDET_Click(object sender, EventArgs e)
        {
            string OtherPassword = te_OtherPassword.Text.Replace("&", "&amp;");

            File.WriteAllText(CONFIG_HELPER._xmlPath, string.Format("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
            "<BILMARKSOFTWARE>" +
                "<LGDB>" + //NODE 0 
                    "<USERNAME>" + te_LogoSqlUsername.Text + "</USERNAME>" +
                    "<PASSWORD>" + te_LogoSqPassword.Text + "</PASSWORD>" +
                    "<SERVER>" + te_LogoSqlServer.Text + "</SERVER>" +
                    "<DATABASE>" + te_LogoSqlDb.Text + "</DATABASE>" +
                "</LGDB>" +
                "<CAPIFIRM>" + //NODE 1
                    "<FIRMNR>" + (te_LogoFirmnr.Text + "</FIRMNR>" +
                    "<PERIOD>" + te_LogoPeriodnr.Text + "</PERIOD>" +
                    "<ISFIRMBASEDCURR>" + (rb_FirmBasedCurr.Checked == true ? "1" : "0") + "</ISFIRMBASEDCURR>" +
                    "<DODEBTCLOSE>" + (rb_DoDebtCloseYes.Checked == true ? "1" : "0") + "</DODEBTCLOSE>" +
                    "<DefaultBranchForGeniusSending>" + te_LogoDefaultBranchForGeniusSending.Text + "</DefaultBranchForGeniusSending>" +
                "</CAPIFIRM>" +
                "<DEFAULTUSERS>" + //NODE 2
                    "<LOBJECTUSERNAME>" + te_LogoUsername.Text + "</LOBJECTUSERNAME>" +
                    "<LOBJECTPASSWORD>" + te_LogoPassword.Text + "</LOBJECTPASSWORD>" +
                "</DEFAULTUSERS>" +
                "<OTHERSERVER>" + //NODE 3 
                    "<OTHERUSERNAME>" + te_OtherUsername.Text + "</OTHERUSERNAME>" +
                    "<OTHERPASSWORD>" + OtherPassword + "</OTHERPASSWORD>" +
                    "<OTHERSERVERURL>" + te_OtherServer.Text + "</OTHERSERVERURL>" +
                    "<OTHERPORT>" + te_OtherPort.Text + "</OTHERPORT>" +
                    "<OTHERDATABASE>" + te_OtherDb.Text + "</OTHERDATABASE>" +
                "</OTHERSERVER>" +
                "<ORION>" + //NODE 4
                    "<APIURL>" + te_ApiUrl.Text + "</APIURL>" +
                    "<APIUSERNAME>" + te_ApiUsername.Text + "</APIUSERNAME>" +
                    "<APIPASSWORD>" + te_ApiPassword.Text + "</APIPASSWORD>" +
                "</ORION>" +
                "<INTEGRATION>" + //NODE 5
                    "<ISNCRACTIVE>" + (cb_NCRDevrede.Checked == true ? "1" : "0") + "</ISNCRACTIVE>" +
                    "<ISGENIUSACTIVE>" + (cb_GeniusDevrede.Checked == true ? "1" : "0") + "</ISGENIUSACTIVE>" +
                    "<NCRBASEURL>" + te_NcrBaseUrl.Text + "</NCRBASEURL>" +
                    "<NCRUSERNAME>" + te_NcrUsername.Text + "</NCRUSERNAME>" +
                    "<NCRPASSWORD>" + te_NcrPassword.Text + "</NCRPASSWORD>" +
                    "<FILEENCODING>" + (rb_EncodingANSI.Checked ? "ANSI" : "UTF8") + "</FILEENCODING>" +
                    "<ITEM_SERVICE_BRANCHES>" + te_ItemServiceBranches.Text + "</ITEM_SERVICE_BRANCHES>" +
                    "<GENIUSAPIPORT>" + te_GeniusApiPort.Text + "</GENIUSAPIPORT>" +
                "</INTEGRATION>" +
            "</BILMARKSOFTWARE>")));

            if (CONFIG_HELPER.EncryptFile(CONFIG_HELPER._xmlPath, CONFIG_HELPER._datPath, CONFIG_HELPER._key))
            {
                try
                {
                    CONFIG_HELPER.DecryptFile(CONFIG_HELPER._datPath, CONFIG_HELPER._xmlPath, CONFIG_HELPER._key);
                    if (File.Exists(CONFIG_HELPER._xmlPath))
                        File.Delete(CONFIG_HELPER._xmlPath);
                }
                catch { }
            }
            MessageBox.Show("VERİTABANI KONFİGÜRASYON DOSYASI KAYDEDİLDİ.\n\nDEĞİŞİKLİKLERİN ETKİLİ OLABİLMESİ İÇİN PROGRAMA YENİDEN GİRİŞ YAPILMASI GEREKMEKTEDİR.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.OK;
            Close();
        }
        private void simpleButtonVAZGEC_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}