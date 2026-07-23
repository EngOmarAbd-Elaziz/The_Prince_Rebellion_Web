using System;
using System.Collections.Generic;
using System.Linq;
using AdventureGameWeb.Models;

namespace AdventureGameWeb.Services
{
    public class AchievementService
    {
        public event Action<Achievement>? OnAchievementUnlocked;

        private readonly List<Achievement> achievements = new List<Achievement>
        {
            new Achievement { Id = "revolution_begins", Title = "Revolution Begins", Description = "Started the rebellion against the King.", Icon = "⚔️" },
            new Achievement { Id = "chains_broken", Title = "Chains Broken", Description = "Freed yourself from handcuffs on the prison ship.", Icon = "⛓️" },
            new Achievement { Id = "first_blood", Title = "First Blood", Description = "Defeated your first enemy in combat.", Icon = "🩸" },
            new Achievement { Id = "treasure_hunter", Title = "Treasure Hunter", Description = "Discovered a hidden treasure chest.", Icon = "💎" },
            new Achievement { Id = "freedom_fighter", Title = "Freedom Fighter", Description = "Rescued prisoners from the ship.", Icon = "🕊️" },
            new Achievement { Id = "captain_down", Title = "Captain Down", Description = "Defeated the Ship Captain and liberated the vessel.", Icon = "⚓" },
            new Achievement { Id = "king_slayer", Title = "King Slayer", Description = "Defeated The King and freed the realm!", Icon = "👑" }
        };

        public IReadOnlyList<Achievement> Achievements => achievements.AsReadOnly();

        public void Unlock(string achievementId)
        {
            var ach = achievements.FirstOrDefault(a => a.Id == achievementId);
            if (ach != null && !ach.IsUnlocked)
            {
                ach.IsUnlocked = true;
                ach.UnlockedAt = DateTime.Now;
                OnAchievementUnlocked?.Invoke(ach);
            }
        }

        public void Reset()
        {
            foreach (var ach in achievements)
            {
                ach.IsUnlocked = false;
                ach.UnlockedAt = null;
            }
        }
    }
}
