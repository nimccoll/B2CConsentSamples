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
using B2CConsentAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace B2CConsentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsentController : ControllerBase
    {
        private static readonly string TABLE_NAME = "B2CConsent";
        private static readonly int CONSENT_MONTHS = 6;
        private readonly IConfiguration _configuration;
        private readonly string _tableStorageConnectionString;
        private CloudTableClient _cloudTableClient;
        private readonly string _aadInstance = "https://login.microsoftonline.com/";
        private readonly string _aadGraphResourceId = "https://graph.windows.net/";
        private readonly string _aadGraphEndpoint = "https://graph.windows.net/";
        private readonly string _aadGraphVersion = "api-version=1.6";
        private readonly string _fhirServerServicePrincipalId;
        private readonly string _tenant;
        private ClientCredential _credential;
        private AuthenticationContext _authContext;
        private AuthenticationResult _authenticationResult;

        public ConsentController(IConfiguration configuration)
        {
            _configuration = configuration;
            _tableStorageConnectionString = _configuration.GetValue<string>("TableStorageConnectionString");
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_tableStorageConnectionString);
            _cloudTableClient = cloudStorageAccount.CreateCloudTableClient(new TableClientConfiguration());
            _tenant = _configuration.GetValue<string>("Tenant");
            _authContext = new AuthenticationContext($"{_aadInstance}{_tenant}");
            string clientId = _configuration.GetValue<string>("ClientId");
            string clientSecret = _configuration.GetValue<string>("ClientSecret");
            _credential = new ClientCredential(clientId, clientSecret);
            _fhirServerServicePrincipalId = _configuration.GetValue<string>("FHIRServerServicePrincipalId");
        }

        [HttpGet]
        public async Task<IActionResult> Get(string clientId, string objectId)
        {
            ResponseContent responseContent = new ResponseContent()
            {
                version = "1.0.0",
                status = (int)HttpStatusCode.OK,
                clientId = clientId,
                objectId = objectId
            };

            CloudTable consentTable = _cloudTableClient.GetTableReference(TABLE_NAME);
            bool exists = await consentTable.ExistsAsync();

            if (exists)
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<ConsentEntity>(clientId, objectId);
                TableResult result = await consentTable.ExecuteAsync(retrieveOperation);
                ConsentEntity consentEntity = result.Result as ConsentEntity;
                if (consentEntity != null)
                {
                    if (consentEntity.ConsentDateTime >= DateTime.Now)
                    {
                        responseContent.hasConsented = "true";
                    }
                    else
                    {
                        responseContent.hasConsented = "false";
                    }
                }
                else
                {
                    responseContent.hasConsented = "false";
                }
                responseContent.userRole = await GetUserRole(objectId);

                return new OkObjectResult(responseContent);
            }
            else
            {
                return new BadRequestObjectResult($"Table {TABLE_NAME} does not exist.");
            }
        }

        [Route("/api/UpdateConsent")]
        [HttpGet]
        public async Task<IActionResult> SaveConsent(string clientId, string objectId)
        {
            ResponseContent responseContent = new ResponseContent()
            {
                version = "1.0.0",
                status = (int)HttpStatusCode.OK,
                clientId = clientId,
                objectId = objectId
            };

            CloudTable consentTable = _cloudTableClient.GetTableReference(TABLE_NAME);
            bool exists = await consentTable.ExistsAsync();

            if (exists)
            {
                ConsentEntity consentEntity = new ConsentEntity()
                {
                    ClientId = clientId,
                    ObjectId = objectId,
                    ConsentDateTime = DateTime.Now.AddMonths(CONSENT_MONTHS)
                };
                TableOperation updateOperation = TableOperation.InsertOrReplace(consentEntity);
                TableResult result = await consentTable.ExecuteAsync(updateOperation);
                if (result.HttpStatusCode == (int)HttpStatusCode.NoContent)
                {
                    responseContent.hasConsented = "true";
                    return new OkObjectResult(responseContent);
                }
                else
                {
                    return new BadRequestObjectResult($"Failed to update consent information in table {TABLE_NAME}. HTTP status code is {result.HttpStatusCode}.");
                }
            }
            else
            {
                return new BadRequestObjectResult($"Table {TABLE_NAME} does not exist.");
            }
        }

        public async Task<string> GetUserRole(string objectId)
        {
            string role = string.Empty;
            AppRoleAssignments roles = null;
            
            string result = await SendGraphRequest($"/users/{objectId}/appRoleAssignments", null, null, HttpMethod.Get);
            roles = JsonConvert.DeserializeObject<AppRoleAssignments>(result);

            if (roles != null)
            {
                foreach (AppRoleAssignment appRoleAssignment in roles.value)
                {
                    if (appRoleAssignment.resourceId == _fhirServerServicePrincipalId)
                    {
                        ServicePrincipal servicePrincipal = await GetServicePrincipal(_fhirServerServicePrincipalId);
                        foreach (AppRole appRole in servicePrincipal.appRoles)
                        {
                            if (appRole.id == appRoleAssignment.id)
                            {
                                role = appRole.value;
                                break;
                            }
                        }
                    }
                }
            }

            return role;
        }

        public async Task<ServicePrincipal> GetServicePrincipal(string objectId)
        {
            ServicePrincipal servicePrincipal = null;
            string result = await SendGraphRequest($"/servicePrincipals/{objectId}", null, null, HttpMethod.Get);
            servicePrincipal = JsonConvert.DeserializeObject<ServicePrincipal>(result);

            return servicePrincipal;
        }

        /// <summary>
        /// Handle Graph user API, support following HTTP methods: GET, POST and PATCH
        /// </summary>
        private async Task<string> SendGraphRequest(string api, string query, string data, HttpMethod method)
        {
            // Get the access toke to Graph API
            string accessToken = await AcquireAccessToken();

            // Set the Graph url. Including: Graph-endpoint/tenant/users?api-version&query
            string url = $"{_aadGraphEndpoint}{_tenant}{api}?{_aadGraphVersion}";

            if (!string.IsNullOrEmpty(query))
            {
                url += "&" + query;
            }

            //Trace.WriteLine($"Graph API call: {url}");
            try
            {
                using (HttpClient http = new HttpClient())
                using (HttpRequestMessage request = new HttpRequestMessage(method, url))
                {
                    // Set the authorization header
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    // For POST and PATCH set the request content 
                    if (!string.IsNullOrEmpty(data))
                    {
                        //Trace.WriteLine($"Graph API data: {data}");
                        request.Content = new StringContent(data, Encoding.UTF8, "application/json");
                    }

                    // Send the request to Graph API endpoint
                    using (HttpResponseMessage response = await http.SendAsync(request))
                    {
                        string error = await response.Content.ReadAsStringAsync();

                        // Check the result for error
                        if (!response.IsSuccessStatusCode)
                        {
                            // Throw server busy error message
                            if (response.StatusCode == (HttpStatusCode)429)
                            {
                                // TBD: Add you error handling here
                            }

                            throw new Exception(error);
                        }

                        // Return the response body, usually in JSON format
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception)
            {
                // TBD: Add you error handling here
                throw;
            }
        }

        private async Task<string> AcquireAccessToken()
        {
            if (_authenticationResult == null ||
                (_authenticationResult.ExpiresOn.UtcDateTime < DateTime.UtcNow))
            {
                try
                {
                    _authenticationResult = await _authContext.AcquireTokenAsync(_aadGraphResourceId, _credential);
                }
                catch (Exception ex)
                {
                    // TBD: Add you error handling here
                    throw;
                }
            }

            return _authenticationResult.AccessToken;
        }
    }
}
