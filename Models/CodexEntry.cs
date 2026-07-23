namespace AdventureGameWeb.Models
{
    public class CodexEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "Character"; // Character or Bestiary
        public string Role { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "📜";
        public bool IsUnlocked { get; set; }
        
        // Bestiary stats if applicable
        public int Health { get; set; }
        public string DamageRange { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}
