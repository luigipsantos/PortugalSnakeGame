using System.Collections.Generic;
using UnityEngine;

namespace SnakeJam
{
    /// The rules of Snake. Knows nothing about sprites, input or UI,
    /// so it's easy to test and to reuse.
    public class SnakeBoard
    {
        public int Width { get; }
        public int Height { get; }
        public Vector2Int Food { get; private set; }
        public Vector2Int Direction { get; private set; }

        // --- Bonus food ---
        public bool BonusActive { get; private set; }
        public Vector2Int BonusFood { get; private set; }
        /// Remaining lifetime in ticks. -1 when inactive.
        public int BonusTicksLeft { get; private set; }
        public int BonusTotalTicks { get; private set; }

        /// Snake cells, head at index 0.
        public IReadOnlyList<Vector2Int> Cells => cells;

        readonly List<Vector2Int> cells = new List<Vector2Int>();
        readonly System.Random rng = new System.Random();
        Vector2Int queuedDirection;

        // Tunables (could move to SnakeSettings later if you want them in the Inspector)
        const int BonusLifetimeTicks = 30;
        const float BonusSpawnChance = 0.25f;

        public SnakeBoard(int width, int height)
        {
            Width = width;
            Height = height;
            Reset();
        }

        public void Reset()
        {
            cells.Clear();

            var start = new Vector2Int(Width / 2, Height / 2);
            for (int i = 0; i < 3; i++)
                cells.Add(start - new Vector2Int(i, 0));

            Direction = queuedDirection = Vector2Int.right;
            BonusActive = false;
            BonusTicksLeft = -1;
            SpawnFood();
        }

        /// Request a turn. Ignored if it would reverse into the snake's own neck.
        public void QueueDirection(Vector2Int newDirection)
        {
            if (newDirection + Direction == Vector2Int.zero) return;
            queuedDirection = newDirection;
        }

        /// Advance the game by one tick.
        public StepResult Step()
        {
            Direction = queuedDirection;
            Vector2Int newHead = cells[0] + Direction;

            bool eatingNormal = newHead == Food;
            bool eatingBonus = BonusActive && newHead == BonusFood;

            // Walls
            if (newHead.x < 0 || newHead.x >= Width || newHead.y < 0 || newHead.y >= Height)
                return StepResult.Died;

            // Self (the tail moves away this tick unless we're growing)
            bool growing = eatingNormal || eatingBonus;
            int checkCount = growing ? cells.Count : cells.Count - 1;
            for (int i = 0; i < checkCount; i++)
            {
                if (cells[i] == newHead)
                    return StepResult.Died;
            }

            cells.Insert(0, newHead);

            if (!growing)
            {
                cells.RemoveAt(cells.Count - 1);
                TickBonus();
                return StepResult.Moved;
            }

            // Ate something
            if (eatingBonus)
            {
                // Bonus doesn't grow the snake (or you could grow it too if you want).
                BonusActive = false;
                BonusTicksLeft = -1;
                // If we also ate normal food same tick (rare), handle below anyway.
                if (!eatingNormal)
                {
                    TickBonus();
                    return StepResult.AteBonus;
                }
            }

            // Ate normal food -> grow + maybe spawn bonus
            bool stillFood = SpawnFood();
            TrySpawnBonus();
            TickBonus();

            if (!stillFood) return StepResult.Won;
            return eatingBonus ? StepResult.AteBonusAndNormal : StepResult.Ate;
        }

        void TickBonus()
        {
            if (!BonusActive) return;

            BonusTicksLeft--;
            if (BonusTicksLeft <= 0)
            {
                BonusActive = false;
                BonusTicksLeft = -1;
            }
        }

        void TrySpawnBonus()
        {
            if (BonusActive) return;
            if (rng.NextDouble() > BonusSpawnChance) return;
            if (!TryFindFreeCell(out Vector2Int cell, avoidFood: true)) return;

            BonusFood = cell;
            BonusActive = true;
            BonusTotalTicks = BonusLifetimeTicks;
            BonusTicksLeft = BonusLifetimeTicks;
        }

        bool SpawnFood()
        {
            if (!TryFindFreeCell(out Vector2Int cell, avoidFood: false))
                return false;

            Food = cell;
            return true;
        }

        bool TryFindFreeCell(out Vector2Int cell, bool avoidFood)
        {
            var free = new List<Vector2Int>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var c = new Vector2Int(x, y);
                    if (cells.Contains(c)) continue;
                    if (avoidFood && c == Food) continue;
                    free.Add(c);
                }
            }

            if (free.Count == 0)
            {
                cell = default;
                return false;
            }

            cell = free[rng.Next(free.Count)];
            return true;
        }
    }
}