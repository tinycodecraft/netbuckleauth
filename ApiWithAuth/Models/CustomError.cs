using System.Text.Json.Serialization;

namespace ApiWithAuth.Models
{
    public class CustomError
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        /// <summary>
        /// A message from and to the Developer
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
