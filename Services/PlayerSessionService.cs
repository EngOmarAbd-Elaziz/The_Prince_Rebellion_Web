using System.Threading.Tasks;
using AdventureGameWeb.Engine;
using AdventureGameWeb.Models;

namespace AdventureGameWeb.Services
{
    /// <summary>
    /// Resolves the active player identity from the game engine or persisted local storage.
    /// </summary>
    public class PlayerSessionService
    {
        private readonly LocalStorageService _storage;
        private readonly IGameEngineService _gameEngine;
        private readonly ScoreService _scoreService;

        public PlayerSessionService(
            LocalStorageService storage,
            IGameEngineService gameEngine,
            ScoreService scoreService)
        {
            _storage = storage;
            _gameEngine = gameEngine;
            _scoreService = scoreService;
        }

        public async Task<string> GetCurrentUsernameAsync()
        {
            if (!string.IsNullOrWhiteSpace(_gameEngine.UserName))
            {
                return _gameEngine.UserName.Trim();
            }

            var saved = await _storage.LoadPlayerNameAsync();
            if (!string.IsNullOrWhiteSpace(saved))
            {
                var trimmed = saved.Trim();
                _gameEngine.SetUserName(trimmed);
                return trimmed;
            }

            return string.Empty;
        }

        public async Task<UserScore?> GetCurrentPlayerScoreAsync()
        {
            var username = await GetCurrentUsernameAsync();
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return await _scoreService.GetScoreByUsernameAsync(username);
        }
    }
}
