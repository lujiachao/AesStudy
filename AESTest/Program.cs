using Gaea.MySql;
using System;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace AESTest
{
    class Program
    {
        public static int _count = 0;
        static void Main(string[] args)
        {
            GaeaMySqlPower.Register("server=mysql01.rds.ucitymetro.com;port=3306;user id=dbtrans;password=Zhang123;database=itps_test;SslMode=none");
            var resultDataTable = OpenCSV(@"E:\csvs\result.csv");
            Console.WriteLine("程序运行结束");
            Console.ReadKey();
        }

        //手机号和姓名
        public static string AESDecrypt(string text, string key = "Jhek5ie*6ldh/kdb8g5da>ljbz-jhbvd")
        {
            try
            {
                var encryptBytes = Convert.FromBase64String(text);
                var aes = Aes.Create();
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Encoding.UTF8.GetBytes(key.Substring(0, 32));
                aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                var transform = aes.CreateDecryptor();
                var decryptBytes = transform.TransformFinalBlock(encryptBytes, 0, encryptBytes.Length);
                return Encoding.UTF8.GetString(decryptBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        //身份证号
        public static string Decrypt(string cipherText, string key = "Jhek5ie*6ldh/kdb8g5da>ljbz-jhbvd")
        {
            string plainText = "";
            try
            {
                var ivData = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                byte[] Key = Encoding.UTF8.GetBytes(key.Substring(0, 32));
                RijndaelManaged rijndael = new RijndaelManaged();
                ICryptoTransform transform = rijndael.CreateDecryptor(Key, ivData);
                byte[] bCipherText = Convert.FromBase64String(cipherText);//这里要用这个函数来正确转换Base64字符串成Byte数组
                MemoryStream ms = new MemoryStream(bCipherText);
                CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Read);
                byte[] bPlainText = new byte[bCipherText.Length];
                cs.Read(bPlainText, 0, bPlainText.Length); plainText = Encoding.ASCII.GetString(bPlainText);
                plainText = plainText.Trim('\0');
            }
            catch {
                return string.Empty;
            }
            return plainText;
        }

        public static DataTable OpenCSV(string filePath)
        {
            Encoding encoding = GetType(filePath); //Encoding.ASCII;//
            DataTable dt = new DataTable();
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            StreamReader sr = new StreamReader(fs, encoding);
            //string fileContent = sr.ReadToEnd();
            //encoding = sr.CurrentEncoding;
            //记录每次读取的一行记录
            string strLine = "";
            //记录每行记录中的各字段内容
            string[] arryLine = null;
            //逐行读取CSV中的数据
            while ((strLine = sr.ReadLine()) != null)
            {
                Console.WriteLine();
                try
                {
                    _count++;
                    arryLine = strLine.Split(",");
                    string mobilephone = AESDecrypt(arryLine[1]);
                    string license = Decrypt(arryLine[3]);
                    string name = AESDecrypt(arryLine[4]);
                    Console.WriteLine($"mobilephone:{mobilephone},license:{license},name:{name},count:{_count}");
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    ApplicationAuthUserAdapter applicationAuthUserAdapter = new ApplicationAuthUserAdapter();
                    applicationAuthUserAdapter.UpdateAuthUser(mobilephone,name,license);
                    stopwatch.Stop();
                    long time = stopwatch.ElapsedMilliseconds;
                }
                catch(Exception ex)
                {
                    continue;
                }
            }

            sr.Close();
            fs.Close();
            return dt;
        }

        /// 给定文件的路径，读取文件的二进制数据，判断文件的编码类型
        /// <param name="FILE_NAME">文件路径</param>
        /// <returns>文件的编码类型</returns>

        public static Encoding GetType(string FILE_NAME)
        {
            FileStream fs = new FileStream(FILE_NAME, FileMode.Open,FileAccess.Read);
            Encoding r = GetType(fs);
            fs.Close();
            return r;
        }

        /// 通过给定的文件流，判断文件的编码类型
        /// <param name="fs">文件流</param>
        /// <returns>文件的编码类型</returns>
        public static Encoding GetType(FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
            Encoding reVal = Encoding.Default;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = Encoding.Unicode;
            }
            r.Close();
            return reVal;
        }

        /// 判断是否是不带 BOM 的 UTF8 格式
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1;  //计算当前正分析的字符应还有的字节数
            byte curByte; //当前分析的字节.
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X　
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }

    }
}
