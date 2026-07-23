# Implementation Plan - The Prince Rebellion Web (Blazor WASM .NET 9)

Transform "The Prince Rebellion" from a C# console application into a modern, responsive, high-end Dark Fantasy Blazor WebAssembly game (`AdventureGameWeb`) while preserving **100% of original story, dialogues, characters, stats, damage ranges, random events, balancing, mechanics, and endings**.

---

## User Review Required

> [!IMPORTANT]
> The game logic, formulas, text strings, dialogues, choices, stats, enemy parameters, and RNG bounds will be copied **verbatim** from `Program.cs`. The only change is the presentation layer: replacing synchronous `Console` inputs/outputs with asynchronous event-driven Razor UI components, reactive state bindings, animated cards, and Dark Fantasy CSS styling.

---

## Proposed Architecture & Structure

We will create a standalone Blazor WebAssembly project inside `AdventureGameWeb/` under .NET 9 (`net9.0`).

### Folder Structure
```
AdventureGameWeb/
├── Pages/
│   └── Index.razor                  # Primary game container component
├── Components/
│   ├── PlayerHud.razor              # Live status bar (HP, Shield, Ammo, Coins, Prisoners)
│   ├── DialoguePanel.razor          # Story text reader with typewriter effect & Continue button
│   ├── NameInputModal.razor         # Character creation input modal
│   ├── ChoicePanel.razor            # Interactive story/room choices
│   ├── BattleScreen.razor           # Combat interface with animated health bars & damage popups
│   ├── EventCard.razor              # Animated room event notification card
│   └── GameOverModal.razor          # Victory / Defeat modals
├── Layouts/
│   └── MainLayout.razor             # Dark Fantasy frame layout
├── Services/
│   ├── IGameEngineService.cs        # Interface for game state & transitions
│   └── GameEngineService.cs         # Core game state machine and event dispatcher
├── Engine/
│   ├── GameState.cs                 # Holds active Player, Enemy, Room, History & Log
│   ├── GamePhase.cs                 # Enum of game flow phases
│   ├── StorySequences.cs            # Complete story script matching Program.cs dialogues
│   └── EventGenerator.cs            # Event pool implementation with exact probabilities
├── Models/
│   ├── Character.cs                 # Abstract base character class
│   ├── Player.cs                    # Player model with shield absorption logic
│   ├── Enemy.cs                     # Enemy model with scalable combat ranges
│   ├── RoomEvent.cs                 # Room event definition
│   └── CombatLog.cs                 # Battle turn results & visual effects
├── wwwroot/
│   ├── css/
│   │   ├── app.css                  # Core CSS reset and typography
│   │   └── theme.css                # Dark Fantasy design system (colors, glassmorphism, animations)
│   └── index.html                   # HTML host entry point with Google Fonts
├── Program.cs                       # Blazor WASM entry point & Dependency Injection setup
└── AdventureGameWeb.csproj          # .NET 9 Blazor WASM project file
```

---

## Technical Details & Logic Verification

### 1. Models & Mechanics (100% Identical)
- **Player**:
  - `Health` = 50 (Starts at 50, boosted to 100 before Final Boss).
  - `Shield` = 0 (Starts at 0, boosted to 200 before Final Boss).
  - `Ammo` = 0 (Boosted to 50 before Final Boss).
  - `Coins` = 0.
  - `PrisonersRemaining` = 25.
  - `ApplyDamage(amount)`: `Shield` absorbs first; remainder hits `Health`.
- **Enemy Scaling**:
  - **Guard / Wild Animal**: HP `rng.Next(25, 36)`, GunDmg `[5..10]`, SwordDmg `[8..14]`, DefendDmg `[0..4]`, PlayerGun `[20..25]`, PlayerSword `[10..15]`.
  - **Ship Captain**: HP `80`, GunDmg `[5..10]`, SwordDmg `[8..14]`, DefendDmg `[0..4]`, PlayerGun `[20..25]`, PlayerSword `[10..15]`.
  - **The King**: HP `400`, GunDmg `[15..30]`, SwordDmg `[14..28]`, DefendDmg `[0..4]`, PlayerGun `[30..50]`, PlayerSword `[20..40]`.
- **Room Events**:
  1. Guard battle: `CreateGuard(isAnimal: false)`
  2. Treasure: `+50 Coins, +10 HP`
  3. Escape Prisoners: `PrisonersRemaining = Math.Max(0, PrisonersRemaining - 5)`. (If `PrisonersRemaining <= 0`, displays `"Empty room!!"`).
  4. Supplies: `+5 Ammo, +15 Shield`
  5. Wild Animal battle: `CreateGuard(isAnimal: true)`
- **Battle Flow**:
  - **Gun**: Costs 1 ammo. If ammo <= 0, displays `"You don't have Ammo!!"` without ending turn.
    - Player dmg: `rng.Next(PlayerGunMin, PlayerGunMax + 1)`.
    - Enemy dmg: `rng.Next(GunDmgMin, GunDmgMax + 1)`.
  - **Sword**:
    - Player dmg: `rng.Next(PlayerSwordMin, PlayerSwordMax + 1)`.
    - Enemy dmg: `rng.Next(SwordDmgMin, SwordDmgMax + 1)`.
  - **Defend**:
    - Player dmg: 0.
    - Enemy dmg: `rng.Next(DefendDmgMin, DefendDmgMax + 1)`.
  - Victory Rewards (Non-King): `+5 Coins`, `+3 Shield`.

### 2. Modern UI & Dark Fantasy Design System
- Palette:
  - Background: `#0F172A`
  - Panels: `#1E293B`
  - Cards: `#334155`
  - Primary Accent: `#38BDF8`
  - Success: `#22C55E`
  - Danger: `#EF4444`
  - Warning: `#FACC15`
  - Text: `#F8FAFC` / Soft Gray `#94A3B8`
- Responsive layout with glassmorphism card containers, ambient glowing highlights, floating damage popups, animated progress bars for HP/Shield, and typewriter dialogue transitions.

---

## Verification Plan

### Automated Build & Test
- Run `dotnet build AdventureGameWeb/AdventureGameWeb.csproj` to ensure zero compilation warnings or errors.

### Manual Verification
- Test all game flows in browser:
  1. Name entry screen -> Dialogue sequence -> Hideout betrayal sequence.
  2. Ship choice (Option 1 vs Option 2 handcuffs dialogue).
  3. Room exploration loop (Entering rooms, checking prisoners count, handling empty rooms, fighting guards/animals, gathering loot).
  4. Attempting to go to roof early (verifying "Find all Prisoners first!!" message).
  5. Defeating Ship Captain -> Transition sequence -> King Boss powerup (HP 100, Shield 200, Ammo 50) -> Final King Battle.
  6. Victory screen and Defeat screen restart loop.
