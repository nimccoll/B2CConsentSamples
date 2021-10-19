using Newtonsoft.Json;
using System.Collections.Generic;

namespace B2CConsentAPI.Models
{
    public class AppRole
    {
        public List<string> allowedMemberTypes { get; set; }
        public string description { get; set; }
        public string displayName { get; set; }
        public string id { get; set; }
        public bool isEnabled { get; set; }
        public string value { get; set; }
    }

    public class InformationalUrls
    {
        public object termsOfService { get; set; }
        public object support { get; set; }
        public object privacy { get; set; }
        public object marketing { get; set; }
    }

    public class Oauth2Permissions
    {
        public string adminConsentDescription { get; set; }
        public string adminConsentDisplayName { get; set; }
        public string id { get; set; }
        public bool isEnabled { get; set; }
        public string type { get; set; }
        public string userConsentDescription { get; set; }
        public string userConsentDisplayName { get; set; }
        public string value { get; set; }
    }

    public class ServicePrincipal
    {
        [JsonProperty("odata.type")]
        public string OdataType { get; set; }
        public string objectType { get; set; }
        public string objectId { get; set; }
        public object deletionTimestamp { get; set; }
        public bool accountEnabled { get; set; }
        public List<object> addIns { get; set; }
        public List<object> alternativeNames { get; set; }
        public string appDisplayName { get; set; }
        public string appId { get; set; }
        public object applicationTemplateId { get; set; }
        public string appOwnerTenantId { get; set; }
        public bool appRoleAssignmentRequired { get; set; }
        public List<AppRole> appRoles { get; set; }
        public string displayName { get; set; }
        public object errorUrl { get; set; }
        public string homepage { get; set; }
        public InformationalUrls informationalUrls { get; set; }
        public List<object> keyCredentials { get; set; }
        public string logoutUrl { get; set; }
        public List<object> notificationEmailAddresses { get; set; }
        public List<Oauth2Permissions> oauth2Permissions { get; set; }
        public List<object> passwordCredentials { get; set; }
        public object preferredSingleSignOnMode { get; set; }
        public object preferredTokenSigningKeyEndDateTime { get; set; }
        public object preferredTokenSigningKeyThumbprint { get; set; }
        public string publisherName { get; set; }
        public List<string> replyUrls { get; set; }
        public object samlMetadataUrl { get; set; }
        public object samlSingleSignOnSettings { get; set; }
        public List<string> servicePrincipalNames { get; set; }
        public string servicePrincipalType { get; set; }
        public string signInAudience { get; set; }
        public List<string> tags { get; set; }
        public object tokenEncryptionKeyId { get; set; }
    }

    public class ServicePrincipals
    {
        [JsonProperty("odata.metadata")]
        public string OdataMetadata { get; set; }
        public List<ServicePrincipal> value { get; set; }
    }

}
