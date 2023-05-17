using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace UrestComplaintWebApi
{
    public class Integrations
    {
        //private string smsUrl = ConfigurationManager.AppSettings.Keys[4].  //"https://api.textlocal.in/send/?"; //Startup.Configuration.GetSection("SMSInfo").GetSection("Url").Value.ToString();
        //private string Key = ConfigurationManager.AppSettings.Keys[5].ToString();  //"NGY0ODY4NDQ3MDRlMzU0ZjU4NDg0MTY4NTQ0NTc0NjM="; //Startup.Configuration.GetSection("SMSInfo").GetSection("Key").Value.ToString();
        //private string Sender = ConfigurationManager.AppSettings.Keys[6].ToString();  //"URSTIN";// Startup.Configuration.GetSection("SMSInfo").GetSection("Sender").Value.ToString();

        private string Url = "https://api.textlocal.in/send/?"; //Startup.Configuration.GetSection("SMSInfo").GetSection("Url").Value.ToString();
        private string Key = "NGY0ODY4NDQ3MDRlMzU0ZjU4NDg0MTY4NTQ0NTc0NjM="; //Startup.Configuration.GetSection("SMSInfo").GetSection("Key").Value.ToString();
        private string Sender = "URSTIN";// Startup.Configuration.GetSection("SMSInfo").GetSection("Sender").Value.ToString();

        public async Task<string> SendOTP(string ComplainBy, int Length)
        {
            string response = "Success";
            try
            {
                string otp = GenerateOTP(Length);
                string clientSMS = "Your OTP to access Urest facility management dashboard is " + otp;// GenerateOTP(Length);
                bool smsst = await SendSMS(ComplainBy, clientSMS);

                response = otp;
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            return response;
        }

        private async Task<bool> SendSMS(string mobileNo, string message)
        {
            bool st = false;
            string finalUrl = "";

            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(Url) && !string.IsNullOrEmpty(Key) && !string.IsNullOrEmpty(Sender))
                {
                    finalUrl = $"{Url}apiKey={Key}&sender={Sender}&numbers=91{mobileNo}&message={message}";
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(finalUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                string res = reader.ReadToEnd();
                dynamic stuff = Newtonsoft.Json.JsonConvert.DeserializeObject(res);
                if (stuff != null)
                {
                    if (stuff.status == "success")
                    {
                        st = true;
                    }
                }

            });

            return st;
        }

        private string GenerateOTP(int Length)
        {
            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };

            return GenerateRandomOTP(Length, saAllowedCharacters);
        }

        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;

            Random rand = new Random();
            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }

            return sOTP;
        }

    }
}