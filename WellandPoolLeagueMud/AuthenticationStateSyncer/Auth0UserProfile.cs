using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace WellandPoolLeagueMud.AuthenticationStateSyncer
{
    public partial class Auth0UserProfile
    {
        [JsonProperty("email_verified")]
        public bool EmailVerified { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("picture")]
        public Uri? Picture { get; set; }

        [JsonProperty("family_name")]
        public string? FamilyName { get; set; }

        [JsonProperty("user_id")]
        public string? UserId { get; set; }

        [JsonProperty("nickname")]
        public string? Nickname { get; set; }

        [JsonProperty("given_name")]
        public string? GivenName { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("identities")]
        public Identity[]? Identities { get; set; }

        [JsonProperty("last_login")]
        public DateTimeOffset LastLogin { get; set; }

        [JsonProperty("last_ip")]
        public string? LastIp { get; set; }

        [JsonProperty("logins_count")]
        public long LoginsCount { get; set; }

        [JsonProperty("app_metadata")]
        public AppMetadata? AppMetadata { get; set; }
    }

    public partial class AppMetadata
    {
        [JsonProperty("roles")]
        public string[]? Roles { get; set; }
    }

    public partial class Identity
    {
        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("access_token")]
        public string? AccessToken { get; set; }

        [JsonProperty("connection")]
        public string? Connection { get; set; }

        [JsonProperty("user_id")]
        public string? UserId { get; set; }

        [JsonProperty("provider")]
        public string? Provider { get; set; }

        [JsonProperty("isSocial")]
        public bool IsSocial { get; set; }
    }

    public partial class Welcome6
    {
        public static Welcome6[] FromJson(string json) => JsonConvert.DeserializeObject<Welcome6[]>(json, Converter.Settings) ?? Array.Empty<Welcome6>();
    }

    public static class Serialize
    {
        public static string ToJson(this Welcome6[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

}

