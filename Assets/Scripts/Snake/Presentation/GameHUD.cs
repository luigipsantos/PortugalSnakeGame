using UnityEngine;

namespace SnakeJam
{
    /// Score and game over text
    public class GameHUD : MonoBehaviour
    {
        GameController game;
        GUIStyle labelStyle, bigStyle, centerStyle;

        void Awake()
        {
            game = GetComponent<GameController>();
        }

        void OnGUI()
        {
            if (game == null) return;

            // GUI.skin is only safe to use inside OnGUI, so build styles lazily here.
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
                labelStyle.normal.textColor = Color.white;
                bigStyle = new GUIStyle(labelStyle) { fontSize = 36, alignment = TextAnchor.MiddleCenter };
                centerStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter };
            }

            GUI.Label(new Rect(15, 10, 400, 40), $"Score: {game.Score}   Best: {game.Best}", labelStyle);

            if (!game.IsAlive)
            {
                string title = game.HasWon ? "YOU WIN!" : "GAME OVER";
                GUI.Label(new Rect(0, Screen.height / 2f - 40, Screen.width, 50), title, bigStyle);
                GUI.Label(new Rect(0, Screen.height / 2f + 5, Screen.width, 40), "Press Space or R to restart", centerStyle);
            }
        }
    }
}