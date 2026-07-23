namespace AdventureGameWeb.Models
{
    public class DialogueMessage
    {
        public string Speaker { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool IsNarrator => string.IsNullOrEmpty(Speaker) || Speaker == "Narrator";
        public bool IsSystem { get; set; }
        public string AvatarIcon { get; set; } = "📜";
        public string Expression { get; set; } = "Neutral"; // Neutral, Angry, Shocked, Confident, Sad, Thinking, Happy
        public string Position { get; set; } = "Left"; // Left, Right, Center
        public bool ShakeScreen { get; set; }
        public string AccentColor { get; set; } = "#38BDF8";
    }
}
