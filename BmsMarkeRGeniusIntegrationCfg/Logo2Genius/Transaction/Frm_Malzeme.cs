using BmsMarkeRGeniusIntegrationLibrary;
using BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.TextEditController.Utils;
using DevExpress.XtraSplashScreen;
using Newtonsoft.Json;
using Newtonsoft.Json;
using Newtonsoft.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Serialization;
using System;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq;
using System.Net;
using System.Net;
using System.Net;
using System.Net.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Text;
using System.Text;
using System.Threading;
using System.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace BmsMarkeRGeniusIntegrationCfg.Logo2Genius.Transaction
{

    // 1) Swagger'ın beklediği DTO'lar

    public partial class Frm_Malzeme : DevExpress.XtraEditors.XtraForm
    {
        CONFIG CFG;
        string BMTABLENAME = "Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler";
        string VALUETABLE = "Bms_XXX_MarkeRGeniusIntegration_IbmKasa";
        List<Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler> OList = new List<Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler>();
        public Frm_Malzeme(string HEADERNAME)
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
            // NCR ve Genius buton görünürlükleri
            button1.Visible = CFG.ISNCRACTIVE == "1";  // NCR KAYDET
            button2.Visible = CFG.ISGENIUSACTIVE == "1";  // GENIUS KAYDET
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
            if (rb_ChangedItems.Checked)
            {
                where = $@" AND (TARIH>= '{sqlFormattedDateStart}' AND TARIH<= '{sqlFormattedDateEnd}') ";

            }
            if (rb_UnusedItems.Checked)
            {
                where = $@" AND ACTIVE = 1 ";

            }
            OList = HELPER.DataTableToList<Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler>(HELPER.SqlSelectLogo($@"SELECT * FROM {BMTABLENAME}({wh}) WHERE 1=1 {where} "));
            grc_Malzeme.DataSource = OList;
            SplashScreenManager.CloseForm(false);
        }

        private void ExportToExcel(object sender, EventArgs e)
        {
            HELPER.DxExportGridToExcel(grv_Malzeme, true);
        }

        private void sb_SaveToBM_Click(object sender, EventArgs e)
        {
            SplashScreenManager.ShowForm(this, typeof(PROGRESSFORM), true, true, false);
            SplashScreenManager.Default.SetWaitFormCaption("LÜTFEN BEKLEYİN.");
            SplashScreenManager.Default.SetWaitFormDescription("");
            string LGCONSTR = string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3};MultipleActiveResultSets=True;", CFG.LGDBSERVER, CFG.LGDBDATABASE, CFG.LGDBUSERNAME, CFG.LGDBPASSWORD);
            SqlConnection CON = new SqlConnection(LGCONSTR);
            SqlTransaction TRANSACTION = null;
            if (CON.State != ConnectionState.Open)
                CON.Open();
            TRANSACTION = CON.BeginTransaction();
            var selectedRows = grv_Malzeme.GetSelectedRows();
            for (int i = 0; i < selectedRows.Length; i++)
            {
                Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler item = (Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler)grv_Malzeme.GetRow(selectedRows[i]);
                //if (HELPER.SqlSelectLogo("SELECT * FROM " + BMTABLENAME + " WITH(NOLOCK) WHERE id = " + item.id).Rows.Count > 0)
                //    continue;
                SqlCommand com = null;
                //com = SIC.Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler_INSERT(item, true, false, CFG.FIRMNR);
                com.Connection = CON;
                com.Transaction = TRANSACTION;
                com.ExecuteScalar();
            }
            TRANSACTION.Commit();
            SplashScreenManager.CloseForm(false);
            InitializeData(null, null);
            XtraMessageBox.Show("Kayıt İşlemi Tamamlandı ", "İşlem Sonucu", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void sb_SaveFile_Click(object sender, EventArgs e)
        {
        
        }
        string PrmControl(string text)
        {
            //returned text should be string and should be between 1-32000 else return empty
            bool isTextNumeric = Int16.TryParse(text, out Int16 textFixed);
            if (isTextNumeric)
            {
                if (textFixed > 0 && textFixed <= 32000)
                    return textFixed.ToString();
                else
                    return "";
            }
            else
                return "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (gle_Value.EditValue == null)
                {
                    XtraMessageBox.Show("Lütfen Değer Seçiniz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string fileNameFromDateTime = "PLU_" + gle_Value.EditValue.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                string path = l_FilePath.Text;
                var selectedRows = grv_Malzeme.GetSelectedRows();
                if (selectedRows.Length == 0)
                {
                    XtraMessageBox.Show("Lütfen Kayıt Seçiniz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string g3Value = HELPER.SqlSelectLogo($@"SELECT top 1 G3Value FROM {VALUETABLE} WHERE LogoValue = '{gle_Value.EditValue}'").Rows[0][0].ToString();
                if (string.IsNullOrEmpty(g3Value))
                {
                    XtraMessageBox.Show("Lütfen IbmKasa Tanımlamalarından G3Value Değerini Doldurunuz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Encoding ayarını CFG'den al
                Encoding fileEncoding = CFG.FILEENCODING == "ANSI" ? Encoding.GetEncoding(1254) : Encoding.UTF8;

                // Max dosya boyutu: 10 MB
                const long MAX_FILE_SIZE = 10 * 1024 * 1024;
                long currentFileSize = 0;
                int partNumber = 1;
                int totalParts = 1;

                // Dosya adı oluştur (ilk part için _part1 eklenmez, sadece birden fazla part olursa eklenir)
                string GetFilePath(int part, bool multiPart) => multiPart
                    ? path + fileNameFromDateTime + "_part" + part + ".txt"
                    : path + fileNameFromDateTime + ".txt";

                string currentFilePath = GetFilePath(partNumber, false);
                StreamWriter sw = new StreamWriter(currentFilePath, false, fileEncoding);
                List<string> createdFiles = new List<string> { currentFilePath };

                try
                {
                    for (int i = 0; i < selectedRows.Length; i++)
                    {
                        Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler item = (Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler)grv_Malzeme.GetRow(selectedRows[i]);
                        string line1 = "", line2 = "", line3 = "", line4 = "", line5 = "", line6 = "", line7 = "", line8 = "", line9 = "";

                        // ANSI formatında \r\n karakterlerini temizle
                        string barcode = CFG.FILEENCODING == "ANSI" ? (item.BARCODE ?? "").Replace("\r", "").Replace("\n", "") : item.BARCODE ?? "";
                        string explanation = CFG.FILEENCODING == "ANSI" ? (item.EXPLANATION ?? "").Replace("\r", "").Replace("\n", "") : item.EXPLANATION ?? "";

                        string sellingPrice = item.SELLING_PRICE1.ToString().Replace(",", ".");
                        if (!sellingPrice.Contains("."))
                            sellingPrice = sellingPrice + ".00";

                        bool isSPECODEisNumeric = Int16.TryParse(item.SPECODE, out Int16 SPECODEFixed);
                        bool isSPECODE2isNumeric = Int16.TryParse(item.SPECODE2, out Int16 SPECODE2Fixed);
                        bool isSPECODE3isNumeric = Int16.TryParse(item.SPECODE3, out Int16 SPECODE3Fixed);
                        bool isSPECODE4isNumeric = Int16.TryParse(item.SPECODE4, out Int16 SPECODE4Fixed);
                        bool isSPECODE5isNumeric = Int16.TryParse(item.SPECODE5, out Int16 SPECODE5Fixed);
                        if (!isSPECODEisNumeric) SPECODEFixed = 0;
                        if (!isSPECODE2isNumeric) SPECODE2Fixed = 0;
                        if (!isSPECODE3isNumeric) SPECODE3Fixed = 0;
                        if (!isSPECODE4isNumeric) SPECODE4Fixed = 0;
                        if (!isSPECODE5isNumeric) SPECODE5Fixed = 0;

                        #region ERDENER
                        line1 = $@"1;{barcode};{explanation};0;{explanation};{explanation};{explanation};{item.UNIT1IBM};1;0;{(item.UNIT1IBM == "1000" ? "1" : "0")};{item.VAT_CODE};{(item.UNIT1IBM == "1000" ? "1" : "0")};0";
                        line2 = $@"2;{g3Value};{barcode};{explanation};{explanation};{explanation};;0;0;0;0;0;0;0;0;0;0;0;0;0;0;1;0";
                        line3 = $@"3;{barcode};{barcode};0;1;1";
                        line4 = $@"4;{g3Value};{barcode};1;{sellingPrice};{sellingPrice};0";
                        line5 = $@"4;{g3Value};{barcode};2;{sellingPrice};{sellingPrice};0";
                        line6 = $@"5;{g3Value};{barcode};1;{SPECODE3Fixed.ToString()};{item.SPECODE3}";
                        line7 = $@"5;{g3Value};{barcode};2;{SPECODEFixed.ToString()};{item.SPECODE}";
                        line8 = $@"5;{g3Value};{barcode};3;{SPECODE2Fixed.ToString()};{item.SPECODE2}";
                        line9 = $@"5;{g3Value};{barcode};4;{SPECODE5Fixed.ToString()};{item.SPECODE5}";
                        #endregion

                        #region ERULKU
                        line1 = $@"1;{barcode};{explanation};0;{explanation};{explanation};{explanation};{item.UNIT1IBM};1;0;{(item.UNIT1IBM == "1000" ? "1" : "0")};{item.VAT_CODE};{(item.UNIT1IBM == "1000" ? "1" : "0")};0;";
                        line2 = $@"2;{g3Value};{barcode};{explanation};{explanation};{explanation};;0;0;0;0;0;0;0;0;0;0;0;0;0;0;{(item.ACTIVE == 0 ? "1" : "0")};0";
                        line3 = $@"3;{barcode};{barcode};0;1;1;";
                        line4 = $@"4;{g3Value};{barcode};1;{sellingPrice};{sellingPrice};0;";
                        line5 = $@"4;{g3Value};{barcode};2;{sellingPrice};{sellingPrice};0;";
                        line6 = $@"5;{g3Value};{barcode};1;0;{PrmControl(item.SPECODE)};";
                        line7 = $@"5;{g3Value};{barcode};2;0;{PrmControl(item.SPECODE2)};";
                        line8 = $@"5;{g3Value};{barcode};3;0;{PrmControl(item.SPECODE3)};";
                        line9 = $@"5;{g3Value};{barcode};4;0;{PrmControl(item.MARKCODE)};";
                        #endregion

                        // Satırları bir listeye ekle
                        List<string> lines = new List<string> { line1, line2 };
                        if (!string.IsNullOrEmpty(item.BARCODE))
                            lines.Add(line3);
                        lines.Add(line4);
                        lines.Add(line5);
                        lines.Add(line6);
                        lines.Add(line7);
                        lines.Add(line8);
                        lines.Add(line9);

                        // Tüm satırların boyutunu hesapla
                        long blockSize = 0;
                        foreach (var line in lines)
                        {
                            blockSize += fileEncoding.GetByteCount(line + Environment.NewLine);
                        }

                        // Yeni blok eklenince 10 MB'ı aşacaksa yeni dosya oluştur
                        if (currentFileSize > 0 && currentFileSize + blockSize > MAX_FILE_SIZE)
                        {
                            // Mevcut dosyayı kapat
                            sw.Close();

                            // İlk dosyayı _part1 olarak yeniden adlandır (birden fazla part varsa)
                            if (partNumber == 1)
                            {
                                string newFirstFilePath = GetFilePath(1, true);
                                File.Move(currentFilePath, newFirstFilePath);
                                createdFiles[0] = newFirstFilePath;
                            }

                            // Yeni part numarası
                            partNumber++;
                            totalParts = partNumber;
                            currentFilePath = GetFilePath(partNumber, true);
                            createdFiles.Add(currentFilePath);

                            // Yeni dosya oluştur
                            sw = new StreamWriter(currentFilePath, false, fileEncoding);
                            currentFileSize = 0;
                        }

                        // Satırları yaz
                        foreach (var line in lines)
                        {
                            sw.WriteLine(line);
                        }
                        currentFileSize += blockSize;
                    }
                }
                finally
                {
                    sw.Close();
                }

                // Her dosya için .rdy dosyası oluştur
                foreach (var file in createdFiles)
                {
                    File.Create(file.Replace(".txt", ".rdy")).Close();
                }

                string message = totalParts > 1
                    ? $"Kayıt İşlemi Tamamlandı\n{totalParts} parça dosya oluşturuldu."
                    : "Kayıt İşlemi Tamamlandı";
                XtraMessageBox.Show(message, "İşlem Sonucu", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) {
                HELPER.LOGYAZ(ex.ToString(), null);
                XtraMessageBox.Show("Hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private async void button1_Click(object sender, EventArgs e)
        {
            // Ön kontroller
       

            var selectedRows = grv_Malzeme.GetSelectedRows();
            if (selectedRows.Length == 0)
            {
                XtraMessageBox.Show("Lütfen Kayıt Seçiniz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }



            button1.Enabled = false;
            string globaltoken = "";
            // Base URL'i config'den al
            AuthApi.SetBaseUrl(CFG.NCRBASEURL);
            try
            {
                globaltoken = await AuthApi.GetTokenAsync(
                    storeId: 0, posId: 0, cashierId: 0,
                    username: CFG.NCRUSERNAME,
                    password: CFG.NCRPASSWORD,
                    timeout: TimeSpan.FromSeconds(30));

            //         XtraMessageBox.Show("Token alındı.");
                 }
                    catch (Exception ex)
                     {
                    XtraMessageBox.Show(ex.Message);
                    button1.Enabled = true;
                    return;
                 }

            var handler = new HttpClientHandler
            {
                UseProxy = false,
#if DEBUG
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                CheckCertificateRevocationList = false
#endif
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(CFG.NCRBASEURL)
            }; 
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", globaltoken);

            var successes = new List<string>();
            var failures = new List<string>();
            var details = new List<string>();

            var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

            var productsPayload = new List<object>();
            var barcodesPayload = new List<object>();
            var groupPricesPayload = new List<object>();
            var vatsPayload = new List<object>();
            var salesMessagesPayload = new List<object>();
            var productCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var barcodeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


            var duplicateNotes = new List<string>();


            for (int i = 0; i < selectedRows.Length; i++)
            {
                var item = (Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler)grv_Malzeme.GetRow(selectedRows[i]);

                // Normalize alanlar
                var codeRaw = item.CODE ?? string.Empty;
                var barcodeRaw = item.BARCODE ?? string.Empty;
                var code = codeRaw.Trim();
                var barcode = barcodeRaw.Trim();
                var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var isKg = string.Equals(item.UNIT1, "KG", StringComparison.OrdinalIgnoreCase);
                var unitCode = isKg ? "KG" : "ADET";

                // VAT mapping — 4 => 15 olarak düzeltildi
                int vatPercent;
                switch (item.VAT_CODE_N)
                {
                    case 1: vatPercent = 0; break;
                    case 2: vatPercent = 5; break;
                    case 3: vatPercent = 10; break;
                    case 4: vatPercent = 16; break;  // düzeltme
                    case 5: vatPercent = 20; break;
                    default: vatPercent = 0; break;
                }

         


                // ——— products (unique by code) ———
                if (productCodes.Add(code))
                {
                    if (!seenCodes.Add(code))
                        continue;

               
                    productsPayload.Add(new
                    {
                        id = 0,
                        code = code,
                        isActive = true,
                        shortName = item.EXPLANATION,
                        name = item.EXPLANATION,
                        unitCode = unitCode,

                        vatPercent = vatPercent,
                        vatId = item.VAT_CODE_N,
                        buyingVatPercent = vatPercent,
                        buyingVatId = item.VAT_CODE_N,

                        categoryId = 0,
                        categoryCode = "",
                        categoryName = "",
                        brandId = 0,
                        brandCode = "",
                        brandName = item.MARKCODE,

                        genericId = 0,
                        genericCode = "",
                        genericName = "",
                        supplierId = 0,
                        supplierCode = "",
                        supplierName = "",

          

                        code1 = item.GROUPID,
                        code1Id = item.GROUPID,
                        code1Name = item.GROUPINFO,
                        code2 = "",
                        code2Id = 0,
                        code2Name = item.SPECODE2,
                        code3 = "",
                        code3Id = 0,
                        code3Name = item.SPECODE3,
                        code4 = "",
                        code4Id = 0,
                        code4Name = item.SPECODE4,
                        code5 = "",
                        code5Id = 0,
                        code5Name = item.SPECODE5,

                        isGivesBonus = false,
                        bonusMultiplier = 0,
                        priceEntryType = 1,
                        returnType = 1,
                        quantityType = 0,
                        discountType = 1,
                        scaleType = isKg ? 1 : 0,

                        // Mevcutta böyle kullanmışsın; API adını korudum
                        currneysTypeId = 1,     // TRY
                        salesmanSetting = 0,
                        isLunchVoucher = false,
                        installmentNumber = 0,
                        installmentType = 0,

                        description = item.EXPLANATION,
                        productType = 0,
                        salesInformationsId = (int[])null,
                        maxQuantity = 0
                    }); 
                }
                else
                {
                    duplicateNotes.Add($"DUP-PRODUCT ignored: {code}");
                }

                // ——— barcodes (unique by code|barcode) ———
                if (!string.IsNullOrWhiteSpace(barcode))
                {
                    string bkey = $"{code}|{barcode}";
                    if (barcodeKeys.Add(bkey))
                    {
                        barcodesPayload.Add(new
                        {
                            code = code,
                            barcode = barcode,
                            unitCode = unitCode,
                            priceId = 0,
                            quantity = 1
                        });
                    }
                    else
                    {
                        duplicateNotes.Add($"DUP-BARCODE ignored: {bkey}");
                    }
                }

                // ——— groupPrices ——— (eğer burada da duplikasyon yaşanıyorsa benzer set ile filtreleyebilirsin)
                groupPricesPayload.Add(new
                {
                    code = code,
                    groupId = 1,
                    priceId = 0,
                    price = item.SELLING_PRICE1,
                    price2 = 0,
                    price3 = 0,
                    price4 = 0,
                    price5 = 0,
                    nextPrices = new { }
                });

                if (item.ALCOHOL == 1) {
                 
                    salesMessagesPayload.Add(new
                    {
                        code = code,
                        salesMessagesId = 1,

                    });
                }

                vatsPayload.Add(new
                {
                    code = code,
                    countryCode = "CY",
                    percent = vatPercent
                });
            }

            // Now send ONCE
            var postData = new
            {
                products = productsPayload,
                barcodes = barcodesPayload,
                groupPrices = groupPricesPayload,
                vats = vatsPayload,
                salesMessages = salesMessagesPayload
            };

            // İstersen duplicate notlarını logla
            if (duplicateNotes.Count > 0)
                details.Add($"Duplicates filtered: {duplicateNotes.Count} | {string.Join(" ; ", duplicateNotes.Take(10))}{(duplicateNotes.Count > 10 ? " ; ..." : "")}");

            var json = JsonConvert.SerializeObject(postData, jsonSettings);
            HELPER.LOGYAZ(json, null);

 


                        var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                 var resp = await client.PostAsync("api/Product/add-bulk", content);
                  var body = await resp.Content.ReadAsStringAsync();

               
                         bool logicalOk = false;
                try
                {
                                   var obj = JsonConvert.DeserializeObject<dynamic>(body);
                                   logicalOk = (bool?)obj?.success == true && ((int?)obj?.httpStatusCode ?? 200) < 400;
                }
                catch { /* ignore parse issues */ }

                         var codesJoined = string.Join(",", productsPayload.Select(p => (string)p.GetType().GetProperty("code").GetValue(p)));
                            var line = $"BULK[{productsPayload.Count}] -> {(int)resp.StatusCode} {resp.ReasonPhrase} | {body}";
                             details.Add(line);

                             if (resp.IsSuccessStatusCode && logicalOk)
                             successes.Add($"BULK:{productsPayload.Count}");
                             else
                                 failures.Add(line);
            }
            catch (Exception ex)
            {
                           failures.Add($"BULK:{productsPayload.Count} ||| {ex.Message}");
            }

            var total = selectedRows.Length;
            var sb = new StringBuilder();
            sb.AppendLine($"Toplam: {total}");
            sb.AppendLine($"Başarılı: {successes.Count}");
            sb.AppendLine($"Hatalı: {failures.Count}");
            sb.AppendLine();
            sb.AppendLine(string.Join(Environment.NewLine + "--------------------------------" + Environment.NewLine, details));

            XtraMessageBox.Show($"İşlem tamamlandı.\n\n{sb}", "İşlem Sonucu",
                MessageBoxButtons.OK, failures.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            button1.Enabled = true;

        }

    }

    }
