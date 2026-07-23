namespace AdventureGameWeb.Models
{
    public class GameStats
    {
        public int TotalDamageDealt { get; set; }
        public int TotalDamageReceived { get; set; }
        public int EnemiesDefeated { get; set; }
        public int CoinsCollected { get; set; }
        public int RoomsExplored { get; set; }
        public int RoomsSkipped { get; set; }
        public int BattlesWon { get; set; }
        public int BattlesLost { get; set; }
        public int PrisonersRescued { get; set; }
        public int AmmoUsed { get; set; }
    }
}
