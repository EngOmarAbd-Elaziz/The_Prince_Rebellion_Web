namespace AdventureGameWeb.Models
{
    // Enemy class inheriting from Character
    public class Enemy : Character
    {
        // Damage boundaries for scalable combat
        public int GunDmgMin { get; set; }
        public int GunDmgMax { get; set; }
        public int SwordDmgMin { get; set; }
        public int SwordDmgMax { get; set; }
        public int DefendDmgMin { get; set; }
        public int DefendDmgMax { get; set; }

        public int PlayerGunMin { get; set; }
        public int PlayerGunMax { get; set; }
        public int PlayerSwordMin { get; set; }
        public int PlayerSwordMax { get; set; }

        public override void ApplyDamage(int amount)
        {
            Health -= amount;
        }
    }
}
