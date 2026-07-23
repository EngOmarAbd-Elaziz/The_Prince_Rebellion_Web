using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AdventureGameWeb.Models;

namespace AdventureGameWeb.Services
{
    public class SupabaseLeaderboardService
    {
        private readonly HttpClient _http;
        private readonly LocalStorageService _localStorage;
        private const string LOCAL_LEADERBOARD_KEY = "pr_local_leaderboard";

        // Replace these placeholders with your actual Supabase credentials
        public string SupabaseUrl { get; set; } = "https://bpjiyonvntouocxkycej.supabase.co";
        public string SupabaseAnonKey { get; set; } = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJwaml5b252bnRvdW9jeGt5Y2VqIiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODQ4MTA5NDIsImV4cCI6MjEwMDM4Njk0Mn0.c2Fvkxz3SV55f-JkUhXv4MgcqY4i47bnIVHc7hVjIds";

        // Optional: when a user is authenticated, set their JWT here so requests
        // use the user token for Authorization (required for owner-based RLS).
        public string? UserAccessToken { get; set; }

        public SupabaseLeaderboardService(HttpClient http, LocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(SupabaseUrl) &&
                                 !string.IsNullOrWhiteSpace(SupabaseAnonKey) &&
                                 !SupabaseUrl.Contains("YOUR_SUPABASE_URL");

        public async Task<List<LeaderboardEntry>> GetTopLeaderboardAsync(int limit = 10)
        {
            if (!IsConfigured) return await LoadLocalLeaderboardAsync(limit);

            try
            {
                var requestUrl = $"{SupabaseUrl.TrimEnd('/')}/rest/v1/leaderboard?select=*&order=score.desc&limit={limit}";

                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                ConfigureHeaders(request);

                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<LeaderboardEntry>>();
                    if (data != null && data.Count > 0)
                    {
                        return data;
                    }
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Supabase Fetch Error] Status: {response.StatusCode} - {err}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Supabase Leaderboard] Exception: {ex.Message}");
            }

            return await LoadLocalLeaderboardAsync(limit);
        }

        public async Task<bool> SubmitScoreAsync(LeaderboardEntry entry)
        {
            if (!IsConfigured) return await SaveScoreLocallyAsync(entry);

            try
            {
                // 1. فحص وجود اللاعب
                var queryUrl = $"{SupabaseUrl.TrimEnd('/')}/rest/v1/leaderboard?username=eq.{Uri.EscapeDataString(entry.Username)}";
                using var getReq = new HttpRequestMessage(HttpMethod.Get, queryUrl);
                ConfigureHeaders(getReq);

                var getResp = await _http.SendAsync(getReq);
                if (getResp.IsSuccessStatusCode)
                {
                    var existing = await getResp.Content.ReadFromJsonAsync<List<LeaderboardEntry>>();
                    if (existing != null && existing.Count > 0)
                    {
                        var existingEntry = existing[0];
                        if (entry.Score > existingEntry.Score)
                        {
                            var patchUrl = $"{SupabaseUrl.TrimEnd('/')}/rest/v1/leaderboard?username=eq.{Uri.EscapeDataString(existingEntry.Username)}";
                            using var patchReq = new HttpRequestMessage(HttpMethod.Patch, patchUrl);
                            ConfigureHeaders(patchReq);

                            patchReq.Content = JsonContent.Create(new
                            {
                                score = entry.Score,
                                character_class = entry.CharacterClass,
                                play_time_seconds = entry.PlayTimeSeconds,
                                achievements_count = entry.AchievementsCount,
                                is_victory = entry.IsVictory
                            });

                            var patchResp = await _http.SendAsync(patchReq);
                            return patchResp.IsSuccessStatusCode;
                        }
                        return true;
                    }
                }

                // 2. لاعب جديد - إرسال الحقول المطلوبة فقط (بدون ID أو CreatedAt)
                var requestUrl = $"{SupabaseUrl.TrimEnd('/')}/rest/v1/leaderboard";
                using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                ConfigureHeaders(request);

                var payload = new
                {
                    username = entry.Username,
                    character_class = entry.CharacterClass,
                    score = entry.Score,
                    play_time_seconds = entry.PlayTimeSeconds,
                    achievements_count = entry.AchievementsCount,
                    is_victory = entry.IsVictory
                };

                request.Content = JsonContent.Create(payload);
                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Supabase Insert Error] Status: {response.StatusCode} - {err}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Supabase Submit Exception] {ex.Message}");
                return false;
            }
        }        // Save locally when Supabase not configured
        public async Task<bool> SaveScoreLocallyAsync(LeaderboardEntry entry)
        {
            try
            {
                var json = await _localStorage.GetItemAsync(LOCAL_LEADERBOARD_KEY);
                List<LeaderboardEntry> list = new();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var existing = JsonSerializer.Deserialize<List<LeaderboardEntry>>(json);
                    if (existing != null) list = existing;
                }

                // Merge/update by username
                var idx = list.FindIndex(e => string.Equals(e.Username, entry.Username, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0)
                {
                    list[idx] = MergeEntries(list[idx], entry);
                }
                else
                {
                    // assign temporary id
                    entry.Id = (list.Count > 0) ? list[list.Count - 1].Id + 1 : 1000;
                    list.Add(entry);
                }

                var outJson = JsonSerializer.Serialize(list);
                await _localStorage.SetItemAsync(LOCAL_LEADERBOARD_KEY, outJson);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupabaseLeaderboard] Error saving local score: {ex.Message}");
                return false;
            }
        }

        private LeaderboardEntry MergeEntries(LeaderboardEntry? existing, LeaderboardEntry incoming)
        {
            if (existing == null) return incoming;
            return new LeaderboardEntry
            {
                Id = existing.Id,
                Username = existing.Username ?? incoming.Username,
                CharacterClass = string.IsNullOrWhiteSpace(existing.CharacterClass) ? incoming.CharacterClass : existing.CharacterClass,
                Score = Math.Max(existing.Score, incoming.Score),
                PlayTimeSeconds = (existing.PlayTimeSeconds > 0 && incoming.PlayTimeSeconds > 0) ? Math.Min(existing.PlayTimeSeconds, incoming.PlayTimeSeconds) : Math.Max(existing.PlayTimeSeconds, incoming.PlayTimeSeconds),
                AchievementsCount = Math.Max(existing.AchievementsCount, incoming.AchievementsCount),
                IsVictory = existing.IsVictory || incoming.IsVictory
            };
        }

        public async Task<LeaderboardEntry?> GetEntryByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            if (!IsConfigured)
            {
                try
                {
                    var json = await _localStorage.GetItemAsync(LOCAL_LEADERBOARD_KEY);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var list = JsonSerializer.Deserialize<List<LeaderboardEntry>>(json);
                        if (list != null)
                        {
                            return list.Find(e => string.Equals(e.Username, username, StringComparison.OrdinalIgnoreCase));
                        }
                    }
                }
                catch { }
                return null;
            }

            try
            {
                var q = $"{SupabaseUrl.TrimEnd('/')}/rest/v1/leaderboard?username=eq.{Uri.EscapeDataString(username)}";
                using var req = new HttpRequestMessage(HttpMethod.Get, q);
                ConfigureHeaders(req);
                var resp = await _http.SendAsync(req);
                if (resp.IsSuccessStatusCode)
                {
                    var data = await resp.Content.ReadFromJsonAsync<List<LeaderboardEntry>>();
                    if (data != null && data.Count > 0) return data[0];
                }
            }
            catch { }
            var localFallback = await LoadLocalLeaderboardAsync();
            return localFallback.Find(e => string.Equals(e.Username, username, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<List<LeaderboardEntry>> LoadLocalLeaderboardAsync(int limit = int.MaxValue)
        {
            try
            {
                var json = await _localStorage.GetItemAsync(LOCAL_LEADERBOARD_KEY);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var list = JsonSerializer.Deserialize<List<LeaderboardEntry>>(json);
                    if (list != null)
                    {
                        list.Sort((a, b) => b.Score.CompareTo(a.Score));
                        return list.GetRange(0, Math.Min(limit, list.Count));
                    }
                }
            }
            catch { }
            return new List<LeaderboardEntry>();
        }

        private void ConfigureHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("apikey", SupabaseAnonKey);
            // If a user access token is available, use it for Authorization so
            // owner-based RLS (matching JWT claims) can apply. Otherwise fall
            // back to the anon key as Bearer for unauthenticated requests.
            var bearer = !string.IsNullOrWhiteSpace(UserAccessToken) ? UserAccessToken : SupabaseAnonKey;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (request.Method == HttpMethod.Post || request.Method == HttpMethod.Patch)
            {
                request.Headers.Add("Prefer", "return=minimal");
            }
        }

        private List<LeaderboardEntry> GetMockLeaderboard()
        {
            return new List<LeaderboardEntry>
            {
                new LeaderboardEntry { Id = 1, Username = "Prince Omar", CharacterClass = "Royal Swordsman", Score = 780, PlayTimeSeconds = 340, AchievementsCount = 5, IsVictory = true },
                new LeaderboardEntry { Id = 2, Username = "Shadow Blade", CharacterClass = "Shadow Assassin", Score = 650, PlayTimeSeconds = 290, AchievementsCount = 4, IsVictory = true },
                new LeaderboardEntry { Id = 3, Username = "Iron Shield", CharacterClass = "Knight Commander", Score = 590, PlayTimeSeconds = 410, AchievementsCount = 3, IsVictory = true },
                new LeaderboardEntry { Id = 4, Username = "Valiant Rebel", CharacterClass = "Royal Swordsman", Score = 420, PlayTimeSeconds = 210, AchievementsCount = 2, IsVictory = false },
                new LeaderboardEntry { Id = 5, Username = "Mystic Mage", CharacterClass = "Arcane Sorcerer", Score = 380, PlayTimeSeconds = 180, AchievementsCount = 2, IsVictory = false }
            };
        }
    }
}
