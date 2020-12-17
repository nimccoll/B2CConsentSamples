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
using System;
using System.Net;
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

        public ConsentController(IConfiguration configuration)
        {
            _configuration = configuration;
            _tableStorageConnectionString = _configuration.GetValue<string>("TableStorageConnectionString");
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_tableStorageConnectionString);
            _cloudTableClient = cloudStorageAccount.CreateCloudTableClient(new TableClientConfiguration());
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
    }
}
