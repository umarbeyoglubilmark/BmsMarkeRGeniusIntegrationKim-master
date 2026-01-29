using BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using UnityObjects;
using static BmsMarkeRGeniusIntegrationLibrary.HELPER;
using Application = System.Windows.Forms.Application;

namespace BmsMarkeRGeniusIntegrationLibrary
{
    public class HELPER
    {
        public static string LogoPath()
        {
            return Application.StartupPath + "\\RES\\logo.jpg";
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    
        public static void disableResizingForm(Form form)
        {
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
        }

        /// <summary>
        /// Server={CFG.LGDBSERVER}; Database={CFG.LGDBDATABASE}; User Id ={CFG.LGDBUSERNAME};Password ={CFG.LGDBPASSWORD}
        /// </summary>
        /// <returns></returns>
        public static SqlConnection SqlConnectionSourceLogo()
        {
            CONFIG CFG = CONFIG_HELPER.GET_CONFIG();
            return new SqlConnection(string.Format(@"Server={0}; Database={1}; User Id ={2};Password ={3}", CFG.LGDBSERVER, CFG.LGDBDATABASE, CFG.LGDBUSERNAME, CFG.LGDBPASSWORD));
        }

        /// <summary>
        /// <para>EXAMPLE :</para>
        /// <para>string[] LG_TABLES =  {</para>
        /// <para> "create table test",</para>
        ///<para>  "create table test2"</para>
        /// <para>   };</para>
        /// <para>   HELPER.CREATEDBTABLES(LG_TABLES,false,NULL,CFG);</para>
        /// </summary>
        public static void SqlCreateDbTables(string[] LG_TABLES, bool isCS, string connectionstring)
        {
            CONFIG CFG = CONFIG_HELPER.GET_CONFIG();
            List<string> errorlist = new List<string>();


            SqlDatabase sqlLGDB;
            if (connectionstring == null)
            {
                sqlLGDB = new SqlDatabase(string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3};MultipleActiveResultSets=True;", CFG.LGDBSERVER, CFG.LGDBDATABASE, CFG.LGDBUSERNAME, CFG.LGDBPASSWORD));
            }
            else
            {
                if (isCS == false)
                {
                    sqlLGDB = new SqlDatabase(SqlConnectionSourceLogo().ToString());
                }
                else
                    sqlLGDB = new SqlDatabase(connectionstring);
            }
            foreach (string S in LG_TABLES)
            {
                try { sqlLGDB.ExecuteNonQuery(new SqlCommand(string.Format(S, CFG.FIRMNR))); }
                catch (Exception E)
                {
                    //MessageBox.Show(E.Message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    errorlist.Add(E.Message + "\n");
                }
            }
            //errorlist to string
            string error = "";
            foreach (string S in errorlist)
            {
                error += S;
            }
            MessageBox.Show("TAMAMLANDI\n" + error, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        public static void SqlDeleteCommand(string query,bool isCS, string connectionstring)
        {
            CONFIG CFG = CONFIG_HELPER.GET_CONFIG();

            SqlDatabase sqlLGDB;
            if (connectionstring == null)
            {
                sqlLGDB = new SqlDatabase(string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3};MultipleActiveResultSets=True;", CFG.LGDBSERVER, CFG.LGDBDATABASE, CFG.LGDBUSERNAME, CFG.LGDBPASSWORD));
            }
            else
            {
                if (isCS == false)
                {
                    sqlLGDB = new SqlDatabase(SqlConnectionSourceLogo().ToString());
                }
                else
                    sqlLGDB = new SqlDatabase(connectionstring);
            }
            try { sqlLGDB.ExecuteNonQuery(new SqlCommand(string.Format(query, CFG.FIRMNR))); }
            catch (Exception E)
            {
                MessageBox.Show($"TAMAMLANDI {E}");
            }
            return;
          
        }

        public static void SqlExecute(string sqlQuery)
        {
            SqlCommand sqlCommandItem = new SqlCommand(sqlQuery, SqlConnectionSourceLogo());
            sqlCommandItem.CommandTimeout = 0;
            sqlCommandItem.Connection.Open();
            sqlCommandItem.ExecuteNonQuery();
            sqlCommandItem.Connection.Close();
            SqlConnectionSourceLogo().Close();
        }

        /// <summary>
        /// gridControl1.DataSource = HELPER.SqlSelect("SELECT * FROM BM_PDKS_ADIM2");
        /// </summary>
        public static DataTable SqlSelectLogo(string sqlQuery)
        {
            SqlDataAdapter sqlDataAdapterItem = new SqlDataAdapter(sqlQuery, SqlConnectionSourceLogo());
            sqlDataAdapterItem.SelectCommand.CommandTimeout = 0;
            DataTable dataTableItem = new DataTable();
            sqlDataAdapterItem.Fill(dataTableItem);
            return dataTableItem;
        }

        public static void DxExportGridToExcel(GridView gridview, bool openExcelAfterExport)
        {
            string filename;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = "xlsx files (*.xlsx)|*.xlsx",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            saveFileDialog1.ShowDialog();
            filename = saveFileDialog1.FileName;
            if (filename == "") filename = "";
            else
            {
                gridview.ExportToXlsx(filename);
                if (openExcelAfterExport == true) System.Diagnostics.Process.Start(filename);
            }
        }

        public static void LOGYAZ(string hata, Exception E)
        {
            try
            {
                string text = AppDomain.CurrentDomain.BaseDirectory + "logs\\";
                Directory.CreateDirectory(text);
                string path = text + DateTime.Now.ToString("yyyy.MM.dd") + ".txt";
                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }
                else
                {
                    File.AppendAllText(path, Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine);
                }

                File.AppendAllText(path, DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss") + " : " + hata + Environment.NewLine + ((E != null) ? (" ----- HATA : ----- " + E.ToString()) : ""));
            }
            catch
            {
            }
        }

        //MYSQL SELECT FUNCTION
        public static DataTable MySqlSelect(string sqlQuery)
        {
            CONFIG CFG = CONFIG_HELPER.GET_CONFIG();
            DataTable dataTableItem = new DataTable();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(string.Format("server={0};user id={1};password={2};database={3};persistsecurityinfo=True;port={4};SslMode=none", CFG.OTHERSERVER, CFG.OTHERUSERNAME, CFG.OTHERPASSWORD, CFG.OTHERDATABASE, CFG.OTHERPORT)))
                {
                    connection.Open();
                    MySqlDataAdapter sqlDataAdapterItem = new MySqlDataAdapter(sqlQuery, connection);
                    sqlDataAdapterItem.SelectCommand.CommandTimeout = 0;
                    sqlDataAdapterItem.Fill(dataTableItem);
                    connection.Close();
                }
            }
            catch (Exception E)
            {
                LOGYAZ("MySqlSelect", E);
            }
            return dataTableItem;
        }

        //mysql update function
        public static void MySqlUpdate(string sqlQuery)
        {
            CONFIG CFG = CONFIG_HELPER.GET_CONFIG();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(string.Format("server={0};user id={1};password={2};database={3};persistsecurityinfo=True;port={4};SslMode=none", CFG.OTHERSERVER, CFG.OTHERUSERNAME, CFG.OTHERPASSWORD, CFG.OTHERDATABASE, CFG.OTHERPORT)))
                {
                    connection.Open();
                    MySqlCommand sqlCommandItem = new MySqlCommand(sqlQuery, connection);
                    sqlCommandItem.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception E)
            {
                LOGYAZ("MySqlUpdate", E);
            }
        }

        public static List<T> DataTableToList<T>(DataTable dataTable) where T : new()
        {
            List<T> list = new List<T>();
            var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (DataRow row in dataTable.Rows)
            {
                T item = new T();

                for (int i = 0; i < props.Length; i++)
                {
                    if (dataTable.Columns.Contains(props[i].Name))
                    {
                        object value = row[props[i].Name];
                        try { props[i].SetValue(item, value); } catch { }
                    }
                }

                list.Add(item);
            }
            return list;
        }

        #region Logo
        public static UnityObjects.UnityApplication AppUnity;
        private static string paytransSelectFields = "CARDREF ,DATE_ ,MODULENR ,SIGN ,FICHEREF ,FICHELINEREF ,TRCODE ,TOTAL ,PAID ,EARLYINTRATE ,LATELYINTRATE ,CROSSREF ,PAIDINCASH ,CANCELLED ,PROCDATE ,TRCURR ,TRRATE ,REPORTRATE ,MODIFIED ,REMINDLEV ,REMINDSENT ,CROSSCURR ,CROSSTOTAL ,DISCFLAG ,SITEID ,ORGLOGICREF ,WFSTATUS ,CLOSINGRATE ,DISCDUEDATE ,OPSTAT ,RECSTATUS ,INFIDX ,PAYNO ,DELAYTOTAL ,LASTSENDREMLEV ,POINTTRANS ,BANKPAYDATE ,POSCOMSN ,POINTCOMSN ,BANKACCREF ,PAYMENTTYPE ,CASHACCREF ,TRNET ,REPAYPLANREF ,DUEDIFFCOMSN ,CALCTYPE ,NETTOTAL ,REPYPLNAPPLIED ,PAYTRCURR ,PAYTRRATE ,PAYTRNET ,BNTRCREATED ,BNFCHREF ,BNFLNREF ,INSTALTYPE ,INSTALREF ,MAININSTALREF ,ORGLOGOID ,STLINEREF ,SPECODE ,CREDITCARDNUM ,VALBEGDATE ,RETREFNO ,DOCODE ,BATCHNUM ,APPROVENUM ,POSTERMINALNUM ,CLDIFFINV ,LINEEXP ,DEVIRPROCDATE ,DEVIR ,DEVIRCARDREF ,GLOBALCODE ,CLBNACCOUNTNO ,MATCHDATE ,DEVIRBRANCH ,DEVIRDEPARTMENT ,DEVIRFICHEREF ,DEVIRLINEREF ,CURRDIFFRATE ,CURRDIFFCLOSED ,CURRDIFFCLSREF ,VATFLAG ";

        public static string LOGO_LOGIN(string USERNAME, string PASSWORD, int FIRMNR)
        {
            CONFIG CFG = CONFIG_HELPER.GET_CONFIG();
            String strresult = "";
            try
            {
                LOGYAZ($"LOGO_LOGIN deneniyor - User: {USERNAME}, FirmNr: {FIRMNR}", null);
                if (AppUnity != null && AppUnity.Connected)
                    LOGO_LOGOUT();
                AppUnity = new UnityObjects.UnityApplication();

                LOGYAZ($"LOGO_LOGIN - UnityApplication oluşturuldu, Connected: {AppUnity.Connected}", null);

                // Önce Logo Object Host'a bağlan
                if (!AppUnity.Connect())
                {
                    var connectError = AppUnity.GetLastErrorString();
                    LOGYAZ($"LOGO_LOGIN - Connect() başarısız: {(string.IsNullOrEmpty(connectError) ? "Boş hata" : connectError)}", null);
                    throw new Exception($"Logo Object Host'a bağlanılamadı. Servis çalışıyor mu? Hata: {(string.IsNullOrEmpty(connectError) ? "Bilinmiyor" : connectError)}");
                }

                LOGYAZ($"LOGO_LOGIN - Connect() başarılı, Connected: {AppUnity.Connected}", null);

                if (!AppUnity.Login(USERNAME, PASSWORD, FIRMNR, 2))
                {
                    var logoErrorString = AppUnity.GetLastErrorString();
                    var detailedError = $"ErrorString: {(string.IsNullOrEmpty(logoErrorString) ? "Boş" : logoErrorString)}, " +
                                       $"Connected: {AppUnity.Connected}, LoggedIn: {AppUnity.LoggedIn}";
                    LOGYAZ($"LOGO_LOGIN başarısız - {detailedError}", null);

                    string userMessage = string.IsNullOrEmpty(logoErrorString)
                        ? "Logo giriş başarısız - Logo Object Host servisi çalışıyor mu? Kullanıcı/şifre doğru mu? LO yetkisi var mı?"
                        : logoErrorString;
                    throw new Exception(userMessage);
                }

                strresult = "";
            }
            catch (Exception exp)
            {
                HELPER.LOGYAZ("LOGO_LOGIN", exp);
                strresult = exp.Message;
            }

            return strresult;
        }
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

        public static class LogoInsertHelpers
        {
            // ----------------------- SANITIZE / UTILS -----------------------

            static double ToDoubleInv(object v)
            {
                if (v == null) return 0d;
                var s = Convert.ToString(v, CultureInfo.InvariantCulture);
                if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
                return 0d;
            }

            static double Quantize(double v, int scale = 4)
                => Math.Round(v, scale, MidpointRounding.AwayFromZero);

            static bool IsBad(double v)
                => double.IsNaN(v) || double.IsInfinity(v) || Math.Abs(v) > 1e12;

            static double Sanitize(double v, int scale = 4)
            {
                if (IsBad(v)) return 0d;
                return Quantize(v, scale);
            }

            static double SafeDiv(double num, double den, out bool bad)
            {
                bad = (den == 0d || double.IsNaN(den) || double.IsInfinity(den));
                if (bad) return 0d;
                var r = num / den;
                bad = IsBad(r);
                return bad ? 0d : r;
            }

            static bool ValidateForFloatErrors(UnityObjects.Data inv, out string why)
            {
                var sb = new StringBuilder();
                var lines = inv.DataFields.FieldByName("TRANSACTIONS").Lines;

                for (int i = 0; i < lines.Count; i++)
                {
                    var l = lines[i];

                    double q = ToDoubleInv(l.FieldByName("QUANTITY").Value);
                    double pr = ToDoubleInv(l.FieldByName("PRICE").Value);
                    double vat = ToDoubleInv(l.FieldByName("VAT_RATE").Value);
                    double tot = 0;

                    var totalField = l.FieldByName("TOTAL");
                    if (totalField != null) tot = ToDoubleInv(totalField.Value);

                    if (IsBad(q)) sb.AppendLine($"Satır {i + 1}: QUANTITY geçersiz ({q}).");
                    if (IsBad(pr)) sb.AppendLine($"Satır {i + 1}: PRICE geçersiz ({pr}).");
                    if (IsBad(vat)) sb.AppendLine($"Satır {i + 1}: VAT_RATE geçersiz ({vat}).");
                    if (IsBad(tot)) sb.AppendLine($"Satır {i + 1}: TOTAL geçersiz ({tot}).");
                    if (q == 0 && pr == 0) sb.AppendLine($"Satır {i + 1}: Hem QUANTITY hem PRICE sıfır.");
                }

                why = sb.ToString();
                return sb.Length == 0;
            }

            // (İsteğe bağlı) Logo tarafında belgeyi kolay izlemek için tekil anahtar
            static string MakeIntegrationKey(dynamic _BASLIK)
            {
                // Elindeki alanlara göre özelleştirebilirsin.
                // Fiş numarası + POS + tarih gibi deterministik bir kombinasyon ideal.
                return $"G3|{_BASLIK?.POS}|{_BASLIK?.DOCUMENT_NO}|{_BASLIK?.DATE_:yyyyMMdd}";
            }

            // ----------------------- ANA INSERT -----------------------

            /// <summary>
            /// LOGO satış faturası (perakende, TYPE=7) oluşturur. 
            /// Tüm numeric alanlar sanitize edilir; sıfıra bölme ve NaN/Infinity önlenir.
            /// Başarılıysa "ok", değilse hata metni döner.
            /// </summary>
            public static string InsertInvoice2(
              
              
                    // projendeki gerçek detay sınıfı listesi
           
                string CARI_KOD,
                string BRANCH,
                 Bms_Fiche_Header _BASLIK, List<Bms_Fiche_Detail> _DETAILS,
                     bool withCustomer,
                       int FIRMNR
                // müşterisiz senaryoda kullanılacak cari
                )
            {
                try
                {
                    // --- CARİ KODU mevcut mu? yoksa sol sıfırları temizle ---
                    bool isCustomerExist = false;
                    try
                    {
                        var dt = HELPER.SqlSelectLogo(
                            $"SELECT COUNT(1) FROM LG_{FIRMNR}_CLCARD WITH (NOLOCK) WHERE CODE='{_BASLIK.CUSTOMER_CODE}'");
                        isCustomerExist = Convert.ToInt32(dt.Rows[0][0]) > 0;
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("Müşteri sorgusu sırasında hata oluştu.");
                    }
                    if (!isCustomerExist && _BASLIK.CUSTOMER_CODE != null)
                        _BASLIK.CUSTOMER_CODE = _BASLIK.CUSTOMER_CODE.ToString().TrimStart('0');

                    HELPER.LOGYAZ(Convert.ToString(_BASLIK.DOCUMENT_NO), null);

                    // --- Fatura nesnesi ---
                    UnityObjects.Data invoice = HELPER.NewObjectData(UnityObjects.DataObjectType.doSalesInvoice);
                    invoice.New();

                    // Header
                    invoice.DataFields.FieldByName("TYPE").Value = 7;                    // Perakende
                    invoice.DataFields.FieldByName("NUMBER").Value = "~";
                    invoice.DataFields.FieldByName("DATE").Value = _BASLIK.DATE_;
                    invoice.DataFields.FieldByName("AUXIL_CODE").Value = Convert.ToString(_BASLIK.POS);
                    invoice.DataFields.FieldByName("DOC_NUMBER").Value = Convert.ToString(_BASLIK.DOCUMENT_NO);
                    invoice.DataFields.FieldByName("DOC_TRACK_NR").Value = Convert.ToString(_BASLIK.POS);
                    invoice.DataFields.FieldByName("NOTES6").Value = withCustomer ? Convert.ToString(_BASLIK.FICHE_ID) : "";
                    invoice.DataFields.FieldByName("AUTH_CODE").Value = "BMS-NCR";
                    invoice.DataFields.FieldByName("POST_FLAGS").Value = 243;
                    invoice.DataFields.FieldByName("CURRSEL_TOTALS").Value = 1;
                    invoice.DataFields.FieldByName("DEDUCTIONPART1").Value = 2;
                    invoice.DataFields.FieldByName("DEDUCTIONPART2").Value = 3;
                    invoice.DataFields.FieldByName("POS_TRANSFER_INFO").Value = 1;
                    invoice.DataFields.FieldByName("DOC_DATE").Value = _BASLIK.DATE_;
                    invoice.DataFields.FieldByName("EBOOK_DOCDATE").Value = _BASLIK.DATE_;
                    invoice.DataFields.FieldByName("EBOOK_DOCTYPE").Value = 6;
                    invoice.DataFields.FieldByName("EBOOK_EXPLAIN").Value = "Z Raporu";
                    invoice.DataFields.FieldByName("EBOOK_NOPAY").Value = 1;
                    invoice.DataFields.FieldByName("DIVISION").Value = BRANCH;

                    // İzleme için SPECODE/GENEXP1 damgası (opsiyonel ama tavsiye)
                    var integrationKey = MakeIntegrationKey(_BASLIK);
                    invoice.DataFields.FieldByName("SPECODE").Value = integrationKey;
                    invoice.DataFields.FieldByName("GENEXP1").Value = integrationKey;

                    if (withCustomer)
                    {
                        invoice.DataFields.FieldByName("ARP_CODE").Value = Convert.ToString(_BASLIK.CUSTOMER_CODE);
                        invoice.DataFields.FieldByName("DOC_NUMBER").Value = Convert.ToString(_BASLIK.DOCUMENT_NO);
                    }
                    else
                    {
                        invoice.DataFields.FieldByName("ARP_CODE").Value = CARI_KOD;
                    }

                    // Satırlar
                    var lines = invoice.DataFields.FieldByName("TRANSACTIONS").Lines;

                    foreach (var line in _DETAILS)
                    {
                        if (string.IsNullOrWhiteSpace(Convert.ToString(line.ITEMCODE)))
                        {
                            LOGYAZ($"Boş ITEMCODE atlandı - Tarih: {_BASLIK.DATE_:yyyy-MM-dd}, POS: {_BASLIK.POS}, Ürün: {line.ITEMNAME}, Tutar: {line.LINETOTAL}, Miktar: {line.QUANTITY}", null);
                            continue;
                        }

                        // KDV oranı
                        double vatRate = 0;
                        try
                        {
                            DataTable dtVat = HELPER.SqlSelectLogo(
                                $"SELECT VAT FROM LG_{FIRMNR}_ITEMS WITH(NOLOCK) WHERE CODE='{line.ITEMCODE}'");
                            vatRate = ToDoubleInv(dtVat.Rows[0][0]);
                        }
                        catch
                        {
                            // yoksa 0 kalsın
                        }

                        // Sayısal alanlar (kültür bağımsız)
                        double qty = Sanitize(ToDoubleInv(line.QUANTITY));
                        double priceApi = Sanitize(ToDoubleInv(line.PRICE));
                        double lineTotal = Sanitize(ToDoubleInv(line.LINETOTAL));
                        double discTotal = Sanitize(ToDoubleInv(Math.Abs(line.DISCOUNT_TOTAL)));

                        // Satır
                        lines.AppendLine();
                        var ln = lines[lines.Count - 1];

                        ln.FieldByName("TYPE").Value = 0;                      // malzeme
                        ln.FieldByName("MASTER_CODE").Value = Convert.ToString(line.ITEMCODE);
                        ln.FieldByName("QUANTITY").Value = qty;

                        // Fiyat
                        double price;
                        if (discTotal > 0)
                        {
                            price = priceApi; // kampanyalı/indirimli fiyat mevcutsa onu kullan
                        }
                        else
                        {
                            var p = SafeDiv(lineTotal, qty, out bool bad);
                            price = bad || p == 0 ? priceApi : p;
                        }
                        ln.FieldByName("PRICE").Value = Sanitize(price);

                        ln.FieldByName("UNIT_CODE").Value = Convert.ToString(line.ITEMUNIT);
                        ln.FieldByName("UNIT_CONV1").Value = 1;
                        ln.FieldByName("UNIT_CONV2").Value = 1;
                        ln.FieldByName("VAT_INCLUDED").Value = 1;
                        ln.FieldByName("VAT_RATE").Value = Sanitize(vatRate, 2);
                        ln.FieldByName("BILLED").Value = 1;

                        // Logo yerel para birimi → 0 (160 gibi sabit değerler kullanmayın)
                        ln.FieldByName("EDT_CURR").Value = 0;

                        ln.FieldByName("SALEMANCODE").Value = Convert.ToString(line.SALESMAN);
                        ln.FieldByName("MONTH").Value = _BASLIK.DATE_.Month;
                        ln.FieldByName("YEAR").Value = _BASLIK.DATE_.Year;
                        ln.FieldByName("AFFECT_RISK").Value = 1;
                        ln.FieldByName("BARCODE").Value = Convert.ToString(line.ITEMCODE);

                        // İndirim satırı (TYPE=2), KDV hariç tutar
                        if (discTotal > 0)
                        {
                            // indirim KDV hariğe çevrilir
                            var discExcl = Sanitize(discTotal / ((100.0 + vatRate) / 100.0));

                            lines.AppendLine();
                            var dln = lines[lines.Count - 1];

                            dln.FieldByName("TYPE").Value = 2;   // indirim
                            dln.FieldByName("DISCEXP_CALC").Value = 1;
                            dln.FieldByName("TOTAL").Value = discExcl;
                            dln.FieldByName("BILLED").Value = 1;
                            dln.FieldByName("MONTH").Value = _BASLIK.DATE_.Month;
                            dln.FieldByName("YEAR").Value = _BASLIK.DATE_.Year;
                            dln.FieldByName("AFFECT_RISK").Value = 1;
                        }
                    }

                    // Hesaplat
                    invoice.ReCalculate();

                    // Post öncesi: satır doğrulama (TDS/float hatalarını yakalar)
                    if (!ValidateForFloatErrors(invoice, out var whyBad))
                        throw new Exception("Satır değerleri geçersiz:\n" + whyBad);

                    // POST
                    if (!invoice.Post())
                        throw new Exception(HELPER.GetLastError(invoice));

                    int logicalRef = Convert.ToInt32(invoice.DataFields.DBFieldByName("LOGICALREF").Value);
                    if (logicalRef > 0) return "ok";
                    return "notok";
                }
                catch (Exception ex)
                {
                    List<Bms_Errors> errorList = null;
                    HELPER.LOGYAZ("InsertInvoice_Safe", ex);
                    errorList?.Add(new Bms_Errors
                    {
                        ERRORMESSAGE = "InsertInvoice_Safe hata: " + ex.Message,
                        FICHE_ID = Convert.ToString(_BASLIK?.DOCUMENT_NO)
                    });
                    return ex.Message;
                }
            }
        }

        public static string InsertInvoice(string CARI_KOD, string BRANCH, Bms_Fiche_Header _BASLIK, List<Bms_Fiche_Detail> _DETAILS, bool withCustomer, string FIRMNR)
        {
            bool isCustomerExist = false;
            try { isCustomerExist = Convert.ToBoolean(SqlSelectLogo($"SELECT COUNT(*) FROM LG_{FIRMNR}_CLCARD WHERE CODE='{_BASLIK.CUSTOMER_CODE}'").Rows[0][0]); } catch (Exception ex)
             
            {
                LOGYAZ($"Müşteri Hatası \n Ürün: {_BASLIK.CUSTOMER_CODE} \n Ex: {ex.Message.ToString()}", null);
                isCustomerExist = false;
            }
            if (!isCustomerExist)
                _BASLIK.CUSTOMER_CODE = _BASLIK.CUSTOMER_CODE.TrimStart('0');
            HELPER.LOGYAZ(_BASLIK.DOCUMENT_NO.ToString(), null);
            //TRCODE TYPE 7 PERAKENDE SATIS FATURASI
            try
            {
                UnityObjects.Data invoice = NewObjectData(UnityObjects.DataObjectType.doSalesInvoice);
                invoice.New();
                invoice.DataFields.FieldByName("TYPE").Value = 7;
                invoice.DataFields.FieldByName("NUMBER").Value = "~";
                invoice.DataFields.FieldByName("DATE").Value = _BASLIK.DATE_;
                invoice.DataFields.FieldByName("AUXIL_CODE").Value = _BASLIK.POS.ToString();
                invoice.DataFields.FieldByName("DOC_NUMBER").Value = _BASLIK.DOCUMENT_NO.ToString();
                invoice.DataFields.FieldByName("DOC_TRACK_NR").Value = _BASLIK.POS.ToString();
                invoice.DataFields.FieldByName("NOTES6").Value = withCustomer ? _BASLIK.FICHE_ID : "";
                //invoice.DataFields.FieldByName("AUXIL_CODE").Value = "0";
                invoice.DataFields.FieldByName("AUTH_CODE").Value = "BMS";
                if (withCustomer)
                {
                    invoice.DataFields.FieldByName("ARP_CODE").Value = _BASLIK.CUSTOMER_CODE;
                    invoice.DataFields.FieldByName("DOC_NUMBER").Value = _BASLIK.DOCUMENT_NO.ToString();
                }
                else
                    invoice.DataFields.FieldByName("ARP_CODE").Value = CARI_KOD;
                invoice.DataFields.FieldByName("POST_FLAGS").Value = 243;
                //invoice.DataFields.FieldByName("RC_RATE").Value = getRateFromDB(20, FATURA_TARIHI, FIRMNR);
                //invoice.DataFields.FieldByName("PAYMENT_CODE").Value = _DETAILS.FirstOrDefault().TAKSITPLAN_KODU;
                invoice.DataFields.FieldByName("CURRSEL_TOTALS").Value = 1;
                invoice.DataFields.FieldByName("DEDUCTIONPART1").Value = 2;
                invoice.DataFields.FieldByName("DEDUCTIONPART2").Value = 3;
                invoice.DataFields.FieldByName("POS_TRANSFER_INFO").Value = 1;
                invoice.DataFields.FieldByName("DOC_DATE").Value = _BASLIK.DATE_;
                invoice.DataFields.FieldByName("EBOOK_DOCDATE").Value = _BASLIK.DATE_;
                invoice.DataFields.FieldByName("EBOOK_DOCTYPE").Value = 6;
                invoice.DataFields.FieldByName("EBOOK_EXPLAIN").Value = "Z Raporu";
                invoice.DataFields.FieldByName("EBOOK_NOPAY").Value = 1;
                invoice.DataFields.FieldByName("DIVISION").Value = BRANCH;
                UnityObjects.Lines transactions_lines = invoice.DataFields.FieldByName("TRANSACTIONS").Lines;
                foreach (var line in _DETAILS)
                {
                    if (string.IsNullOrEmpty(line.ITEMCODE))
                    {
                        LOGYAZ($"Boş ITEMCODE atlandı - Tarih: {_BASLIK.DATE_:yyyy-MM-dd}, POS: {_BASLIK.POS}, Ürün: {line.ITEMNAME}, Tutar: {line.LINETOTAL}, Miktar: {line.QUANTITY}", null);
                        continue;
                    }
                    transactions_lines.AppendLine();
                    double VatRate = 0;
                    try { VatRate = double.Parse(HELPER.SqlSelectLogo($"SELECT VAT FROM LG_{FIRMNR}_ITEMS WITH(NOLOCK) WHERE CODE='" + line.ITEMCODE + "'").Rows[0][0].ToString()); }
                    catch(Exception ex) {
                        LOGYAZ($"Vat Hatası \n Ürün: {line.ITEMCODE} \n Ex: {ex.Message.ToString()}",null);
                        VatRate = 0;
                    }


                    double priceFromDecmailToDouble = 0;
                    try { priceFromDecmailToDouble = Convert.ToDouble(line.PRICE.ToString().Replace(".", ",")); } catch { }

                    double linetotalFromDecmailToDouble = 0;
                    try { linetotalFromDecmailToDouble = Convert.ToDouble(line.LINETOTAL.ToString().Replace(".", ",")); } catch { }

                    transactions_lines[transactions_lines.Count - 1].FieldByName("TYPE").Value = 0;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("MASTER_CODE").Value = line.ITEMCODE;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("QUANTITY").Value = line.QUANTITY;

                    if (Math.Abs(line.DISCOUNT_TOTAL) > 0)
                        transactions_lines[transactions_lines.Count - 1].FieldByName("PRICE").Value = priceFromDecmailToDouble;
                    else
                        transactions_lines[transactions_lines.Count - 1].FieldByName("PRICE").Value = linetotalFromDecmailToDouble / line.QUANTITY;

                    //transactions_lines[transactions_lines.Count - 1].FieldByName("TOTAL").Value = linetotalFromDecmailToDouble;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("UNIT_CODE").Value = line.ITEMUNIT;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("UNIT_CONV1").Value = 1;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("UNIT_CONV2").Value = 1;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("VAT_INCLUDED").Value = 1;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("VAT_RATE").Value = VatRate;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("BILLED").Value = 1;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("EDT_CURR").Value = 160;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("SALEMANCODE").Value = line.SALESMAN;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("MONTH").Value = _BASLIK.DATE_.Month;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("YEAR").Value = _BASLIK.DATE_.Year;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("AFFECT_RISK").Value = 1;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("BARCODE").Value = line.ITEMCODE;
                    if (Math.Abs(line.DISCOUNT_TOTAL) > 0)
                    {
                        //double dividationOfG3Bug = 1.00;
                        //if (line.DISCOUNT_TOTAL == line.CAMPAIGN_DISCOUNT)
                        //    dividationOfG3Bug = 2.00;
                        double discountFromDecmailToDouble = 0;
                        try { discountFromDecmailToDouble = Math.Abs(Convert.ToDouble(line.DISCOUNT_TOTAL.ToString().Replace(".", ","))); } catch { }

                        //vatRateFixed 

                        //discountFromDecmailToDouble = discountFromDecmailToDouble * (VatRate / 100 + 1);
                        discountFromDecmailToDouble = discountFromDecmailToDouble / ((100 + VatRate) / 100);
                        transactions_lines.AppendLine();
                        transactions_lines[transactions_lines.Count - 1].FieldByName("TYPE").Value = 2;
                        //transactions_lines[transactions_lines.Count - 1].FieldByName("DETAIL_LEVEL").Value = 1;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("DISCEXP_CALC").Value = 1;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("TOTAL").Value = discountFromDecmailToDouble /*/ dividationOfG3Bug*/;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("BILLED").Value = 1;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("MONTH").Value = _BASLIK.DATE_.Month;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("YEAR").Value = _BASLIK.DATE_.Year;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("AFFECT_RISK").Value = 1;
                    }
                }

                //invoice.FillAccCodes();
                invoice.ReCalculate();

                if (!invoice.Post())
                    throw new Exception(GetLastError(invoice));
                int LOGOLREF = Convert.ToInt32(invoice.DataFields.DBFieldByName("LOGICALREF").Value);
                DateTime LOGOINSERTDATE = DateTime.Now;
                if (LOGOLREF > 0)
                    return "ok";
                else return "notok";
            }
            catch (Exception E)
            {
                LOGYAZ("InsertInvoice", E);
                return E.Message;
            }
        }
        public static string InsertReturnInvoice(string CARI_KOD, string BRANCH, Bms_Fiche_Header _BASLIK, List<Bms_Fiche_Detail> _DETAILS, bool withCustomer, string FIRMNR,string AUTHCODE2)
        {
            //TRCODE TYPE 7 PERAKENDE SATIS FATURASI
            try
            {
                UnityObjects.Data invoice = NewObjectData(UnityObjects.DataObjectType.doSalesInvoice);
                invoice.New();
                invoice.DataFields.FieldByName("TYPE").Value = 2;
                invoice.DataFields.FieldByName("NUMBER").Value = "~";
                invoice.DataFields.FieldByName("DATE").Value = _BASLIK.DATE_;
                invoice.DataFields.FieldByName("AUXIL_CODE").Value = _BASLIK.POS.ToString();
                invoice.DataFields.FieldByName("DOC_NUMBER").Value = _BASLIK.FICHE_ID.ToString(); //DÜZELT
                invoice.DataFields.FieldByName("DOC_TRACK_NR").Value = _BASLIK.POS.ToString();
                invoice.DataFields.FieldByName("NOTES6").Value = withCustomer ? _BASLIK.FICHE_ID : "";
                //invoice.DataFields.FieldByName("AUXIL_CODE").Value = "0";
                invoice.DataFields.FieldByName("AUTH_CODE").Value = AUTHCODE2;
                if (withCustomer)
                {
                    invoice.DataFields.FieldByName("ARP_CODE").Value = _BASLIK.CUSTOMER_CODE;
                    invoice.DataFields.FieldByName("DOC_NUMBER").Value = _BASLIK.FICHE_ID.ToString(); //DÜZELT
                }
                else
                    invoice.DataFields.FieldByName("ARP_CODE").Value = CARI_KOD;
                invoice.DataFields.FieldByName("POST_FLAGS").Value = 243;
                //invoice.DataFields.FieldByName("RC_RATE").Value = getRateFromDB(20, FATURA_TARIHI, FIRMNR);
                //invoice.DataFields.FieldByName("PAYMENT_CODE").Value = _DETAILS.FirstOrDefault().TAKSITPLAN_KODU;
                invoice.DataFields.FieldByName("CURRSEL_TOTALS").Value = 1;
                invoice.DataFields.FieldByName("DEDUCTIONPART1").Value = 2;
                invoice.DataFields.FieldByName("DEDUCTIONPART2").Value = 3;
                invoice.DataFields.FieldByName("POS_TRANSFER_INFO").Value = 1;
                invoice.DataFields.FieldByName("DOC_DATE").Value = _BASLIK.DATE_;
                invoice.DataFields.FieldByName("EBOOK_DOCDATE").Value = _BASLIK.DATE_;
                invoice.DataFields.FieldByName("EBOOK_DOCTYPE").Value = 6;
                invoice.DataFields.FieldByName("EBOOK_EXPLAIN").Value = "Z Raporu";
                invoice.DataFields.FieldByName("EBOOK_NOPAY").Value = 1;
                invoice.DataFields.FieldByName("DIVISION").Value = BRANCH;
                UnityObjects.Lines transactions_lines = invoice.DataFields.FieldByName("TRANSACTIONS").Lines;
                foreach (var line in _DETAILS)
                {
                    if (string.IsNullOrEmpty(line.ITEMCODE))
                    {
                        LOGYAZ($"Boş ITEMCODE atlandı - Tarih: {_BASLIK.DATE_:yyyy-MM-dd}, POS: {_BASLIK.POS}, Ürün: {line.ITEMNAME}, Tutar: {line.LINETOTAL}, Miktar: {line.QUANTITY}", null);
                        continue;
                    }
                    transactions_lines.AppendLine();
                    double VatRate = 0;
                    try { VatRate = double.Parse(HELPER.SqlSelectLogo($"SELECT VAT FROM LG_{FIRMNR}_ITEMS WITH(NOLOCK) WHERE CODE='" + line.ITEMCODE + "'").Rows[0][0].ToString()); } catch { }

                    double priceFromDecmailToDouble = 0;
                    try { priceFromDecmailToDouble = Convert.ToDouble(line.PRICE.ToString().Replace(".", ",")); } catch { }

                    double linetotalFromDecmailToDouble = 0;
                    try { linetotalFromDecmailToDouble = Convert.ToDouble(line.LINETOTAL.ToString().Replace(".", ",")); } catch { }

                    transactions_lines[transactions_lines.Count - 1].FieldByName("TYPE").Value = 0;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("MASTER_CODE").Value = line.ITEMCODE;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("QUANTITY").Value = line.QUANTITY;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("PRICE").Value = linetotalFromDecmailToDouble / line.QUANTITY;
                    //transactions_lines[transactions_lines.Count - 1].FieldByName("TOTAL").Value = linetotalFromDecmailToDouble;
                    //transactions_lines[transactions_lines.Count - 1].FieldByName("PRICE").Value = priceFromDecmailToDouble;
                    //transactions_lines[transactions_lines.Count - 1].FieldByName("TOTAL").Value = linetotalFromDecmailToDouble;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("UNIT_CODE").Value = line.ITEMUNIT;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("UNIT_CONV1").Value = 1;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("UNIT_CONV2").Value = 1;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("VAT_INCLUDED").Value = 1;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("VAT_RATE").Value = VatRate;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("BILLED").Value = 1;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("EDT_CURR").Value = 160;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("SALEMANCODE").Value = line.SALESMAN;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("MONTH").Value = _BASLIK.DATE_.Month;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("YEAR").Value = _BASLIK.DATE_.Year;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("AFFECT_RISK").Value = 1;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("BARCODE").Value = line.ITEMCODE;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("RET_COST_TYPE").Value = 1;
                    if (Math.Abs(line.DISCOUNT_TOTAL) > 0)
                    {
                        //double dividationOfG3Bug = 1.00;
                        //if (line.DISCOUNT_TOTAL == line.CAMPAIGN_DISCOUNT)
                        //    dividationOfG3Bug = 2.00;
                        double discountFromDecmailToDouble = 0;
                        try { discountFromDecmailToDouble = Math.Abs(Convert.ToDouble(line.DISCOUNT_TOTAL.ToString().Replace(".", ","))); } catch { }
                        discountFromDecmailToDouble = discountFromDecmailToDouble / ((100 + VatRate) / 100);
                        transactions_lines.AppendLine();
                        transactions_lines[transactions_lines.Count - 1].FieldByName("TYPE").Value = 2;
                        //transactions_lines[transactions_lines.Count - 1].FieldByName("DETAIL_LEVEL").Value = 1;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("DISCEXP_CALC").Value = 1;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("TOTAL").Value = discountFromDecmailToDouble /*/ dividationOfG3Bug*/;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("BILLED").Value = 1;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("MONTH").Value = _BASLIK.DATE_.Month;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("YEAR").Value = _BASLIK.DATE_.Year;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("AFFECT_RISK").Value = 1;
                    }
                }

                //invoice.FillAccCodes();
                invoice.ReCalculate();

                if (!invoice.Post())
                    throw new Exception(GetLastError(invoice));
                int LOGOLREF = Convert.ToInt32(invoice.DataFields.DBFieldByName("LOGICALREF").Value);
                DateTime LOGOINSERTDATE = DateTime.Now;
                if (LOGOLREF > 0)
                    return "ok";
                else return "notok";
            }
            catch (Exception E)
            {
                LOGYAZ("InsertReturnInvoice", E);
                return E.Message;
            }
        }

        public static string deleteInvoice(int REF, string FIRMNR)
        {
            //TRCODE TYPE 7 PERAKENDE SATIS FATURASI
            try
            {
                UnityObjects.Data invoice = NewObjectData(UnityObjects.DataObjectType.doSalesInvoice);

                if (!invoice.Delete(REF))
                    throw new Exception(GetLastError(invoice));
                //if (!invoice.Post())
                //    throw new Exception(GetLastError(invoice));
                else return "ok";
            }
            catch (Exception E)
            {
                LOGYAZ("deleteInvoice", E);
                return E.Message;
            }
        } //32465 Hareket Bulunamadı
        public static string rollBackDebtClose(int REF)
        {
            try
            {

                if (!AppUnity.RollBackDebtClose(REF)) {
                    
                    LOGYAZ("rollBackDebtClose "+ REF + "", null);
                    throw new Exception(AppUnity.GetLastError() + " " + AppUnity.GetLastErrorString());
                }
                   
                else return "ok";
            }
            catch (Exception E)
            {
                LOGYAZ("rollBackDebtClose", E);
                return E.Message;
            }
        }
        public static string deleteCheque(int REF, string FIRMNR)
        {
            //TRCODE TYPE 7 PERAKENDE SATIS FATURASI
            try
            {
                UnityObjects.Data cheque = NewObjectData(UnityObjects.DataObjectType.doCQPnRoll);
                if (!cheque.Delete(REF))
                    throw new Exception(GetLastError(cheque));
                //if (!invoice.Post())
                //    throw new Exception(GetLastError(invoice));
                else return "ok";
            }
            catch (Exception E)
            {
                LOGYAZ("deleteCheque", E);
                return E.Message;
            }
        }
        public static string deleteCLFiche(int REF, string FIRMNR)
        {
            //TRCODE TYPE 7 PERAKENDE SATIS FATURASI
            try
            {
                UnityObjects.Data arp = NewObjectData(UnityObjects.DataObjectType.doARAPVoucher);
                if (!arp.Delete(REF))
                    throw new Exception(GetLastError(arp));
                //if (!invoice.Post())
                //    throw new Exception(GetLastError(invoice));
                else return "ok";
            }
            catch (Exception E)
            {
                LOGYAZ("deleteCLFiche", E);
                return E.Message;
            }
        }
        public static string deleteKsLines(int REF, string FIRMNR)
        {
            //TRCODE TYPE 7 PERAKENDE SATIS FATURASI
            try
            {
                UnityObjects.Data ksline = NewObjectData(UnityObjects.DataObjectType.doSafeDepositTrans);
                if (!ksline.Delete(REF))
                    throw new Exception(GetLastError(ksline));
                //if (!invoice.Post())
                //    throw new Exception(GetLastError(invoice));
                else return "ok";
            }
            catch (Exception E)
            {
                LOGYAZ("deleteKsLines", E);
                return E.Message;
            }
        }
        public static UnityObjects.Data NewObjectData(UnityObjects.DataObjectType objecttype)
        {
            if (AppUnity == null)
            {
                LOGYAZ("NewObjectData", new Exception($"AppUnity null. Logo'ya login yapılmamış olabilir. ObjectType: {objecttype}"));
                throw new Exception($"Logo bağlantısı yok (AppUnity null). Önce Logo'ya login yapılmalı. ObjectType: {objecttype}");
            }
            if (!AppUnity.Connected)
            {
                LOGYAZ("NewObjectData", new Exception($"AppUnity Connected=false. Logo bağlantısı kopmuş. ObjectType: {objecttype}"));
                throw new Exception($"Logo bağlantısı kopmuş (Connected=false). Yeniden login yapılmalı. ObjectType: {objecttype}");
            }
            var result = AppUnity.NewDataObject(objecttype);
            if (result == null)
            {
                LOGYAZ("NewObjectData", new Exception($"NewDataObject null döndü. ObjectType: {objecttype}"));
                throw new Exception($"Logo NewDataObject null döndü. ObjectType: {objecttype}");
            }
            return result;
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

        static string setClcardSpecode(int? CURRENCY_TYPE)
        {
            string SPECODE = "";
            try
            {
                if (CURRENCY_TYPE == 0 || CURRENCY_TYPE == 160) SPECODE = "TL";
                if (CURRENCY_TYPE == 1) SPECODE = "USD";
                if (CURRENCY_TYPE == 17) SPECODE = "GBP";
                if (CURRENCY_TYPE == 20) SPECODE = "EUR";
            }
            catch { }
            return SPECODE;
        }

        //SATIN
        //RollBackDebtClose (BORÇ KAPATMA)
        //SELECT CROSSREF FROM LG_{FIRMNR}_01_PAYTRANS WHERE CARDREF={sRef} AND MODULENR=3 AND TRCODE=1 AND PAID<>0 AND FICHEREF='{ORFICHELOGICALREF}'
        private static void DoOldChangeOps(string STUDENT_NO, string FACULTY_CODE, int? DEPARTMENT_CODE, string FIRMNR)
        {
            //İLGİLİ ÖĞRENCİNİN BUGUNE AİT SİPARİŞİ VAR İSE BORÇ TAKİP GERİ AL YAPILIP MUHASEBE FİŞİ SİLİNİNİP ORDER TABLOSU GÜNCELLENİP SİPARİŞLER TEKRAR LOGOYA GÖNDERİLECEK
            int sRef = Convert.ToInt32(SqlSelectLogo($@"SELECT LOGICALREF FROM LG_{FIRMNR}_CLCARD WHERE CODE='{STUDENT_NO}'").Rows[0][0]);
            #region RollBack
            DataTable DT = SqlSelectLogo($@"SELECT * FROM LG_{FIRMNR}_01_ORFICHE WHERE CLIENTREF='{sRef}' AND DATE_>='{DateTime.Now.ToString("yyyy-MM-dd")}'");
            if (DT.Rows.Count > 0)
            {
                int ORFICHELOGICALREF = Convert.ToInt32(DT.Rows[0]["LOGICALREF"]);
                string FICHENO = DT.Rows[0]["FICHENO"].ToString();
                int PaymentRefForRollback = 0;
                try { PaymentRefForRollback = Convert.ToInt32(SqlSelectLogo($@"SELECT CROSSREF FROM LG_{FIRMNR}_01_PAYTRANS WHERE CARDREF={sRef} AND MODULENR=3 AND TRCODE=1 AND PAID<>0 AND FICHEREF='{ORFICHELOGICALREF}'").Rows[0][0]); } catch { }
       //         if (PaymentRefForRollback > 0)AppUnity.RollBackDebtClose(PaymentRefForRollback);
                #region MUHASEBE FİŞİNİ SİL

                if (!string.IsNullOrEmpty(FICHENO))
                {
                    int EmficheLogicalref = 0;
                    try { EmficheLogicalref = Convert.ToInt32(SqlSelectLogo($@"SELECT LOGICALREF FROM LG_{FIRMNR}_01_EMFICHE WHERE TRCODE=4 AND GENEXP4 LIKE 'SIPARISNO:{FICHENO}%' AND DATE_>='{DateTime.Now.ToString("yyyy-MM-dd")}'").Rows[0][0]); } catch { }
                    if (EmficheLogicalref > 0)
                    {
                        UnityObjects.Data F = NewObjectData(UnityObjects.DataObjectType.doGLVoucher);
                        F.Delete(EmficheLogicalref);
                    }
                }
                #endregion
            }
            SqlExecute($@"UPDATE BMS_{FIRMNR}_STUDENT SET FACULTY_CODE={FACULTY_CODE},DEPARTMENT_CODE={DEPARTMENT_CODE},ERPTSTATUS=0 WHERE STUDENT_NO= '" + STUDENT_NO + "'");

            #endregion

        }

        private static int getOldDepartmentCodeFromXt(string STUDENT_NO, string FIRMNR)
        {
            int result = 0;
            try
            {
                DataTable dt = SqlSelectLogo($@"SELECT DEPARTMENT_CODE FROM LG_XT051_{FIRMNR} WHERE STUDENT_NO='{STUDENT_NO}'");
                if (dt.Rows.Count > 0)
                    result = Convert.ToInt32(dt.Rows[0][0]);
            }
            catch (Exception E)
            {
                LOGYAZ("getOldDepartmentCodeFromXt", E);
            }
            return result;
        }

        private static string getOldFacultyCodeFromXt(string STUDENT_NO, string FIRMNR)
        {
            string result = "";
            try
            {
                DataTable dt = SqlSelectLogo($@"SELECT FACULTY_CODE FROM LG_XT051_{FIRMNR} WHERE STUDENT_NO='{STUDENT_NO}'");
                if (dt.Rows.Count > 0)
                    result = dt.Rows[0][0].ToString();
            }
            catch (Exception E)
            {
                LOGYAZ("getOldFacultyCodeFromXt", E);
            }
            return result;
        }




        private static dynamic getVatRateFromSrvcard(string CODE, string FIRMNR)
        {
            double vatRate = 0;
            try { vatRate = double.Parse(HELPER.SqlSelectLogo($"SELECT VAT FROM LG_{FIRMNR}_ITEMS WHERE CODE='" + CODE + "'").Rows[0][0].ToString()); } catch { }
            return vatRate;
        }

        private static dynamic get4LogoMasrafMerkezi(string STUDENT_NO, string FIRMNR)
        {
            string MapType = "Masraf Merkezi - Bölüm Kodu"; /*Bölüm - Egitim Seviyesi*/ /*Masraf Merkezi - Bölüm Kodu*/
            string logoMasrafMerkezi = "";

            string XT_DEPARTMENT_CODE = "";
            try { XT_DEPARTMENT_CODE = SqlSelectLogo($"SELECT TOP 1 DEPARTMENT_CODE FROM LG_XT051_{FIRMNR} WHERE STUDENT_NO='{STUDENT_NO}'").Rows[0][0].ToString(); } catch { }

            DataTable dt = SqlSelectLogo($@"SELECT TOP 1 LOGOFIELD FROM BMS_{FIRMNR}_WPU_MAP WHERE TYPE='{MapType}' and MAPFIELD='{XT_DEPARTMENT_CODE}'");
            if (dt.Rows.Count > 0)
                logoMasrafMerkezi = dt.Rows[0][0].ToString();
            return logoMasrafMerkezi;
        }

        private static dynamic get4LogoDepartment(string STUDENT_NO, string FIRMNR)
        {
            string MapType = "Bölüm - Egitim Seviyesi"; /*Bölüm - Egitim Seviyesi*/ /*Masraf Merkezi - Bölüm Kodu*/
            int logoDepartment = 0;

            string XT_PROGRAM_LEVEL = "";
            try { XT_PROGRAM_LEVEL = SqlSelectLogo($"SELECT TOP 1 PROGRAM_LEVEL FROM LG_XT051_{FIRMNR} WHERE STUDENT_NO='{STUDENT_NO}'").Rows[0][0].ToString(); } catch { }

            DataTable dt = SqlSelectLogo($@"SELECT TOP 1 LOGOFIELD FROM BMS_{FIRMNR}_WPU_MAP WHERE TYPE='{MapType}' and MAPFIELD='{XT_PROGRAM_LEVEL}'");
            if (dt.Rows.Count > 0)
                logoDepartment = Convert.ToInt32(dt.Rows[0][0]);
            return logoDepartment;
        }

        private static dynamic get4LogoDivision(string STUDENT_NO, string FIRMNR)
        {
            string MapType = "Isyeri - Fakülte Kodu"; /*Bölüm - Egitim Seviyesi*/ /*Masraf Merkezi - Bölüm Kodu*/
            int logoDivision = 0;

            string XT_FACULTY_CODE = "";
            try { XT_FACULTY_CODE = SqlSelectLogo($"SELECT TOP 1 FACULTY_CODE FROM LG_XT051_{FIRMNR} WHERE STUDENT_NO='{STUDENT_NO}'").Rows[0][0].ToString(); } catch { }

            DataTable dt = SqlSelectLogo($@"SELECT TOP 1 LOGOFIELD FROM BMS_{FIRMNR}_WPU_MAP WHERE TYPE='{MapType}' and MAPFIELD='{XT_FACULTY_CODE}'");
            if (dt.Rows.Count > 0)
                logoDivision = Convert.ToInt32(dt.Rows[0][0]);
            return logoDivision;
        }

        private static dynamic getSpecode(DateTime? SIPARIS_TARIHI, string FIRMNR)
        {
            string specode = "";
            DataTable dt = SqlSelectLogo($@"SELECT TOP 1 SPECODE FROM BMS_{FIRMNR}_WPU_FISTARIHARALIKOZELKOD WHERE DATE1<='{SIPARIS_TARIHI.Value.Date.ToString("yyyyMMdd")}' AND DATE2>='{SIPARIS_TARIHI.Value.Date.ToString("yyyyMMdd")}'");
            if (dt.Rows.Count > 0)
                specode = dt.Rows[0][0].ToString();
            return specode;
        }

        private static void createAccountingFiche(/*BMS_XXX_ORDER ORDER,*/ int lOGOLREF, string FIRMNR)
        {
            DateTime ORDER_SIPARIS_TARIHI = new DateTime();
            try { ORDER_SIPARIS_TARIHI = Convert.ToDateTime(SqlSelectLogo($"SELECT DATE_ FROM LG_{FIRMNR}_01_ORFICHE WHERE LOGICALREF={lOGOLREF}").Rows[0][0]); } catch { }
            string ORDER_SIPARIS_NO = "";
            try { ORDER_SIPARIS_NO = SqlSelectLogo($"SELECT FICHENO FROM LG_{FIRMNR}_01_ORFICHE WHERE LOGICALREF={lOGOLREF}").Rows[0][0].ToString(); } catch { }

            string ORDER_CARI_KOD = "";
            try { ORDER_CARI_KOD = SqlSelectLogo($"SELECT (SELECT TOP 1 CODE FROM LG_{FIRMNR}_CLCARD C WHERE C.LOGICALREF=CLIENTREF) FROM LG_{FIRMNR}_01_ORFICHE WHERE LOGICALREF={lOGOLREF}").Rows[0][0].ToString(); } catch { }

            int ORDER_DOVIZ_TIPI = 0;
            try { ORDER_DOVIZ_TIPI = Convert.ToInt32(SqlSelectLogo($"SELECT TRCURR FROM LG_{FIRMNR}_01_ORFICHE WHERE LOGICALREF={lOGOLREF}").Rows[0][0]); } catch { }
            try
            {
                double raporlamaDoviziRate = getRateFromDB(20, ORDER_SIPARIS_TARIHI.Date, FIRMNR);
                double? islemDoviziRate = getRateFromDB(ORDER_DOVIZ_TIPI, ORDER_SIPARIS_TARIHI.Date, FIRMNR);
                int OrficheAccountref = 0;      //120.01.001.03.01.03
                string OrficheAccountCode = ""; //120.01.001.03.01.03 
                double OrficheNettotal = 0.00;
                try { OrficheAccountref = Convert.ToInt32(SqlSelectLogo($@"SELECT TOP 1 ACCOUNTREF FROM LG_{FIRMNR}_01_ORFICHE WHERE LOGICALREF={lOGOLREF}").Rows[0][0]); } catch { }

                try { OrficheNettotal = Convert.ToDouble(SqlSelectLogo($@"SELECT TOP 1 NETTOTAL FROM LG_{FIRMNR}_01_ORFICHE WHERE LOGICALREF={lOGOLREF}").Rows[0][0]); } catch { }
                if (OrficheAccountref > 0)
                    OrficheAccountCode = SqlSelectLogo($@"SELECT TOP 1 CODE FROM LG_{FIRMNR}_EMUHACC WHERE LOGICALREF={OrficheAccountref}").Rows[0][0].ToString();


                UnityObjects.Data glvoucher = NewObjectData(UnityObjects.DataObjectType.doGLVoucher);
                glvoucher.New();
                glvoucher.DataFields.FieldByName("TYPE").Value = 4;
                glvoucher.DataFields.FieldByName("NUMBER").Value = "~";
                glvoucher.DataFields.FieldByName("AUTH_CODE").Value = "BMS";
                glvoucher.DataFields.FieldByName("DATE").Value = ORDER_SIPARIS_TARIHI.Date;
                glvoucher.DataFields.FieldByName("NOTES4").Value = "SIPARISNO:" + ORDER_SIPARIS_NO;
                glvoucher.DataFields.FieldByName("CURRSEL_TOTALS").Value = 1;
                glvoucher.DataFields.FieldByName("CURRSEL_DETAILS").Value = 2;
                glvoucher.DataFields.FieldByName("DOC_DATE").Value = ORDER_SIPARIS_TARIHI.Date;
                glvoucher.DataFields.FieldByName("DIVISION").Value = get4LogoDivision(ORDER_CARI_KOD, FIRMNR); /*ISYERI*/
                glvoucher.DataFields.FieldByName("DEPARTMENT").Value = get4LogoDepartment(ORDER_CARI_KOD, FIRMNR); /*BOLUM*/
                UnityObjects.Lines transactions_lines = glvoucher.DataFields.FieldByName("TRANSACTIONS").Lines;
                transactions_lines.AppendLine();
                transactions_lines[transactions_lines.Count - 1].FieldByName("GL_CODE").Value = OrficheAccountCode;
                transactions_lines[transactions_lines.Count - 1].FieldByName("PARENT_GLCODE").Value = OrficheAccountCode.Substring(0, 3);
                //transactions_lines[transactions_lines.Count - 1].FieldByName("DEBIT").Value = OrficheNettotal;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DEBIT").Value = Math.Round(OrficheNettotal, 2);
                transactions_lines[transactions_lines.Count - 1].FieldByName("LINENO").Value = 1;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DESCRIPTION").Value = ORDER_CARI_KOD;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURR_TRANS").Value = ORDER_DOVIZ_TIPI;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_XRATE").Value = raporlamaDoviziRate;
                //transactions_lines[transactions_lines.Count - 1].FieldByName("RC_AMOUNT").Value = OrficheNettotal / raporlamaDoviziRate;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_AMOUNT").Value = Math.Round(OrficheNettotal / raporlamaDoviziRate, 2);
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_XRATE").Value = islemDoviziRate;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_AMOUNT").Value = Math.Round((double)(OrficheNettotal / islemDoviziRate), 2);
                transactions_lines[transactions_lines.Count - 1].FieldByName("QUANTITY").Value = 0;
                //transactions_lines[transactions_lines.Count - 1].FieldByName("EURO_DEBIT").Value = OrficheNettotal / raporlamaDoviziRate;
                transactions_lines[transactions_lines.Count - 1].FieldByName("EURO_DEBIT").Value = Math.Round(OrficheNettotal / raporlamaDoviziRate, 2);
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURRSEL_TRANS").Value = 2;
                transactions_lines[transactions_lines.Count - 1].FieldByName("MONTH").Value = ORDER_SIPARIS_TARIHI.Date.Month;
                transactions_lines[transactions_lines.Count - 1].FieldByName("YEAR").Value = ORDER_SIPARIS_TARIHI.Date.Year;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DOC_DATE").Value = ORDER_SIPARIS_TARIHI.Date;
                transactions_lines[transactions_lines.Count - 1].FieldByName("OHP_CODE").Value = get4LogoMasrafMerkezi(ORDER_CARI_KOD, FIRMNR); /*MM*/
                transactions_lines[transactions_lines.Count - 1].FieldByName("DEPARTMENT").Value = get4LogoDepartment(ORDER_CARI_KOD, FIRMNR); /*BOLUM*/

                DataTable dataTable = SqlSelectLogo($@"SELECT * FROM LG_{FIRMNR}_01_ORFLINE WHERE ORDFICHEREF={lOGOLREF}");
                int lineNo = 2;
                foreach (DataRow dr in dataTable.Rows)
                {
                    int OrflineAccountref = 0;      //380.01.001.03.01.03
                    string OrflineAccountCode = ""; //380.01.001.03.01.03
                    int OrflineVatAccref = 0;       //391.01.001.04.01.03
                    string OrflineVatAccCode = "";  //391.01.001.04.01.03
                    int OrflineVat = 0;
                    double OflineLineNet = 0.00;
                    double OrflineVatamnt = 0.00;
                    try { OrflineAccountref = Convert.ToInt32(dr["ACCOUNTREF"]); } catch { }
                    try { OrflineVatAccref = Convert.ToInt32(dr["VATACCREF"]); } catch { }
                    try { OrflineVat = Convert.ToInt32(dr["VAT"]); } catch { }
                    try { OflineLineNet = Math.Round(Convert.ToDouble(dr["LINENET"]) /*/ Convert.ToDouble(islemDoviziRate)*/, 2); } catch { }
                    try { OrflineVatamnt = Math.Round(Convert.ToDouble(dr["VATAMNT"]) /*/ Convert.ToDouble(islemDoviziRate)*/, 2); } catch { }
                    if (OrflineAccountref > 0)
                        OrflineAccountCode = SqlSelectLogo($@"SELECT TOP 1 CODE FROM LG_{FIRMNR}_EMUHACC WHERE LOGICALREF={OrflineAccountref}").Rows[0][0].ToString();
                    if (OrflineVatAccref > 0)
                        OrflineVatAccCode = SqlSelectLogo($@"SELECT TOP 1 CODE FROM LG_{FIRMNR}_EMUHACC WHERE LOGICALREF={OrflineVatAccref}").Rows[0][0].ToString();

                    string ORDERLINE_STOK_KODU = "";
                    string ORDER_STOK_REF = dr["STOCKREF"].ToString();
                    try { ORDERLINE_STOK_KODU = SqlSelectLogo($@"SELECT TOP 1 CODE FROM LG_{FIRMNR}_SRVCARD WHERE LOGICALREF={ORDER_STOK_REF}").Rows[0][0].ToString(); } catch { }

                    int ORDERLINE_DOVIZ_TIPI = 0;
                    try { ORDERLINE_DOVIZ_TIPI = Convert.ToInt32(dr["TRCURR"]); } catch { }

                    transactions_lines.AppendLine();
                    transactions_lines[transactions_lines.Count - 1].FieldByName("SIGN").Value = 1;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("GL_CODE").Value = OrflineAccountCode;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("PARENT_GLCODE").Value = OrflineAccountCode.Substring(0, 3);
                    transactions_lines[transactions_lines.Count - 1].FieldByName("CREDIT").Value = Math.Round(OflineLineNet, 2);
                    transactions_lines[transactions_lines.Count - 1].FieldByName("LINENO").Value = lineNo++;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("DESCRIPTION").Value = ORDERLINE_STOK_KODU;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("CURR_TRANS").Value = ORDERLINE_DOVIZ_TIPI;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("RC_XRATE").Value = raporlamaDoviziRate;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("RC_AMOUNT").Value = Math.Round(OflineLineNet / raporlamaDoviziRate, 2);
                    transactions_lines[transactions_lines.Count - 1].FieldByName("TC_XRATE").Value = islemDoviziRate;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("TC_AMOUNT").Value = Math.Round((double)(OflineLineNet / islemDoviziRate), 2);
                    transactions_lines[transactions_lines.Count - 1].FieldByName("QUANTITY").Value = 0;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("EURO_CREDIT").Value = Math.Round(OflineLineNet / raporlamaDoviziRate, 2);
                    transactions_lines[transactions_lines.Count - 1].FieldByName("CURRSEL_TRANS").Value = 2;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("DATA_REFERENCE").Value = 5;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("MONTH").Value = ORDER_SIPARIS_TARIHI.Date.Month;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("YEAR").Value = ORDER_SIPARIS_TARIHI.Date.Year;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("DOC_DATE").Value = ORDER_SIPARIS_TARIHI.Date;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("OHP_CODE").Value = get4LogoMasrafMerkezi(ORDER_CARI_KOD, FIRMNR); /*MM*/
                    transactions_lines[transactions_lines.Count - 1].FieldByName("DEPARTMENT").Value = get4LogoDepartment(ORDER_CARI_KOD, FIRMNR); /*BOLUM*/
                    if (OrflineVat > 0)
                    {
                        transactions_lines.AppendLine();
                        transactions_lines[transactions_lines.Count - 1].FieldByName("SIGN").Value = 1;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("GL_CODE").Value = OrflineVatAccCode;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("PARENT_GLCODE").Value = OrflineVatAccCode.Substring(0, 3);
                        transactions_lines[transactions_lines.Count - 1].FieldByName("CREDIT").Value = Math.Round(OrflineVatamnt, 2);
                        transactions_lines[transactions_lines.Count - 1].FieldByName("LINENO").Value = lineNo++;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("DESCRIPTION").Value = "KDV % " + OrflineVat.ToString();
                        transactions_lines[transactions_lines.Count - 1].FieldByName("CURR_TRANS").Value = ORDERLINE_DOVIZ_TIPI;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("RC_XRATE").Value = raporlamaDoviziRate;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("RC_AMOUNT").Value = Math.Round(OrflineVatamnt / raporlamaDoviziRate, 2);
                        transactions_lines[transactions_lines.Count - 1].FieldByName("TC_XRATE").Value = islemDoviziRate;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("TC_AMOUNT").Value = Math.Round((double)(OrflineVatamnt / islemDoviziRate), 2);
                        transactions_lines[transactions_lines.Count - 1].FieldByName("QUANTITY").Value = 0;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("EURO_CREDIT").Value = Math.Round(OrflineVatamnt / raporlamaDoviziRate, 2);
                        transactions_lines[transactions_lines.Count - 1].FieldByName("CURRSEL_TRANS").Value = 2;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("MONTH").Value = ORDER_SIPARIS_TARIHI.Date.Month;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("YEAR").Value = ORDER_SIPARIS_TARIHI.Date.Year;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("DOC_DATE").Value = ORDER_SIPARIS_TARIHI.Date;
                        transactions_lines[transactions_lines.Count - 1].FieldByName("OHP_CODE").Value = get4LogoMasrafMerkezi(ORDER_CARI_KOD, FIRMNR); /*MM*/
                        transactions_lines[transactions_lines.Count - 1].FieldByName("DEPARTMENT").Value = get4LogoDepartment(ORDER_CARI_KOD, FIRMNR); /*BOLUM*/
                    }
                }


                string FISNO = DateTime.Now.ToString("yyyyMMddHHmmss").ToString();
                glvoucher.ExportToXML("GL_VOUCHERS", FISNO + ".xml");
                if (!glvoucher.Post())
                    throw new Exception(GetLastError(glvoucher));
            }
            catch (Exception E)
            {
                LOGYAZ("createAccountingFiche " + ORDER_SIPARIS_NO, E);
            }
        }

        private static void RevisetInvoiceForOrderToInvoiceFromPaymentIntegration(int LOGOLREF, string FIRMNR)
        {
            bool hasKayitUcretGeliri = false;
            bool hasEgitimOgretimGeliri = false;

            int AccFicheref = 0;
            try { AccFicheref = Convert.ToInt32(SqlSelectLogo($"SELECT TOP 1 ACCFICHEREF FROM LG_{FIRMNR}_01_INVOICE WHERE LOGICALREF={LOGOLREF}").Rows[0][0]); } catch { }
            if (AccFicheref > 0)
            {
                DataRow Dr120AkaOldLine = null;
                try { Dr120AkaOldLine = SqlSelectLogo($"SELECT TOP 1 * FROM LG_{FIRMNR}_01_EMFLINE WHERE ACCFICHEREF={AccFicheref} AND ACCOUNTCODE LIKE '120.01.%' ORDER BY LOGICALREF ASC").Rows[0]; } catch { }

                DataRow Dr600AkaCopyFrom = null;
                try { Dr600AkaCopyFrom = SqlSelectLogo($"SELECT TOP 1 * FROM LG_{FIRMNR}_01_EMFLINE WHERE ACCFICHEREF={AccFicheref} AND ACCOUNTCODE LIKE '600.01.%' ORDER BY LOGICALREF ASC").Rows[0]; } catch { }
                DataRow Dr600Sums = null;
                try { Dr600Sums = SqlSelectLogo($"SELECT SUM(CREDIT) CREDIT, SUM(REPORTNET) REPORTNET, SUM(TRNET) TRNET, SUM(EMUDEBIT) EMUDEBIT FROM LG_{FIRMNR}_01_EMFLINE WHERE ACCFICHEREF={AccFicheref} AND ACCOUNTCODE LIKE '600.01.%'").Rows[0]; } catch { }

                DataRow Dr391AkaCopyFrom = null;
                try { Dr391AkaCopyFrom = SqlSelectLogo($"SELECT TOP 1 * FROM LG_{FIRMNR}_01_EMFLINE WHERE ACCFICHEREF={AccFicheref} AND ACCOUNTCODE LIKE '391.01.%' ORDER BY LOGICALREF ASC").Rows[0]; } catch { }
                DataRow Dr391Sums = null;
                try { Dr391Sums = SqlSelectLogo($"SELECT SUM(CREDIT) CREDIT, SUM(REPORTNET) REPORTNET, SUM(TRNET) TRNET, SUM(EMUDEBIT) EMUDEBIT FROM LG_{FIRMNR}_01_EMFLINE WHERE ACCFICHEREF={AccFicheref} AND ACCOUNTCODE LIKE '391.01.%'").Rows[0]; } catch { }

                #region Update380 
                string newAccountCode = Dr120AkaOldLine["ACCOUNTCODE"].ToString().Replace("120.01.", "380.01.");
                int newAccountref = Convert.ToInt32(SqlSelectLogo($"SELECT TOP 1 LOGICALREF FROM LG_{FIRMNR}_EMUHACC WHERE CODE='{newAccountCode}'").Rows[0][0]);
                string newKebirCode = newAccountCode.Substring(0, 3);
                double newDebit = Convert.ToDouble(Dr600Sums["CREDIT"]);
                double newReportnet = Convert.ToDouble(Dr600Sums["REPORTNET"]);
                double newTrnet = Convert.ToDouble(Dr600Sums["TRNET"]);
                double newEmuDebit = Convert.ToDouble(Dr600Sums["EMUDEBIT"]);
                //update sql 
                SqlExecute($"UPDATE LG_{FIRMNR}_01_EMFLINE SET ACCOUNTREF={newAccountref}, ACCOUNTCODE='{newAccountCode}', KEBIRCODE='{newKebirCode}', DEBIT={newDebit.ToString().Replace(",", ".")}, REPORTNET={newReportnet.ToString().Replace(",", ".")}, TRNET={newTrnet.ToString().Replace(",", ".")}, EMUDEBIT={newEmuDebit.ToString().Replace(",", ".")} WHERE LOGICALREF={Dr120AkaOldLine["LOGICALREF"]}");
                #endregion
                #region AppendNewLine390
                UnityObjects.Data F = NewObjectData(UnityObjects.DataObjectType.doGLVoucher);
                F.Read(AccFicheref);
                UnityObjects.Lines transactions_lines = F.DataFields.FieldByName("TRANSACTIONS").Lines;
                transactions_lines.AppendLine();
                //GL_CODE
                transactions_lines[transactions_lines.Count - 1].FieldByName("GL_CODE").Value = "390.01.001"; //Satis Siparisi KDV
                transactions_lines[transactions_lines.Count - 1].FieldByName("OHP_CODE").Value = getEmcodeFromLogicalref(Dr391AkaCopyFrom["CENTERREF"], FIRMNR);
                transactions_lines[transactions_lines.Count - 1].FieldByName("PARENT_GLCODE").Value = "390.01";
                transactions_lines[transactions_lines.Count - 1].FieldByName("LINENO").Value = transactions_lines.Count + 1;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURR_TRANS").Value = Convert.ToInt32(Dr391AkaCopyFrom["TRCURR"]);
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_XRATE").Value = Convert.ToDouble(Dr391AkaCopyFrom["REPORTRATE"]);
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_AMOUNT").Value = Convert.ToDouble(Dr391Sums["REPORTNET"]);
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_XRATE").Value = Convert.ToDouble(Dr391AkaCopyFrom["TRRATE"]);
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_AMOUNT").Value = Convert.ToDouble(Dr391Sums["TRNET"]);
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURRSEL_TRANS").Value = 2;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DEPARTMENT").Value = Convert.ToInt32(Dr391AkaCopyFrom["DEPARTMENT"]);
                transactions_lines[transactions_lines.Count - 1].FieldByName("DOC_DATE").Value = Convert.ToDateTime(Dr391AkaCopyFrom["DOCDATE"]);
                transactions_lines[transactions_lines.Count - 1].FieldByName("MONTH").Value = Convert.ToInt32(Dr391AkaCopyFrom["MONTH_"]);
                transactions_lines[transactions_lines.Count - 1].FieldByName("DESCRIPTION").Value = Dr391AkaCopyFrom["LINEEXP"].ToString();
                transactions_lines[transactions_lines.Count - 1].FieldByName("DEBIT").Value = Convert.ToDouble(Dr391Sums["CREDIT"]);
                transactions_lines[transactions_lines.Count - 1].FieldByName("EURO_DEBIT").Value = Convert.ToDouble(Dr391Sums["EMUDEBIT"]);
                if (!F.Post())
                    throw new Exception(GetLastError(F));
                int LOGOLEFREF = Convert.ToInt32(F.DataFields.DBFieldByName("LOGICALREF").Value);
                #endregion
            }
        }

        private static dynamic getEmcodeFromLogicalref(object LOGICALREF, string FIRMNR)
        {
            string emcode = "";
            try { emcode = SqlSelectLogo($"SELECT TOP 1 CODE FROM LG_{FIRMNR}_EMCENTER WHERE LOGICALREF={LOGICALREF}").Rows[0][0].ToString(); } catch { }
            return emcode;
        }

        public static void UPDATE_ORFICHE_SHIPPED_AMOUNT(int ORFICHEREF, double AMOUNT, string FIRMNR)
        {
            try
            {
                string query = string.Format("UPDATE LG_{0}_{1}_ORFLINE SET SHIPPEDAMOUNT = {2} WHERE ORDFICHEREF = {3}  AND LINETYPE = 4", FIRMNR, "01", AMOUNT.ToString().Replace(",", "."), ORFICHEREF);
                SqlExecute(query);
            }
            catch (Exception EX)
            {
                LOGYAZ("InsertInvoice-UPDATE_ORFICHE_SHIPPED_AMOUNT", EX);
            }
        }

        private static string getOrflineItemName(int oRDER_REF, string fIRMNR)
        {
            string itemName = "";
            DataTable dt = SqlSelectLogo($@"SELECT TOP 1 DEFINITION_ FROM LG_{fIRMNR}_SRVCARD WHERE LOGICALREF=(SELECT TOP 1 STOCKREF FROM LG_{fIRMNR}_01_ORFLINE ORF WHERE ORF.LINETYPE=4 AND ORF.ORDFICHEREF={oRDER_REF})");
            if (dt.Rows.Count > 0)
                itemName = dt.Rows[0][0].ToString();
            return itemName;
        }

        private static string getOrflineItemCode(int oRDER_REF, string fIRMNR)
        {
            string itemCode = "";
            DataTable dt = SqlSelectLogo($@"SELECT TOP 1 CODE FROM LG_{fIRMNR}_SRVCARD WHERE LOGICALREF=(SELECT TOP 1 STOCKREF FROM LG_{fIRMNR}_01_ORFLINE ORF WHERE ORF.LINETYPE=4 AND ORF.ORDFICHEREF={oRDER_REF})");
            if (dt.Rows.Count > 0)
                itemCode = dt.Rows[0][0].ToString();
            return itemCode;
        }

        private static double getRateFromDB(int? dOVIZ_TIPI, DateTime? sIPARIS_TARIHI, string fIRMNR)
        {
            double rate = 1;
            string dateSqlFormat = DateTime.Now.ToString("yyyyMMdd");
            try { dateSqlFormat = sIPARIS_TARIHI.Value.ToString("yyyyMMdd"); } catch { }
            if (dOVIZ_TIPI == 0 || dOVIZ_TIPI == 160)
                return rate = 1;
            else
                return rate = double.Parse(HELPER.SqlSelectLogo($"SELECT TOP 1 RATES1 FROM BMS_{fIRMNR}_EXCHANGE  WHERE CRTYPE='" + dOVIZ_TIPI + "' AND EDATE<='" + dateSqlFormat + "' ORDER BY EDATE DESC").Rows[0][0].ToString());
        }

        private static dynamic setAgentCode(string _AGENTCODE, string FIRMNR)
        {
            //IF AGENT CODE IS NOT IN CLCARD THEN SET IT A
            string AGENTCODE = "A";
            try
            {
                AGENTCODE = SqlSelectLogo($"SELECT TOP 1 CODE FROM LG_{FIRMNR}_CLCARD WHERE CODE='{_AGENTCODE}'").Rows[0][0].ToString();
            }
            catch { }
            return AGENTCODE;
        }

        public static bool controlClcardIfUpdate(int? studentid, DateTime? UPDATE_DATE, string FIRMNR)
        {
            bool result = false;
            DateTime? RECEIVEDDATE = null;
            try { RECEIVEDDATE = Convert.ToDateTime(HELPER.SqlSelectLogo("SELECT RECEIVEDDATE FROM BMS_" + FIRMNR.ToString() + "_STUDENT WITH(NOLOCK) WHERE studentid = " + studentid).Rows[0][0]); } catch { RECEIVEDDATE = null; }
            if ((UPDATE_DATE != null || RECEIVEDDATE != null))
                if (UPDATE_DATE > RECEIVEDDATE)
                    result = true;
            return result;
        }

        private static int getDovizTipiOfStudent(string STUDENT_NO, string FIRMNR)
        {
            int dovizTipi = 0;
            DataTable dt = SqlSelectLogo($@"SELECT TOP 1 CCURRENCY FROM BMS_{FIRMNR}_CLCARD WHERE CODE='" + STUDENT_NO + "'");
            if (dt.Rows.Count > 0)
                dovizTipi = int.Parse(dt.Rows[0][0].ToString());
            return dovizTipi;
        }

        public static double getBakiyeOfStudent(string STUDENT_NO, string FIRMNR)
        {
            double bakiye = 0;
            DataTable dt = SqlSelectLogo($@"SELECT TOP 1 BAKIYE FROM BMS_{FIRMNR}_CARIBAKIYE_ID WHERE OGRENCI_NO='" + STUDENT_NO + "'");
            if (dt.Rows.Count > 0)
                bakiye = double.Parse(dt.Rows[0][0].ToString());
            return bakiye;
        }

        private static string orderFicheNo(string FICHENO)
        {
            //from orders table get last FICHENO where starts with GF- AND ADD 1 (FORMAT IS  GF-000001)
            string ficheNo = "";
            DataTable dt = SqlSelectLogo($@"SELECT TOP 1 FICHENO FROM LG_{FICHENO}_01_ORFICHE WHERE TRCODE=1 AND FICHENO LIKE 'GF%' ORDER BY FICHENO DESC");
            if (dt.Rows.Count > 0)
            {
                ficheNo = dt.Rows[0][0].ToString();
                ficheNo = ficheNo.Substring(3, ficheNo.Length - 3);
                ficheNo = (int.Parse(ficheNo) + 1).ToString();
                ficheNo = "GF-" + ficheNo.PadLeft(6, '0');
            }
            else
                ficheNo = "GF-000001";
            return ficheNo;
        }

        private static double getRateFromDB(int? dOVIZ_TIPI, DateTime? sIPARIS_TARIHI, string ISFIRMBASEDCURR, string fIRMNR)
        {
            string tableName = "L_DAILYEXCHANGES";
            if (ISFIRMBASEDCURR == "1")
                tableName = "LG_EXCHANGE_" + fIRMNR;
            double rate = 1;
            string dateSqlFormat = sIPARIS_TARIHI.Value.ToString("yyyyMMdd");
            if (dOVIZ_TIPI == 0 || dOVIZ_TIPI == 160)
                return rate = 1;
            else
                return rate = double.Parse(HELPER.SqlSelectLogo($"SELECT TOP 1 RATES1 FROM {tableName} WHERE CRTYPE='" + dOVIZ_TIPI + "' AND EDATE<='" + dateSqlFormat + "' ORDER BY EDATE DESC").Rows[0][0].ToString());
        }

        private static readonly string key = "ThisIsASecretKey"; // Replace with your key

        public static string encodeStrx(string text)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = new byte[aes.BlockSize / 8];

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(text);
                        }
                    }

                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        public static string decodeStrx(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = new byte[aes.BlockSize / 8];

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static string InsertCheque(string BRANCH, Bms_Fiche_Payment _PAYMENT, string FIRMNR)
        {
            bool isCustomerExist = false;
            try { isCustomerExist = Convert.ToBoolean(SqlSelectLogo($"SELECT COUNT(*) FROM LG_{FIRMNR}_CLCARD WHERE CODE='{_PAYMENT.CUSTOMER_CODE}'").Rows[0][0]); } catch { }
            if (!isCustomerExist)
                _PAYMENT.CUSTOMER_CODE = _PAYMENT.CUSTOMER_CODE.TrimStart('0');
            try
            {
                UnityObjects.Data rolls = NewObjectData(UnityObjects.DataObjectType.doCQPnRoll);
                rolls.New();
                rolls.DataFields.FieldByName("TYPE").Value = 1;
                rolls.DataFields.FieldByName("NUMBER").Value = "~";
                rolls.DataFields.FieldByName("DOC_NUMBER").Value = _PAYMENT.DOCUMENT_NO.ToString();
                rolls.DataFields.FieldByName("MASTER_MODULE").Value = 5;
                rolls.DataFields.FieldByName("MASTER_CODE").Value = _PAYMENT.CUSTOMER_CODE;
                rolls.DataFields.FieldByName("AUXIL_CODE").Value = _PAYMENT.POS.ToString();
                rolls.DataFields.FieldByName("AUTH_CODE").Value = "BMS";
                rolls.DataFields.FieldByName("DATE").Value = _PAYMENT.DATE_.Date;
                rolls.DataFields.FieldByName("DIVISION").Value = BRANCH;
                //rolls.DataFields.FieldByName("AVERAGE_AGE").Value = 234;
                rolls.DataFields.FieldByName("DOCUMENT_COUNT").Value = 1;
                rolls.DataFields.FieldByName("TOTAL").Value = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString().Replace(".", ","));
                //rolls.DataFields.FieldByName("TC_XRATE").Value = 1;
                //rolls.DataFields.FieldByName("TC_TOTAL").Value = 1234;
                //rolls.DataFields.FieldByName("RC_XRATE").Value = 21.45340751;
                //rolls.DataFields.FieldByName("RC_TOTAL").Value = 57.52;
                //rolls.DataFields.FieldByName("NOTES1").Value = BORDROACIKLAMA;
                //rolls.DataFields.FieldByName("ACCFICHEREF").Value = 89525;
                //rolls.DataFields.FieldByName("GL_CODE").Value = 320.10.01.001;
                rolls.DataFields.FieldByName("CURRSEL_TOTALS").Value = 1;
                rolls.DataFields.FieldByName("CURRSEL_DETAILS").Value = 2;

                UnityObjects.Lines transactions_lines = rolls.DataFields.FieldByName("TRANSACTIONS").Lines;
                transactions_lines.AppendLine();
                transactions_lines[transactions_lines.Count - 1].FieldByName("TYPE").Value = 1;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURRENT_STATUS").Value = 1;
                transactions_lines[transactions_lines.Count - 1].FieldByName("NUMBER").Value = "~";
                transactions_lines[transactions_lines.Count - 1].FieldByName("AUXIL_CODE").Value = _PAYMENT.POS.ToString();
                transactions_lines[transactions_lines.Count - 1].FieldByName("AUTH_CODE").Value = "BMS";
                transactions_lines[transactions_lines.Count - 1].FieldByName("OWING").Value = _PAYMENT.CUSTOMER_NAME;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DIVISION").Value = BRANCH;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DUE_DATE").Value = _PAYMENT.DATE_.Date;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATE").Value = _PAYMENT.DATE_.Date;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AMOUNT").Value = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString().Replace(".", ","));
                //transactions_lines[transactions_lines.Count - 1].FieldByName("TC_XRATE").Value = 1;
                //transactions_lines[transactions_lines.Count - 1].FieldByName("TC_AMOUNT").Value = 1234;
                //transactions_lines[transactions_lines.Count - 1].FieldByName("RC_XRATE").Value = 21.4534;
                //transactions_lines[transactions_lines.Count - 1].FieldByName("RC_AMOUNT").Value = 57.52;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRANS_STATUS").Value = 1;
                transactions_lines[transactions_lines.Count - 1].FieldByName("STATUS_ORDER").Value = 1;
                transactions_lines[transactions_lines.Count - 1].FieldByName("SERIAL_NR").Value = _PAYMENT.SERIAL_NO;
                transactions_lines[transactions_lines.Count - 1].FieldByName("XML_ATTRIBUTE1").Value = 1;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AFFECT_RISK").Value = 1;

                rolls.ReCalculate();

                if (!rolls.Post())
                    throw new Exception(GetLastError(rolls));
                int LOGOLREF = Convert.ToInt32(rolls.DataFields.DBFieldByName("LOGICALREF").Value);
                DateTime LOGOINSERTDATE = DateTime.Now;
                if (LOGOLREF > 0)
                    return "ok";
                else return "notok";
            }
            catch (Exception E)
            {
                LOGYAZ("InsertCheque", E);
                return E.Message;
            }
        }
        public static string InsertCHFiche(string BRANCH, Bms_Fiche_Payment _PAYMENT, string FIRMNR)
        {
            LOGYAZ($"InsertCHFiche BASLADI - BRANCH:{BRANCH}, FIRMNR:{FIRMNR}, CUSTOMER_CODE:{_PAYMENT?.CUSTOMER_CODE}, LOGO_FICHE_TYPE:{_PAYMENT?.LOGO_FICHE_TYPE}, PAYMENT_TOTAL:{_PAYMENT?.PAYMENT_TOTAL}, DATE:{_PAYMENT?.DATE_}, DOCUMENT_NO:{_PAYMENT?.DOCUMENT_NO}", null);

            // Parametre kontrolleri
            if (_PAYMENT == null)
            {
                LOGYAZ("InsertCHFiche", new Exception("_PAYMENT parametresi null"));
                return "_PAYMENT parametresi null";
            }
            if (string.IsNullOrEmpty(BRANCH))
            {
                LOGYAZ("InsertCHFiche", new Exception("BRANCH parametresi boş"));
                return "BRANCH parametresi boş";
            }
            if (string.IsNullOrEmpty(FIRMNR))
            {
                LOGYAZ("InsertCHFiche", new Exception("FIRMNR parametresi boş"));
                return "FIRMNR parametresi boş";
            }
            if (string.IsNullOrEmpty(_PAYMENT.CUSTOMER_CODE))
            {
                LOGYAZ("InsertCHFiche", new Exception("CUSTOMER_CODE boş"));
                return "CUSTOMER_CODE boş";
            }
            if (string.IsNullOrEmpty(_PAYMENT.LOGO_FICHE_TYPE))
            {
                LOGYAZ("InsertCHFiche", new Exception("LOGO_FICHE_TYPE boş"));
                return "LOGO_FICHE_TYPE boş";
            }

            bool isCustomerExist = false;
            try
            {
                LOGYAZ($"InsertCHFiche - Müşteri sorgusu yapılıyor: LG_{FIRMNR}_CLCARD, CODE={_PAYMENT.CUSTOMER_CODE}", null);
                isCustomerExist = Convert.ToBoolean(SqlSelectLogo($"SELECT COUNT(*) FROM LG_{FIRMNR}_CLCARD WHERE CODE='{_PAYMENT.CUSTOMER_CODE}'").Rows[0][0]);
                LOGYAZ($"InsertCHFiche - Müşteri sorgusu sonucu: isCustomerExist={isCustomerExist}", null);
            }
            catch (Exception custEx)
            {
                LOGYAZ($"InsertCHFiche - Müşteri sorgusu hatası", custEx);
            }

            if (!isCustomerExist)
            {
                string originalCode = _PAYMENT.CUSTOMER_CODE;
                _PAYMENT.CUSTOMER_CODE = _PAYMENT.CUSTOMER_CODE.TrimStart('0');
                LOGYAZ($"InsertCHFiche - Müşteri bulunamadı, kod düzeltildi: {originalCode} -> {_PAYMENT.CUSTOMER_CODE}", null);
            }

            try
            {
                LOGYAZ("InsertCHFiche - NewObjectData çağrılıyor (doARAPVoucher)", null);
                UnityObjects.Data arpvoucher = NewObjectData(UnityObjects.DataObjectType.doARAPVoucher);
                LOGYAZ("InsertCHFiche - NewObjectData başarılı, arpvoucher.New() çağrılıyor", null);
                arpvoucher.New();
                LOGYAZ("InsertCHFiche - arpvoucher.New() başarılı, field'lar ayarlanıyor", null);

                arpvoucher.DataFields.FieldByName("NUMBER").Value = "~";
                arpvoucher.DataFields.FieldByName("DATE").Value = _PAYMENT.DATE_.Date;
                if (_PAYMENT.LOGO_FICHE_TYPE == "CH Kredi Karti" || _PAYMENT.LOGO_FICHE_TYPE == "CH Kredi Karti Iade" || _PAYMENT.LOGO_FICHE_TYPE == "CH Borc" || _PAYMENT.LOGO_FICHE_TYPE == "CH Alacak")
                    arpvoucher.DataFields.FieldByName("TYPE").Value = 70;
                if (_PAYMENT.LOGO_FICHE_TYPE == "CH Kredi Karti Iade")
                    arpvoucher.DataFields.FieldByName("TYPE").Value = 71;
                if (_PAYMENT.LOGO_FICHE_TYPE == "CH Borc")
                    arpvoucher.DataFields.FieldByName("TYPE").Value = 3;
                if (_PAYMENT.LOGO_FICHE_TYPE == "CH Alacak")
                    arpvoucher.DataFields.FieldByName("TYPE").Value = 4;
                arpvoucher.DataFields.FieldByName("AUXIL_CODE").Value = _PAYMENT.POS.ToString();
                arpvoucher.DataFields.FieldByName("AUTH_CODE").Value = "BMS";
                arpvoucher.DataFields.FieldByName("DIVISION").Value = BRANCH;
                arpvoucher.DataFields.FieldByName("CURRSEL_TOTALS").Value = 1;

                LOGYAZ("InsertCHFiche - Header field'ları ayarlandı, satırlar ekleniyor", null);

                UnityObjects.Lines transactions_lines = arpvoucher.DataFields.FieldByName("TRANSACTIONS").Lines;
                transactions_lines.AppendLine();
                transactions_lines[transactions_lines.Count - 1].FieldByName("ARP_CODE").Value = _PAYMENT.CUSTOMER_CODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AUXIL_CODE").Value = _PAYMENT.POS.ToString();
                transactions_lines[transactions_lines.Count - 1].FieldByName("AUTH_CODE").Value = "BMS";
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRANNO").Value = "~";
                transactions_lines[transactions_lines.Count - 1].FieldByName("DOC_NUMBER").Value = _PAYMENT.DOCUMENT_NO.ToString();
                if (_PAYMENT.LOGO_FICHE_TYPE == "CH Kredi Karti" || _PAYMENT.LOGO_FICHE_TYPE == "CH Alacak")
                    transactions_lines[transactions_lines.Count - 1].FieldByName("CREDIT").Value = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString().Replace(".", ","));
                if (_PAYMENT.LOGO_FICHE_TYPE == "CH Kredi Karti Iade" || _PAYMENT.LOGO_FICHE_TYPE == "CH Borc")
                    transactions_lines[transactions_lines.Count - 1].FieldByName("DEBIT").Value = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString().Replace(".", ","));
                if (_PAYMENT.LOGO_FICHE_TYPE == "CH Kredi Karti" || _PAYMENT.LOGO_FICHE_TYPE == "CH Kredi Karti Iade")
                    transactions_lines[transactions_lines.Count - 1].FieldByName("BANKACC_CODE").Value = _PAYMENT.LOGO_BANK_OR_KS_CODE;

                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_XRATE").Value = 1;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_AMOUNT").Value = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString().Replace(".", ","));
                transactions_lines[transactions_lines.Count - 1].FieldByName("BNLN_TC_XRATE").Value = 1;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BNLN_TC_AMOUNT").Value = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString().Replace(".", ","));
                transactions_lines[transactions_lines.Count - 1].FieldByName("MONTH").Value = _PAYMENT.DATE_.Month;
                transactions_lines[transactions_lines.Count - 1].FieldByName("YEAR").Value = _PAYMENT.DATE_.Year;

                LOGYAZ("InsertCHFiche - Satırlar eklendi, ReCalculate çağrılıyor", null);
                arpvoucher.ReCalculate();

                LOGYAZ("InsertCHFiche - ReCalculate tamamlandı, Post çağrılıyor", null);
                if (!arpvoucher.Post())
                {
                    string postError = GetLastError(arpvoucher);
                    LOGYAZ($"InsertCHFiche - Post BAŞARISIZ: {postError}", null);
                    throw new Exception(postError);
                }

                int LOGOLREF = Convert.ToInt32(arpvoucher.DataFields.DBFieldByName("LOGICALREF").Value);
                LOGYAZ($"InsertCHFiche - Post BAŞARILI, LOGICALREF={LOGOLREF}", null);

                if (LOGOLREF > 0)
                    return "ok";
                else return "notok";
            }
            catch (Exception E)
            {
                LOGYAZ($"InsertCHFiche HATA - BRANCH:{BRANCH}, CUSTOMER_CODE:{_PAYMENT?.CUSTOMER_CODE}, LOGO_FICHE_TYPE:{_PAYMENT?.LOGO_FICHE_TYPE}", E);
                return E.Message;
            }
        }

        public static string InsertKsFiche(string BRANCH, Bms_Fiche_Payment _PAYMENT, string FIRMNR)
        {
            bool isCustomerExist = false;
            try
            {
                var query = $"SELECT COUNT(*) FROM LG_{FIRMNR}_CLCARD WHERE CODE='{_PAYMENT.CUSTOMER_CODE}'";
                LOGYAZ($"InsertKsFiche - Sorgu: {query}", null);
                var result = SqlSelectLogo(query).Rows[0][0];
                LOGYAZ($"InsertKsFiche - Sonuç: {result}", null);
                isCustomerExist = Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                LOGYAZ($"InsertKsFiche - Müşteri sorgusu HATA: {ex.Message}", ex);
            }
            if (!isCustomerExist)
                _PAYMENT.CUSTOMER_CODE = _PAYMENT.CUSTOMER_CODE.TrimStart('0');
            try
            {
                UnityObjects.Data sd_trans = NewObjectData(UnityObjects.DataObjectType.doSafeDepositTrans);
                sd_trans.New();

                if (_PAYMENT.FTYPE == "SATIS")
                    sd_trans.DataFields.FieldByName("TYPE").Value = 11;
                else if (_PAYMENT.FTYPE == "IADE")
                    sd_trans.DataFields.FieldByName("TYPE").Value = 12;
                sd_trans.DataFields.FieldByName("SD_CODE").Value = _PAYMENT.LOGO_BANK_OR_KS_CODE;
                sd_trans.DataFields.FieldByName("DATE").Value = _PAYMENT.DATE_.Date;
                sd_trans.DataFields.FieldByName("DIVISION").Value = BRANCH;
                sd_trans.DataFields.FieldByName("AUXIL_CODE").Value = _PAYMENT.POS.ToString();
                sd_trans.DataFields.FieldByName("AUTH_CODE").Value = "BMS";
                sd_trans.DataFields.FieldByName("NUMBER").Value = "~";
                sd_trans.DataFields.FieldByName("MASTER_TITLE").Value = _PAYMENT.CUSTOMER_NAME;
                //sd_trans.DataFields.FieldByName("DESCRIPTION").Value = KASAACIKLAMASI;
                sd_trans.DataFields.FieldByName("AMOUNT").Value = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString().Replace(".", ","));
                sd_trans.DataFields.FieldByName("TC_XRATE").Value = 1;
                sd_trans.DataFields.FieldByName("TC_AMOUNT").Value = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString().Replace(".", ","));

                UnityObjects.Lines attachment_arp_lines = sd_trans.DataFields.FieldByName("ATTACHMENT_ARP").Lines;
                attachment_arp_lines.AppendLine();
                attachment_arp_lines[attachment_arp_lines.Count - 1].FieldByName("ARP_CODE").Value = _PAYMENT.CUSTOMER_CODE;
                //attachment_arp_lines[attachment_arp_lines.Count - 1].FieldByName("GL_CODE2").Value = 100;
                attachment_arp_lines[attachment_arp_lines.Count - 1].FieldByName("AUXIL_CODE").Value = _PAYMENT.POS.ToString();
                attachment_arp_lines[attachment_arp_lines.Count - 1].FieldByName("AUTH_CODE").Value = "BMS";
                attachment_arp_lines[attachment_arp_lines.Count - 1].FieldByName("TRANNO").Value = "~";
                attachment_arp_lines[attachment_arp_lines.Count - 1].FieldByName("DOC_NUMBER").Value = _PAYMENT.DOCUMENT_NO.ToString();
                //attachment_arp_lines[attachment_arp_lines.Count - 1].FieldByName("DESCRIPTION").Value = KASAACIKLAMASI;
                if (_PAYMENT.FTYPE == "SATIS")
                    attachment_arp_lines[attachment_arp_lines.Count - 1].FieldByName("CREDIT").Value = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString().Replace(".", ","));
                else if (_PAYMENT.FTYPE == "IADE")
                    attachment_arp_lines[attachment_arp_lines.Count - 1].FieldByName("DEBIT").Value = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString().Replace(".", ","));
                attachment_arp_lines[attachment_arp_lines.Count - 1].FieldByName("TC_XRATE").Value = 1;
                attachment_arp_lines[attachment_arp_lines.Count - 1].FieldByName("TC_AMOUNT").Value = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString().Replace(".", ","));

                sd_trans.ReCalculate();
                if (!sd_trans.Post())
                    throw new Exception(GetLastError(sd_trans));
                int LOGOLREF = Convert.ToInt32(sd_trans.DataFields.DBFieldByName("LOGICALREF").Value);
                DateTime LOGOINSERTDATE = DateTime.Now;
                if (LOGOLREF > 0)
                    return "ok";
                else return "notok";
            }
            catch (Exception E)
            {
                LOGYAZ("InsertKsFiche", E);
                return E.Message;
            }
        }
        public static void LOBJECTSKILLER()
        {
            try
            {
                foreach (Process p in Process.GetProcessesByName("LOBJECTS"))
                    p.Kill();
            }
            catch (Exception EX)
            {
                LOGYAZ("LOBJECTSKILLER", EX);
            }
        }

        public class AdditionalDatum
        {
            public string key { get; set; }
            public string value { get; set; }
        }

        public class AdditionalInfo
        {
            public string techPosIdAddress { get; set; }
            public string techPosSerialNumber { get; set; }
            public string fiscalBoxIpAddress { get; set; }
            public int tokenCompanyId { get; set; }
            public string merchantId { get; set; }
            public string cashRegisterFactoryNumber { get; set; }
            public string cashRegisterModel { get; set; }
            public string cashBoxFactoryNumber { get; set; }
            public string cashBoxTaxNumber { get; set; }
            public string companyTaxNumber { get; set; }
            public string objectTaxNumber { get; set; }
            public string objectName { get; set; }
            public string objectAddress { get; set; }
            public string firmwareVersion { get; set; }
            public int posIntegrationMethod { get; set; }
            public int freeOfChargeCounter { get; set; }
            public string pin { get; set; }
            public string role { get; set; }
            public string adminPin { get; set; }
            public string adminPassword { get; set; }
            public DateTime notBefore { get; set; }
            public DateTime notAfter { get; set; }
            public DateTime fiscalInfoUpdatedAt { get; set; }
            public bool sendOnlyChanges { get; set; }
        }

        public class AdditionalSalesInformations
        {
            public int id { get; set; }
            public string header { get; set; }
            public string description { get; set; }
            public int minLength { get; set; }
            public int maxLength { get; set; }
            public bool isRequired { get; set; }
            public bool isNumeric { get; set; }
            public bool isActive { get; set; }
            public List<string> productAdditionalSalesInformations { get; set; }
        }

        public class AdditionalSalesMessages
        {
            public int id { get; set; }
            public string header { get; set; }
            public string description { get; set; }
            public bool isActive { get; set; }
            public List<string> productAdditionalSalesMessages { get; set; }
        }

        public class AlternatePrice
        {
            public int priceId { get; set; }
            public bool isInstallmentsAvaible { get; set; }
        }

        public class Audit
        {
            public int userId { get; set; }
            public string userCode { get; set; }
            public string userName { get; set; }
            public int @event { get; set; }
            public DateTime date { get; set; }
            public int id { get; set; }
            public int storeId { get; set; }
            public int posId { get; set; }
            public int type { get; set; }
            public string additionalData { get; set; }
            public string store { get; set; }
            public string pos { get; set; }
            public string user { get; set; }
        }

        public class Azerbaijan
        {
            public bool audit { get; set; }
            public bool correction { get; set; }
            public bool checkControlTape { get; set; }
            public bool rollBack { get; set; }
            public bool zReportCopy { get; set; }
            public bool documentCopy { get; set; }
        }

        public class Banks
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> paymentTypes { get; set; }
        }

        public class Barcode
        {
            public int id { get; set; }
            public int productsId { get; set; }
            public string barcodeNo { get; set; }
            public int quantity { get; set; }
            public string unit { get; set; }
            public int priceId { get; set; }
            public string products { get; set; }
        }

        public class BillPaying
        {
            public int id { get; set; }
            public long posDocumentId { get; set; }
            public string documentNo { get; set; }
            public DateTime date { get; set; }
            public int type { get; set; }
            public string companyName { get; set; }
            public bool isValid { get; set; }
            public int storeId { get; set; }
            public int posId { get; set; }
            public int userId { get; set; }
            public int invoiceAmount { get; set; }
            public int feeAmount { get; set; }
            public int totalAmount { get; set; }
            public int customersId { get; set; }
            public string customerCardNo { get; set; }
            public Details details { get; set; }
            public List<Payment> payments { get; set; }
            public int transferStatus { get; set; }
            public string stores { get; set; }
            public string pos { get; set; }
            public string users { get; set; }
            public BillPaymentTaxPayer billPaymentTaxPayer { get; set; }
        }

        public class BillPaymentTaxPayer
        {
            public int id { get; set; }
            public string taxNumber { get; set; }
            public string name { get; set; }
            public string address { get; set; }
            public int billPayingsId { get; set; }
            public string passportNo { get; set; }
            public string residence { get; set; }
            public string passportCountry { get; set; }
            public bool isEInvoiceRegistered { get; set; }
            public string billPayings { get; set; }
        }

        public class Bonuse
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int bonus { get; set; }
            public int spendableBonus { get; set; }
            public int refundableAmount { get; set; }
            public int pendingBonus { get; set; }
            public int minimumSpend { get; set; }
            public int multiplier { get; set; }
            public int usageLimit { get; set; }
            public int receiptLimit { get; set; }
            public int receiptItemLimit { get; set; }
            public int monthlyDiscountLimit { get; set; }
            public int spendType { get; set; }
            public DateTime bonusExpireDate { get; set; }
            public string bonusCustomerNotification { get; set; }
            public int bonusId { get; set; }
            public int amount { get; set; }
            public int lineId { get; set; }
            public string provisionId { get; set; }
            public int transactionType { get; set; }
            public DateTime activationDate { get; set; }
            public DateTime expireDate { get; set; }
        }

        public class BonusQuota1
        {
            public int percentage { get; set; }
            public int amount { get; set; }
        }

        public class BonusQuota2
        {
            public int percentage { get; set; }
            public int amount { get; set; }
        }

        public class Campaign
        {
            public int id { get; set; }
            public string name { get; set; }
            public DateTime beginDate { get; set; }
            public DateTime endDate { get; set; }
            public int beginHour { get; set; }
            public int endHour { get; set; }
            public bool isForCustomersOnly { get; set; }
            public bool validForAllStores { get; set; }
            public int campaignTypeId { get; set; }
            public int conditionAmount { get; set; }
            public int conditionQuantity { get; set; }
            public bool isConditionProductExcluded { get; set; }
            public int mainConditionType { get; set; }
            public int mainDiscountType { get; set; }
            public int mainExcConditionType { get; set; }
            public bool isActive { get; set; }
            public string b2CCampaignId { get; set; }
            public int defaultApplicationType { get; set; }
            public int buyNPayN_1ConditionNum { get; set; }
            public int buyNPayN_1PayNum { get; set; }
            public bool day1 { get; set; }
            public bool day2 { get; set; }
            public bool day3 { get; set; }
            public bool day4 { get; set; }
            public bool day5 { get; set; }
            public bool day6 { get; set; }
            public bool day7 { get; set; }
            public int sequence { get; set; }
            public int setId { get; set; }
            public int groupConditionType { get; set; }
            public int groupConditionId { get; set; }
            public int groupConditionValue { get; set; }
            public int spendType { get; set; }
            public int giftCardType { get; set; }
            public int version { get; set; }
            public bool isDynamicMixMatch { get; set; }
            public bool isFoldingConditionAmountEnabled { get; set; }
            public bool isDigitalStoreCard { get; set; }
            public int executionType { get; set; }
            public int executionValue { get; set; }
            public int netTotal { get; set; }
            public bool hasRemainder { get; set; }
            public bool hasScales { get; set; }
            public int discountToDocumentCondition { get; set; }
            public int discountToDocumentValue { get; set; }
            public bool distributeDiscountToAllProducts { get; set; }
            public bool requiresCouponsToRun { get; set; }
            public bool isWinningCampaign { get; set; }
            public string winningCampaignList { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<CampaignProduct> campaignProducts { get; set; }
            public List<CampaignProductExcluded> campaignProductExcludeds { get; set; }
            public List<CampaignSegment> campaignSegments { get; set; }
            public List<CampaignPayment> campaignPayments { get; set; }
            public List<string> campaignStores { get; set; }
            public List<CampaignScale> campaignScales { get; set; }
            public List<PublishedCampaign> publishedCampaigns { get; set; }
        }

        public class CampaignCode
        {
            public int id { get; set; }
            public string campaignCode { get; set; }
            public string description { get; set; }
            public string receiptHeader { get; set; }
            public string receiptMessage { get; set; }
            public bool isCampaignFound { get; set; }
        }

        public class CampaignPayment
        {
            public int id { get; set; }
            public int campaignId { get; set; }
            public int paymentTypesId { get; set; }
            public int installmentLimit { get; set; }
            public string paymentMethod { get; set; }
            public string paymentTypes { get; set; }
            public string campaign { get; set; }
        }

        public class CampaignProduct
        {
            public int id { get; set; }
            public int campaignId { get; set; }
            public int conditionId { get; set; }
            public int conditionType { get; set; }
            public int conditionProductQuantity { get; set; }
            public int discountType { get; set; }
            public int discountTypeId { get; set; }
            public int applicationType { get; set; }
            public int applicationValue { get; set; }
            public string campaign { get; set; }
        }

        public class CampaignProductExcluded
        {
            public int id { get; set; }
            public int campaignId { get; set; }
            public int conditionId { get; set; }
            public int conditionType { get; set; }
            public string campaign { get; set; }
        }

        public class CampaignScale
        {
            public int id { get; set; }
            public int campaignId { get; set; }
            public int scale { get; set; }
            public int value { get; set; }
            public int type { get; set; }
            public string campaign { get; set; }
        }

        public class CampaignSegment
        {
            public int id { get; set; }
            public int campaignId { get; set; }
            public int segmentId { get; set; }
            public string campaign { get; set; }
        }

        public class CampaignStore
        {
            public int id { get; set; }
            public int campaignId { get; set; }
            public int storesId { get; set; }
            public string stores { get; set; }
            public Campaign campaign { get; set; }
        }

        public class CancelDocument
        {
            public int authority { get; set; }
            public int limit { get; set; }
        }

        public class CancelledSale
        {
            public int id { get; set; }
            public long posDocumentId { get; set; }
            public int documentsTypeId { get; set; }
            public string documentNo { get; set; }
            public int storesId { get; set; }
            public DateTime date { get; set; }
            public DateTime startDate { get; set; }
            public int usersId { get; set; }
            public int posId { get; set; }
            public int refundReasonId { get; set; }
            public string posVersion { get; set; }
            public int salesType { get; set; }
            public int transferStatus { get; set; }
            public int transferBatchId { get; set; }
            public string closureNo { get; set; }
            public CustomerData customerData { get; set; }
            public string receiptNo { get; set; }
            public int customersId { get; set; }
            public string customerCardNo { get; set; }
            public int invoiceType { get; set; }
            public string taxNumber { get; set; }
            public DateTime createDate { get; set; }
            public int lineCount { get; set; }
            public int cancelledLineCount { get; set; }
            public int totalAmount { get; set; }
            public int vatTotal { get; set; }
            public int discountTotal { get; set; }
            public int grossTotal { get; set; }
            public List<Audit> audits { get; set; }
            public List<Job> jobs { get; set; }
            public string documents { get; set; }
            public string stores { get; set; }
            public string users { get; set; }
            public string pos { get; set; }
            public List<CancelledSalesProduct> cancelledSalesProducts { get; set; }
        }

        public class CancelledSalesProduct
        {
            public int id { get; set; }
            public int cancelledSalesId { get; set; }
            public long posDocumentId { get; set; }
            public int productsId { get; set; }
            public int vatPercent { get; set; }
            public int vatId { get; set; }
            public int Amount { get; set; }
            public decimal TotalPrice { get; set; }
            public int vatTotal { get; set; }
            public bool isValid { get; set; }
            public int discountTotalDirect { get; set; }
            public int discountTotalIndirect { get; set; }
            public int discountTotalCampaign { get; set; }
            public int salesmanId { get; set; }
            public string description { get; set; }
            public string barcodeNo { get; set; }
            public int sequence { get; set; }
            public string cancelledSales { get; set; }
            public string products { get; set; }
        }

        public class CancelLine
        {
            public int authority { get; set; }
            public int limit { get; set; }
        }

        public class Cash
        {
            public int level1 { get; set; }
            public int level2 { get; set; }
            public int maximum { get; set; }
        }

        public class City
        {
            public int id { get; set; }
            public string name { get; set; }
            public int plateCode { get; set; }
            public int regionId { get; set; }
            public int countryId { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public Country country { get; set; }
            public Region region { get; set; }
            public List<string> districts { get; set; }
        }

        public class Closure
        {
            public int id { get; set; }
            public DateTime date { get; set; }
            public int storeId { get; set; }
            public int posId { get; set; }
            public string xmlData { get; set; }
            public string no { get; set; }
            public string fisNo { get; set; }
            public int auto { get; set; }
            public string serialNumber { get; set; }
            public DateTime reportDate { get; set; }
            public string stores { get; set; }
            public string pos { get; set; }
            public List<ClosureDetail> closureDetails { get; set; }
        }

        public class ClosureDetail
        {
            public int id { get; set; }
            public int closureId { get; set; }
            public int group { get; set; }
            public int deptIndex { get; set; }
            public int deptPercentage { get; set; }
            public int deptTotal { get; set; }
            public int deptItem { get; set; }
            public int deptVat { get; set; }
            public int totalSales { get; set; }
            public int totalVat { get; set; }
            public int counter { get; set; }
            public int abortCount { get; set; }
            public int fiscalCount { get; set; }
            public int fiscalAbortCount { get; set; }
            public int nonFiscalCount { get; set; }
            public int discountCount { get; set; }
            public int ticketTotal { get; set; }
            public int ticketCount { get; set; }
            public int cashSum { get; set; }
            public int cashlessSum { get; set; }
            public int prepaymentSum { get; set; }
            public int creditSum { get; set; }
            public int bonusSum { get; set; }
            public string closure { get; set; }
        }

        public class Countries
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> citys { get; set; }
            public List<string> productVats { get; set; }
            public List<VatNumber> vatNumbers { get; set; }
        }

        public class Country
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> citys { get; set; }
            public List<string> productVats { get; set; }
            public List<VatNumber> vatNumbers { get; set; }
        }

        public class Coupon
        {
            public string couponNo { get; set; }
            public int matchBy { get; set; }
            public int printType { get; set; }
            public bool isExclusiveToCustomer { get; set; }
            public List<CampaignCode> campaignCodes { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime validFrom { get; set; }
            public DateTime validThru { get; set; }
            public List<AdditionalDatum> additionalData { get; set; }
        }

        public class CreditCardDetails
        {
            public string AcquirerId { get; set; }
            public string BatchNo { get; set; }
            public string InstallmentCount { get; set; }
            public string CardNo { get; set; }
            public string StanNo { get; set; }
            public string TerminalId { get; set; }
            public string AuthorizationCode { get; set; }
            public string IssuerId { get; set; }
            public string MerchantId { get; set; }
            public string ReferenceNumber { get; set; }
            public string CardType { get; set; }
            public string Posem { get; set; }
            public string TransactionType { get; set; }
            public string Aid { get; set; }
            public string RRN { get; set; }
            public string ApplicationId { get; set; }
        }

        public class CrmApiDetails
        {
            public string url { get; set; }
            public string username { get; set; }
            public string password { get; set; }
        }

        public class CustomerData
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public string cardNumber { get; set; }
            public string mobilePhone { get; set; }
            public string email { get; set; }
            public DateTime dateOfBirth { get; set; }
            public DateTime dateOfMarrige { get; set; }
            public DateTime dateOfBirthWife { get; set; }
            public string address { get; set; }
            public string cityName { get; set; }
            public string districtName { get; set; }
            public string taxOffice { get; set; }
            public string taxNumber { get; set; }
            public string identityNumber { get; set; }
            public bool allowPromotionEmail { get; set; }
            public bool allowPromotionSms { get; set; }
            public string segmentText { get; set; }
            public string passportNo { get; set; }
            public string nationality { get; set; }
            public string country { get; set; }
            public string groupCodeERP { get; set; }
            public List<int> groups { get; set; }
            public int accountType { get; set; }
            public bool returnEmpty { get; set; }
            public bool searchInAllFields { get; set; }
            public int posId { get; set; }
            public int storeId { get; set; }
            public int userId { get; set; }
            public string storeCode { get; set; }
            public string posCode { get; set; }
            public string cashierCode { get; set; }
            public int searchType { get; set; }
            public bool isEInvoiceCustomer { get; set; }
            public int allowGdprEnum { get; set; }
            public bool allowGdpr { get; set; }
            public int source { get; set; }
            public bool runOnlineCampaigns { get; set; }
            public bool forceOnlineCampaigns { get; set; }
            public List<int> segments { get; set; }
            public List<SegmentList> segmentList { get; set; }
            public List<Bonuse> bonuses { get; set; }
            public string image { get; set; }
            public int priceIndex { get; set; }
            public bool showInvitationButton { get; set; }
            public bool noteOfExpenseSerialNumber { get; set; }
            public int whenCustomerNotFoundContinueWith { get; set; }
            public int smsRequestId { get; set; }
            public bool isValidationRequired { get; set; }
            public string validationCode { get; set; }
            public List<int> customerValidateEnum { get; set; }
            public List<Coupon> coupons { get; set; }
            public bool isQuerableCustomerCampaigns { get; set; }
            public Message message { get; set; }
            public bool hasMessage { get; set; }
            public bool forceCashierToEditPage { get; set; }
            public int campaignDiscountLimit { get; set; }
            public string maskedMobilePhone { get; set; }
            public string maskedCardNumber { get; set; }
            public string maskedNameSurname { get; set; }
            public string maskedEmail { get; set; }
        }

        public class Data
        {
            public int id { get; set; }
            public long posDocumentId { get; set; }
            public string receiptNo { get; set; }
            public int total { get; set; }
            public int documentType { get; set; }
            public string documentTypeName { get; set; }
            public string documentNo { get; set; }
            public string storeCode { get; set; }
            public string posCode { get; set; }
            public string userCode { get; set; }
            public DateTime date { get; set; }
            public DateTime startDate { get; set; }
            public int invoiceType { get; set; }
            public int salesType { get; set; }
            public int refundReasonId { get; set; }
            public string taxNumber { get; set; }
            public int customerId { get; set; }
            public string customerCardNo { get; set; }
            public string closureNo { get; set; }
            public int linkedDocumentId { get; set; }
            public string linkedDocumentNo { get; set; }
            public string couponReserveId { get; set; }
            public int priceId { get; set; }
            public CustomerData customerData { get; set; }
            public TaxPayer taxPayer { get; set; }
            public List<Line> lines { get; set; }
            public List<Payment> payments { get; set; }
            public List<Bonuse> bonuses { get; set; }
            public List<Detail> details { get; set; }
        }

        public class Detail
        {
            public int key { get; set; }
            public string value { get; set; }
            public string MobilivaProvisionId { get; set; }
            public string SuspendedDocDate { get; set; }
            public string SuspendedDocInfo { get; set; }
            public string DocumentProvisionId { get; set; }
            public string GlobalBlueDocId { get; set; }
            public string EcommerceSiteId { get; set; }
            public string EcommerceOrderNo { get; set; }
            public string EcommerceAdditionalCode { get; set; }
            public string EcommerceSalesman { get; set; }
            public string EcommerceUrl { get; set; }
            public string Coupons { get; set; }
            public string CompletionTimeExpired { get; set; }
            public string CreatedCoupons { get; set; }
            public string EarnedPoint { get; set; }
            public string EcommerceMarketId { get; set; }
            public string EcommerceMarketPlaceName { get; set; }
            public string EcommerceMarketType { get; set; }
            public string CreatedGiftCards { get; set; }
            public int priceQueryModalWaitInSeconds { get; set; }
            public string description { get; set; }
            public string companyName { get; set; }
            public string invoiceNo { get; set; }
            public int invoiceAmount { get; set; }
            public int feeAmount { get; set; }
            public int totalAmount { get; set; }
            public DateTime paymentDate { get; set; }
            public DateTime invoiceDate { get; set; }
            public string subscriberName { get; set; }
            public string subscriberNumber { get; set; }
            public GiftCard giftCard { get; set; }
            public Ibb ibb { get; set; }
            public Donation donation { get; set; }
            public int isLunchVoucher { get; set; }
            public int priceType { get; set; }
            public int returnType { get; set; }
            public int quantityType { get; set; }
            public int discountType { get; set; }
            public int scaleType { get; set; }
            public int salesInstallmentType { get; set; }
            public bool isGivesBonus { get; set; }
            public int bonusMultiplier { get; set; }
            public int installmentNumber { get; set; }
            public int productType { get; set; }
            public int maxQuantity { get; set; }
            public Azerbaijan azerbaijan { get; set; }
            public Report report { get; set; }
            public EftPos eftPos { get; set; }
            public DocumentDiscountByPercent documentDiscountByPercent { get; set; }
            public DocumentDiscountByAmount documentDiscountByAmount { get; set; }
            public LineDiscountByPercent lineDiscountByPercent { get; set; }
            public LineDiscountByAmount lineDiscountByAmount { get; set; }
            public DiscountByGiftCard discountByGiftCard { get; set; }
            public ShelveDocument shelveDocument { get; set; }
            public UnshelveDocument unshelveDocument { get; set; }
            public CancelDocument cancelDocument { get; set; }
            public CancelLine cancelLine { get; set; }
            public PaymentCancel paymentCancel { get; set; }
            public Sales sales { get; set; }
            public General general { get; set; }
            public Menu menu { get; set; }
            public Payment payment { get; set; }
            public BonusQuota1 bonusQuota1 { get; set; }
            public BonusQuota2 bonusQuota2 { get; set; }
            public Cash cash { get; set; }
            public string BackEndVersion { get; set; }
            public string OkcDllVersion { get; set; }
            public string FiscalFirmware { get; set; }
            public bool notUsableWithCampaigns { get; set; }
            public bool notUsableWithCashierDiscounts { get; set; }
            public bool notUsableWithBonusDiscounts { get; set; }
            public bool notUsableWithDiscountVouchers { get; set; }
            public bool notUsableWithBonusPayments { get; set; }
            public bool notUsableWithPaymentVouchers { get; set; }
            public bool notUsableWithDonations { get; set; }
            public bool useInFreeOfChargeDocuments { get; set; }
            public int virtualPaymentType { get; set; }
            public GlobalBlue globalBlue { get; set; }
            public IbbKart ibbKart { get; set; }
            public int freeOfChargeCounter { get; set; }
            public string code2 { get; set; }
            public bool ignoreProductQuantityLimits { get; set; }
            public bool restrictCashierToLogInMultipleCashRegisters { get; set; }
            public bool doNotPrintDocumentsWhenCancellingSalesDocuments { get; set; }
            public bool isRefundAndSaleRequiredForChange { get; set; }
            public bool shouldMaskedPhoneNumberOnSearch { get; set; }
            public string AcquirerId { get; set; }
            public string BatchNo { get; set; }
            public string InstallmentCount { get; set; }
            public string CardNo { get; set; }
            public string StanNo { get; set; }
            public string TerminalId { get; set; }
            public string AuthorizationCode { get; set; }
            public string IssuerId { get; set; }
            public string MerchantId { get; set; }
            public string ReferenceNumber { get; set; }
            public string CardType { get; set; }
            public string Posem { get; set; }
            public string TransactionType { get; set; }
            public string Aid { get; set; }
            public string RRN { get; set; }
            public string ApplicationId { get; set; }
        }

        public class Discount
        {
            public int source { get; set; }
            public int bonusId { get; set; }
            public string giftCardCode { get; set; }
            public int totalDiscount { get; set; }
            public int distributedAmount { get; set; }
            public int discountReasonId { get; set; }
            public string discountReason { get; set; }
            public string targetProvider { get; set; }
            public string code { get; set; }
            public string referenceNumber { get; set; }
        }

        public class DiscountByGiftCard
        {
            public int authority { get; set; }
            public int limit { get; set; }
            public bool canQueryInMainMenu { get; set; }
        }

        public class DisplaySettings
        {
            public int id { get; set; }
            public string area { get; set; }
            public string name { get; set; }
            public string settings { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> storeSettingDocuments { get; set; }
        }

        public class District
        {
            public int id { get; set; }
            public string name { get; set; }
            public int cityId { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public City city { get; set; }
            public List<string> stores { get; set; }
        }

        public class DocumentDiscountByAmount
        {
            public int authority { get; set; }
            public int limit { get; set; }
        }

        public class DocumentDiscountByPercent
        {
            public int authority { get; set; }
            public int limit { get; set; }
        }

        public class Documents
        {
            public int id { get; set; }
            public string name { get; set; }
            public int documentTypesId { get; set; }
            public int sequence { get; set; }
            public int upperLimit { get; set; }
            public int lowerLimit { get; set; }
            public string colorCode { get; set; }
            public string logoPath { get; set; }
            public int returnDocumentType { get; set; }
            public bool askForStoreManagerPassword { get; set; }
            public int printingOption { get; set; }
            public int printCopyCount { get; set; }
            public int isLunchVoucher { get; set; }
            public int priceId { get; set; }
            public int offlinePriceId { get; set; }
            public bool noCampaign { get; set; }
            public int smsConfirmType { get; set; }
            public int customerType { get; set; }
            public List<AlternatePrice> alternatePrices { get; set; }
            public int ecommerceId { get; set; }
            public bool ignoreProductQuantityLimits { get; set; }
            public int maxLineCount { get; set; }
            public int manuelRefundOption { get; set; }
            public bool useOneSumLinePerProduct { get; set; }
            public List<int> excludedPayments { get; set; }
            public List<int> relatedDocuments { get; set; }
            public int noteOption { get; set; }
            public bool ignoreDocumentCompletionThreshold { get; set; }
            public bool allowRefundProductPickFromList { get; set; }
            public int campaignSequence { get; set; }
            public int ruleFlag { get; set; }
            public int changeDocumentPriceOption { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public Ecommerces ecommerces { get; set; }
            public DocumentTypes documentTypes { get; set; }
            public List<StoreSettingDocument> storeSettingDocuments { get; set; }
            public List<CancelledSale> cancelledSales { get; set; }
            public List<string> sales { get; set; }
            public List<UserRoleDocument> userRoleDocuments { get; set; }
        }

        public class DocumentTypes
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool isAllowedToSale { get; set; }
            public List<string> documents { get; set; }
        }

        public class Donation
        {
            public string phoneNumber { get; set; }
            public string fullName { get; set; }
            public string corporate { get; set; }
            public int amount { get; set; }
            public string transactionId { get; set; }
            public List<int> services { get; set; }
        }

        public class Ecommerces
        {
            public int id { get; set; }
            public string name { get; set; }
            public int paymentId { get; set; }
            public int additionalCode { get; set; }
            public int salesman { get; set; }
            public int paymentType { get; set; }
            public int productMatching { get; set; }
            public Options options { get; set; }
            public int permissions { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public PaymentTypes paymentTypes { get; set; }
            public List<string> documents { get; set; }
        }

        public class EftPos
        {
            public bool endOfDay { get; set; }
            public bool updateParameter { get; set; }
            public bool function1 { get; set; }
            public bool function2 { get; set; }
            public bool function3 { get; set; }
            public bool function4 { get; set; }
            public bool function5 { get; set; }
        }

        public class General
        {
            public bool onlineRefund { get; set; }
            public bool taxFreeSales { get; set; }
            public bool diplomaticSales { get; set; }
            public bool changePasswordOnFirstLogin { get; set; }
            public bool canChangePassword { get; set; }
            public int priceQuery { get; set; }
            public int priceOverride { get; set; }
            public int cancelReservation { get; set; }
            public bool printGiftChangeSlip { get; set; }
            public int printLastReceiptCopy { get; set; }
        }

        public class GiftCard
        {
            public string code { get; set; }
            public int amount { get; set; }
            public string groupCode { get; set; }
            public string description { get; set; }
            public string receiptMessage { get; set; }
            public DateTime validThru { get; set; }
            public string cardType { get; set; }
        }

        public class GiftChangeReceipt
        {
            public int id { get; set; }
            public DateTime date { get; set; }
            public int salesId { get; set; }
            public int storesId { get; set; }
            public int posId { get; set; }
            public int usersId { get; set; }
            public string documentNo { get; set; }
            public bool isActive { get; set; }
            public bool isSpend { get; set; }
            public int printCount { get; set; }
            public string stores { get; set; }
            public string pos { get; set; }
            public string users { get; set; }
            public List<GiftChangeReceiptDetail> giftChangeReceiptDetails { get; set; }
        }

        public class GiftChangeReceiptDetail
        {
            public int id { get; set; }
            public int giftChangeReceiptsId { get; set; }
            public int productsId { get; set; }
            public string barcode { get; set; }
            public int Amount { get; set; }
            public decimal TotalPrice { get; set; }
            public string products { get; set; }
            public string giftChangeReceipts { get; set; }
        }

        public class GlobalBlue
        {
            public string url { get; set; }
            public string username { get; set; }
            public string password { get; set; }
        }

        public class Ibb
        {
            public string transactionId { get; set; }
            public int amount { get; set; }
        }

        public class IbbKart
        {
            public string clientKey { get; set; }
            public string clientPass { get; set; }
        }

        public class Info
        {
            public IstanbulKart istanbulKart { get; set; }
            public string giftCardCode { get; set; }
            public string giftCardType { get; set; }
            public string securityCode { get; set; }
            public bool isGiftCard { get; set; }
            public bool isRefundVoucher { get; set; }
            public string slip { get; set; }
            public string provisionId { get; set; }
        }

        public class InvoiceHeaders
        {
            public string nameSurname { get; set; }
            public string taxOffice { get; set; }
            public string taxNumber { get; set; }
            public string address { get; set; }
            public string businessName { get; set; }
            public string mersisNumber { get; set; }
            public string phoneNumber { get; set; }
        }

        public class IstanbulKart
        {
            public List<string> additionalProp1 { get; set; }
            public List<string> additionalProp2 { get; set; }
            public List<string> additionalProp3 { get; set; }
        }

        public class IstanbulKartInfo
        {
            public List<string> additionalProp1 { get; set; }
            public List<string> additionalProp2 { get; set; }
            public List<string> additionalProp3 { get; set; }
        }

        public class Job
        {
            public int type { get; set; }
            public int status { get; set; }
            public DateTime completedAt { get; set; }
            public string message { get; set; }
            public string key { get; set; }
        }

        public class KeyboardButtons
        {
            public int id { get; set; }
            public int code { get; set; }
            public int multiKey { get; set; }
            public string name { get; set; }
            public List<string> keyboardDetails { get; set; }
        }

        public class KeyboardDefinitions
        {
            public int id { get; set; }
            public string name { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<KeyboardDetail> keyboardDetails { get; set; }
            public List<string> posSettings { get; set; }
        }

        public class KeyboardDetail
        {
            public int id { get; set; }
            public int targetId { get; set; }
            public int keyboardId { get; set; }
            public int headerId { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public KeyboardButtons keyboardButtons { get; set; }
            public KeyboardTargets keyboardTargets { get; set; }
            public string keyboardDefinitions { get; set; }
        }

        public class KeyboardTargets
        {
            public int id { get; set; }
            public string target { get; set; }
            public string action { get; set; }
            public string name { get; set; }
            public List<string> keyboardDetails { get; set; }
        }

        public class Line
        {
            public int id { get; set; }
            public int pickupId { get; set; }
            public int paymentId { get; set; }
            public int quantity { get; set; }
            public int banknote { get; set; }
            public int totalAmount { get; set; }
            public int exchangeRate { get; set; }
            public string pickupLoan { get; set; }
            public string paymentTypes { get; set; }
            public int sequence { get; set; }
            public long posDocumentId { get; set; }
            public string productCode { get; set; }
            public string productName { get; set; }
            public string productUnit { get; set; }
            public int vatPercent { get; set; }
            public int vatId { get; set; }
            public double amount { get; set; }
            public decimal TotalPrice { get; set; }
            public bool isValid { get; set; }
            public int taxableTotal { get; set; }
            public int vatTotal { get; set; }
            public string barcodeNo { get; set; }
            public int discountTotal { get; set; }
            public int discountTotalDirect { get; set; }
            public int discountTotalIndirect { get; set; }
            public int discountTotalCampaign { get; set; }
            public int salesmanId { get; set; }
            public string salesmanCode { get; set; }
            public string description { get; set; }
            public bool isPriceEnteredByUser { get; set; }
            public List<LineCampaign> lineCampaigns { get; set; }
            public List<LineBonuse> lineBonuses { get; set; }
            public List<Discount> discounts { get; set; }
            public int refundReasonId { get; set; }
            public int priceChangeReasonId { get; set; }
            public string refundReasonName { get; set; }
            public string priceChangeReasonCode { get; set; }
            public string priceChangeReasonName { get; set; }
        }

        public class LineBonuse
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public DateTime activationDate { get; set; }
            public DateTime expireDate { get; set; }
            public int amount { get; set; }
            public int transactionType { get; set; }
        }

        public class LineCampaign
        {
            public int id { get; set; }
            public int campaignId { get; set; }
            public int campaignVersion { get; set; }
            public string campaignErpId { get; set; }
            public string campaignName { get; set; }
            public string couponNo { get; set; }
            public int totalDiscount { get; set; }
            public int distributedAmount { get; set; }
            public int campaignType { get; set; }
        }

        public class LineDiscountByAmount
        {
            public int authority { get; set; }
            public int limit { get; set; }
        }

        public class LineDiscountByPercent
        {
            public int authority { get; set; }
            public int limit { get; set; }
        }

        public class Menu
        {
            public bool pickup { get; set; }
            public bool loan { get; set; }
            public bool lockScreen { get; set; }
            public bool openCashDrawer { get; set; }
        }

        public class Message
        {
            public int location { get; set; }
            public string title { get; set; }
            public string body { get; set; }
            public List<string> bulletList { get; set; }
        }

        public class NextPrices
        {
            public List<Price1> price1 { get; set; }
            public List<Price2> price2 { get; set; }
            public List<Price3> price3 { get; set; }
            public List<Price4> price4 { get; set; }
            public List<Price5> price5 { get; set; }
        }

        public class Options
        {
            public int siteId { get; set; }
            public int siteType { get; set; }
            public int marketPlace { get; set; }
            public string description { get; set; }
            public string userName { get; set; }
            public string password { get; set; }
            public string apiKey { get; set; }
            public string secretKey { get; set; }
            public string merchantCode { get; set; }
            public string baseUrl { get; set; }
            public int orderMethod { get; set; }
            public string orderUrl { get; set; }
            public int tokenMethod { get; set; }
            public string tokenUrl { get; set; }
            public string tokenBody { get; set; }
            public string billedUrl { get; set; }
        }

        public class Payment
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int amount { get; set; }
            public long posDocumentId { get; set; }
            public string paymentCode { get; set; }
            public string paymentName { get; set; }
            public bool isChangeAmount { get; set; }
            public string installmentCount { get; set; }
            public int creditCardBatchNo { get; set; }
            public int creditCardStanNo { get; set; }
            public string creditCardTerminalId { get; set; }
            public string creditCardNo { get; set; }
            public int creditCardAcquirerId { get; set; }
            public string rrn { get; set; }
            public string currneysTypeCode { get; set; }
            public int exchangeAmount { get; set; }
            public string creditCardAuthorizationCode { get; set; }
            public int exchangeRate { get; set; }
            public string giftCardNo { get; set; }
            public string giftCardType { get; set; }
            public string refundVoucherdNo { get; set; }
            public string securityCode { get; set; }
            public string provisionId { get; set; }
            public IstanbulKartInfo istanbulKartInfo { get; set; }
            public string slip { get; set; }
            public CreditCardDetails creditCardDetails { get; set; }
        }

        public class Payment2
        {
            public bool tomPayment { get; set; }
            public bool istanbulCardLoad { get; set; }
            public bool istanbulCardLoadCancel { get; set; }
            public bool istanbulCardDayEnd { get; set; }
        }

        public class PaymentBanknote
        {
            public int id { get; set; }
            public int paymentCurrneysTypeId { get; set; }
            public int banknote { get; set; }
            public string paymentCurrneysTypes { get; set; }
        }

        public class PaymentCancel
        {
            public int authority { get; set; }
            public int limit { get; set; }
        }

        public class PaymentCurrenysTypes
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int exchangeRate { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> paymentTypes { get; set; }
            public List<PaymentBanknote> paymentBanknotes { get; set; }
            public List<string> products { get; set; }
        }

        public class PaymentCurrneysTypes
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int exchangeRate { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> paymentTypes { get; set; }
            public List<PaymentBanknote> paymentBanknotes { get; set; }
            public List<string> products { get; set; }
        }

        public class PaymentGroups
        {
            public int id { get; set; }
            public string name { get; set; }
            public string colorCode { get; set; }
            public int sequence { get; set; }
            public int eftPosGroupId { get; set; }
            public int eftPosBonusOption { get; set; }
            public int slipOption { get; set; }
            public bool isUsableWithLunchVoucher { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> paymentTypes { get; set; }
        }

        public class PaymentStatus
        {
            public int id { get; set; }
            public string name { get; set; }
            public List<string> paymentTypes { get; set; }
        }

        public class PaymentTypes
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public string targetProvider { get; set; }
            public int paymentStatusId { get; set; }
            public int paymentGroupsId { get; set; }
            public int paymentCurrenysTypesId { get; set; }
            public int lowerLimit { get; set; }
            public int upperLimit { get; set; }
            public int isRefundable { get; set; }
            public int isSalable { get; set; }
            public bool isOverRefundAllowed { get; set; }
            public int isOpensCashDrawer { get; set; }
            public string colorCode { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public bool isDeleted { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public int banksId { get; set; }
            public int buttonType { get; set; }
            public int sequence { get; set; }
            public bool workWithOneButton { get; set; }
            public bool isUsableWithLunchVoucher { get; set; }
            public bool isUsableWithGiftVoucher { get; set; }
            public bool isUsableWithProducts { get; set; }
            public int banknoteDelivery { get; set; }
            public bool isRefundableToBank { get; set; }
            public bool isActive { get; set; }
            public bool employeeRestrictionsEnabled { get; set; }
            public Details details { get; set; }
            public PaymentCurrenysTypes paymentCurrenysTypes { get; set; }
            public PaymentGroups paymentGroups { get; set; }
            public PaymentStatus paymentStatus { get; set; }
            public Banks banks { get; set; }
            public List<CampaignPayment> campaignPayments { get; set; }
            public List<string> ecommerces { get; set; }
            public List<PickupLoanLine> pickupLoanLines { get; set; }
            public List<PublishedCampaignPayment> publishedCampaignPayments { get; set; }
            public List<string> salesPayments { get; set; }
        }

        public class PickupLoan
        {
            public int id { get; set; }
            public long posDocumentId { get; set; }
            public string documentNo { get; set; }
            public DateTime date { get; set; }
            public int type { get; set; }
            public bool isValid { get; set; }
            public int storeId { get; set; }
            public int posId { get; set; }
            public int userId { get; set; }
            public int totalCash { get; set; }
            public int total { get; set; }
            public string stores { get; set; }
            public string pos { get; set; }
            public string users { get; set; }
            public List<Line> lines { get; set; }
        }

        public class PickupLoanLine
        {
            public int id { get; set; }
            public int pickupId { get; set; }
            public int paymentId { get; set; }
            public int quantity { get; set; }
            public int banknote { get; set; }
            public int totalAmount { get; set; }
            public int exchangeRate { get; set; }
            public string pickupLoan { get; set; }
            public string paymentTypes { get; set; }
        }

        public class Po
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int posStatusId { get; set; }
            public int storeId { get; set; }
            public int posGroupId { get; set; }
            public string serialNumber { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public bool isDeleted { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime lastPingDate { get; set; }
            public DateTime lastMetaDataDate { get; set; }
            public DateTime systemMetaDataDate { get; set; }
            public DateTime lastCampaignDate { get; set; }
            public DateTime systemCampaignDate { get; set; }
            public string version { get; set; }
            public DateTime lastProductDate { get; set; }
            public DateTime lastBulkProductDate { get; set; }
            public DateTime storeProductDate { get; set; }
            public bool isAllProductsChanged { get; set; }
            public int logLevel { get; set; }
            public string notes { get; set; }
            public int registerStatus { get; set; }
            public DateTime registerDate { get; set; }
            public int appAddressNo { get; set; }
            public string backendIpAddress { get; set; }
            public int systemMinCashRegisterVersion { get; set; }
            public Details details { get; set; }
            public bool isActive { get; set; }
            public AdditionalInfo additionalInfo { get; set; }
            public int cashierId { get; set; }
            public int hqProductCount { get; set; }
            public int hqBarcodeCount { get; set; }
            public int productCount { get; set; }
            public int barcodeCount { get; set; }
            public string catalogTransferInfo { get; set; }
            public bool shouldSendCatalogToHq { get; set; }
            public PosGroup posGroup { get; set; }
            public PosStatus posStatus { get; set; }
            public string store { get; set; }
            public string user { get; set; }
            public List<Audit> audits { get; set; }
            public List<CancelledSale> cancelledSales { get; set; }
            public List<Closure> closures { get; set; }
            public List<GiftChangeReceipt> giftChangeReceipts { get; set; }
            public List<PickupLoan> pickupLoans { get; set; }
            public List<PosPing> posPings { get; set; }
            public List<string> sales { get; set; }
            public List<BillPaying> billPayings { get; set; }
        }

        public class PortSettings
        {
            public int id { get; set; }
            public int typeId { get; set; }
            public string name { get; set; }
            public string command { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> portSettings { get; set; }
            public List<string> scaleSettings { get; set; }
        }

        public class PosGroup
        {
            public int id { get; set; }
            public string name { get; set; }
            public int posSettingsId { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public PosSettings posSettings { get; set; }
            public List<string> pos { get; set; }
        }

        public class PosList
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int posStatusId { get; set; }
            public int storeId { get; set; }
            public int posGroupId { get; set; }
            public string serialNumber { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public bool isDeleted { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime lastPingDate { get; set; }
            public DateTime lastMetaDataDate { get; set; }
            public DateTime systemMetaDataDate { get; set; }
            public DateTime lastCampaignDate { get; set; }
            public DateTime systemCampaignDate { get; set; }
            public string version { get; set; }
            public DateTime lastProductDate { get; set; }
            public DateTime lastBulkProductDate { get; set; }
            public DateTime storeProductDate { get; set; }
            public bool isAllProductsChanged { get; set; }
            public int logLevel { get; set; }
            public string notes { get; set; }
            public int registerStatus { get; set; }
            public DateTime registerDate { get; set; }
            public int appAddressNo { get; set; }
            public string backendIpAddress { get; set; }
            public int systemMinCashRegisterVersion { get; set; }
            public Details details { get; set; }
            public bool isActive { get; set; }
            public AdditionalInfo additionalInfo { get; set; }
            public int cashierId { get; set; }
            public int hqProductCount { get; set; }
            public int hqBarcodeCount { get; set; }
            public int productCount { get; set; }
            public int barcodeCount { get; set; }
            public string catalogTransferInfo { get; set; }
            public bool shouldSendCatalogToHq { get; set; }
            public PosGroup posGroup { get; set; }
            public PosStatus posStatus { get; set; }
            public string store { get; set; }
            public string user { get; set; }
            public List<Audit> audits { get; set; }
            public List<CancelledSale> cancelledSales { get; set; }
            public List<Closure> closures { get; set; }
            public List<GiftChangeReceipt> giftChangeReceipts { get; set; }
            public List<PickupLoan> pickupLoans { get; set; }
            public List<PosPing> posPings { get; set; }
            public List<string> sales { get; set; }
            public List<BillPaying> billPayings { get; set; }
        }

        public class PosPing
        {
            public int id { get; set; }
            public int storesId { get; set; }
            public int posId { get; set; }
            public int usersId { get; set; }
            public DateTime date { get; set; }
            public string stores { get; set; }
            public string pos { get; set; }
        }

        public class PosSettings
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool isAllowToRefund { get; set; }
            public bool isAllowedToSale { get; set; }
            public bool isAllowedToInvoices { get; set; }
            public bool isAllowedToDiplomaticSale { get; set; }
            public bool isAllowedToWaybill { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public bool isDeleted { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public int refundReasonType { get; set; }
            public int quickProductSetsId { get; set; }
            public int posSalesmanSetting { get; set; }
            public int cashDrawerStatus { get; set; }
            public int portSettingsId { get; set; }
            public int scaleSettingsId { get; set; }
            public int suspendTimeout { get; set; }
            public int logoutTimeout { get; set; }
            public int documentCompletionThreshold { get; set; }
            public bool showPossibleCampaigns { get; set; }
            public bool allowRefundProductPickFromList { get; set; }
            public int showSuspendDocLatestNdays { get; set; }
            public string peripherals { get; set; }
            public int pingInterval { get; set; }
            public int language { get; set; }
            public int keyboardDefinitionsId { get; set; }
            public bool requestAuthorizationForBatchSelling { get; set; }
            public bool readSalesmanFromOnlineReceipt { get; set; }
            public bool printProductBarcode { get; set; }
            public bool addFromProductQuery { get; set; }
            public int pullChangedProductsViaCashierConfirm { get; set; }
            public int cashRegisterProductBatchSize { get; set; }
            public int cafeMode { get; set; }
            public bool canQueryPriceWithoutLogin { get; set; }
            public bool runCampaignsAfterEveryProduct { get; set; }
            public int discountReasonOption { get; set; }
            public int priceChangeReasonOption { get; set; }
            public Details details { get; set; }
            public List<int> excludedPayments { get; set; }
            public PortSettings portSettings { get; set; }
            public ScaleSettings scaleSettings { get; set; }
            public List<string> posGroups { get; set; }
            public QuickProductSets quickProductSets { get; set; }
            public KeyboardDefinitions keyboardDefinitions { get; set; }
        }

        public class Details
        {
            public string description { get; set; }
            public string companyName { get; set; }
            public string invoiceNo { get; set; }
            public int invoiceAmount { get; set; }
            public int feeAmount { get; set; }
            public int totalAmount { get; set; }
            public DateTime paymentDate { get; set; }
            public DateTime invoiceDate { get; set; }
            public string subscriberName { get; set; }
            public string subscriberNumber { get; set; }
            public GiftCard giftCard { get; set; }
            public Ibb ibb { get; set; }
            public Donation donation { get; set; }
        }

        public class Root
        {
            public Details details { get; set; }
        }

        public class PosStatus
        {
            public int id { get; set; }
            public string name { get; set; }
            public List<string> pos { get; set; }
        }

        public class Price1
        {
            public DateTime date { get; set; }
            public int price { get; set; }
        }

        public class Price2
        {
            public DateTime date { get; set; }
            public int price { get; set; }
        }

        public class Price3
        {
            public DateTime date { get; set; }
            public int price { get; set; }
        }

        public class Price4
        {
            public DateTime date { get; set; }
            public int price { get; set; }
        }

        public class Price5
        {
            public DateTime date { get; set; }
            public int price { get; set; }
        }

        public class ProductAdditionalSalesInformation
        {
            public int id { get; set; }
            public int additionalSalesInformationsId { get; set; }
            public int productsId { get; set; }
            public string products { get; set; }
            public AdditionalSalesInformations additionalSalesInformations { get; set; }
        }

        public class ProductAdditionalSalesMessage
        {
            public int id { get; set; }
            public int additionalSalesMessagesId { get; set; }
            public int productsId { get; set; }
            public string products { get; set; }
            public AdditionalSalesMessages additionalSalesMessages { get; set; }
        }

        public class ProductAttribute
        {
            public int id { get; set; }
            public int productsId { get; set; }
            public string shelfLife { get; set; }
            public string storageConditions { get; set; }
            public string ingredients { get; set; }
            public string code1 { get; set; }
            public string code2 { get; set; }
            public string code3 { get; set; }
            public string code4 { get; set; }
            public string code5 { get; set; }
            public string brandCode { get; set; }
            public string categoryCode { get; set; }
            public string genericCode { get; set; }
            public string supplierCode { get; set; }
            public string products { get; set; }
        }

        public class ProductBrand
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public List<string> products { get; set; }
        }

        public class ProductCategory
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int parentId { get; set; }
            public List<string> products { get; set; }
        }

        public class ProductCode1
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public bool isDefault { get; set; }
            public List<string> products { get; set; }
        }

        public class ProductCode2
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public bool isDefault { get; set; }
            public List<string> products { get; set; }
        }

        public class ProductCode3
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public bool isDefault { get; set; }
            public List<string> products { get; set; }
        }

        public class ProductCode4
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public bool isDefault { get; set; }
            public List<string> products { get; set; }
        }

        public class ProductCode5
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public bool isDefault { get; set; }
            public List<string> products { get; set; }
        }

        public class ProductGeneric
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public List<string> products { get; set; }
        }

        public class Products
        {
            public int id { get; set; }
            public string code { get; set; }
            public string shortenedName { get; set; }
            public string name { get; set; }
            public bool isActive { get; set; }
            public int productCode1Id { get; set; }
            public int productCode2Id { get; set; }
            public int productCode3Id { get; set; }
            public int productCode4Id { get; set; }
            public int productCode5Id { get; set; }
            public int categoryId { get; set; }
            public int brandId { get; set; }
            public int genericId { get; set; }
            public int supplierId { get; set; }
            public int paymentCurrneysTypesId { get; set; }
            public int salesStaffMessageType { get; set; }
            public string unit { get; set; }
            public int vatPercent { get; set; }
            public int vatId { get; set; }
            public int buyingVatPercent { get; set; }
            public int buyingVatId { get; set; }
            public int integrationId { get; set; }
            public int salesmanSetting { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public bool isDeleted { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public Details details { get; set; }
            public int isLunchVoucher { get; set; }
            public int priceType { get; set; }
            public int returnType { get; set; }
            public int quantityType { get; set; }
            public int discountType { get; set; }
            public int scaleType { get; set; }
            public int salesInstallmentType { get; set; }
            public bool isGivesBonus { get; set; }
            public int bonusMultiplier { get; set; }
            public int installmentNumber { get; set; }
            public string description { get; set; }
            public int productType { get; set; }
            public int maxQuantity { get; set; }
            public ProductCode1 productCode1 { get; set; }
            public ProductCode2 productCode2 { get; set; }
            public ProductCode3 productCode3 { get; set; }
            public ProductCode4 productCode4 { get; set; }
            public ProductCode5 productCode5 { get; set; }
            public ProductBrand productBrand { get; set; }
            public ProductGeneric productGeneric { get; set; }
            public ProductCategory productCategory { get; set; }
            public ProductSupplier productSupplier { get; set; }
            public PaymentCurrneysTypes paymentCurrneysTypes { get; set; }
            public List<string> storePrice { get; set; }
            public List<StoreGroupPrice> storeGroupPrice { get; set; }
            public List<QuickProduct> quickProducts { get; set; }
            public List<ProductAdditionalSalesInformation> productAdditionalSalesInformations { get; set; }
            public List<ProductAdditionalSalesMessage> productAdditionalSalesMessages { get; set; }
            public List<Barcode> barcodes { get; set; }
            public List<ProductVat> productVats { get; set; }
            public List<ProductAttribute> productAttributes { get; set; }
            public List<CancelledSalesProduct> cancelledSalesProducts { get; set; }
            public List<GiftChangeReceiptDetail> giftChangeReceiptDetails { get; set; }
            public List<string> salesProducts { get; set; }
        }

        public class ProductSupplier
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public List<string> products { get; set; }
        }

        public class ProductVat
        {
            public int id { get; set; }
            public int productsId { get; set; }
            public int countryId { get; set; }
            public int vatPercent { get; set; }
            public Countries countries { get; set; }
            public string products { get; set; }
        }

        public class PublishedCampaign
        {
            public int id { get; set; }
            public int campaignId { get; set; }
            public DateTime publishDate { get; set; }
            public int version { get; set; }
            public List<int> modification { get; set; }
            public string name { get; set; }
            public DateTime beginDate { get; set; }
            public DateTime endDate { get; set; }
            public int beginHour { get; set; }
            public int endHour { get; set; }
            public bool isForCustomersOnly { get; set; }
            public bool validForAllStores { get; set; }
            public int campaignTypeId { get; set; }
            public int conditionAmount { get; set; }
            public int conditionQuantity { get; set; }
            public int mainConditionType { get; set; }
            public int mainExcConditionType { get; set; }
            public int mainDiscountType { get; set; }
            public bool isActive { get; set; }
            public string b2CCampaignId { get; set; }
            public int defaultApplicationType { get; set; }
            public int buyNPayN_1ConditionNum { get; set; }
            public int buyNPayN_1PayNum { get; set; }
            public bool day1 { get; set; }
            public bool day2 { get; set; }
            public bool day3 { get; set; }
            public bool day4 { get; set; }
            public bool day5 { get; set; }
            public bool day6 { get; set; }
            public bool day7 { get; set; }
            public int sequence { get; set; }
            public int setId { get; set; }
            public int groupConditionType { get; set; }
            public int groupConditionId { get; set; }
            public int groupConditionValue { get; set; }
            public int spendType { get; set; }
            public int giftCardType { get; set; }
            public bool isDynamicMixMatch { get; set; }
            public bool isFoldingConditionAmountEnabled { get; set; }
            public bool isDigitalStoreCard { get; set; }
            public int executionType { get; set; }
            public int executionValue { get; set; }
            public int discountToDocumentCondition { get; set; }
            public int discountToDocumentValue { get; set; }
            public bool distributeDiscountToAllProducts { get; set; }
            public bool requiresCouponsToRun { get; set; }
            public bool isWinningCampaign { get; set; }
            public string winningCampaignList { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public string campaign { get; set; }
            public List<PublishedCampaignPayment> publishedCampaignPayments { get; set; }
            public List<PublishedCampaignProduct> publishedCampaignProducts { get; set; }
            public List<PublishedCampaignSegment> publishedCampaignSegments { get; set; }
            public List<string> publishedCampaignStores { get; set; }
        }

        public class PublishedCampaign2
        {
            public int id { get; set; }
            public int campaignId { get; set; }
            public DateTime publishDate { get; set; }
            public int version { get; set; }
            public List<int> modification { get; set; }
            public string name { get; set; }
            public DateTime beginDate { get; set; }
            public DateTime endDate { get; set; }
            public int beginHour { get; set; }
            public int endHour { get; set; }
            public bool isForCustomersOnly { get; set; }
            public bool validForAllStores { get; set; }
            public int campaignTypeId { get; set; }
            public int conditionAmount { get; set; }
            public int conditionQuantity { get; set; }
            public int mainConditionType { get; set; }
            public int mainExcConditionType { get; set; }
            public int mainDiscountType { get; set; }
            public bool isActive { get; set; }
            public string b2CCampaignId { get; set; }
            public int defaultApplicationType { get; set; }
            public int buyNPayN_1ConditionNum { get; set; }
            public int buyNPayN_1PayNum { get; set; }
            public bool day1 { get; set; }
            public bool day2 { get; set; }
            public bool day3 { get; set; }
            public bool day4 { get; set; }
            public bool day5 { get; set; }
            public bool day6 { get; set; }
            public bool day7 { get; set; }
            public int sequence { get; set; }
            public int setId { get; set; }
            public int groupConditionType { get; set; }
            public int groupConditionId { get; set; }
            public int groupConditionValue { get; set; }
            public int spendType { get; set; }
            public int giftCardType { get; set; }
            public bool isDynamicMixMatch { get; set; }
            public bool isFoldingConditionAmountEnabled { get; set; }
            public bool isDigitalStoreCard { get; set; }
            public int executionType { get; set; }
            public int executionValue { get; set; }
            public int discountToDocumentCondition { get; set; }
            public int discountToDocumentValue { get; set; }
            public bool distributeDiscountToAllProducts { get; set; }
            public bool requiresCouponsToRun { get; set; }
            public bool isWinningCampaign { get; set; }
            public string winningCampaignList { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public string campaign { get; set; }
            public List<PublishedCampaignPayment> publishedCampaignPayments { get; set; }
            public List<PublishedCampaignProduct> publishedCampaignProducts { get; set; }
            public List<PublishedCampaignSegment> publishedCampaignSegments { get; set; }
            public List<string> publishedCampaignStores { get; set; }
        }

        public class PublishedCampaignPayment
        {
            public int id { get; set; }
            public int campaignId { get; set; }
            public int paymentTypesId { get; set; }
            public int installmentLimit { get; set; }
            public string paymentMethod { get; set; }
            public string paymentTypes { get; set; }
            public string publishedCampaign { get; set; }
        }

        public class PublishedCampaignProduct
        {
            public int id { get; set; }
            public int campaignId { get; set; }
            public int conditionId { get; set; }
            public int conditionType { get; set; }
            public int conditionProductQuantity { get; set; }
            public int discountType { get; set; }
            public int discountTypeId { get; set; }
            public int applicationType { get; set; }
            public int applicationValue { get; set; }
            public string publishedCampaign { get; set; }
        }

        public class PublishedCampaignSegment
        {
            public int id { get; set; }
            public int campaignId { get; set; }
            public int segmentId { get; set; }
            public string publishedCampaign { get; set; }
        }

        public class PublishedCampaignStore
        {
            public int id { get; set; }
            public int campaignId { get; set; }
            public int storesId { get; set; }
            public string stores { get; set; }
            public PublishedCampaign publishedCampaign { get; set; }
        }

        public class QuickProduct
        {
            public int id { get; set; }
            public int productsId { get; set; }
            public string keyName { get; set; }
            public int sequence { get; set; }
            public string colorCode { get; set; }
            public int quickProductSubGroupsId { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public string products { get; set; }
            public QuickProductSubGroups quickProductSubGroups { get; set; }
        }

        public class QuickProductMainGroup
        {
            public int id { get; set; }
            public string name { get; set; }
            public int sequence { get; set; }
            public string colorCode { get; set; }
            public int quickProductSetsId { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public string quickProductSets { get; set; }
            public List<string> quickProductSubGroups { get; set; }
        }

        public class QuickProductSets
        {
            public int id { get; set; }
            public string name { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<QuickProductMainGroup> quickProductMainGroups { get; set; }
            public List<string> posSettings { get; set; }
        }

        public class QuickProductSubGroups
        {
            public int id { get; set; }
            public string name { get; set; }
            public int sequence { get; set; }
            public string colorCode { get; set; }
            public int quickProductMainGroupsId { get; set; }
            public string productCodesToGroup { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public QuickProductMainGroups quickProductMainGroups { get; set; }
            public List<string> quickProducts { get; set; }
        }

        public class QuickProductMainGroups
        {
            public int id { get; set; }
            public string name { get; set; }
            public int sequence { get; set; }
            public string colorCode { get; set; }
            public int quickProductSetsId { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public string quickProductSets { get; set; }
            public List<string> quickProductSubGroups { get; set; }
        }

        public class Region
        {
            public int id { get; set; }
            public string name { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> citys { get; set; }
        }

        public class Report
        {
            public int canQueryReport { get; set; }
            public bool salesmanCanTakeHisOwnReport { get; set; }
            public bool reportX { get; set; }
            public bool reportZ { get; set; }
            public bool reportCashRegister { get; set; }
            public bool fiscalReport { get; set; }
            public bool ekuReport { get; set; }
            public bool report0 { get; set; }
            public bool report1 { get; set; }
            public bool report2 { get; set; }
            public bool report3 { get; set; }
            public bool report4 { get; set; }
            public bool report5 { get; set; }
            public bool report6 { get; set; }
            public bool report7 { get; set; }
            public bool report8 { get; set; }
            public bool report9 { get; set; }
        }


        public class Sales
        {
            public int id { get; set; }
            public long posDocumentId { get; set; }
            public int documentsTypeId { get; set; }
            public string receiptNo { get; set; }
            public string documentNo { get; set; }
            public int storesId { get; set; }
            public DateTime date { get; set; }
            public DateTime startDate { get; set; }
            public int usersId { get; set; }
            public int posId { get; set; }
            public int customersId { get; set; }
            public string customerCardNo { get; set; }
            public int invoiceType { get; set; }
            public int salesType { get; set; }
            public string taxNumber { get; set; }
            public string posVersion { get; set; }
            public int refundReasonId { get; set; }
            public int transferStatus { get; set; }
            public int transferBatchId { get; set; }
            public string closureNo { get; set; }
            public string linkedDocumentNo { get; set; }
            public int linkedDocumentId { get; set; }
            public CustomerData customerData { get; set; }
            public int priceId { get; set; }
            public int exchangeRate { get; set; }
            public Details details { get; set; }
            public List<Audit> audits { get; set; }
            public List<Job> jobs { get; set; }
            public DateTime createDate { get; set; }
            public int lineCount { get; set; }
            public int cancelledLineCount { get; set; }
            public int totalAmount { get; set; }
            public int vatTotal { get; set; }
            public int discountTotal { get; set; }
            public int grossTotal { get; set; }
            public int netTotal { get; set; }
            public Stores stores { get; set; }
            public Users users { get; set; }
            public Documents documents { get; set; }
            public Pos pos { get; set; }
            public List<SalesProduct> salesProducts { get; set; }
            public List<SalesPayment> salesPayments { get; set; }
            public List<SalesBonu> salesBonus { get; set; }
            public List<SalesProductCampaign> salesProductCampaigns { get; set; }
            public string taxPayer { get; set; }
            public bool skipSubTotalsPage { get; set; }
            public bool sellMultiplePieceAtOneTime { get; set; }
            public bool printGiftChangeSlip { get; set; }
            public int storeManagerProductDiscountLimit { get; set; }
            public int storeManagerDocumentDiscountLimit { get; set; }
            public bool enterPriceOnRefund { get; set; }
            public bool enterPriceForScales { get; set; }
            public bool useBonusDiscount { get; set; }
            public bool getBatchRefund { get; set; }
            public bool queryEcommerceOrders { get; set; }
            public bool switchPrices { get; set; }
            public bool createCustomer { get; set; }
        }

        public class Pos
        {

            public string stringg { get; set; }
        }

        public class SalesBonu
        {
            public int id { get; set; }
            public int salesId { get; set; }
            public int lineId { get; set; }
            public string provisionId { get; set; }
            public int bonusId { get; set; }
            public int amount { get; set; }
            public int state { get; set; }
            public string errorMessage { get; set; }
            public int transactionType { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public DateTime activationDate { get; set; }
            public DateTime expireDate { get; set; }
            public string sales { get; set; }
        }

        public class SalesPayment
        {
            public int id { get; set; }
            public int salesId { get; set; }
            public long posDocumentId { get; set; }
            public int paymentTypesId { get; set; }
            public int amount { get; set; }
            public int exchangeAmount { get; set; }
            public int isChangeAmount { get; set; }
            public int creditCardBatchNo { get; set; }
            public int creditCardStanNo { get; set; }
            public string creditCardTerminalId { get; set; }
            public string creditCardInstallmentCount { get; set; }
            public string creditCardNo { get; set; }
            public int creditCardAcquirerId { get; set; }
            public Details details { get; set; }
            public Info info { get; set; }
            public string sales { get; set; }
            public PaymentTypes paymentTypes { get; set; }
        }

        public class SalesProduct
        {
            public int id { get; set; }
            public int salesId { get; set; }
            public int sequence { get; set; }
            public long posDocumentId { get; set; }
            public int productsId { get; set; }
            public int vatPercent { get; set; }
            public int vatId { get; set; }
            public int amount { get; set; }
            public decimal TotalPrice { get; set; }
            public int taxableTotal { get; set; }
            public int vatTotal { get; set; }
            public bool isValid { get; set; }
            public int discountTotalDirect { get; set; }
            public int discountTotalIndirect { get; set; }
            public int discountTotalCampaign { get; set; }
            public string barcodeNo { get; set; }
            public int salesmanId { get; set; }
            public string description { get; set; }
            public int returnAmount { get; set; }
            public bool isPriceEnteredByUser { get; set; }
            public int refundReasonId { get; set; }
            public int priceChangeReasonId { get; set; }
            public Products products { get; set; }
            public string sales { get; set; }
        }

        public class SalesProductCampaign
        {
            public int id { get; set; }
            public int salesId { get; set; }
            public int productSequence { get; set; }
            public int campaignId { get; set; }
            public int campaignVersion { get; set; }
            public string campaignCode { get; set; }
            public string campaignName { get; set; }
            public string couponNo { get; set; }
            public int totalDiscount { get; set; }
            public int distributedAmount { get; set; }
            public int distributedDiscountAmount { get; set; }
            public bool isOnline { get; set; }
            public int source { get; set; }
            public string sales { get; set; }
        }

        public class ScaleSettings
        {
            public int id { get; set; }
            public int typeId { get; set; }
            public string name { get; set; }
            public string command { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> portSettings { get; set; }
            public List<string> scaleSettings { get; set; }
        }

        public class SegmentList
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class ShelveDocument
        {
            public int authority { get; set; }
            public int limit { get; set; }
        }

        public class StoreGroup1
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> stores { get; set; }
            public List<StoreGroupPrice> storeGroupPrice { get; set; }
        }

        public class StoreGroup2
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> stores { get; set; }
        }

        public class StoreGroup3
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> stores { get; set; }
        }

        public class StoreGroupPrice
        {
            public int id { get; set; }
            public int storeGroup1Id { get; set; }
            public int productsId { get; set; }
            public int priceId { get; set; }
            public bool isActive { get; set; }
            public int price1 { get; set; }
            public int price2 { get; set; }
            public int price3 { get; set; }
            public int price4 { get; set; }
            public int price5 { get; set; }
            public NextPrices nextPrices { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public string products { get; set; }
            public string storeGroup1 { get; set; }
        }

        public class StorePrice
        {
            public int id { get; set; }
            public int storesId { get; set; }
            public int productsId { get; set; }
            public int priceId { get; set; }
            public bool isActive { get; set; }
            public bool isChanged { get; set; }
            public int price1 { get; set; }
            public int price2 { get; set; }
            public int price3 { get; set; }
            public int price4 { get; set; }
            public int price5 { get; set; }
            public int maxQuantity { get; set; }
            public bool isOnSale { get; set; }
            public NextPrices nextPrices { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public Products products { get; set; }
            public string stores { get; set; }
        }

        public class StoreReceiptFooter
        {
            public int id { get; set; }
            public int storeId { get; set; }
            public int sequence { get; set; }
            public string name { get; set; }
            public string store { get; set; }
        }

        public class StoreReceiptHeader
        {
            public int id { get; set; }
            public int storeId { get; set; }
            public int sequence { get; set; }
            public string name { get; set; }
            public string store { get; set; }
        }

        public class Stores
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int storeStatusId { get; set; }
            public int storeGroup1Id { get; set; }
            public int storeGroup2Id { get; set; }
            public int storeGroup3Id { get; set; }
            public int storeSettingGroupId { get; set; }
            public string postalCode { get; set; }
            public int districtId { get; set; }
            public int countryId { get; set; }
            public string districtCode { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public bool isDeleted { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public string suspendDirectory { get; set; }
            public string backupDirectory { get; set; }
            public int defaultSalesmanId { get; set; }
            public int multiTypeSalesInstallmentType { get; set; }
            public string receiptHeader1 { get; set; }
            public string receiptHeader2 { get; set; }
            public string receiptHeader3 { get; set; }
            public string receiptHeader4 { get; set; }
            public string receiptHeader5 { get; set; }
            public string receiptHeader6 { get; set; }
            public string receiptHeader7 { get; set; }
            public string receiptHeader8 { get; set; }
            public string receiptHeader9 { get; set; }
            public string receiptHeader10 { get; set; }
            public string receiptFooter1 { get; set; }
            public string receiptFooter2 { get; set; }
            public string receiptFooter3 { get; set; }
            public string receiptFooter4 { get; set; }
            public string receiptFooter5 { get; set; }
            public Details details { get; set; }
            public int negativeStockStatus { get; set; }
            public District district { get; set; }
            public StoreGroup1 storeGroup1 { get; set; }
            public StoreGroup2 storeGroup2 { get; set; }
            public StoreGroup3 storeGroup3 { get; set; }
            public StoreSettingGroup storeSettingGroup { get; set; }
            public StoreStatus storeStatus { get; set; }
            public List<Po> pos { get; set; }
            public List<StoreReceiptFooter> storeReceiptFooters { get; set; }
            public List<StoreReceiptHeader> storeReceiptHeaders { get; set; }
            public List<User> users { get; set; }
            public List<StorePrice> storePrice { get; set; }
            public List<CampaignStore> campaignStores { get; set; }
            public List<Audit> audits { get; set; }
            public List<CancelledSale> cancelledSales { get; set; }
            public List<Closure> closures { get; set; }
            public List<GiftChangeReceipt> giftChangeReceipts { get; set; }
            public List<PickupLoan> pickupLoans { get; set; }
            public List<PosPing> posPings { get; set; }
            public List<PublishedCampaignStore> publishedCampaignStores { get; set; }
            public List<string> sales { get; set; }
            public List<BillPaying> billPayings { get; set; }
            public List<WeighingScale> weighingScales { get; set; }
        }

        public class StoreSettingDocument
        {
            public int id { get; set; }
            public int storeSettingsId { get; set; }
            public int documentsId { get; set; }
            public int askCustomerOption { get; set; }
            public int displaySettingsId { get; set; }
            public int askCustomerThreshold { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public StoreSettings storeSettings { get; set; }
            public string documents { get; set; }
            public DisplaySettings displaySettings { get; set; }
        }

        public class StoreSettingGroup
        {
            public int id { get; set; }
            public string name { get; set; }
            public int storeSettingsId { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public StoreSettings storeSettings { get; set; }
            public List<string> stores { get; set; }
        }

        public class StoreSettings
        {
            public int id { get; set; }
            public string name { get; set; }
            public int defaultUserStatusId { get; set; }
            public int minimumAmount { get; set; }
            public int maximumAmount { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public bool isDeleted { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public bool isAskForCustomerBeforeSale { get; set; }
            public bool isSingleReceiptOnRefunds { get; set; }
            public int exhangeVoucherOption { get; set; }
            public int postProcessReportX { get; set; }
            public int postProcessReportZ { get; set; }
            public int postProcessCashUp { get; set; }
            public CrmApiDetails crmApiDetails { get; set; }
            public string discountSummaryDescription { get; set; }
            public int compoundBarcodeLength { get; set; }
            public InvoiceHeaders invoiceHeaders { get; set; }
            public bool acceptSingleGiftCardPayment { get; set; }
            public bool acceptMultiGiftCardSale { get; set; }
            public int multiGiftCardSaleLimit { get; set; }
            public bool doNotPrintCancelledLines { get; set; }
            public int cashLevel1 { get; set; }
            public int cashLevel2 { get; set; }
            public int cashLevelMaximum { get; set; }
            public bool onlyPasswordForAuthorizedUserInfo { get; set; }
            public bool allowReturnsFromDifferentStore { get; set; }
            public bool canReceiveReturnsFromDifferentStore { get; set; }
            public bool ignoreProductQuantityLimitIfCustomerSelected { get; set; }
            public Details details { get; set; }
            public List<string> storeSettingGroups { get; set; }
            public List<string> storeSettingDocuments { get; set; }
        }

        public class StoreStatus
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool isActive { get; set; }
            public List<string> stores { get; set; }
        }

        public class TaxPayer
        {
            public int id { get; set; }
            public string taxNumber { get; set; }
            public string taxOffice { get; set; }
            public string name { get; set; }
            public string address { get; set; }
            public int salesId { get; set; }
            public string passportNo { get; set; }
            public string residence { get; set; }
            public string passportCountry { get; set; }
            public bool isEInvoiceRegistered { get; set; }
            public Sales sales { get; set; }
        }

        public class UnshelveDocument
        {
            public int authority { get; set; }
            public int limit { get; set; }
        }

        public class User
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public string surName { get; set; }
            public string fullName { get; set; }
            public string nameOnReceipt { get; set; }
            public int userStatusId { get; set; }
            public int userRolesId { get; set; }
            public int userDepartmentsId { get; set; }
            public int storeId { get; set; }
            public bool isAdmin { get; set; }
            public bool isActive { get; set; }
            public string adminPassword { get; set; }
            public string password { get; set; }
            public string smsCode { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public bool isDeleted { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public string mobilePhone { get; set; }
            public string email { get; set; }
            public string identityNo { get; set; }
            public string employeeNo { get; set; }
            public string store { get; set; }
            public UserDepartments userDepartments { get; set; }
            public UserRoles userRoles { get; set; }
            public UserStatus userStatus { get; set; }
            public List<Audit> audits { get; set; }
            public List<CancelledSale> cancelledSales { get; set; }
            public List<GiftChangeReceipt> giftChangeReceipts { get; set; }
            public List<PickupLoan> pickupLoans { get; set; }
            public List<string> sales { get; set; }
            public List<BillPaying> billPayings { get; set; }
            public List<PosList> posList { get; set; }
        }

        public class UserDepartments
        {
            public int id { get; set; }
            public string name { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> users { get; set; }
        }

        public class UserRoleDocument
        {
            public int id { get; set; }
            public int userRolesId { get; set; }
            public int documentsId { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public string userRoles { get; set; }
            public string documents { get; set; }
        }

        public class UserRoles
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool isAdmin { get; set; }
            public bool showCashierTab { get; set; }
            public bool showStoreManagerTab { get; set; }
            public bool showSalesmanTab { get; set; }
            public bool isAllowedToOnlineRefund { get; set; }
            public bool isAllowedToTaxFreeSales { get; set; }
            public bool isAllowedToDiplomaticSales { get; set; }
            public bool skipSubTotalsPage { get; set; }
            public int isAllowedToRowCancellation { get; set; }
            public int rowCancellationLimit { get; set; }
            public int isAllowedToDocumentCancellation { get; set; }
            public int documentCancellationLimit { get; set; }
            public int isAllowedToSuspendDocument { get; set; }
            public int documentSuspendLimit { get; set; }
            public int isAllowedToCallSuspendedDocument { get; set; }
            public int suspendedDocumentCallLimit { get; set; }
            public int isAllowedToPriceQuery { get; set; }
            public int isAllowedToPriceOverride { get; set; }
            public int isAllowedToCancelReservation { get; set; }
            public bool isAllowedToPrintGiftChangeSlip { get; set; }
            public bool isAllowedToOpenCashDrawer { get; set; }
            public bool isAllowedToReportX { get; set; }
            public bool isAllowedToReportZ { get; set; }
            public int isAllowedToMakeDiscountToProduct { get; set; }
            public int productDiscountLimitPercent { get; set; }
            public int isAllowedToMakeDiscountToDocument { get; set; }
            public int documentDiscountLimitPercent { get; set; }
            public int isAllowedToMakeAmountDiscountToProduct { get; set; }
            public int productAmountDiscountLimitPercent { get; set; }
            public int isAllowedToMakeAmountDiscountToDocument { get; set; }
            public int documentAmountDiscountLimitPercent { get; set; }
            public int cashLevel1 { get; set; }
            public int cashLevel2 { get; set; }
            public int cashLevelMaximum { get; set; }
            public bool isAllowedToSellMultiplePieceAtOneTime { get; set; }
            public bool changePasswordOnFirstLogin { get; set; }
            public bool isAllowedToChangePassword { get; set; }
            public bool isAllowedToReport0 { get; set; }
            public bool isAllowedToReport1 { get; set; }
            public bool isAllowedToReport2 { get; set; }
            public bool isAllowedToReport3 { get; set; }
            public bool isAllowedToReport4 { get; set; }
            public bool isAllowedToReport5 { get; set; }
            public bool isAllowedToReport6 { get; set; }
            public bool isAllowedToReport7 { get; set; }
            public bool isAllowedToReport8 { get; set; }
            public bool isAllowedToReport9 { get; set; }
            public int storeManagerProductDiscountLimit { get; set; }
            public int storeManagerDocumentDiscountLimit { get; set; }
            public int bonusQuotaPercent1 { get; set; }
            public int bonusQuotaAmount1 { get; set; }
            public int bonusQuotaPercent2 { get; set; }
            public int bonusQuotaAmount2 { get; set; }
            public bool salesmanCanTakeHisOwnReport { get; set; }
            public int isAllowedToApplyDiscountByGiftCard { get; set; }
            public int discountByGiftCardLimit { get; set; }
            public bool isAllowedToEnterPriceInRefund { get; set; }
            public bool isAllowedToLockScreen { get; set; }
            public bool eftPosEndOfDay { get; set; }
            public bool eftPosUpdateParameter { get; set; }
            public bool eftPosFunction1 { get; set; }
            public bool eftPosFunction2 { get; set; }
            public bool eftPosFunction3 { get; set; }
            public bool eftPosFunction4 { get; set; }
            public bool eftPosFunction5 { get; set; }
            public bool isAllowedToEnterPriceForScales { get; set; }
            public bool isAllowedToPickup { get; set; }
            public bool isAllowedToLoan { get; set; }
            public bool fiscalReport { get; set; }
            public bool ekuReport { get; set; }
            public bool createCustomer { get; set; }
            public bool canQueryEcommerceOrders { get; set; }
            public bool canSwitchPrices { get; set; }
            public bool isAllowedToUseBonusDiscount { get; set; }
            public bool audit { get; set; }
            public bool correction { get; set; }
            public bool checkControlTape { get; set; }
            public bool rollBack { get; set; }
            public bool zReportCopy { get; set; }
            public int canQueryReport { get; set; }
            public bool canGetBatchRefund { get; set; }
            public bool tomPayment { get; set; }
            public bool istanbulCardLoad { get; set; }
            public bool istanbulCardLoadCancel { get; set; }
            public bool istanbulCardDayEnd { get; set; }
            public Details details { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> users { get; set; }
            public List<UserRoleDocument> userRoleDocuments { get; set; }
        }

        public class UserStatus
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool isActive { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public List<string> users { get; set; }
        }

        public class VatNumber
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int percentage { get; set; }
            public int countryId { get; set; }
            public int azVatOption { get; set; }
            public bool isDisabled { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public string countries { get; set; }
        }

        public class WeighingScale
        {
            public int id { get; set; }
            public string name { get; set; }
            public string serialNumber { get; set; }
            public string path { get; set; }
            public int type { get; set; }
            public int storeId { get; set; }
            public DateTime productDateOnScale { get; set; }
            public DateTime productDateOnStore { get; set; }
            public bool isActive { get; set; }
            public bool transferRequired { get; set; }
            public bool lastTransferStatus { get; set; }
            public string lastTransferDescription { get; set; }
            public AdditionalInfo additionalInfo { get; set; }
            public int createdBy { get; set; }
            public int updatedBy { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public string store { get; set; }
        }



    }
}
#endregion