using BmsMarkeRGeniusIntegrationLibrary.METHODS.MODELS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using static BmsMarkeRGeniusIntegrationLibrary.HELPER;

namespace BmsMarkeRGeniusIntegrationLibrary
{
    /// <summary>
    /// Logo Objects yerine SQL INSERT ile doğrudan veritabanına kayıt yapan metodlar.
    /// Logo Objects lisansı gerektirmez.
    /// </summary>
    public static class HelperSQL
    {
        #region Invoice Methods

        /// <summary>
        /// SQL ile doğrudan satış faturası kaydı oluşturur
        /// TRCODE 7 = Perakende Satış Faturası
        /// </summary>
        public static string InsertInvoiceSQL(string CARI_KOD, string BRANCH, Bms_Fiche_Header _BASLIK, List<Bms_Fiche_Detail> _DETAILS, bool withCustomer, string FIRMNR, string PERIODNR = "01")
        {
            try
            {
                // Müşteri kontrolü
                string customerCode = withCustomer ? _BASLIK.CUSTOMER_CODE : CARI_KOD;
                bool isCustomerExist = false;
                try { isCustomerExist = Convert.ToInt32(SqlSelectLogo($"SELECT COUNT(*) FROM LG_{FIRMNR}_CLCARD WHERE CODE='{customerCode}'").Rows[0][0]) > 0; }
                catch { }
                if (!isCustomerExist)
                    customerCode = customerCode.TrimStart('0');

                // Müşteri LOGICALREF'i al
                int clientRef = 0;
                try { clientRef = Convert.ToInt32(SqlSelectLogo($"SELECT LOGICALREF FROM LG_{FIRMNR}_CLCARD WHERE CODE='{customerCode}'").Rows[0][0]); }
                catch { }

                // FICHENO al
                string ficheNo = GetNextFicheNo($"LG_{FIRMNR}_{PERIODNR}_INVOICE", "FICHENO", 7, FIRMNR);

                // Tarih formatları
                string dateStr = _BASLIK.DATE_.ToString("yyyy-MM-dd");
                int timeVal = _BASLIK.DATE_.Hour * 256 * 256 + _BASLIK.DATE_.Minute * 256 + _BASLIK.DATE_.Second;

                // Header INSERT - LG_{FIRMNR}_{PERIODNR}_INVOICE (LOGICALREF otomatik oluşur)
                string headerSql = $@"
                INSERT INTO LG_{FIRMNR}_{PERIODNR}_INVOICE (
                    TRCODE, FICHENO, DATE_, TIME_, DOCODE, SPECODE, CYPHCODE,
                    CLIENTREF, DOCTRACKINGNR, GENEXP6, ACCOUNTED, GENEXCTYP,
                    DEDUCTIONPART1, DEDUCTIONPART2, DOCDATE, BRANCH, SITEID, SOURCEINDEX
                ) VALUES (
                    7, '{ficheNo}', '{dateStr}', {timeVal}, '{_BASLIK.DOCUMENT_NO}', '{_BASLIK.POS}', 'BMS-NCR',
                    {clientRef}, '{_BASLIK.POS}', '{(withCustomer ? _BASLIK.FICHE_ID : "")}', 0, 1,
                    2, 3, '{dateStr}', {BRANCH}, 0, 0
                );
                SELECT SCOPE_IDENTITY();";

                // INSERT ve LOGICALREF al
                int invoiceLogicalRef = Convert.ToInt32(SqlSelectLogo(headerSql).Rows[0][0]);
                LOGYAZ($"InsertInvoiceSQL Header OK - LOGICALREF: {invoiceLogicalRef}, FICHENO: {ficheNo}", null);

                // STFICHE INSERT - Stok Fişi Header (TRCODE 8 = Perakende Satış)
                string stFicheNo = GetNextFicheNo($"LG_{FIRMNR}_{PERIODNR}_STFICHE", "FICHENO", 8, FIRMNR);
                string stFicheSql = $@"
                INSERT INTO LG_{FIRMNR}_{PERIODNR}_STFICHE (
                    GRPCODE, TRCODE, FICHENO, DATE_, FTIME, DOCODE, SPECODE, CYPHCODE,
                    INVOICEREF, SOURCEINDEX, BRANCH, SITEID,
                    GENEXCTYP, DEDUCTIONPART1, DEDUCTIONPART2, PRINTDATE, CANCELLED, STATUS,
                    IOCODE, BILLED
                ) VALUES (
                    0, 8, '{stFicheNo}', '{dateStr}', {timeVal}, '{_BASLIK.DOCUMENT_NO}', '{_BASLIK.POS}', 'BMS-NCR',
                    {invoiceLogicalRef}, 0, {BRANCH}, 0,
                    1, 2, 3, '{dateStr}', 0, 1,
                    4, 1
                );
                SELECT SCOPE_IDENTITY();";

                int stFicheLogicalRef = Convert.ToInt32(SqlSelectLogo(stFicheSql).Rows[0][0]);
                LOGYAZ($"InsertInvoiceSQL STFICHE OK - LOGICALREF: {stFicheLogicalRef}, FICHENO: {stFicheNo}", null);

                // Lines INSERT - LG_{FIRMNR}_{PERIODNR}_STLINE
                int lineNo = 0;
                decimal totalGross = 0;
                decimal totalNet = 0;
                decimal totalVat = 0;
                decimal totalDiscount = 0;

                foreach (var line in _DETAILS)
                {
                    if (string.IsNullOrEmpty(line.ITEMCODE))
                    {
                        LOGYAZ($"Boş ITEMCODE atlandı - Tarih: {_BASLIK.DATE_:yyyy-MM-dd}, POS: {_BASLIK.POS}, Ürün: {line.ITEMNAME}", null);
                        continue;
                    }

                    // Ürün bilgilerini al
                    int stockRef = 0;
                    double vatRate = 0;
                    int unitRef = 0;
                    try
                    {
                        DataTable dtItem = SqlSelectLogo($"SELECT LOGICALREF, VAT FROM LG_{FIRMNR}_ITEMS WHERE CODE='{line.ITEMCODE}'");
                        if (dtItem.Rows.Count > 0)
                        {
                            stockRef = Convert.ToInt32(dtItem.Rows[0]["LOGICALREF"]);
                            vatRate = Convert.ToDouble(dtItem.Rows[0]["VAT"]);
                        }
                    }
                    catch { }

                    // Birim REF al
                    try { unitRef = Convert.ToInt32(SqlSelectLogo($"SELECT LOGICALREF FROM LG_{FIRMNR}_UNITSETL WHERE CODE='{line.ITEMUNIT}'").Rows[0][0]); }
                    catch { }

                    // Satıcı REF al
                    int salesmanRef = 0;
                    try { salesmanRef = Convert.ToInt32(SqlSelectLogo($"SELECT LOGICALREF FROM LG_{FIRMNR}_SLSMAN WHERE CODE='{line.SALESMAN}'").Rows[0][0]); }
                    catch { }

                    double priceVal = Convert.ToDouble(line.PRICE.ToString(CultureInfo.InvariantCulture));
                    double lineTotalVal = Convert.ToDouble(line.LINETOTAL.ToString(CultureInfo.InvariantCulture));
                    double unitPrice = Math.Abs(line.DISCOUNT_TOTAL) > 0 ? priceVal : lineTotalVal / line.QUANTITY;

                    // KDV hesapla
                    double lineNetVal = lineTotalVal / (1 + vatRate / 100);
                    double lineVatVal = lineTotalVal - lineNetVal;

                    totalNet += (decimal)lineNetVal;
                    totalVat += (decimal)lineVatVal;
                    totalGross += line.LINETOTAL;

                    string lineSql = $@"
                    INSERT INTO LG_{FIRMNR}_{PERIODNR}_STLINE (
                        INVOICEREF, STFICHEREF, STOCKREF, LINETYPE,
                        DETLINE, AMOUNT, PRICE, TOTAL,
                        UINFO1, UINFO2, VATINC, VAT, VATAMNT, BILLED,
                        SALESMANREF, MONTH_, YEAR_, AFFECTRISK, STFICHELNNO,
                        SITEID, TRCODE, DATE_, FTIME, CANCELLED, CPSTFLAG, SOURCEINDEX,
                        USREF, IOCODE
                    ) VALUES (
                        {invoiceLogicalRef}, {stFicheLogicalRef}, {stockRef}, 0,
                        0, {line.QUANTITY.ToString(CultureInfo.InvariantCulture)}, {unitPrice.ToString(CultureInfo.InvariantCulture)}, {lineTotalVal.ToString(CultureInfo.InvariantCulture)},
                        1, 1, 1, {vatRate.ToString(CultureInfo.InvariantCulture)}, {lineVatVal.ToString(CultureInfo.InvariantCulture)}, 1,
                        {salesmanRef}, {_BASLIK.DATE_.Month}, {_BASLIK.DATE_.Year}, 1, {lineNo},
                        0, 7, '{dateStr}', {timeVal}, 0, 1, 0,
                        {unitRef}, 4
                    )";

                    SqlExecute(lineSql);
                    lineNo++;

                    // İndirim satırı varsa ekle
                    if (Math.Abs(line.DISCOUNT_TOTAL) > 0)
                    {
                        double discountVal = Math.Abs(Convert.ToDouble(line.DISCOUNT_TOTAL.ToString(CultureInfo.InvariantCulture)));
                        double discountNetVal = discountVal / (1 + vatRate / 100);
                        totalDiscount += (decimal)discountVal;

                        string discLineSql = $@"
                        INSERT INTO LG_{FIRMNR}_{PERIODNR}_STLINE (
                            INVOICEREF, STFICHEREF, STOCKREF, LINETYPE,
                            DETLINE, AMOUNT, PRICE, TOTAL, DISCEXP,
                            VATINC, BILLED, MONTH_, YEAR_, AFFECTRISK, STFICHELNNO,
                            SITEID, TRCODE, DATE_, FTIME, CANCELLED, CPSTFLAG, SOURCEINDEX,
                            IOCODE
                        ) VALUES (
                            {invoiceLogicalRef}, {stFicheLogicalRef}, 0, 2,
                            1, 0, 0, {discountNetVal.ToString(CultureInfo.InvariantCulture)}, 1,
                            0, 1, {_BASLIK.DATE_.Month}, {_BASLIK.DATE_.Year}, 1, {lineNo},
                            0, 7, '{dateStr}', {timeVal}, 0, 1, 0,
                            4
                        )";

                        SqlExecute(discLineSql);
                        lineNo++;
                    }
                }

                // Fatura toplamlarını güncelle
                string updateTotalsSql = $@"
                UPDATE LG_{FIRMNR}_{PERIODNR}_INVOICE SET
                    GROSSTOTAL = {totalGross.ToString(CultureInfo.InvariantCulture)},
                    NETTOTAL = {(totalGross - totalDiscount).ToString(CultureInfo.InvariantCulture)},
                    TOTALDISCOUNTS = {totalDiscount.ToString(CultureInfo.InvariantCulture)},
                    TOTALVAT = {totalVat.ToString(CultureInfo.InvariantCulture)}
                WHERE LOGICALREF = {invoiceLogicalRef}";
                SqlExecute(updateTotalsSql);

                // STFICHE toplamlarını güncelle
                string updateStFicheTotalsSql = $@"
                UPDATE LG_{FIRMNR}_{PERIODNR}_STFICHE SET
                    GROSSTOTAL = {totalGross.ToString(CultureInfo.InvariantCulture)},
                    NETTOTAL = {(totalGross - totalDiscount).ToString(CultureInfo.InvariantCulture)},
                    TOTALDISCOUNTS = {totalDiscount.ToString(CultureInfo.InvariantCulture)},
                    TOTALVAT = {totalVat.ToString(CultureInfo.InvariantCulture)}
                WHERE LOGICALREF = {stFicheLogicalRef}";
                SqlExecute(updateStFicheTotalsSql);

                LOGYAZ($"InsertInvoiceSQL Lines OK - {lineNo} satır eklendi, Gross: {totalGross}, Net: {totalGross - totalDiscount}", null);
                return "ok";
            }
            catch (Exception E)
            {
                LOGYAZ("InsertInvoiceSQL", E);
                return E.Message;
            }
        }

        /// <summary>
        /// SQL ile doğrudan iade faturası kaydı oluşturur
        /// TRCODE 2 = Satış İade Faturası, TRCODE 8 = Perakende İade
        /// </summary>
        public static string InsertReturnInvoiceSQL(string CARI_KOD, string BRANCH, Bms_Fiche_Header _BASLIK, List<Bms_Fiche_Detail> _DETAILS, bool withCustomer, string FIRMNR, string AUTHCODE2, string PERIODNR = "01")
        {
            try
            {
                // Müşteri kontrolü
                string customerCode = withCustomer ? _BASLIK.CUSTOMER_CODE : CARI_KOD;
                bool isCustomerExist = false;
                try { isCustomerExist = Convert.ToInt32(SqlSelectLogo($"SELECT COUNT(*) FROM LG_{FIRMNR}_CLCARD WHERE CODE='{customerCode}'").Rows[0][0]) > 0; }
                catch { }
                if (!isCustomerExist)
                    customerCode = customerCode.TrimStart('0');

                // Müşteri LOGICALREF'i al
                int clientRef = 0;
                try { clientRef = Convert.ToInt32(SqlSelectLogo($"SELECT LOGICALREF FROM LG_{FIRMNR}_CLCARD WHERE CODE='{customerCode}'").Rows[0][0]); }
                catch { }

                // FICHENO al - TRCODE 8 = Perakende İade
                string ficheNo = GetNextFicheNo($"LG_{FIRMNR}_{PERIODNR}_INVOICE", "FICHENO", 8, FIRMNR);

                // Tarih formatları
                string dateStr = _BASLIK.DATE_.ToString("yyyy-MM-dd");
                int timeVal = _BASLIK.DATE_.Hour * 256 * 256 + _BASLIK.DATE_.Minute * 256 + _BASLIK.DATE_.Second;

                // Header INSERT - LG_{FIRMNR}_{PERIODNR}_INVOICE (LOGICALREF otomatik oluşur)
                string headerSql = $@"
                INSERT INTO LG_{FIRMNR}_{PERIODNR}_INVOICE (
                    TRCODE, FICHENO, DATE_, TIME_, DOCODE, SPECODE, CYPHCODE,
                    CLIENTREF, DOCTRACKINGNR, GENEXP6, ACCOUNTED, GENEXCTYP,
                    DEDUCTIONPART1, DEDUCTIONPART2, DOCDATE, BRANCH, SITEID, SOURCEINDEX
                ) VALUES (
                    8, '{ficheNo}', '{dateStr}', {timeVal}, '{_BASLIK.FICHE_ID}', '{_BASLIK.POS}', '{AUTHCODE2}',
                    {clientRef}, '{_BASLIK.POS}', '{(withCustomer ? _BASLIK.FICHE_ID : "")}', 0, 1,
                    2, 3, '{dateStr}', {BRANCH}, 0, 0
                );
                SELECT SCOPE_IDENTITY();";

                int invoiceLogicalRef = Convert.ToInt32(SqlSelectLogo(headerSql).Rows[0][0]);
                LOGYAZ($"InsertReturnInvoiceSQL Header OK - LOGICALREF: {invoiceLogicalRef}", null);

                // STFICHE INSERT - Stok Fişi Header (TRCODE 3 = Perakende İade)
                string stFicheNo = GetNextFicheNo($"LG_{FIRMNR}_{PERIODNR}_STFICHE", "FICHENO", 3, FIRMNR);
                string stFicheSql = $@"
                INSERT INTO LG_{FIRMNR}_{PERIODNR}_STFICHE (
                    GRPCODE, TRCODE, FICHENO, DATE_, FTIME, DOCODE, SPECODE, CYPHCODE,
                    INVOICEREF, SOURCEINDEX, BRANCH, SITEID,
                    GENEXCTYP, DEDUCTIONPART1, DEDUCTIONPART2, PRINTDATE, CANCELLED, STATUS,
                    IOCODE, BILLED
                ) VALUES (
                    0, 3, '{stFicheNo}', '{dateStr}', {timeVal}, '{_BASLIK.FICHE_ID}', '{_BASLIK.POS}', '{AUTHCODE2}',
                    {invoiceLogicalRef}, 0, {BRANCH}, 0,
                    1, 2, 3, '{dateStr}', 0, 1,
                    1, 1
                );
                SELECT SCOPE_IDENTITY();";

                int stFicheLogicalRef = Convert.ToInt32(SqlSelectLogo(stFicheSql).Rows[0][0]);
                LOGYAZ($"InsertReturnInvoiceSQL STFICHE OK - LOGICALREF: {stFicheLogicalRef}, FICHENO: {stFicheNo}", null);

                // Lines INSERT - LG_{FIRMNR}_{PERIODNR}_STLINE
                int lineNo = 0;
                decimal totalGross = 0;
                decimal totalNet = 0;
                decimal totalVat = 0;
                decimal totalDiscount = 0;

                foreach (var line in _DETAILS)
                {
                    if (string.IsNullOrEmpty(line.ITEMCODE))
                    {
                        LOGYAZ($"Boş ITEMCODE atlandı - Tarih: {_BASLIK.DATE_:yyyy-MM-dd}, POS: {_BASLIK.POS}", null);
                        continue;
                    }

                    // Ürün bilgilerini al
                    int stockRef = 0;
                    double vatRate = 0;
                    int unitRef = 0;
                    try
                    {
                        DataTable dtItem = SqlSelectLogo($"SELECT LOGICALREF, VAT FROM LG_{FIRMNR}_ITEMS WHERE CODE='{line.ITEMCODE}'");
                        if (dtItem.Rows.Count > 0)
                        {
                            stockRef = Convert.ToInt32(dtItem.Rows[0]["LOGICALREF"]);
                            vatRate = Convert.ToDouble(dtItem.Rows[0]["VAT"]);
                        }
                    }
                    catch { }

                    // Birim REF al
                    try { unitRef = Convert.ToInt32(SqlSelectLogo($"SELECT LOGICALREF FROM LG_{FIRMNR}_UNITSETL WHERE CODE='{line.ITEMUNIT}'").Rows[0][0]); }
                    catch { }

                    // Satıcı REF al
                    int salesmanRef = 0;
                    try { salesmanRef = Convert.ToInt32(SqlSelectLogo($"SELECT LOGICALREF FROM LG_{FIRMNR}_SLSMAN WHERE CODE='{line.SALESMAN}'").Rows[0][0]); }
                    catch { }

                    double lineTotalVal = Math.Abs(Convert.ToDouble(line.LINETOTAL.ToString(CultureInfo.InvariantCulture)));
                    double unitPrice = lineTotalVal / line.QUANTITY;

                    // KDV hesapla
                    double lineNetVal = lineTotalVal / (1 + vatRate / 100);
                    double lineVatVal = lineTotalVal - lineNetVal;

                    totalNet += (decimal)lineNetVal;
                    totalVat += (decimal)lineVatVal;
                    totalGross += (decimal)lineTotalVal;

                    string lineSql = $@"
                    INSERT INTO LG_{FIRMNR}_{PERIODNR}_STLINE (
                        INVOICEREF, STFICHEREF, STOCKREF, LINETYPE,
                        DETLINE, AMOUNT, PRICE, TOTAL,
                        UINFO1, UINFO2, VATINC, VAT, VATAMNT, BILLED,
                        SALESMANREF, MONTH_, YEAR_, AFFECTRISK, STFICHELNNO,
                        SITEID, TRCODE, DATE_, FTIME, CANCELLED, CPSTFLAG, SOURCEINDEX, RETCOSTTYPE,
                        USREF, IOCODE
                    ) VALUES (
                        {invoiceLogicalRef}, {stFicheLogicalRef}, {stockRef}, 0,
                        0, {line.QUANTITY.ToString(CultureInfo.InvariantCulture)}, {unitPrice.ToString(CultureInfo.InvariantCulture)}, {lineTotalVal.ToString(CultureInfo.InvariantCulture)},
                        1, 1, 1, {vatRate.ToString(CultureInfo.InvariantCulture)}, {lineVatVal.ToString(CultureInfo.InvariantCulture)}, 1,
                        {salesmanRef}, {_BASLIK.DATE_.Month}, {_BASLIK.DATE_.Year}, 1, {lineNo},
                        0, 8, '{dateStr}', {timeVal}, 0, 1, 0, 1,
                        {unitRef}, 1
                    )";

                    SqlExecute(lineSql);
                    lineNo++;

                    // İndirim satırı varsa ekle
                    if (Math.Abs(line.DISCOUNT_TOTAL) > 0)
                    {
                        double discountVal = Math.Abs(Convert.ToDouble(line.DISCOUNT_TOTAL.ToString(CultureInfo.InvariantCulture)));
                        double discountNetVal = discountVal / (1 + vatRate / 100);
                        totalDiscount += (decimal)discountVal;

                        string discLineSql = $@"
                        INSERT INTO LG_{FIRMNR}_{PERIODNR}_STLINE (
                            INVOICEREF, STFICHEREF, STOCKREF, LINETYPE,
                            DETLINE, AMOUNT, PRICE, TOTAL, DISCEXP,
                            VATINC, BILLED, MONTH_, YEAR_, AFFECTRISK, STFICHELNNO,
                            SITEID, TRCODE, DATE_, FTIME, CANCELLED, CPSTFLAG, SOURCEINDEX,
                            IOCODE
                        ) VALUES (
                            {invoiceLogicalRef}, {stFicheLogicalRef}, 0, 2,
                            1, 0, 0, {discountNetVal.ToString(CultureInfo.InvariantCulture)}, 1,
                            0, 1, {_BASLIK.DATE_.Month}, {_BASLIK.DATE_.Year}, 1, {lineNo},
                            0, 8, '{dateStr}', {timeVal}, 0, 1, 0,
                            1
                        )";

                        SqlExecute(discLineSql);
                        lineNo++;
                    }
                }

                // Fatura toplamlarını güncelle
                string updateTotalsSql = $@"
                UPDATE LG_{FIRMNR}_{PERIODNR}_INVOICE SET
                    GROSSTOTAL = {totalGross.ToString(CultureInfo.InvariantCulture)},
                    NETTOTAL = {(totalGross - totalDiscount).ToString(CultureInfo.InvariantCulture)},
                    TOTALDISCOUNTS = {totalDiscount.ToString(CultureInfo.InvariantCulture)},
                    TOTALVAT = {totalVat.ToString(CultureInfo.InvariantCulture)}
                WHERE LOGICALREF = {invoiceLogicalRef}";
                SqlExecute(updateTotalsSql);

                // STFICHE toplamlarını güncelle
                string updateStFicheTotalsSql = $@"
                UPDATE LG_{FIRMNR}_{PERIODNR}_STFICHE SET
                    GROSSTOTAL = {totalGross.ToString(CultureInfo.InvariantCulture)},
                    NETTOTAL = {(totalGross - totalDiscount).ToString(CultureInfo.InvariantCulture)},
                    TOTALDISCOUNTS = {totalDiscount.ToString(CultureInfo.InvariantCulture)},
                    TOTALVAT = {totalVat.ToString(CultureInfo.InvariantCulture)}
                WHERE LOGICALREF = {stFicheLogicalRef}";
                SqlExecute(updateStFicheTotalsSql);

                LOGYAZ($"InsertReturnInvoiceSQL Lines OK - {lineNo} satır eklendi", null);
                return "ok";
            }
            catch (Exception E)
            {
                LOGYAZ("InsertReturnInvoiceSQL", E);
                return E.Message;
            }
        }

        #endregion

        #region Payment Methods

        /// <summary>
        /// SQL ile doğrudan çek/senet bordrosu kaydı oluşturur
        /// TRCODE 1 = Müşteri Çeki Giriş Bordrosu
        /// </summary>
        public static string InsertChequeSQL(string BRANCH, Bms_Fiche_Payment _PAYMENT, string FIRMNR, string PERIODNR = "01")
        {
            try
            {
                // Müşteri kontrolü
                bool isCustomerExist = false;
                try { isCustomerExist = Convert.ToInt32(SqlSelectLogo($"SELECT COUNT(*) FROM LG_{FIRMNR}_CLCARD WHERE CODE='{_PAYMENT.CUSTOMER_CODE}'").Rows[0][0]) > 0; }
                catch { }
                if (!isCustomerExist)
                    _PAYMENT.CUSTOMER_CODE = _PAYMENT.CUSTOMER_CODE.TrimStart('0');

                // Müşteri LOGICALREF'i al
                int clCardRef = 0;
                try { clCardRef = Convert.ToInt32(SqlSelectLogo($"SELECT LOGICALREF FROM LG_{FIRMNR}_CLCARD WHERE CODE='{_PAYMENT.CUSTOMER_CODE}'").Rows[0][0]); }
                catch { }

                // FICHENO al
                string ficheNo = GetNextFicheNo($"LG_{FIRMNR}_{PERIODNR}_CSROLL", "FICHENO", 1, FIRMNR);

                // Tarih formatları
                string dateStr = _PAYMENT.DATE_.ToString("yyyy-MM-dd");
                int timeVal = _PAYMENT.DATE_.Hour * 256 * 256 + _PAYMENT.DATE_.Minute * 256 + _PAYMENT.DATE_.Second;

                double paymentTotal = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString(CultureInfo.InvariantCulture));

                // Header INSERT - LG_{FIRMNR}_{PERIODNR}_CSROLL (Çek/Senet Bordrosu Header)
                string headerSql = $@"
                INSERT INTO LG_{FIRMNR}_{PERIODNR}_CSROLL (
                    TRCODE, FICHENO, DATE_, TIME_, DOCODE,
                    MODULENR, CARDREF, SPECODE, CYPHCODE,
                    TOTAL, DOCTOTAL, BRANCH, SITEID,
                    GENEXP1, GENEXP2, GENEXP3, GENEXP4, GENEXP5, GENEXP6,
                    PRINTDATE, ACCOUNTED, CANCELLED
                ) VALUES (
                    1, '{ficheNo}', '{dateStr}', {timeVal}, '{_PAYMENT.DOCUMENT_NO}',
                    5, {clCardRef}, '{_PAYMENT.POS}', 'BMS-NCR',
                    {paymentTotal.ToString(CultureInfo.InvariantCulture)}, 1, {BRANCH}, 0,
                    '', '', '', '', '', '',
                    '{dateStr}', 0, 0
                );
                SELECT SCOPE_IDENTITY();";

                int rollLogicalRef = Convert.ToInt32(SqlSelectLogo(headerSql).Rows[0][0]);
                LOGYAZ($"InsertChequeSQL Header OK - LOGICALREF: {rollLogicalRef}", null);

                // Line INSERT - LG_{FIRMNR}_{PERIODNR}_CSCARD (Çek/Senet Kartı)
                string cardSerialNo = GetNextFicheNo($"LG_{FIRMNR}_{PERIODNR}_CSCARD", "SERIALNR", 1, FIRMNR);

                string lineSql = $@"
                INSERT INTO LG_{FIRMNR}_{PERIODNR}_CSCARD (
                    CARDTYPE, CURRSTATUS, SERIALNR, OWING,
                    SPECODE, CYPHCODE, SETDATE, DESSION, DUEDATE,
                    AMOUNT, PORTFOYREF, BRANCH, SITEID,
                    AFFECTRISK, STATUSORD, CURRSEL, XMLATTR1,
                    CANCELLED, PRINTDATE
                ) VALUES (
                    1, 1, '{(string.IsNullOrEmpty(_PAYMENT.SERIAL_NO) ? cardSerialNo : _PAYMENT.SERIAL_NO)}', '{_PAYMENT.CUSTOMER_NAME?.Replace("'", "''")}',
                    '{_PAYMENT.POS}', 'BMS-NCR', '{dateStr}', {rollLogicalRef}, '{dateStr}',
                    {paymentTotal.ToString(CultureInfo.InvariantCulture)}, {rollLogicalRef}, {BRANCH}, 0,
                    1, 1, 1, 1,
                    0, '{dateStr}'
                )";

                SqlExecute(lineSql);
                LOGYAZ($"InsertChequeSQL Card OK", null);

                return "ok";
            }
            catch (Exception E)
            {
                LOGYAZ("InsertChequeSQL", E);
                return E.Message;
            }
        }

        /// <summary>
        /// SQL ile doğrudan cari hesap fişi kaydı oluşturur
        /// TRCODE 70 = Kredi Kartı, 71 = Kredi Kartı İade, 3 = Borç, 4 = Alacak
        /// </summary>
        public static string InsertCHFicheSQL(string BRANCH, Bms_Fiche_Payment _PAYMENT, string FIRMNR, string PERIODNR = "01")
        {
            try
            {
                LOGYAZ($"InsertCHFicheSQL BASLADI - BRANCH:{BRANCH}, FIRMNR:{FIRMNR}, CUSTOMER_CODE:{_PAYMENT?.CUSTOMER_CODE}", null);

                // Parametre kontrolleri
                if (_PAYMENT == null) return "_PAYMENT parametresi null";
                if (string.IsNullOrEmpty(BRANCH)) return "BRANCH parametresi boş";
                if (string.IsNullOrEmpty(FIRMNR)) return "FIRMNR parametresi boş";
                if (string.IsNullOrEmpty(_PAYMENT.CUSTOMER_CODE)) return "CUSTOMER_CODE boş";
                if (string.IsNullOrEmpty(_PAYMENT.LOGO_FICHE_TYPE)) return "LOGO_FICHE_TYPE boş";

                // Müşteri kontrolü
                bool isCustomerExist = false;
                try { isCustomerExist = Convert.ToInt32(SqlSelectLogo($"SELECT COUNT(*) FROM LG_{FIRMNR}_CLCARD WHERE CODE='{_PAYMENT.CUSTOMER_CODE}'").Rows[0][0]) > 0; }
                catch { }
                if (!isCustomerExist)
                    _PAYMENT.CUSTOMER_CODE = _PAYMENT.CUSTOMER_CODE.TrimStart('0');

                // Müşteri LOGICALREF'i al
                int clCardRef = 0;
                try { clCardRef = Convert.ToInt32(SqlSelectLogo($"SELECT LOGICALREF FROM LG_{FIRMNR}_CLCARD WHERE CODE='{_PAYMENT.CUSTOMER_CODE}'").Rows[0][0]); }
                catch { }

                // TRCODE belirle
                int trCode = 70; // Default: Kredi Kartı
                if (_PAYMENT.LOGO_FICHE_TYPE == "CH Kredi Karti Iade") trCode = 71;
                else if (_PAYMENT.LOGO_FICHE_TYPE == "CH Borc") trCode = 3;
                else if (_PAYMENT.LOGO_FICHE_TYPE == "CH Alacak") trCode = 4;

                // FICHENO al
                string ficheNo = GetNextFicheNo($"LG_{FIRMNR}_{PERIODNR}_CLFICHE", "FICHENO", trCode, FIRMNR);

                // Tarih formatları
                string dateStr = _PAYMENT.DATE_.ToString("yyyy-MM-dd");
                int timeVal = _PAYMENT.DATE_.Hour * 256 * 256 + _PAYMENT.DATE_.Minute * 256 + _PAYMENT.DATE_.Second;

                double paymentTotal = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString(CultureInfo.InvariantCulture));

                // Banka hesabı REF al (kredi kartı işlemleri için)
                int bankAccRef = 0;
                if (trCode == 70 || trCode == 71)
                {
                    try { bankAccRef = Convert.ToInt32(SqlSelectLogo($"SELECT LOGICALREF FROM LG_{FIRMNR}_BANKACC WHERE CODE='{_PAYMENT.LOGO_BANK_OR_KS_CODE}'").Rows[0][0]); }
                    catch { }
                }

                // Header INSERT - LG_{FIRMNR}_{PERIODNR}_CLFICHE (Cari Hesap Fişi Header)
                string headerSql = $@"
                INSERT INTO LG_{FIRMNR}_{PERIODNR}_CLFICHE (
                    TRCODE, FICHENO, DATE_,
                    CYPHCODE, BRANCH, SITEID,
                    PRINTDATE, ACCOUNTED, CANCELLED, GENEXCTYP
                ) VALUES (
                    {trCode}, '{ficheNo}', '{dateStr}',
                    'BMS-NCR', {BRANCH}, 0,
                    '{dateStr}', 0, 0, 1
                );
                SELECT SCOPE_IDENTITY();";

                int ficheLogicalRef = Convert.ToInt32(SqlSelectLogo(headerSql).Rows[0][0]);
                LOGYAZ($"InsertCHFicheSQL Header OK - LOGICALREF: {ficheLogicalRef}", null);

                // Line INSERT - LG_{FIRMNR}_{PERIODNR}_CLFLINE (Cari Hesap Fişi Satırı)
                string tranNo = GetNextFicheNo($"LG_{FIRMNR}_{PERIODNR}_CLFLINE", "TRANNO", trCode, FIRMNR);

                string lineSql = $@"
                INSERT INTO LG_{FIRMNR}_{PERIODNR}_CLFLINE (
                    CLIENTREF, TRCODE, TRANNO,
                    DATE_, DOCODE, CYPHCODE,
                    AMOUNT, TRRATE, TRNET, REPORTRATE, REPORTNET,
                    MONTH_, YEAR_, BRANCH, SITEID,
                    ACCOUNTED, CANCELLED, AFFECTRISK
                    {(bankAccRef > 0 ? ", BANKACCREF" : "")}
                ) VALUES (
                    {clCardRef}, {trCode}, '{tranNo}',
                    '{dateStr}', '{_PAYMENT.DOCUMENT_NO}', 'BMS-NCR',
                    {paymentTotal.ToString(CultureInfo.InvariantCulture)}, 1, {paymentTotal.ToString(CultureInfo.InvariantCulture)}, 1, {paymentTotal.ToString(CultureInfo.InvariantCulture)},
                    {_PAYMENT.DATE_.Month}, {_PAYMENT.DATE_.Year}, {BRANCH}, 0,
                    0, 0, 1
                    {(bankAccRef > 0 ? $", {bankAccRef}" : "")}
                )";

                SqlExecute(lineSql);
                LOGYAZ($"InsertCHFicheSQL Line OK", null);

                return "ok";
            }
            catch (Exception E)
            {
                LOGYAZ("InsertCHFicheSQL", E);
                return E.Message;
            }
        }

        /// <summary>
        /// SQL ile doğrudan kasa fişi kaydı oluşturur
        /// TRCODE 11 = Nakit Tahsilat, 12 = Nakit Ödeme
        /// </summary>
        public static string InsertKsFicheSQL(string BRANCH, Bms_Fiche_Payment _PAYMENT, string FIRMNR, string PERIODNR = "01")
        {
            try
            {
                // Müşteri kontrolü
                bool isCustomerExist = false;
                try { isCustomerExist = Convert.ToInt32(SqlSelectLogo($"SELECT COUNT(*) FROM LG_{FIRMNR}_CLCARD WHERE CODE='{_PAYMENT.CUSTOMER_CODE}'").Rows[0][0]) > 0; }
                catch { }
                if (!isCustomerExist)
                    _PAYMENT.CUSTOMER_CODE = _PAYMENT.CUSTOMER_CODE.TrimStart('0');

                // Müşteri LOGICALREF'i al
                int clCardRef = 0;
                try { clCardRef = Convert.ToInt32(SqlSelectLogo($"SELECT LOGICALREF FROM LG_{FIRMNR}_CLCARD WHERE CODE='{_PAYMENT.CUSTOMER_CODE}'").Rows[0][0]); }
                catch { }

                // Kasa REF al
                int ksCardRef = 0;
                try { ksCardRef = Convert.ToInt32(SqlSelectLogo($"SELECT LOGICALREF FROM LG_{FIRMNR}_KSCARD WHERE CODE='{_PAYMENT.LOGO_BANK_OR_KS_CODE}'").Rows[0][0]); }
                catch { }

                // TRCODE belirle
                int trCode = _PAYMENT.FTYPE == "SATIS" ? 11 : 12;

                // FICHENO al
                string ficheNo = GetNextFicheNo($"LG_{FIRMNR}_{PERIODNR}_KSLINES", "FICHENO", trCode, FIRMNR);

                // Tarih formatları
                string dateStr = _PAYMENT.DATE_.ToString("yyyy-MM-dd");
                int timeVal = _PAYMENT.DATE_.Hour * 256 * 256 + _PAYMENT.DATE_.Minute * 256 + _PAYMENT.DATE_.Second;

                double paymentTotal = Convert.ToDouble(_PAYMENT.PAYMENT_TOTAL.ToString(CultureInfo.InvariantCulture));

                // Kasa İşlem Satırı INSERT - LG_{FIRMNR}_{PERIODNR}_KSLINES
                // Not: KSLINES tablosunda trigger var, CSHTOTS tablosuna kayıt yapmaya çalışıyor
                // Duplicate key hatası alınabilir, bu durumda sadece CLFLINE kaydı yapılır
                try
                {
                    string ksSql = $@"
                    INSERT INTO LG_{FIRMNR}_{PERIODNR}_KSLINES (
                        TRCODE, FICHENO, DATE_,
                        CARDREF, AMOUNT, TRRATE, TRNET, REPORTRATE, REPORTNET,
                        CYPHCODE, BRANCH, SITEID,
                        ACCOUNTED, CANCELLED, LINEEXP
                    ) VALUES (
                        {trCode}, '{ficheNo}', '{dateStr}',
                        {ksCardRef}, {paymentTotal.ToString(CultureInfo.InvariantCulture)}, 1, {paymentTotal.ToString(CultureInfo.InvariantCulture)}, 1, {paymentTotal.ToString(CultureInfo.InvariantCulture)},
                        'BMS-NCR', {BRANCH}, 0,
                        0, 0, '{_PAYMENT.CUSTOMER_NAME?.Replace("'", "''")}'
                    )";

                    SqlExecute(ksSql);
                    LOGYAZ($"InsertKsFicheSQL KS Line OK", null);
                }
                catch (Exception ksEx)
                {
                    // KSLINES insert hatası - CSHTOTS trigger sorunu olabilir, devam et
                    LOGYAZ($"InsertKsFicheSQL KS Line SKIP (trigger error): {ksEx.Message}", null);
                }

                // Cari Hesap Satırı INSERT - LG_{FIRMNR}_{PERIODNR}_CLFLINE
                string tranNo = GetNextFicheNo($"LG_{FIRMNR}_{PERIODNR}_CLFLINE", "TRANNO", trCode, FIRMNR);

                string clfSql = $@"
                INSERT INTO LG_{FIRMNR}_{PERIODNR}_CLFLINE (
                    CLIENTREF, TRCODE, TRANNO,
                    DATE_, DOCODE, CYPHCODE,
                    AMOUNT, TRRATE, TRNET, REPORTRATE, REPORTNET,
                    MONTH_, YEAR_, BRANCH, SITEID,
                    ACCOUNTED, CANCELLED, AFFECTRISK
                ) VALUES (
                    {clCardRef}, {trCode}, '{tranNo}',
                    '{dateStr}', '{_PAYMENT.DOCUMENT_NO}', 'BMS-NCR',
                    {paymentTotal.ToString(CultureInfo.InvariantCulture)}, 1, {paymentTotal.ToString(CultureInfo.InvariantCulture)}, 1, {paymentTotal.ToString(CultureInfo.InvariantCulture)},
                    {_PAYMENT.DATE_.Month}, {_PAYMENT.DATE_.Year}, {BRANCH}, 0,
                    0, 0, 1
                )";

                SqlExecute(clfSql);
                LOGYAZ($"InsertKsFicheSQL CLF Line OK", null);

                return "ok";
            }
            catch (Exception E)
            {
                LOGYAZ("InsertKsFicheSQL", E);
                return E.Message;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Belirtilen tablo için bir sonraki LOGICALREF değerini döndürür
        /// </summary>
        private static int GetNextLogicalRef(string tableName, string FIRMNR)
        {
            int maxRef = 0;
            try
            {
                DataTable dt = SqlSelectLogo($"SELECT ISNULL(MAX(LOGICALREF), 0) AS MAXREF FROM {tableName}");
                if (dt.Rows.Count > 0)
                    maxRef = Convert.ToInt32(dt.Rows[0]["MAXREF"]);
            }
            catch (Exception ex)
            {
                LOGYAZ($"GetNextLogicalRef Error - Table: {tableName}", ex);
            }
            return maxRef + 1;
        }

        /// <summary>
        /// Belirtilen tablo ve TRCODE için bir sonraki fiş numarasını döndürür
        /// </summary>
        private static string GetNextFicheNo(string tableName, string ficheNoColumn, int trCode, string FIRMNR)
        {
            string prefix = "";
            int maxNo = 0;

            // TRCODE'a göre prefix belirle
            switch (trCode)
            {
                case 2: prefix = "SI"; break;  // Satış İade
                case 3: prefix = "BC"; break;  // Borç
                case 4: prefix = "AC"; break;  // Alacak / Mahsup
                case 7: prefix = "PS"; break;  // Perakende Satış
                case 8: prefix = "PI"; break;  // Perakende İade
                case 11: prefix = "NT"; break; // Nakit Tahsilat
                case 12: prefix = "NO"; break; // Nakit Ödeme
                case 70: prefix = "KK"; break; // Kredi Kartı
                case 71: prefix = "KI"; break; // Kredi Kartı İade
                case 1: prefix = "CK"; break;  // Çek Girişi
                default: prefix = "XX"; break;
            }

            try
            {
                // TRCODE'a göre en son numarayı bul
                DataTable dt = SqlSelectLogo($@"
                    SELECT TOP 1 {ficheNoColumn} FROM {tableName}
                    WHERE {ficheNoColumn} LIKE '{prefix}%' AND TRCODE={trCode}
                    ORDER BY {ficheNoColumn} DESC");

                if (dt.Rows.Count > 0)
                {
                    string lastNo = dt.Rows[0][0].ToString();
                    if (lastNo.Length > prefix.Length)
                    {
                        int.TryParse(lastNo.Substring(prefix.Length), out maxNo);
                    }
                }
            }
            catch { }

            return $"{prefix}{(maxNo + 1).ToString("D10")}";
        }

        #endregion
    }
}
