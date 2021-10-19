using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace B2CConsentAPI.Models
{
    public class AppRoleAssignment
    {
        [JsonProperty("odata.type")]
        public string OdataType { get; set; }
        public string objectType { get; set; }
        public string objectId { get; set; }
        public object deletionTimestamp { get; set; }
        public DateTime creationTimestamp { get; set; }
        public string id { get; set; }
        public string principalDisplayName { get; set; }
        public string principalId { get; set; }
        public string principalType { get; set; }
        public string resourceDisplayName { get; set; }
        public string resourceId { get; set; }
    }

    public class AppRoleAssignments
    {
        [JsonProperty("odata.metadata")]
        public string OdataMetadata { get; set; }
        public List<AppRoleAssignment> value { get; set; }
    }
}
