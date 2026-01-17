using BmsMarkeRGeniusIntegrationLibrary;
using BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BmsMarkeRGeniusIntegrationItemService
{
    internal class Program
    {
        static CONFIG CFG;
        static string BMTABLENAME = "Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler";
        static string VALUETABLE = "Bms_XXX_MarkeRGeniusIntegration_IbmKasa";

        [STAThread]
        static void Main(string[] args)
        {
            HELPER.LOGYAZ("ITEM SERVICE STARTED!", null);
            CFG = CONFIG_HELPER.GET_CONFIG();

            if (CFG == null)
            {
                Console.WriteLine("CONFIG ERROR.");
                HELPER.LOGYAZ("CONFIG ERROR - CFG is null", null);
                return;
            }

            BMTABLENAME = BMTABLENAME.Replace("XXX", CFG.FIRMNR);
            VALUETABLE = VALUETABLE.Replace("XXX", CFG.FIRMNR);

            try
            {
                // CFG'den mağaza numaralarını al (varsayılan: "0,1")
                string branchList = CFG.ITEM_SERVICE_BRANCHES;
                if (string.IsNullOrEmpty(branchList))
                    branchList = "0,1";

                string[] branches = branchList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                HELPER.LOGYAZ($"Item Service - Processing branches: {branchList}", null);
                Console.WriteLine($"Item Service - Processing branches: {branchList}");

                // Genius aktif mi kontrol et
                bool isGeniusActive = CFG.ISGENIUSACTIVE == "1";
                // NCR aktif mi kontrol et
                bool isNcrActive = CFG.ISNCRACTIVE == "1";

                HELPER.LOGYAZ($"Genius Active: {isGeniusActive}, NCR Active: {isNcrActive}", null);
                Console.WriteLine($"Genius Active: {isGeniusActive}, NCR Active: {isNcrActive}");

                foreach (string branch in branches)
                {
                    string branchTrimmed = branch.Trim();
                    HELPER.LOGYAZ($"Processing branch: {branchTrimmed}", null);
                    Console.WriteLine($"Processing branch: {branchTrimmed}");

                    try
                    {
                        // Değişen malzemeleri getir (son 1 gün)
                        var items = GetChangedItems(branchTrimmed);

                        if (items == null || items.Count == 0)
                        {
                            HELPER.LOGYAZ($"Branch {branchTrimmed}: No changed items found.", null);
                            Console.WriteLine($"Branch {branchTrimmed}: No changed items found.");
                            continue;
                        }

                        HELPER.LOGYAZ($"Branch {branchTrimmed}: Found {items.Count} changed items.", null);
                        Console.WriteLine($"Branch {branchTrimmed}: Found {items.Count} changed items.");

                        // Genius'a gönder (dosya oluşturma)
                        if (isGeniusActive)
                        {
                            SendToGenius(branchTrimmed, items);
                        }

                        // NCR'a gönder (API)
                        if (isNcrActive)
                        {
                            SendToNcrAsync(branchTrimmed, items).Wait();
                        }
                    }
                    catch (Exception ex)
                    {
                        HELPER.LOGYAZ($"Error processing branch {branchTrimmed}: {ex.Message}", ex);
                        Console.WriteLine($"Error processing branch {branchTrimmed}: {ex.Message}");
                    }
                }

                HELPER.LOGYAZ("ITEM SERVICE FINISHED!", null);
                Console.WriteLine("ITEM SERVICE FINISHED!");
            }
            catch (Exception ex)
            {
                HELPER.LOGYAZ("ITEM SERVICE ERROR!", ex);
                Console.WriteLine($"ITEM SERVICE ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Belirtilen mağaza için değişen malzemeleri getirir
        /// </summary>
        private static List<Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler> GetChangedItems(string warehouseNr)
        {
            try
            {
                // Son 1 günlük değişiklikleri al
                string sqlFormattedDateStart = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                string sqlFormattedDateEnd = DateTime.Now.ToString("yyyy-MM-dd");

                string where = $@" AND (TARIH >= '{sqlFormattedDateStart}' AND TARIH <= '{sqlFormattedDateEnd}') ";

                string query = $@"SELECT * FROM {BMTABLENAME}({warehouseNr}) WHERE 1=1 {where}";

                HELPER.LOGYAZ($"Query: {query}", null);

                var items = HELPER.DataTableToList<Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler>(HELPER.SqlSelectLogo(query));
                return items;
            }
            catch (Exception ex)
            {
                HELPER.LOGYAZ($"GetChangedItems Error: {ex.Message}", ex);
                return new List<Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler>();
            }
        }

        /// <summary>
        /// Genius'a dosya olarak gönderir (G3 formatı)
        /// </summary>
        private static void SendToGenius(string warehouseNr, List<Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler> items)
        {
            try
            {
                // G3Value ve Path bilgisini al
                DataTable valueTable = HELPER.SqlSelectLogo($@"SELECT * FROM {VALUETABLE} WHERE LogoValue = '{warehouseNr}'");
                if (valueTable.Rows.Count == 0)
                {
                    HELPER.LOGYAZ($"SendToGenius: No IbmKasa record found for warehouse {warehouseNr}", null);
                    Console.WriteLine($"SendToGenius: No IbmKasa record found for warehouse {warehouseNr}");
                    return;
                }

                string g3Value = valueTable.Rows[0]["G3Value"]?.ToString();
                string path = valueTable.Rows[0]["Path"]?.ToString();

                if (string.IsNullOrEmpty(g3Value))
                {
                    HELPER.LOGYAZ($"SendToGenius: G3Value is empty for warehouse {warehouseNr}", null);
                    Console.WriteLine($"SendToGenius: G3Value is empty for warehouse {warehouseNr}");
                    return;
                }

                if (string.IsNullOrEmpty(path))
                {
                    HELPER.LOGYAZ($"SendToGenius: Path is empty for warehouse {warehouseNr}", null);
                    Console.WriteLine($"SendToGenius: Path is empty for warehouse {warehouseNr}");
                    return;
                }

                if (!path.EndsWith("\\"))
                    path += "\\";

                // Encoding ayarı
                Encoding fileEncoding = CFG.FILEENCODING == "ANSI" ? Encoding.GetEncoding(1254) : Encoding.UTF8;

                string fileNameFromDateTime = "PLU_" + warehouseNr + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                // Max dosya boyutu: 10 MB
                const long MAX_FILE_SIZE = 10 * 1024 * 1024;
                long currentFileSize = 0;
                int partNumber = 1;
                int totalParts = 1;

                Func<int, bool, string> GetFilePath = (part, multiPart) => multiPart
                    ? path + fileNameFromDateTime + "_part" + part + ".txt"
                    : path + fileNameFromDateTime + ".txt";

                string currentFilePath = GetFilePath(partNumber, false);
                StreamWriter sw = new StreamWriter(currentFilePath, false, fileEncoding);
                List<string> createdFiles = new List<string> { currentFilePath };

                try
                {
                    foreach (var item in items)
                    {
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

                        // ERULKU format
                        line1 = $@"1;{barcode};{explanation};0;{explanation};{explanation};{explanation};{item.UNIT1IBM};1;0;{(item.UNIT1IBM == "1000" ? "1" : "0")};{item.VAT_CODE};{(item.UNIT1IBM == "1000" ? "1" : "0")};0;";
                        line2 = $@"2;{g3Value};{barcode};{explanation};{explanation};{explanation};;0;0;0;0;0;0;0;0;0;0;0;0;0;0;{(item.ACTIVE == 0 ? "1" : "0")};0";
                        line3 = $@"3;{barcode};{barcode};0;1;1;";
                        line4 = $@"4;{g3Value};{barcode};1;{sellingPrice};{sellingPrice};0;";
                        line5 = $@"4;{g3Value};{barcode};2;{sellingPrice};{sellingPrice};0;";
                        line6 = $@"5;{g3Value};{barcode};1;0;{PrmControl(item.SPECODE)};";
                        line7 = $@"5;{g3Value};{barcode};2;0;{PrmControl(item.SPECODE2)};";
                        line8 = $@"5;{g3Value};{barcode};3;0;{PrmControl(item.SPECODE3)};";
                        line9 = $@"5;{g3Value};{barcode};4;0;{PrmControl(item.MARKCODE)};";

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
                            sw.Close();

                            if (partNumber == 1)
                            {
                                string newFirstFilePath = GetFilePath(1, true);
                                File.Move(currentFilePath, newFirstFilePath);
                                createdFiles[0] = newFirstFilePath;
                            }

                            partNumber++;
                            totalParts = partNumber;
                            currentFilePath = GetFilePath(partNumber, true);
                            createdFiles.Add(currentFilePath);

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
                    ? $"Branch {warehouseNr}: {items.Count} items saved to {totalParts} files."
                    : $"Branch {warehouseNr}: {items.Count} items saved to file.";

                HELPER.LOGYAZ($"SendToGenius: {message}", null);
                Console.WriteLine($"SendToGenius: {message}");
            }
            catch (Exception ex)
            {
                HELPER.LOGYAZ($"SendToGenius Error: {ex.Message}", ex);
                Console.WriteLine($"SendToGenius Error: {ex.Message}");
            }
        }

        /// <summary>
        /// NCR API'ye gönderir
        /// </summary>
        private static async Task SendToNcrAsync(string warehouseNr, List<Bmsf_XXX_MarkeRGeniusIntegration_Malzemeler> items)
        {
            try
            {
                // G3Value bilgisini al
                DataTable valueTable = HELPER.SqlSelectLogo($@"SELECT TOP 1 G3Value FROM {VALUETABLE} WHERE LogoValue = '{warehouseNr}'");
                if (valueTable.Rows.Count == 0)
                {
                    HELPER.LOGYAZ($"SendToNcr: No IbmKasa record found for warehouse {warehouseNr}", null);
                    return;
                }

                string g3Value = valueTable.Rows[0][0]?.ToString();
                if (string.IsNullOrEmpty(g3Value))
                {
                    HELPER.LOGYAZ($"SendToNcr: G3Value is empty for warehouse {warehouseNr}", null);
                    return;
                }

                // Base URL'i config'den al
                AuthApi.SetBaseUrl(CFG.NCRBASEURL);

                string token = await AuthApi.GetTokenAsync(
                    storeId: 0, posId: 0, cashierId: 0,
                    username: CFG.NCRUSERNAME,
                    password: CFG.NCRPASSWORD,
                    timeout: TimeSpan.FromSeconds(30));

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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

                var productsPayload = new List<object>();
                var barcodesPayload = new List<object>();
                var groupPricesPayload = new List<object>();
                var vatsPayload = new List<object>();
                var salesMessagesPayload = new List<object>();
                var productCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var barcodeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var item in items)
                {
                    var codeRaw = item.CODE ?? string.Empty;
                    var barcodeRaw = item.BARCODE ?? string.Empty;
                    var code = codeRaw.Trim();
                    var barcode = barcodeRaw.Trim();

                    var isKg = string.Equals(item.UNIT1, "KG", StringComparison.OrdinalIgnoreCase);
                    var unitCode = isKg ? "KG" : "ADET";

                    // VAT mapping
                    int vatPercent;
                    switch (item.VAT_CODE_N)
                    {
                        case 1: vatPercent = 0; break;
                        case 2: vatPercent = 5; break;
                        case 3: vatPercent = 10; break;
                        case 4: vatPercent = 16; break;
                        case 5: vatPercent = 20; break;
                        default: vatPercent = 0; break;
                    }

                    // products (unique by code)
                    if (productCodes.Add(code))
                    {
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
                            currneysTypeId = 1,
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

                    // barcodes (unique by code|barcode)
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
                    }

                    // groupPrices
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

                    // salesMessages (for alcohol)
                    if (item.ALCOHOL == 1)
                    {
                        salesMessagesPayload.Add(new
                        {
                            code = code,
                            salesMessagesId = 1,
                        });
                    }

                    // vats
                    vatsPayload.Add(new
                    {
                        code = code,
                        countryCode = "CY",
                        percent = vatPercent
                    });
                }

                var postData = new
                {
                    products = productsPayload,
                    barcodes = barcodesPayload,
                    groupPrices = groupPricesPayload,
                    vats = vatsPayload,
                    salesMessages = salesMessagesPayload
                };

                var json = JsonConvert.SerializeObject(postData, jsonSettings);
                HELPER.LOGYAZ($"SendToNcr Request: {json}", null);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await client.PostAsync("api/Product/add-bulk", content);
                var body = await resp.Content.ReadAsStringAsync();

                bool logicalOk = false;
                try
                {
                    var obj = JsonConvert.DeserializeObject<dynamic>(body);
                    logicalOk = (bool?)obj?.success == true && ((int?)obj?.httpStatusCode ?? 200) < 400;
                }
                catch { }

                if (resp.IsSuccessStatusCode && logicalOk)
                {
                    HELPER.LOGYAZ($"SendToNcr: Branch {warehouseNr} - {productsPayload.Count} products sent successfully.", null);
                    Console.WriteLine($"SendToNcr: Branch {warehouseNr} - {productsPayload.Count} products sent successfully.");
                }
                else
                {
                    HELPER.LOGYAZ($"SendToNcr: Branch {warehouseNr} - Failed. Status: {(int)resp.StatusCode}, Body: {body}", null);
                    Console.WriteLine($"SendToNcr: Branch {warehouseNr} - Failed. Status: {(int)resp.StatusCode}");
                }

                client.Dispose();
            }
            catch (Exception ex)
            {
                HELPER.LOGYAZ($"SendToNcr Error: {ex.Message}", ex);
                Console.WriteLine($"SendToNcr Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Parametre kontrolü - 1-32000 arası numeric değer döner
        /// </summary>
        private static string PrmControl(string text)
        {
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
    }
}
