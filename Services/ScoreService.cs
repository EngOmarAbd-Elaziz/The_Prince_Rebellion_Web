using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AdventureGameWeb.Models;

namespace AdventureGameWeb.Services
{
    public class ScoreService
    {
        private const string StorageKey = "pr_user_scores";
        private readonly LocalStorageService _storage;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public ScoreService(LocalStorageService storage)
        {
            _storage = storage;
        }

        public async Task<bool> SaveOrUpdateScoreAsync(string username, int score)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            var normalizedName = username.Trim();
            var normalizedScore = Math.Max(0, score);
            var scores = await LoadAllAsync();
            var existingIndex = scores.FindIndex(s =>
                string.Equals(s.Username, normalizedName, StringComparison.OrdinalIgnoreCase));

            if (existingIndex >= 0)
            {
                var existing = scores[existingIndex];
                if (normalizedScore > existing.HighScore)
                {
                    existing.HighScore = normalizedScore;
                    existing.LastUpdated = DateTime.UtcNow;
                }
            }
            else
            {
                scores.Add(new UserScore
                {
                    Username = normalizedName,
                    HighScore = normalizedScore,
                    LastUpdated = DateTime.UtcNow
                });
            }

            scores.Sort((a, b) => b.HighScore.CompareTo(a.HighScore));
            return await SaveAllAsync(scores);
        }

        public async Task<List<UserScore>> GetTop10ScoresAsync()
        {
            var scores = await LoadAllAsync();
            return scores
                .OrderByDescending(s => s.HighScore)
                .ThenBy(s => s.LastUpdated)
                .Take(10)
                .ToList();
        }

        public async Task<UserScore?> GetScoreByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var scores = await LoadAllAsync();
            return scores.Find(s =>
                string.Equals(s.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static UserScore CreateEmptyProfile(string username) => new()
        {
            Username = string.IsNullOrWhiteSpace(username) ? "Guest" : username.Trim(),
            HighScore = 0,
            LastUpdated = DateTime.MinValue
        };

        private async Task<List<UserScore>> LoadAllAsync()
        {
            try
            {
                var json = await _storage.GetItemAsync(StorageKey);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<UserScore>();
                }

                var scores = JsonSerializer.Deserialize<List<UserScore>>(json, JsonOptions);
                return scores ?? new List<UserScore>();
            }
            catch
            {
                return new List<UserScore>();
            }
        }

        private async Task<bool> SaveAllAsync(List<UserScore> scores)
        {
            try
            {
                var json = JsonSerializer.Serialize(scores, JsonOptions);
                await _storage.SetItemAsync(StorageKey, json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
