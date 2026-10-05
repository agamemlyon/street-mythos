using System;
using System.Collections.Generic;

namespace StreetMythos.Core
{
    // Machine à états du jeu, en C# pur pour être testée en EditMode
    public sealed class GameStateMachine
    {
        static readonly Dictionary<GameState, GameState[]> Allowed = new Dictionary<GameState, GameState[]>
        {
            { GameState.Boot,        new[] { GameState.Menu } },
            { GameState.Menu,        new[] { GameState.Exploration, GameState.Cinematic } },
            { GameState.Exploration, new[] { GameState.Dialogue, GameState.Combat, GameState.Cinematic, GameState.Pause, GameState.Menu } },
            { GameState.Dialogue,    new[] { GameState.Exploration, GameState.Combat, GameState.Cinematic } },
            { GameState.Combat,      new[] { GameState.Exploration, GameState.Cinematic, GameState.Pause, GameState.Menu } },
            { GameState.Cinematic,   new[] { GameState.Exploration, GameState.Menu, GameState.Combat } },
            { GameState.Pause,       new GameState[0] }, // la pause revient toujours à l'état précédent
        };

        GameState _beforePause;

        public GameState Current { get; private set; } = GameState.Boot;
        public event Action<GameState, GameState> Changed;

        public bool CanEnter(GameState next)
        {
            if (Current == GameState.Pause) return next == _beforePause || next == GameState.Menu;
            return Array.IndexOf(Allowed[Current], next) >= 0;
        }

        public void Enter(GameState next)
        {
            if (!CanEnter(next))
                throw new InvalidOperationException($"Transition interdite : {Current} -> {next}");
            if (next == GameState.Pause) _beforePause = Current;
            var previous = Current;
            Current = next;
            Changed?.Invoke(previous, next);
        }
    }
}
