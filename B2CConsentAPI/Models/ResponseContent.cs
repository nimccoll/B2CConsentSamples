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
namespace B2CConsentAPI.Models
{
    public class ResponseContent
    {
        public string version { get; set; }
        public int status { get; set; }
        public string clientId { get; set; }
        public string objectId { get; set; }
        public string hasConsented { get; set; }
    }
}
