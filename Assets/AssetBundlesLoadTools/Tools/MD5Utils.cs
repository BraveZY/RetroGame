using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
public class MD5Utils
{

    public static string GetMD5HashFromFile(string fileName)
    {
        StringBuilder sb = new StringBuilder();
        using (FileStream file = new FileStream(fileName, FileMode.Open))
        {
            try
            {
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }

            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
        return sb.ToString();
    }
}
