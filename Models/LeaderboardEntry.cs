using System;
using System.Text.Json.Serialization;

namespace AdventureGameWeb.Models
{
    public class LeaderboardEntry
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = "Anonymous";

        [JsonPropertyName("character_class")]
        public string CharacterClass { get; set; } = "Warrior";

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("play_time_seconds")]
        public int PlayTimeSeconds { get; set; }

        [JsonPropertyName("achievements_count")]
        public int AchievementsCount { get; set; }

        [JsonPropertyName("is_victory")]
        public bool IsVictory { get; set; }
    }
}
