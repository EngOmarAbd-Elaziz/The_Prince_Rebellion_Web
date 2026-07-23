using System.Threading.Tasks;
using AdventureGameWeb.Models;

namespace AdventureGameWeb.Services
{
    public class PlayerSessionService
    {
        public Task<string> GetCurrentUsernameAsync() => Task.FromResult(string.Empty);
        public Task<UserScore?> GetCurrentPlayerScoreAsync() => Task.FromResult<UserScore?>(null);
    }
}
