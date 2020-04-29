using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OwnIdSdk.NetCore3.Web.Abstractions;

namespace OwnIdSdk.NetCore3.Server
{
    public class SampleChallengeHandler : IChallengeHandler
    {
        public SampleChallengeHandler()
        {
            
        }
        
        public async Task UpdateProfileAsync(string did, Dictionary<string, string> profileFields, string publicKey)
        {
            string apiKey = "3_s5-gLs4aLp5FXluP8HXs7_JN40XWNlbvYWVCCkbNCqlhW6Sm5Z4tXGGsHcSJYD3W";
            string userKey = "your-user/app-key";
            string secretKey = "g157+kUR3kxvgIX4MneEWnVgBVzhQe4dXfoNe9ceSNA=";
            string method = "accounts.getAccountInfo";

            try
            {
                var http = new HttpClient();
                var s = await http.PostAsync(new Uri("https://accounts.us1.gigya.com/accounts.getAccountInfo"), new FormUrlEncodedContent(
                    new[]
                    {
                        new KeyValuePair<string, string>("apiKey", apiKey),
                        new KeyValuePair<string, string>("secret", secretKey),
                        new KeyValuePair<string, string>("UID", "did:idw:951dd54b-ce06-4421-a0ed-4296d6085e96")
                    }));
                var content = await s.Content.ReadAsStringAsync();
                
                
                var a = await http.PostAsync(new Uri("https://accounts.us1.gigya.com/accounts.notifyLogin"), new FormUrlEncodedContent(
                    new[]
                    {
                        new KeyValuePair<string, string>("apiKey", apiKey),
                        new KeyValuePair<string, string>("secret", secretKey),
                        new KeyValuePair<string, string>("siteUID", "did:idw:951dd54b-ce06-4421-a0ed-4296d6085e96"),
                        new KeyValuePair<string, string>("targetEnv", "browser")
                    }));
                var content2 = await a.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public Task OnSuccessLoginAsync(string did, HttpResponse response)
        {
            throw new NotImplementedException();
        }
    }
}