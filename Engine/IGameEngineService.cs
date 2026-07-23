using System;
using System.Collections.Generic;
using AdventureGameWeb.Models;

namespace AdventureGameWeb.Engine
{
    public interface IGameEngineService
    {
        event Action? OnChange;

        Player Player { get; }
        Enemy? CurrentEnemy { get; }
        GamePhase CurrentPhase { get; }
        DialogueMessage? CurrentDialogue { get; }
        int RoomNumber { get; }
        RoomEvent? ActiveRoomEvent { get; }
        string? EventMessage { get; }
        string? BattleMessage { get; }
        string CurrentSceneBackground { get; }
        int PlayerMaxHealth { get; }
        int EnemyMaxHealth { get; }
        bool IsPaused { get; }
        string UserName { get; }
        int PlayTimeSeconds { get; }
        IReadOnlyList<DialogueMessage> DialogueHistory { get; }
        IReadOnlyList<string> BattleLogs { get; }

        void SelectHero(string heroType);
        void StartGame(string playerInputName, string heroType = "Prince");
        void NextDialogue();
        void SelectHandcuffOption(int option);
        void ChooseRoomAction(int action);
        void DismissEventCard();
        CombatTurnResult ExecuteCombatAction(int action);
        void RestartGame();
        void TriggerBattle(Enemy enemy);
        void SetPhase(GamePhase phase);
        void PauseGame();
        void ResumeGame();
        void SetUserName(string name);
        int CalculateScore(bool isVictory);
    }
}
