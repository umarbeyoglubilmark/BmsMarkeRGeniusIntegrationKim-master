using BmsMarkeRGeniusIntegrationLibrary;
using BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS;
using DevExpress.XtraEditors;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BmsMarkeRGeniusIntegrationCfg
{
    public partial class FRM_LOGIN : DevExpress.XtraEditors.XtraForm
    {
        CONFIG CFG;
        public FRM_LOGIN()
        {
            try
            {
                InitializeComponent();
                HELPER.disableResizingForm(this);
                pe_Logo.Image = Image.FromFile(HELPER.LogoPath());
                pe_Logo.Properties.ShowMenu = false;
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "frm_login_error.log");
                File.WriteAllText(logPath, DateTime.Now.ToString() + Environment.NewLine + ex.ToString());
                throw;
            }
        }
        private void sb_Login_Click(object sender, EventArgs e)
        {
            LOGINTO_FRM_MAIN();
        }
        void LOGINTO_FRM_MAIN()
        {
            if (te_Username.Text.ToLower() == "bms" && te_Password.Text.ToLower() == "pk8412500/")
            {
                FRM_MAIN f = new FRM_MAIN(true);
                f.Show();
                Hide();
            }
            else
            {
                FRM_MAIN f = new FRM_MAIN(false);
                f.Show();
                Hide();
                //CFG = CONFIG_HELPER.GET_CONFIG();
                //if (CFG == null)
                //{
                //    XtraMessageBox.Show("Lütfen önce ayarları yapınız.", "Hata", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                //    //USİNG FRM_DBSETTINGS
                //    using (FRM_DBSETTINGS dBSETTINGS = new FRM_DBSETTINGS())
                //    {
                //        if (dBSETTINGS.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                //        {
                //            XtraMessageBox.Show("Ayarların etkili olabilmesi için program yeniden başlatılacaktır.", "Bilgi", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                //            Application.Restart();
                //        }
                //    }
                //}

                //Bms_XXX_ArhunTicimax_Users user = new Bms_XXX_ArhunTicimax_Users();
                //try { user = HELPER.DataTableToList<Bms_XXX_ArhunTicimax_Users>(HELPER.SqlSelectLogo($"SELECT * FROM Bms_{CFG.FIRMNR}_ArhunTicimax_Users WHERE Username='{te_Username.Text}' AND Password='{(te_Password.Text)}'"))[0]; } catch { }
                //if (user != null && user.Id > 0)
                //{
                //    HELPER.user = user;
                //    FRM_MAIN f = new FRM_MAIN();
                //    f.Show();
                //    Hide();
                //}
                //else
                //{
                //    XtraMessageBox.Show("Kullanıcı adı veya şifre hatalı.", "Hata", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                //}
            }

        }
    }
}