using System.Collections.Generic;
using UnityEngine;

namespace SnakeJam
{
    /// Draws a SnakeBoard
    /// Contains no game rules
    public class SnakeView : MonoBehaviour
    {
        readonly SnakeSettings settings;
        readonly Sprite square;
        readonly Sprite[] icons;
        readonly Sprite foodSprite;
        readonly Transform snakeRoot;
        readonly Transform gridRoot;
        readonly Transform foodTransform;
        readonly Transform bonusRoot;
        readonly SpriteRenderer bonusBody;
        readonly SpriteRenderer bonusTimer;

        readonly List<SpriteRenderer> segments = new List<SpriteRenderer>();
        readonly List<int> segmentIconIndex = new List<int>();
        readonly List<float> segmentAnimPhase = new List<float>();

        float animTime;

        bool UseIcons => icons.Length > 0;
        bool UseAnimation => UseIcons && settings.iconsAreAnimation && icons.Length > 1;
        bool UseFoodSprite => foodSprite != null;

        public SnakeView(Transform root, SnakeSettings settings)
        {
            this.settings = settings;
            square = MakeSquareSprite();

            icons = Resources.LoadAll<Sprite>(settings.iconsFolder);
            if (icons.Length == 0)
                Debug.LogWarning($"No sprites found in Resources/{settings.iconsFolder}. Using plain colored squares.");

            Sprite[] foodSprites = string.IsNullOrEmpty(settings.foodIconsFolder)
                ? new Sprite[0]
                : Resources.LoadAll<Sprite>(settings.foodIconsFolder);

            if (foodSprites.Length == 0)
                Debug.LogWarning($"No sprites in Resources/{settings.foodIconsFolder}. Using colored squares for food.");

            foodSprite = foodSprites.Length > 0 ? foodSprites[0] : null;

            SetupCamera();

            Transform boardRoot = NewRoot("Board", root);
            snakeRoot = NewRoot("Snake", root);

            SpriteRenderer background = NewBlock("Background", settings.boardColor, 0, boardRoot);
            background.transform.position = new Vector3((settings.width - 1) / 2f, (settings.height - 1) / 2f, 0);
            background.transform.localScale = new Vector3(settings.width, settings.height, 1);

            gridRoot = NewRoot("Grid", boardRoot);
            if (settings.showGrid) BuildGrid();

            // --- Normal food ---
            SpriteRenderer food = NewBlock("Food", settings.foodColor, 2, root);
            if (UseFoodSprite)
            {
                food.sprite = foodSprite;
                food.color = Color.white;
                food.transform.localScale = FitSprite(foodSprite, 0.7f);
            }
            else
            {
                food.transform.localScale = Vector3.one * 0.7f;
            }
            foodTransform = food.transform;

            // --- Bonus food ---
            bonusRoot = NewRoot("BonusFood", root);
            bonusBody = NewBlock("Body", settings.bonusFoodColor, 2, bonusRoot);
            if (UseFoodSprite)
            {
                bonusBody.sprite = foodSprite;
                bonusBody.transform.localScale = FitSprite(foodSprite, 0.85f);
            }
            else
            {
                bonusBody.transform.localScale = Vector3.one * 0.75f;
            }

            bonusTimer = NewBlock("Timer", settings.bonusTimerColor, 3, bonusRoot);
            bonusTimer.transform.localScale = new Vector3(0.8f, 0.12f, 1f);
            bonusTimer.transform.localPosition = new Vector3(0f, 0.55f, 0f);

            bonusRoot.gameObject.SetActive(false);
        }

        public void Reset(SnakeBoard board)
        {
            segmentIconIndex.Clear();
            segmentAnimPhase.Clear();
            for (int i = 0; i < board.Cells.Count; i++)
            {
                segmentIconIndex.Add(RandomIconIndex());
                segmentAnimPhase.Add(Random.value);
            }

            animTime = 0f;
            Render(board, alive: true);
        }

        public void OnStep(SnakeBoard board, StepResult result)
        {
            segmentIconIndex.Insert(0, RandomIconIndex());
            segmentAnimPhase.Insert(0, Random.value);

            bool grew = result == StepResult.Ate ||
                        result == StepResult.AteBonusAndNormal;

            if (!grew)
            {
                segmentIconIndex.RemoveAt(segmentIconIndex.Count - 1);
                segmentAnimPhase.RemoveAt(segmentAnimPhase.Count - 1);
            }

            Render(board, alive: true);
        }

        public void ShowDeath(SnakeBoard board)
        {
            Render(board, alive: false);
        }

        /// Call this each frame from GameController. Drives both the snake
        /// animation and the bonus-food pulse.
        public void TickAnimation(float deltaTime)
        {
            if (UseAnimation)
            {
                animTime += deltaTime;
                AnimateSegments();
            }

            PulseBonusFood();
        }

        void AnimateSegments()
        {
            if (icons.Length == 0) return;
            int frame = Mathf.FloorToInt(animTime * settings.iconFps) % icons.Length;
            for (int i = 0; i < segments.Count; i++)
            {
                if (!segments[i].gameObject.activeSelf) continue;
                segments[i].sprite = icons[(frame + i) % icons.Length];
                FitSegment(segments[i], i == 0);
            }
        }

        void PulseBonusFood()
        {
            if (!bonusRoot.gameObject.activeSelf) return;

            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * settings.bonusPulseSpeed * Mathf.PI * 2f);

            if (UseFoodSprite)
            {
                // Sprite color acts as a tint: white = original art,
                // other colors multiply on top. Pulse toward the bonus color.
                bonusBody.color = Color.Lerp(Color.white, settings.bonusFoodColor, pulse);
            }
            else
            {
                // Solid square: pulse between base bonus color and a brighter version.
                Color a = settings.bonusFoodColor;
                Color b = Color.Lerp(a, Color.white, settings.bonusPulseStrength);
                bonusBody.color = Color.Lerp(a, b, pulse);
            }
        }

        void Render(SnakeBoard board, bool alive)
        {
            var cells = board.Cells;
            foodTransform.position = new Vector3(board.Food.x, board.Food.y, 0);

            // Bonus food
            if (board.BonusActive && alive)
            {
                bonusRoot.gameObject.SetActive(true);
                bonusRoot.position = new Vector3(board.BonusFood.x, board.BonusFood.y, 0);

                float t = board.BonusTotalTicks > 0
                    ? Mathf.Clamp01((float)board.BonusTicksLeft / board.BonusTotalTicks)
                    : 1f;

                bonusTimer.transform.localScale = new Vector3(0.8f * t, 0.12f, 1f);
                bonusTimer.color = Color.Lerp(Color.red, settings.bonusTimerColor, t);
            }
            else
            {
                bonusRoot.gameObject.SetActive(false);
            }

            // Snake segments
            while (segments.Count < cells.Count)
            {
                segments.Add(NewBlock("Segment", Color.white, 1, snakeRoot));
                segmentIconIndex.Add(RandomIconIndex());
                segmentAnimPhase.Add(Random.value);
            }

            for (int i = 0; i < segments.Count; i++)
            {
                bool used = i < cells.Count;
                segments[i].gameObject.SetActive(used);
                if (!used) continue;

                SpriteRenderer sr = segments[i];
                sr.name = i == 0 ? "Head" : "Segment " + i;
                sr.transform.position = new Vector3(cells[i].x, cells[i].y, 0);

                // Sprite selection
                if (UseAnimation)
                {
                    int frame = Mathf.FloorToInt(animTime * settings.iconFps) % icons.Length;
                    sr.sprite = icons[frame];
                }
                else
                {
                    sr.sprite = UseIcons ? icons[segmentIconIndex[i] % icons.Length] : square;
                }

                if (!alive) sr.color = settings.deathColor;
                else sr.color = UseIcons ? Color.white : (i == 0 ? settings.headColor : settings.bodyColor);

                FitSegment(sr, i == 0);
            }
        }

        void FitSegment(SpriteRenderer sr, bool isHead)
        {
            Vector3 size = sr.sprite.bounds.size;
            float fit = 1f / Mathf.Max(size.x, size.y);
            sr.transform.localScale = Vector3.one * fit * (isHead ? 1f : 0.85f);
        }

        static Vector3 FitSprite(Sprite s, float maxSize)
        {
            Vector3 size = s.bounds.size;
            float fit = 1f / Mathf.Max(size.x, size.y);
            return Vector3.one * fit * maxSize;
        }

        int RandomIconIndex()
        {
            return UseIcons ? Random.Range(0, icons.Length) : 0;
        }

        void BuildGrid()
        {
            // Vertical lines
            for (int x = 0; x <= settings.width; x++)
            {
                float wx = x - 0.5f;
                var sr = NewBlock($"GridV{x}", settings.gridColor, settings.gridSortingOrder, gridRoot);
                sr.transform.position = new Vector3(wx, (settings.height - 1) / 2f, 0);
                sr.transform.localScale = new Vector3(settings.gridLineWidth, settings.height, 1f);
            }

            // Horizontal lines
            for (int y = 0; y <= settings.height; y++)
            {
                float wy = y - 0.5f;
                var sr = NewBlock($"GridH{y}", settings.gridColor, settings.gridSortingOrder, gridRoot);
                sr.transform.position = new Vector3((settings.width - 1) / 2f, wy, 0);
                sr.transform.localScale = new Vector3(settings.width, settings.gridLineWidth, 1f);
            }
        }

        void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            cam.orthographic = true;
            cam.orthographicSize = settings.height / 2f + 1f;
            cam.transform.position = new Vector3((settings.width - 1) / 2f, (settings.height - 1) / 2f, -10f);
            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        Transform NewRoot(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            return go.transform;
        }

        SpriteRenderer NewBlock(string name, Color color, int sortingOrder, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            return sr;
        }

        static Sprite MakeSquareSprite()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}