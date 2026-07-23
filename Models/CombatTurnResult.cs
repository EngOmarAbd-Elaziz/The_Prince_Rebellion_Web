namespace AdventureGameWeb.Models
{
    public class CombatTurnResult
    {
        public int PlayerDamageDealt { get; set; }
        public int EnemyDamageDealt { get; set; }
        public string ActionType { get; set; } = string.Empty; // "Gun", "Sword", "Defend"
        public string Message { get; set; } = string.Empty;
        public bool OutOfAmmo { get; set; }
        public bool PlayerDefeated { get; set; }
        public bool EnemyDefeated { get; set; }
    }
}
