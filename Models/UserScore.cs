using System;
using System.Text.Json.Serialization;

namespace AdventureGameWeb.Models
{
    public class UserScore
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("highScore")]
        public int HighScore { get; set; }

        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; }
    }
}
