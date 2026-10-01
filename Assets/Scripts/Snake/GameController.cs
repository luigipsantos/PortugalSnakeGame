using UnityEngine;

namespace SnakeJam
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] SnakeSettings settings = new SnakeSettings();

        SnakeBoard board;
        SnakeView view;
        GameAudio audio;
        float timer;
        float currentStep;

        public int Score { get; private set; }
        public int Best { get; private set; }
        public bool IsAlive { get; private set; }
        public bool HasWon { get; private set; }

        void Awake()
        {
            if (GetComponent<GameHUD>() == null)
                gameObject.AddComponent<GameHUD>();
        }

        void Start()
        {
            board = new SnakeBoard(settings.width, settings.height);
            view = new SnakeView(transform, settings);
            audio = new GameAudio(gameObject);

            audio.PlayAmbient(settings.ambientLoop, settings.ambientVolume);

            StartGame();
        }

        void Update()
        {
            if (!IsAlive)
            {
                if (SnakeInput.RestartPressed()) StartGame();
                return;
            }

            if (SnakeInput.TryGetDirection(out Vector2Int direction))
                board.QueueDirection(direction);

            view.TickAnimation(Time.deltaTime);

            timer += Time.deltaTime;
            if (timer >= currentStep)
            {
                timer -= currentStep;
                Tick();
            }
        }

        void StartGame()
        {
            board.Reset();
            view.Reset(board);

            audio.RestartAmbient(settings.ambientLoop, settings.ambientVolume);

            Score = 0;
            HasWon = false;
            IsAlive = true;
            timer = 0f;
            currentStep = settings.startStepTime;
        }

        void Tick()
        {
            StepResult result = board.Step();

            switch (result)
            {
                case StepResult.Moved:
                    view.OnStep(board, result);
                    break;

                case StepResult.Ate:
                    Score++;
                    currentStep = Mathf.Max(settings.minStepTime, currentStep - settings.speedUpPerFood);
                    PlayEat();
                    view.OnStep(board, result);
                    break;

                case StepResult.AteBonus:
                    Score += 2;
                    currentStep = Mathf.Max(settings.minStepTime, currentStep - settings.speedUpPerFood);
                    PlayBonusEat();
                    view.OnStep(board, result);
                    break;

                case StepResult.AteBonusAndNormal:
                    Score += 3;
                    currentStep = Mathf.Max(settings.minStepTime, currentStep - settings.speedUpPerFood);
                    // Play both, layered — sounds great for the rare case.
                    PlayEat();
                    PlayBonusEat();
                    view.OnStep(board, result);
                    break;

                case StepResult.Won:
                    Score++;
                    PlayEat();
                    view.OnStep(board, result);
                    EndGame(won: true);
                    break;

                case StepResult.Died:
                    PlayDeath();
                    EndGame(won: false);
                    break;
            }
        }

        void PlayEat()
        {
            audio.Play(settings.eatSound, settings.eatVolume, settings.eatPitch);
        }

        void PlayBonusEat()
        {
            AudioClip clip = settings.bonusEatSound != null
                ? settings.bonusEatSound
                : settings.eatSound;

            audio.Play(clip, settings.bonusEatVolume, settings.bonusEatPitch);
        }

        void PlayDeath()
        {
            audio.Play(settings.deathSound, settings.deathVolume, 1f);
        }

        void EndGame(bool won)
        {
            IsAlive = false;
            HasWon = won;
            Best = Mathf.Max(Best, Score);

            if (!won)
            {
                audio.StopAmbient();
                view.ShowDeath(board);
            }

        }
    }
}