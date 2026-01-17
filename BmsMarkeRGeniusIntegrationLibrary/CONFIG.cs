using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace BmsMarkeRGeniusIntegrationLibrary
{
    public class CONFIG
    {
        public string LGDBSERVER { get; set; } = string.Empty;
        public string LGDBDATABASE { get; set; } = string.Empty;
        public string LGDBUSERNAME { get; set; } = string.Empty;
        public string LGDBPASSWORD { get; set; } = string.Empty;
        public string FIRMNR { get; set; } = string.Empty;
        public string PERIOD { get; set; } = string.Empty;
        public string ISFIRMBASEDCURR { get; set; } = string.Empty;
        public string DODEBTCLOSE { get; set; } = string.Empty;
        public string DefaultBranchForGeniusSending { get; set; } = string.Empty;
        public string LOBJECTDEFAULTUSERNAME { get; set; } = string.Empty;
        public string LOBJECTDEFAULTPASSWORD { get; set; } = string.Empty;
        public string LOGGEDIN_FIRMNR { get; set; } = string.Empty;
        public string OTHERSERVER { get; set; } = string.Empty;
        public string OTHERPORT { get; set; } = string.Empty;
        public string OTHERDATABASE { get; set; } = string.Empty;
        public string OTHERUSERNAME { get; set; } = string.Empty;
        public string OTHERPASSWORD { get; set; } = string.Empty;
        public string APIURL { get; set; } = string.Empty;
        public string APIUSERNAME { get; set; } = string.Empty;
        public string APIPASSWORD { get; set; } = string.Empty;
        public string ISNCRACTIVE { get; set; } = string.Empty;
        public string ISGENIUSACTIVE { get; set; } = string.Empty;
        public string NCRBASEURL { get; set; } = string.Empty;
        public string NCRUSERNAME { get; set; } = string.Empty;
        public string NCRPASSWORD { get; set; } = string.Empty;
        public string FILEENCODING { get; set; } = "UTF8"; // UTF8 veya ANSI
        public string ITEM_SERVICE_BRANCHES { get; set; } = "1,2"; // ItemService için mağaza numaraları (virgülle ayrılmış, IbmKasa tablosundaki LogoValue değerleri)
        public string GENIUSAPIPORT { get; set; } = "9996"; // Genius API Port
    }
    public class CONFIG_HELPER
    {
        public static string _xmlPath = AppDomain.CurrentDomain.BaseDirectory + "BMDB.xml";
        public static string _datPath = AppDomain.CurrentDomain.BaseDirectory + "BMDB.cfg";
        public static string _key = "0WXOM7IKTM012016";
        public static CONFIG GET_CONFIG()
        {
            try
            {
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    byte[] key = ASCIIEncoding.UTF8.GetBytes(_key);

                    byte[] IV = ASCIIEncoding.UTF8.GetBytes(_key);

                    using (FileStream fsCrypt = new FileStream(_datPath, FileMode.Open))
                    {
                        using (FileStream fsOut = new FileStream(_xmlPath, FileMode.Create))
                        {
                            using (ICryptoTransform decryptor = aes.CreateDecryptor(key, IV))
                            {
                                using (CryptoStream cs = new CryptoStream(fsCrypt, decryptor, CryptoStreamMode.Read))
                                {
                                    int data;
                                    while ((data = cs.ReadByte()) != -1)
                                    {
                                        fsOut.WriteByte((byte)data);
                                    }
                                }
                            }
                        }
                    }
                }
                CONFIG CFG = new CONFIG();
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(_xmlPath);
                if (File.Exists(_xmlPath))
                    File.Delete(_xmlPath);
                XmlNodeList xNode = xDoc.GetElementsByTagName("BILMARKSOFTWARE");
                XmlNode xNodeLGDB = xNode[0].ChildNodes[0];
                XmlNode xNodeCAPIFIRM = xNode[0].ChildNodes[1];
                XmlNode xNodeUSERDEFAULTS = xNode[0].ChildNodes[2];
                XmlNode xNodeOTHERDB = xNode[0].ChildNodes[3];
                XmlNode xNodeAPI = xNode[0].ChildNodes[4];
                XmlNode xNodeINTEGRATION = xNode[0].ChildNodes.Count > 5 ? xNode[0].ChildNodes[5] : null;
                try { CFG.LGDBUSERNAME = xNodeLGDB.ChildNodes[0].InnerText; } catch { }
                try { CFG.LGDBPASSWORD = xNodeLGDB.ChildNodes[1].InnerText; } catch { }
                try { CFG.LGDBSERVER = xNodeLGDB.ChildNodes[2].InnerText; } catch { }
                try { CFG.LGDBDATABASE = xNodeLGDB.ChildNodes[3].InnerText; } catch { }
                try { CFG.FIRMNR = xNodeCAPIFIRM.ChildNodes[0].InnerText; } catch { }
                try { CFG.PERIOD = xNodeCAPIFIRM.ChildNodes[1].InnerText; } catch { }
                try { CFG.ISFIRMBASEDCURR = xNodeCAPIFIRM.ChildNodes[2].InnerText; } catch { }
                try { CFG.DODEBTCLOSE = xNodeCAPIFIRM.ChildNodes[3].InnerText; } catch { }
                try { CFG.DefaultBranchForGeniusSending = xNodeCAPIFIRM.ChildNodes[4].InnerText; } catch { }
                try { CFG.LOBJECTDEFAULTUSERNAME = xNodeUSERDEFAULTS.ChildNodes[0].InnerText; } catch { }
                try { CFG.LOBJECTDEFAULTPASSWORD = xNodeUSERDEFAULTS.ChildNodes[1].InnerText; } catch { }
                try { CFG.OTHERUSERNAME = xNodeOTHERDB.ChildNodes[0].InnerText; } catch { }
                try { CFG.OTHERPASSWORD = xNodeOTHERDB.ChildNodes[1].InnerText; } catch { }
                try { CFG.OTHERSERVER = xNodeOTHERDB.ChildNodes[2].InnerText; } catch { }
                try { CFG.OTHERPORT = xNodeOTHERDB.ChildNodes[3].InnerText; } catch { }
                try { CFG.OTHERDATABASE = xNodeOTHERDB.ChildNodes[4].InnerText; } catch { }
                try { CFG.APIURL = xNodeAPI.ChildNodes[0].InnerText; } catch { }
                try { CFG.APIUSERNAME = xNodeAPI.ChildNodes[1].InnerText; } catch { }
                try { CFG.APIPASSWORD = xNodeAPI.ChildNodes[2].InnerText; } catch { }
                if (xNodeINTEGRATION != null)
                {
                    try { CFG.ISNCRACTIVE = xNodeINTEGRATION.ChildNodes[0].InnerText; } catch { }
                    try { CFG.ISGENIUSACTIVE = xNodeINTEGRATION.ChildNodes[1].InnerText; } catch { }
                    try { CFG.NCRBASEURL = xNodeINTEGRATION.ChildNodes[2].InnerText; } catch { }
                    try { CFG.NCRUSERNAME = xNodeINTEGRATION.ChildNodes[3].InnerText; } catch { }
                    try { CFG.NCRPASSWORD = xNodeINTEGRATION.ChildNodes[4].InnerText; } catch { }
                    try { CFG.FILEENCODING = xNodeINTEGRATION.ChildNodes[5].InnerText; } catch { }
                    try { CFG.ITEM_SERVICE_BRANCHES = xNodeINTEGRATION.ChildNodes[6].InnerText; } catch { }
                    try { CFG.GENIUSAPIPORT = xNodeINTEGRATION.ChildNodes[7].InnerText; } catch { }
                }
                return CFG;
            }
            catch
            {
                return null;
            }
        }
        public static void DecryptFile(string inputFile, string outputFile, string skey)
        {
            try
            {
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    byte[] key = ASCIIEncoding.UTF8.GetBytes(skey);
                    byte[] IV = ASCIIEncoding.UTF8.GetBytes(skey);

                    using (FileStream fsCrypt = new FileStream(inputFile, FileMode.Open))
                    {
                        using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                        {
                            using (ICryptoTransform decryptor = aes.CreateDecryptor(key, IV))
                            {
                                using (CryptoStream cs = new CryptoStream(fsCrypt, decryptor, CryptoStreamMode.Read))
                                {
                                    int data;
                                    while ((data = cs.ReadByte()) != -1)
                                    {
                                        fsOut.WriteByte((byte)data);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }
        public static bool EncryptFile(string inputFile, string outputFile, string skey)
        {
            try
            {
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    byte[] key = ASCIIEncoding.UTF8.GetBytes(skey);
                    byte[] IV = ASCIIEncoding.UTF8.GetBytes(skey);

                    using (FileStream fsCrypt = new FileStream(outputFile, FileMode.Create))
                    {
                        using (ICryptoTransform encryptor = aes.CreateEncryptor(key, IV))
                        {
                            using (CryptoStream cs = new CryptoStream(fsCrypt, encryptor, CryptoStreamMode.Write))
                            {
                                using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                                {
                                    int data;
                                    while ((data = fsIn.ReadByte()) != -1)
                                    {
                                        cs.WriteByte((byte)data);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
