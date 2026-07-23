namespace AdventureGameWeb.Models
{
    // OOP: Encapsulation - Player class inheriting from Character
    public class Player : Character
    {
        public int Shield { get; set; } = 0;
        public int Coins { get; set; } = 0;
        public int Ammo { get; set; } = 0;
        public int PrisonersRemaining { get; set; } = 25;
        public string SelectedHero { get; set; } = "Prince"; // "Prince" or "Princess"

        public Player(string name)
        {
            Name = name;
            Health = 50;
        }

        // Custom damage logic to account for the shield
        public override void ApplyDamage(int amount)
        {
            if (Shield >= amount)
            {
                Shield -= amount;
            }
            else
            {
                amount -= Shield;
                Shield = 0;
                Health -= amount;
            }
        }
    }
}
