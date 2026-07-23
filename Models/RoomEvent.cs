using System;

namespace AdventureGameWeb.Models
{
    public class RoomEvent
    {
        public required string Description { get; set; }
        public Action<Player, Engine.IGameEngineService>? ExecuteEvent { get; set; }
        public bool IsPrisonerEvent { get; set; }
        public bool IsCombatEvent { get; set; }
        public bool IsAnimal { get; set; }
    }
}
