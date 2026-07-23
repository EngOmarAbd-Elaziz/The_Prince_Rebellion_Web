using System;

namespace AdventureGameWeb.Models
{
    // OOP: Base class for all living entities in the game
    public abstract class Character
    {
        public string? Name { get; set; }
        public int Health { get; set; }
        public bool IsAlive => Health > 0;

        public abstract void ApplyDamage(int amount);
    }
}
