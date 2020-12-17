//===============================================================================
// Microsoft FastTrack for Azure
// B2C Multiple Application Consent Samples
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
using System.Net.Http;

namespace B2CConsentClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = httpClient.GetAsync("https://localhost:44331/api/updateconsent?clientId=c3833288-9dc4-4d0a-96e8-5c9ee35f3aa4&objectId=c7f5d4a6-b9ae-4f54-b154-318bcc4275bc").Result;
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                }
            }
        }
    }
}
