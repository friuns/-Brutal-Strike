using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using fastJSON;
#if !game && !sdk
using System.Diagnostics;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using WebApplication4;
#endif

public class MyProduct
{
    public int bcoins;
    public string receipt = "";
    public string transation;
    public string productName;
    public bool failed;
    public DateTime date;
    public string orderId;
    public int userid;
    public class Receipt
    {
        public string Payload;
        public string Store;
        public string TransitionID;
    }
    
    public class PayLoad
    {
        public string json;
        public string signature;
        public string skuDetails;    
    }
#if !game && !sdk 
    public bool Validate()
    {
        Receipt r = JSON.ToObject<Receipt>(receipt);
        PayLoad payload = JSON.ToObject<PayLoad>(r.Payload);
        orderId = BsonDocument.Parse(payload.json)["orderId"].AsString;
        byte[] fromBase64String = Convert.FromBase64String(Controller.certPem);
        var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(fromBase64String,out int bts);
        return rsa.VerifyData(UTF8Encoding.UTF8.GetBytes(payload.json), Convert.FromBase64String(payload.signature), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
    }
    public bool Validate2()
    {
        bool valid = true;
        for (int i = 0; i < 50; i++)
        {
            if (!Debugger.IsAttached)
                Thread.Sleep(10000);
            try
            {
                var handler = new HttpClientHandler();
                handler.UseCookies = false;
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (var httpClient = new HttpClient(handler))
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://playconsolemonetization-pa.clients6.google.com/v1/developer/4873487411482083881/orders:fetch?%24httpHeaders=Content-Type%3Aapplication%2Fjson%2Bprotobuf%0D%0AX-Goog-Api-Key%3AAIzaSyBAha_rcoO_aGsmiR5fWbNfdOjqT0gXwbk%0D%0AX-Play-Console-Session-Id%3A46380407%0D%0AX-Goog-AuthUser%3A0%0D%0AAuthorization%3ASAPISIDHASH+1647767700612_b67dfaf3416f7289275880f2ec9e01f2c963a839%0D%0A"))
                    {
                        request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:98.0) Gecko/20100101 Firefox/98.0");
                        request.Headers.TryAddWithoutValidation("Accept", "*/*");
                        request.Headers.TryAddWithoutValidation("Accept-Language", "en-GB");
                        request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
                        request.Headers.TryAddWithoutValidation("Referer", "https://play.google.com/");
                        request.Headers.TryAddWithoutValidation("Origin", "https://play.google.com");
                        request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                        request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                        request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "no-cors");
                        request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-site");
                        request.Headers.TryAddWithoutValidation("Pragma", "no-cache");
                        request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
                        request.Headers.TryAddWithoutValidation("Cookie", "NID=511=mMUdKYsmie9zZZvq5HqZNbTjKCZUh9QoOMiRSvUJZLdfnm1YLZXlmXkgGtGh1e5YFYy974hQjm2SzXL08YLf8kUaQLFnIKXySpPjSF1BsFUObif17fK2pPZNGWk-G0TVbvBAfnzdO9vWW9Aap2u8F-1Rl1L_KTo9m9UXIBmGSARyQu9LXw3_lTPpEvmaa2filyqQFeS5W_nNNRAIgU4sELuCMauitxIqRFYgS6vfBAg8yR1Wfuxs6oF7vwclzQYqfVMJU1l40Lf7MF8pXKvFtuNwh7bCzvdZf6LjDDUCKrZlQcUk3nd63TiYHWcKaw; SID=IQjcXaRD8qcYgHTMI0CPpGKsY1T6g3Sl7wRVWMce2VcFSyX_QkZF9vUpokXewkE0C7b9pg.; __Secure-3PSID=IQjcXaRD8qcYgHTMI0CPpGKsY1T6g3Sl7wRVWMce2VcFSyX_9j9E3E3BR2GdOLHz2CG44w.; HSID=A_mYD2iLUOZPT0EQC; SSID=ABm0HR8_0_nQGOT2I; APISID=FeTl-0UCi2-__gEd/AAjCfZ-_L9ZFtJ2_2; SAPISID=qHo9OSCPzsqpHgUG/Az7y3qDGmDVbHOEi4; __Secure-3PAPISID=qHo9OSCPzsqpHgUG/Az7y3qDGmDVbHOEi4; CONSENT=YES+FI.en+20150712-15-0; SIDCC=AJi4QfEfFn6vXSfRnR2dcLubA6uZR4V3rjofL9i_uAqOD9hSJnwcX7vd4LLdry6SSEQucIcQb10; __Secure-3PSIDCC=AJi4QfGbxYQLnwQXqE5LvJB3vSyeustOo36ddBakZsj9UHNJ0ZK4EK6sL3RdW-wjaNdWpz3inK4; SEARCH_SAMESITE=CgQIn5QB; __Secure-1PSID=IQjcXaRD8qcYgHTMI0CPpGKsY1T6g3Sl7wRVWMce2VcFSyX_agX6QaL1eOBbsZPpkCUUTg.; __Secure-1PAPISID=qHo9OSCPzsqpHgUG/Az7y3qDGmDVbHOEi4; OGPC=19025836-1:; 1P_JAR=2022-03-19-12; ANID=AHWqTUkfgPULIJCf_e11Vs_h2VYf9SELhjon-pE3jVr3kDmb2C4uC68kTUG24M9i; AEC=AVQQ_LAuPN431XNPT_bz_teA5n00YlNK-e3Ydine5vagnV7FdK0MKTE2IU8");
                        request.Content = new StringContent("{\"4\":{\"1\":{\"1\":\"1199145600\",\"2\":0},\"2\":{\"1\":\"1647820799\",\"2\":0},\"3\":\"" + orderId + "\"},\"5\":{\"1\":\"4873487411482083881\"},\"7\":\"\",\"8\":25}");
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain;charset=UTF-8");
                        HttpResponseMessage response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                        string d = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        // Console.WriteLine(d);
                        // var validate = "GPA.3327-4473-3723-15095";
                        if (d.Contains(orderId))
                        {
                            if(d.Contains("successfully charged") || d.Contains("passed all risk checks"))
                            // if (d.Contains(orderId + ":0") || Regex.Match(d, orderId + ".,.2.:..*?,.3.:3").Success)
                            {
                                Controller.LogFile(userid, "order id found " + orderId, LogFiles.purchase);
                                return true;
                            }
                            else if (d.Contains("canceled"))
                            {
                                Controller.LogFile(userid, "canceled " + orderId, LogFiles.purchase);
                                return false;
                            }
                            else
                            {
                                valid = false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Controller.LogFile(userid, "error " + orderId + " " + e.Message, LogFiles.purchase);
            }
        }
        if(!valid)
            Controller.LogFile(userid, "order canceled id" + orderId, LogFiles.purchase);
        else
            Controller.LogFile(userid, "order id not found " + orderId, LogFiles.purchase);
        return valid;
    }
    
#endif
  
}

public class NameID: IEquatable<NameID>
{
    // [FieldAtrStart(inherit = true)] 
    public string name = "";
    // [BsonId]
    public int id;
    public override string ToString()
    {
        return name + "(" + id + ")";
    }
    public static implicit operator NameID(NameIDBase dd)
    {
        return new NameID() {name = dd.name, id = dd.id};
    }
    public bool Equals(NameID other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return id == other.id;
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((NameID)obj);
    }
    public override int GetHashCode()
    {
        return id;
    }
}



public abstract class NameIDBase //use this for base! to avoid whole supper class serialized in mongo db 
{

    // [BsonId]
    // [FieldAtrStart(inherit = true)]
    public int id;
    public string name = "";
    public override string ToString()
    {
        return name + "(" + id + ")";
    }
    // public NameIDBase nameID { get { return new NameIDBase() {id = id, name = name}; } }
}

[Flags]
public enum GameType
{
    // #if !game
    Any=~0,
    // #endif
    Classic=2, DeathMatch=4, TDM=8, Mod=32, Survival=64,Mission=128,
    zombieMode = 256,
    ranked1v1 = 1024,
    RunMode = 2048,
    ImposterMode = 4096
}
[Flags]
public enum SupportedPlatforms
{
    unity3dwindows = 1,
    unity3dandroid = 2,
    unity3dios = 4,
    unity3dwebgl = 8,
    All = ~0,
}

public static class ext234
{
    public static  unsafe int GetHashCode2(this string _str)
    {
        fixed (char* str = _str)
        {
            char* chPtr = str;
            int num = 352654597;
            int num2 = num;
            int* numPtr = (int*)chPtr;
            for (int i = _str.Length; i > 0; i -= 4)
            {
                num = (((num << 5) + num) + (num >> 27)) ^ numPtr[0];
                if (i <= 2)
                {
                    break;
                }
                num2 = (((num2 << 5) + num2) + (num2 >> 27)) ^ numPtr[1];
                numPtr += 2;
            }
            return (num + (num2 * 1566083941));
        }
    }
}


public static class SimpleEncrypt2
{
    static Random r;

    private static byte[] rand = new byte[0];
    public static byte[] Encrypt(byte[] a)
    {
        ResizeRand(a.Length);
        byte[] newa = new byte[a.Length];
        for (int i = 0; i < a.Length; i++)
            newa[i] = (byte)(a[i] ^ rand[i]);
        return newa;
    }

    
    public static string Encrypt(string a)
    {
        ResizeRand(a.Length);
        char[] newa = new char[a.Length];
        for (int i = 0; i < a.Length; i++)
            newa[i] = (char)(a[i] ^ rand[i]);
        return new string(newa);
    }
    
    
    private static void ResizeRand(int aLength)
    {
        if (aLength > rand.Length)
        {
            r = new Random(3489349);
            Array.Resize(ref rand, aLength);
            r.NextBytes(rand);
        }
    }
}

public class UserBan
{
    // #if !game
    // [MongoDB.Bson.Serialization.Attributes.BsonId]
    // #endif
    public string banId;
    public NameID banUserID;
    public NameID bannedby;
    public string msg;
    public DateTime banTime;
    public DateTime banUntil;


    public bool serverBan = true;
    public bool skinBan = false;
}