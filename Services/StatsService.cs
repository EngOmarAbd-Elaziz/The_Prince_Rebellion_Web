using System;
using AdventureGameWeb.Models;

namespace AdventureGameWeb.Services
{
    public class StatsService
    {
        public GameStats Stats { get; private set; } = new GameStats();
        public DateTime StartTime { get; private set; } = DateTime.Now;

        public TimeSpan PlayTime => DateTime.Now - StartTime;

        public void RecordDamageDealt(int amount) => Stats.TotalDamageDealt += amount;
        public void RecordDamageReceived(int amount) => Stats.TotalDamageReceived += amount;
        public void RecordEnemyDefeated() => Stats.EnemiesDefeated++;
        public void RecordCoins(int amount) => Stats.CoinsCollected += amount;
        public void RecordRoomExplored() => Stats.RoomsExplored++;
        public void RecordRoomSkipped() => Stats.RoomsSkipped++;
        public void RecordBattleWon() => Stats.BattlesWon++;
        public void RecordBattleLost() => Stats.BattlesLost++;
        public void RecordPrisonersRescued(int count) => Stats.PrisonersRescued += count;
        public void RecordAmmoUsed() => Stats.AmmoUsed++;

        public void Reset()
        {
            Stats = new GameStats();
            StartTime = DateTime.Now;
        }
    }
}
