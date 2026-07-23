using System;
using System.Collections.Generic;
using System.Diagnostics;
using AdventureGameWeb.Engine;
using AdventureGameWeb.Models;

namespace AdventureGameWeb.Services
{
    public class GameEngineService : IGameEngineService
    {
        private readonly Random rng = new Random();
        private readonly AudioService audioService;
        private readonly AchievementService achievementService;
        private readonly StatsService statsService;

        private List<RoomEvent> eventPool = new List<RoomEvent>();
        private List<DialogueMessage> currentStorySequence = new List<DialogueMessage>();
        private int storyIndex = 0;
        private List<DialogueMessage> dialogueHistory = new List<DialogueMessage>();
        private List<string> battleLogs = new List<string>();

        public event Action? OnChange;

        public Player Player { get; private set; } = null!;
        public Enemy? CurrentEnemy { get; private set; }
        public GamePhase CurrentPhase { get; private set; } = GamePhase.NameInput;
        public DialogueMessage? CurrentDialogue { get; private set; }
        public int RoomNumber { get; private set; } = 0;
        public RoomEvent? ActiveRoomEvent { get; private set; }
        public string? EventMessage { get; private set; }
        public string? BattleMessage { get; private set; }
        public string CurrentSceneBackground { get; private set; } = "PalaceBedroom";
        public int PlayerMaxHealth { get; private set; } = 50;
        public int EnemyMaxHealth { get; private set; } = 35;
        public bool IsPaused { get; private set; } = false;
        public string UserName { get; private set; } = string.Empty;

        // Play Time Tracking
        private readonly Stopwatch _playTimer = new Stopwatch();
        public int PlayTimeSeconds => (int)_playTimer.Elapsed.TotalSeconds;

        public IReadOnlyList<DialogueMessage> DialogueHistory => dialogueHistory.AsReadOnly();
        public IReadOnlyList<string> BattleLogs => battleLogs.AsReadOnly();

        public GameEngineService(AudioService audioService, AchievementService achievementService, StatsService statsService)
        {
            this.audioService = audioService;
            this.achievementService = achievementService;
            this.statsService = statsService;

            Player = new Player("Prince");
            InitializeEvents();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public void PauseGame()
        {
            IsPaused = true;
            NotifyStateChanged();
        }

        public void ResumeGame()
        {
            IsPaused = false;
            NotifyStateChanged();
        }

        private void InitializeEvents()
        {
            eventPool = new List<RoomEvent>
            {
                new RoomEvent
                {
                    Description = "You meet one of the guards. Get ready to confront the guard!",
                    IsCombatEvent = true,
                    IsAnimal = false
                },
                new RoomEvent
                {
                    Description = "You found a treasure chest containing 50 coins and +10 HP.",
                    ExecuteEvent = (p, engine) => 
                    { 
                        p.Coins += 50; 
                        p.Health += 10;
                        statsService.RecordCoins(50);
                        achievementService.Unlock("treasure_hunter");
                        _ = audioService.PlayTreasureOpenAsync();
                    }
                },
                new RoomEvent
                {
                    Description = "You found 5 prisoners and helped them escape.",
                    IsPrisonerEvent = true,
                    ExecuteEvent = (p, engine) => 
                    { 
                        p.PrisonersRemaining = Math.Max(0, p.PrisonersRemaining - 5); 
                        statsService.RecordPrisonersRescued(5);
                        achievementService.Unlock("freedom_fighter");
                        _ = audioService.PlayCoinAsync();
                    }
                },
                new RoomEvent
                {
                    Description = "You found +5 ammo and +15 shield.",
                    ExecuteEvent = (p, engine) => 
                    { 
                        p.Ammo += 5; 
                        p.Shield += 15;
                        _ = audioService.PlayShieldBlockAsync();
                    }
                },
                new RoomEvent
                {
                    Description = "You meet a wild animal, prepare to fight!",
                    IsCombatEvent = true,
                    IsAnimal = true
                }
            };
        }

        public void SetPhase(GamePhase phase)
        {
            CurrentPhase = phase;
            NotifyStateChanged();
        }

        public void SelectHero(string heroType)
        {
            if (Player != null)
            {
                Player.SelectedHero = heroType == "Princess" ? "Princess" : "Prince";
                NotifyStateChanged();
            }
        }

        public void StartGame(string playerInputName, string heroType = "Prince")
        {
            string name = string.IsNullOrWhiteSpace(playerInputName) ? heroType : playerInputName.Trim();
            Player = new Player(name) { SelectedHero = heroType };
            PlayerMaxHealth = 50;
            RoomNumber = 0;
            IsPaused = false;
            dialogueHistory.Clear();
            battleLogs.Clear();
            statsService.Reset();
            _playTimer.Restart();

            achievementService.Unlock("revolution_begins");
            _ = audioService.PlayTrackAsync("Story");

            BuildIntroStorySequence();
            CurrentPhase = GamePhase.IntroStory;
            storyIndex = 0;
            ShowNextStoryDialogue();
        }

        private void BuildIntroStorySequence()
        {
            bool isPrincess = Player.SelectedHero == "Princess";
            string titleGreeting = isPrincess ? "princess" : "prince";
            string pronounText = isPrincess ? "The princess angrily puts on her uniform and leaves the palace." : "The prince angrily puts on his uniform and leaves the palace.";
            string enterText = isPrincess ? "The princess enters the hideout to meet the organization group." : "The prince enters the hideout to meet the organization group.";
            string betrayalText = isPrincess ? "but by my daughter!!" : "but by my son!!";
            string tortureText = isPrincess ? "The king tortured the princess and her group in front of the Public." : "The king tortured the prince and his group in front of the Public.";
            string daughterText = isPrincess ? "If he did this to his daughter, what could he do to the general public?" : "If he did this to his son, what could he do to the general public?";
            string deathSentenceText = isPrincess ? "The princess was sentenced to death on the Prisoners' Island." : "The prince was sentenced to death on the Prisoners' Island.";

            currentStorySequence = new List<DialogueMessage>
            {
                new DialogueMessage { Speaker = "Queen", Text = $"Good morning, {titleGreeting}", AvatarIcon = "👑", Position = "Right", Expression = "Happy", AccentColor = "#FACC15" },
                new DialogueMessage { Speaker = Player.Name!, Text = "Good morning, mom", AvatarIcon = "⚔️", Position = "Left", Expression = "Neutral", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = Player.Name!, Text = "[A normal day like any other day]", AvatarIcon = "⚔️", Position = "Left", Expression = "Thinking", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = Player.Name!, Text = "[Since my father ruled the town]", AvatarIcon = "⚔️", Position = "Left", Expression = "Thinking", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = Player.Name!, Text = "[and it has been in a state of destruction and injustice]", AvatarIcon = "⚔️", Position = "Left", Expression = "Serious", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = Player.Name!, Text = "Today is the promised day", AvatarIcon = "⚔️", Position = "Left", Expression = "Confident", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = "Queen", Text = "Keep in mind what you are intending to do, this is very dangerous", AvatarIcon = "👑", Position = "Right", Expression = "Serious", AccentColor = "#FACC15" },
                new DialogueMessage { Speaker = "Queen", Text = "If this matter fails, your father will become very angry and may sentence you to death", AvatarIcon = "👑", Position = "Right", Expression = "Scared", AccentColor = "#FACC15" },
                new DialogueMessage { Speaker = Player.Name!, Text = "I will never accept injustice, and the coup will happen today", AvatarIcon = "⚔️", Position = "Left", Expression = "Angry", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = "Narrator", Text = pronounText, AvatarIcon = "📜", Position = "Center", Expression = "Neutral", AccentColor = "#94A3B8" },
                new DialogueMessage { Speaker = "Narrator", Text = "The king is so cold he doesn't care about Anything.", AvatarIcon = "📜", Position = "Center", Expression = "Neutral", AccentColor = "#94A3B8" },
                new DialogueMessage { Speaker = "Narrator", Text = "He has a very strong ego, controls the people and does not care about them.", AvatarIcon = "📜", Position = "Center", Expression = "Neutral", AccentColor = "#94A3B8" },
                new DialogueMessage { Speaker = "Narrator", Text = "The people are angry and ready to sacrifice anything to stop the king.", AvatarIcon = "📜", Position = "Center", Expression = "Neutral", AccentColor = "#94A3B8" },
                new DialogueMessage { Speaker = "Narrator", Text = $"{Player.Name} goes to the hideout to meet the campaign members and the seller.", AvatarIcon = "📜", Position = "Center", Expression = "Neutral", AccentColor = "#94A3B8" },
                new DialogueMessage { Speaker = Player.Name!, Text = "Do you have some dragon fruit?", AvatarIcon = "⚔️", Position = "Left", Expression = "Thinking", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = "The Seller", Text = "I don't know, come and take a look in the store.", AvatarIcon = "🏬", Position = "Right", Expression = "Neutral", AccentColor = "#F59E0B" },
                new DialogueMessage { Speaker = Player.Name!, Text = "Well, quickly, because the king is waiting.", AvatarIcon = "⚔️", Position = "Left", Expression = "Serious", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = Player.Name!, Text = "[Normally, the seller does not sell Devil Fruits]", AvatarIcon = "⚔️", Position = "Left", Expression = "Thinking", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = Player.Name!, Text = "[but it is just a password so that we can enter the hideout]", AvatarIcon = "⚔️", Position = "Left", Expression = "Smirking", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = "Narrator", Text = enterText, AvatarIcon = "📜", Position = "Center", Expression = "Neutral", AccentColor = "#94A3B8" },
                new DialogueMessage { Speaker = "Ash", Text = "This is the first time you've come early. You look so excited today.", AvatarIcon = "🛡️", Position = "Right", Expression = "Happy", AccentColor = "#14B8A6" },
                new DialogueMessage { Speaker = "Loge", Text = "We need to talk quickly before anyone comes.", AvatarIcon = "🛡️", Position = "Right", Expression = "Serious", AccentColor = "#14B8A6" },
                new DialogueMessage { Speaker = Player.Name!, Text = "Of course I am prepared for everything.", AvatarIcon = "⚔️", Position = "Left", Expression = "Confident", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = "Tom", Text = "We must wait for all members today, there cannot be any mistake.", AvatarIcon = "🛡️", Position = "Right", Expression = "Thinking", AccentColor = "#14B8A6" },
                new DialogueMessage { Speaker = "Tom", Text = "Thus the king's assistant remains and we will be complete.", AvatarIcon = "🛡️", Position = "Right", Expression = "Neutral", AccentColor = "#14B8A6" },
                new DialogueMessage { Speaker = Player.Name!, Text = "Okay, but does anyone know why he was late??", AvatarIcon = "⚔️", Position = "Left", Expression = "Thinking", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = "King's assistant", Text = "Sorry for being late! >:)", AvatarIcon = "👿", Position = "Right", Expression = "Smirking", AccentColor = "#EF4444" },
                new DialogueMessage { Speaker = "King's assistant", Text = "I HAVE SOMEONE WHO WANTS TO JOIN US!! >:)", AvatarIcon = "👿", Position = "Right", Expression = "Angry", AccentColor = "#EF4444" },
                new DialogueMessage { Speaker = "Narrator", Text = "Loud noise outside the bunker!!!!", AvatarIcon = "💥", Position = "Center", Expression = "Shocked", ShakeScreen = true, AccentColor = "#EF4444" },
                new DialogueMessage { Speaker = "Loge", Text = "WHAT IS HAPPENING !!!", AvatarIcon = "🛡️", Position = "Right", Expression = "Shocked", ShakeScreen = true, AccentColor = "#14B8A6" },
                new DialogueMessage { Speaker = "Tom", Text = "We are being betrayed.", AvatarIcon = "🛡️", Position = "Right", Expression = "Sad", AccentColor = "#14B8A6" },
                new DialogueMessage { Speaker = "Narrator", Text = "The king enters with his entourage of soldiers.", AvatarIcon = "👑", Position = "Center", Expression = "Serious", AccentColor = "#A855F7" },
                new DialogueMessage { Speaker = "The King", Text = "Well, well, well.", AvatarIcon = "👑", Position = "Right", Expression = "Smirking", AccentColor = "#A855F7" },
                new DialogueMessage { Speaker = "The King", Text = "I expected that I would be betrayed because of my injustice...", AvatarIcon = "👑", Position = "Right", Expression = "Serious", AccentColor = "#A855F7" },
                new DialogueMessage { Speaker = "The King", Text = betrayalText, AvatarIcon = "👑", Position = "Right", Expression = "Angry", ShakeScreen = true, AccentColor = "#A855F7" },
                new DialogueMessage { Speaker = "The King", Text = "I will make an example of you and your group. Soldiers, arrest them!", AvatarIcon = "👑", Position = "Right", Expression = "Furious", ShakeScreen = true, AccentColor = "#A855F7" },
                new DialogueMessage { Speaker = "Narrator", Text = "What happened after that was...", AvatarIcon = "📜", Position = "Center", Expression = "Neutral", AccentColor = "#94A3B8" },
                new DialogueMessage { Speaker = "Narrator", Text = tortureText, AvatarIcon = "📜", Position = "Center", Expression = "Sad", AccentColor = "#94A3B8" },
                new DialogueMessage { Speaker = "Narrator", Text = "The people's anger turned into fear.", AvatarIcon = "📜", Position = "Center", Expression = "Scared", AccentColor = "#94A3B8" },
                new DialogueMessage { Speaker = "Narrator", Text = daughterText, AvatarIcon = "📜", Position = "Center", Expression = "Thinking", AccentColor = "#94A3B8" },
                new DialogueMessage { Speaker = "Narrator", Text = deathSentenceText, AvatarIcon = "📜", Position = "Center", Expression = "Sad", AccentColor = "#94A3B8" },
                new DialogueMessage { Speaker = "Narrator", Text = "BUT THIS IS NOT THE END OF THE STORY", AvatarIcon = "🔥", Position = "Center", Expression = "Confident", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = "Narrator", Text = "JUST THE START", AvatarIcon = "🔥", Position = "Center", Expression = "Confident", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = "Narrator", Text = "You are on a ship on its way to Prisoner Island.", AvatarIcon = "⚓", Position = "Center", Expression = "Neutral", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = "Narrator", Text = "Chained in iron with a stranger.", AvatarIcon = "⛓️", Position = "Center", Expression = "Neutral", AccentColor = "#38BDF8" },
                new DialogueMessage { Speaker = "Narrator", Text = "What will you do ??", AvatarIcon = "⛓️", Position = "Center", Expression = "Thinking", AccentColor = "#38BDF8" }
            };
        }

        private void ShowNextStoryDialogue()
        {
            if (IsPaused) return;

            if (storyIndex < currentStorySequence.Count)
            {
                CurrentDialogue = currentStorySequence[storyIndex];
                dialogueHistory.Add(CurrentDialogue);
                
                // Update Scene Background based on story progression
                if (storyIndex <= 8) CurrentSceneBackground = "PalaceBedroom";
                else if (storyIndex <= 13) CurrentSceneBackground = "PalaceHall";
                else if (storyIndex <= 18) CurrentSceneBackground = "TownMarket";
                else if (storyIndex <= 41) CurrentSceneBackground = "BunkerHideout";
                else CurrentSceneBackground = "PrisonShipChains";

                _ = audioService.PlayTypingTickAsync();
                storyIndex++;
            }
            else
            {
                if (CurrentPhase == GamePhase.IntroStory)
                {
                    CurrentPhase = GamePhase.HandcuffChoice;
                    CurrentSceneBackground = "PrisonShipChains";
                }
                else if (CurrentPhase == GamePhase.PostHandcuffStory)
                {
                    CurrentPhase = GamePhase.ShipNavigation;
                    CurrentSceneBackground = "ShipDeck";
                    _ = audioService.PlayTrackAsync("Exploration");
                }
                else if (CurrentPhase == GamePhase.PostCaptainStory)
                {
                    // Power up for final boss
                    Player.Health = 100;
                    Player.Shield = 200;
                    Player.Ammo = 50;
                    PlayerMaxHealth = 100;

                    TriggerBattle(CreateKing());
                    CurrentPhase = GamePhase.KingBattle;
                    CurrentSceneBackground = "KingThroneRoom";
                    _ = audioService.PlayTrackAsync("Boss");
                }
            }
            NotifyStateChanged();
        }

        public void NextDialogue()
        {
            if (IsPaused) return;
            ShowNextStoryDialogue();
        }

        public void SelectHandcuffOption(int option)
        {
            if (IsPaused) return;
            currentStorySequence.Clear();
            storyIndex = 0;

            if (option == 1)
            {
                currentStorySequence.Add(new DialogueMessage { Speaker = Player.Name!, Text = "Do you have a Knife ??", AvatarIcon = "⚔️", Position = "Left", Expression = "Thinking", AccentColor = "#38BDF8" });
                currentStorySequence.Add(new DialogueMessage { Speaker = "Prisoner", Text = "If YES, I would free myself.", AvatarIcon = "⛓️", Position = "Right", Expression = "Smirking", AccentColor = "#F97316" });
                currentStorySequence.Add(new DialogueMessage { Speaker = "Prisoner", Text = "But if we cooperate, we can free ourselves.", AvatarIcon = "⛓️", Position = "Right", Expression = "Confident", AccentColor = "#F97316" });
                currentStorySequence.Add(new DialogueMessage { Speaker = "Jack", Text = "My name is Jack by the way.", AvatarIcon = "🏴‍☠️", Position = "Right", Expression = "Happy", AccentColor = "#F97316" });
                currentStorySequence.Add(new DialogueMessage { Speaker = Player.Name!, Text = $"I'm {Player.Name}.", AvatarIcon = "⚔️", Position = "Left", Expression = "Happy", AccentColor = "#38BDF8" });
                currentStorySequence.Add(new DialogueMessage { Speaker = "Narrator", Text = "You succeeded in breaking the chain.", AvatarIcon = "⛓️", Position = "Center", Expression = "Neutral", AccentColor = "#94A3B8" });
            }
            else
            {
                currentStorySequence.Add(new DialogueMessage { Speaker = "Narrator", Text = "There was breaking glass...", AvatarIcon = "🍷", Position = "Center", Expression = "Shocked", ShakeScreen = true, AccentColor = "#94A3B8" });
                currentStorySequence.Add(new DialogueMessage { Speaker = "Narrator", Text = $"but {Player.Name} failed to break the chain.", AvatarIcon = "⛓️", Position = "Center", Expression = "Sad", AccentColor = "#94A3B8" });
                currentStorySequence.Add(new DialogueMessage { Speaker = "Prisoner", Text = "If we cooperate, we can free ourselves.", AvatarIcon = "⛓️", Position = "Right", Expression = "Confident", AccentColor = "#F97316" });
                currentStorySequence.Add(new DialogueMessage { Speaker = "Jack", Text = "My name is Jack by the way.", AvatarIcon = "🏴‍☠️", Position = "Right", Expression = "Happy", AccentColor = "#F97316" });
                currentStorySequence.Add(new DialogueMessage { Speaker = Player.Name!, Text = $"I'm {Player.Name}.", AvatarIcon = "⚔️", Position = "Left", Expression = "Happy", AccentColor = "#38BDF8" });
                currentStorySequence.Add(new DialogueMessage { Speaker = "Narrator", Text = "You succeeded in breaking the chain.", AvatarIcon = "⛓️", Position = "Center", Expression = "Neutral", AccentColor = "#94A3B8" });
            }

            currentStorySequence.Add(new DialogueMessage { Speaker = "Narrator", Text = "You and Jack found a Sword, it will help in fights.", AvatarIcon = "⚔️", Position = "Center", Expression = "Confident", AccentColor = "#38BDF8" });

            achievementService.Unlock("chains_broken");
            CurrentPhase = GamePhase.PostHandcuffStory;
            ShowNextStoryDialogue();
        }

        public void ChooseRoomAction(int action)
        {
            if (IsPaused || !Player.IsAlive)
            {
                if (!Player.IsAlive) CurrentPhase = GamePhase.GameOver;
                NotifyStateChanged();
                return;
            }

            if (action == 1) // Go to Roof
            {
                if (Player.PrisonersRemaining <= 0)
                {
                    EventMessage = "You have to fight the captain of the ship!";
                    CurrentSceneBackground = "ShipRoof";
                    TriggerBattle(CreateCaptain());
                }
                else
                {
                    EventMessage = "Find all Prisoners first!!";
                    ActiveRoomEvent = null;
                    CurrentPhase = GamePhase.EventNotification;
                    _ = audioService.PlayClickAsync();
                }
            }
            else if (action == 2) // Enter Room
            {
                RoomNumber++;
                statsService.RecordRoomExplored();
                RoomEvent randomEvent = eventPool[rng.Next(eventPool.Count)];
                ActiveRoomEvent = randomEvent;

                if (randomEvent.IsPrisonerEvent && Player.PrisonersRemaining <= 0)
                {
                    EventMessage = "Empty room!!";
                    CurrentPhase = GamePhase.EventNotification;
                    _ = audioService.PlayClickAsync();
                }
                else if (randomEvent.IsCombatEvent)
                {
                    EventMessage = randomEvent.Description;
                    TriggerBattle(CreateGuard(randomEvent.IsAnimal));
                }
                else
                {
                    EventMessage = randomEvent.Description;
                    randomEvent.ExecuteEvent?.Invoke(Player, this);
                    CurrentPhase = GamePhase.EventNotification;
                }
            }
            else if (action == 3) // Skip Room
            {
                RoomNumber++;
                statsService.RecordRoomSkipped();
                CurrentPhase = GamePhase.ShipNavigation;
                _ = audioService.PlayClickAsync();
            }

            NotifyStateChanged();
        }

        public void DismissEventCard()
        {
            if (IsPaused) return;
            ActiveRoomEvent = null;
            EventMessage = null;

            if (!Player.IsAlive)
            {
                CurrentPhase = GamePhase.GameOver;
            }
            else
            {
                CurrentPhase = GamePhase.ShipNavigation;
            }
            NotifyStateChanged();
        }

        public void TriggerBattle(Enemy enemy)
        {
            CurrentEnemy = enemy;
            EnemyMaxHealth = enemy.Health;
            BattleMessage = $"Encountered {enemy.Name}!";
            battleLogs.Clear();
            battleLogs.Add($"Encountered {enemy.Name}! HP: {enemy.Health}");

            string trackName = (enemy.Name == "The King") ? "Boss" : "Battle";
            _ = audioService.PlayTrackAsync(trackName);

            if (CurrentPhase != GamePhase.KingBattle)
            {
                CurrentPhase = GamePhase.Battle;
            }
            NotifyStateChanged();
        }

        public CombatTurnResult ExecuteCombatAction(int action)
        {
            var result = new CombatTurnResult();
            if (IsPaused || CurrentEnemy == null || !Player.IsAlive || !CurrentEnemy.IsAlive)
                return result;

            int playerDmg = 0;
            int enemyDmg = 0;

            if (action == 1) // Use Gun
            {
                result.ActionType = "Gun";
                if (Player.Ammo <= 0)
                {
                    result.OutOfAmmo = true;
                    result.Message = "You don't have Ammo!!";
                    BattleMessage = result.Message;
                    battleLogs.Add(result.Message);
                    _ = audioService.PlayClickAsync();
                    NotifyStateChanged();
                    return result;
                }

                Player.Ammo--;
                statsService.RecordAmmoUsed();
                _ = audioService.PlayGunshotAsync();

                playerDmg = rng.Next(CurrentEnemy.PlayerGunMin, CurrentEnemy.PlayerGunMax + 1);
                enemyDmg = rng.Next(CurrentEnemy.GunDmgMin, CurrentEnemy.GunDmgMax + 1);
            }
            else if (action == 2) // Use Sword
            {
                result.ActionType = "Sword";
                _ = audioService.PlaySwordSlashAsync();
                playerDmg = rng.Next(CurrentEnemy.PlayerSwordMin, CurrentEnemy.PlayerSwordMax + 1);
                enemyDmg = rng.Next(CurrentEnemy.SwordDmgMin, CurrentEnemy.SwordDmgMax + 1);
            }
            else if (action == 3) // Defend
            {
                result.ActionType = "Defend";
                _ = audioService.PlayShieldBlockAsync();
                playerDmg = 0;
                enemyDmg = rng.Next(CurrentEnemy.DefendDmgMin, CurrentEnemy.DefendDmgMax + 1);
            }

            result.PlayerDamageDealt = playerDmg;
            result.EnemyDamageDealt = enemyDmg;

            statsService.RecordDamageDealt(playerDmg);
            statsService.RecordDamageReceived(enemyDmg);

            CurrentEnemy.ApplyDamage(playerDmg);
            Player.ApplyDamage(enemyDmg);

            string logLine = $"{result.ActionType} used! You dealt {playerDmg} DMG. {CurrentEnemy.Name} dealt {enemyDmg} DMG.";
            battleLogs.Add(logLine);

            if (!Player.IsAlive)
            {
                result.PlayerDefeated = true;
                result.Message = "You were defeated in battle. Game over.";
                BattleMessage = result.Message;
                battleLogs.Add(result.Message);
                statsService.RecordBattleLost();
                _ = audioService.PlayDefeatSoundAsync();
                CurrentPhase = GamePhase.GameOver;
            }
            else if (!CurrentEnemy.IsAlive)
            {
                result.EnemyDefeated = true;
                statsService.RecordBattleWon();
                statsService.RecordEnemyDefeated();
                achievementService.Unlock("first_blood");

                if (CurrentEnemy.Name == "The King")
                {
                    result.Message = "You defeated the King! You WIN !!!";
                    BattleMessage = result.Message;
                    battleLogs.Add(result.Message);
                    achievementService.Unlock("king_slayer");
                    _ = audioService.PlayVictoryFanfareAsync();
                    CurrentPhase = GamePhase.Victory;
                }
                else if (CurrentEnemy.Name == "Ship Captain")
                {
                    result.Message = "You win the fight!!";
                    BattleMessage = result.Message;
                    battleLogs.Add(result.Message);
                    achievementService.Unlock("captain_down");

                    // Post captain story transition
                    currentStorySequence = new List<DialogueMessage>
                    {
                        new DialogueMessage { Speaker = "Jack", Text = "So you beat The captain of the ship.", AvatarIcon = "🏴‍☠️", Position = "Right", Expression = "Happy", AccentColor = "#F97316" },
                        new DialogueMessage { Speaker = "Jack", Text = "And all of the prisoners have controlled the ship.", AvatarIcon = "🏴‍☠️", Position = "Right", Expression = "Confident", AccentColor = "#F97316" },
                        new DialogueMessage { Speaker = "Jack", Text = "You are the Captain right now.", AvatarIcon = "🏴‍☠️", Position = "Right", Expression = "Happy", AccentColor = "#F97316" },
                        new DialogueMessage { Speaker = Player.Name!, Text = "We have to go back to town to fight the King.", AvatarIcon = "⚔️", Position = "Left", Expression = "Angry", AccentColor = "#38BDF8" },
                        new DialogueMessage { Speaker = "Narrator", Text = "In the end, the Prince made it to the town to fight the King.", AvatarIcon = "📜", Position = "Center", Expression = "Neutral", AccentColor = "#94A3B8" },
                        new DialogueMessage { Speaker = "Narrator", Text = "He succeeded in uniting the people against the king and his soldiers.", AvatarIcon = "📜", Position = "Center", Expression = "Confident", AccentColor = "#94A3B8" },
                        new DialogueMessage { Speaker = "Narrator", Text = "And he was finally ready to face the king with a new sword and gun.", AvatarIcon = "⚔️", Position = "Center", Expression = "Confident", AccentColor = "#38BDF8" }
                    };

                    storyIndex = 0;
                    CurrentPhase = GamePhase.PostCaptainStory;
                    CurrentSceneBackground = "TownBattlefield";
                    _ = audioService.PlayVictoryFanfareAsync();
                    ShowNextStoryDialogue();
                }
                else
                {
                    result.Message = $"You defeated {CurrentEnemy.Name} and gained 5 coins + 3 shield.";
                    Player.Shield += 3;
                    Player.Coins += 5;
                    statsService.RecordCoins(5);
                    BattleMessage = result.Message;
                    battleLogs.Add(result.Message);

                    ActiveRoomEvent = null;
                    EventMessage = result.Message;
                    CurrentPhase = GamePhase.EventNotification;
                    _ = audioService.PlayVictoryFanfareAsync();
                }
            }

            NotifyStateChanged();
            return result;
        }

        public void RestartGame()
        {
            _playTimer.Stop();
            CurrentPhase = GamePhase.NameInput;
            Player = new Player("Prince");
            RoomNumber = 0;
            CurrentEnemy = null;
            CurrentDialogue = null;
            ActiveRoomEvent = null;
            EventMessage = null;
            BattleMessage = null;
            CurrentSceneBackground = "PalaceBedroom";
            IsPaused = false;
            dialogueHistory.Clear();
            battleLogs.Clear();
            statsService.Reset();

            _ = audioService.PlayTrackAsync("Menu");
            NotifyStateChanged();
        }

        public void SetUserName(string name)
        {
            UserName = string.IsNullOrWhiteSpace(name) ? "Anonymous" : name.Trim();
        }

        public int CalculateScore(bool isVictory)
        {
            int score = 0;
            // Rooms explored
            score += RoomNumber * 10;
            // Coins collected
            score += Player.Coins * 2;
            // Prisoners rescued (25 - remaining)
            score += (25 - Player.PrisonersRemaining) * 20;
            // Victory bonus
            if (isVictory) score += 500;
            // Ensure non-negative
            return Math.Max(0, score);
        }

        private Enemy CreateGuard(bool isAnimal = false) => new Enemy
        {
            Name = isAnimal ? "Wild Animal" : "Guard",
            Health = rng.Next(25, 36),
            GunDmgMin = 5,
            GunDmgMax = 10,
            SwordDmgMin = 8,
            SwordDmgMax = 14,
            DefendDmgMin = 0,
            DefendDmgMax = 4,
            PlayerGunMin = 20,
            PlayerGunMax = 25,
            PlayerSwordMin = 10,
            PlayerSwordMax = 15
        };

        private Enemy CreateCaptain() => new Enemy
        {
            Name = "Ship Captain",
            Health = 80,
            GunDmgMin = 5,
            GunDmgMax = 10,
            SwordDmgMin = 8,
            SwordDmgMax = 14,
            DefendDmgMin = 0,
            DefendDmgMax = 4,
            PlayerGunMin = 20,
            PlayerGunMax = 25,
            PlayerSwordMin = 10,
            PlayerSwordMax = 15
        };

        private Enemy CreateKing() => new Enemy
        {
            Name = "The King",
            Health = 400,
            GunDmgMin = 15,
            GunDmgMax = 30,
            SwordDmgMin = 14,
            SwordDmgMax = 28,
            DefendDmgMin = 0,
            DefendDmgMax = 4,
            PlayerGunMin = 30,
            PlayerGunMax = 50,
            PlayerSwordMin = 20,
            PlayerSwordMax = 40
        };
    }
}
