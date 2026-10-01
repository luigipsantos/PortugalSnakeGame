using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SnakeJam
{
    public static class SnakeInput
    {
#if ENABLE_INPUT_SYSTEM
        public static bool TryGetDirection(out Vector2Int direction)
        {
            direction = Vector2Int.zero;
            var k = Keyboard.current;
            if (k == null) return false;

            if (k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame) direction = Vector2Int.up;
            else if (k.sKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame) direction = Vector2Int.down;
            else if (k.aKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame) direction = Vector2Int.left;
            else if (k.dKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame) direction = Vector2Int.right;

            return direction != Vector2Int.zero;
        }

        public static bool RestartPressed()
        {
            var k = Keyboard.current;
            return k != null && (k.spaceKey.wasPressedThisFrame || k.rKey.wasPressedThisFrame);
        }
#else
        public static bool TryGetDirection(out Vector2Int direction)
        {
            direction = Vector2Int.zero;

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector2Int.right;

            return direction != Vector2Int.zero;
        }

        public static bool RestartPressed()
        {
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R);
        }
#endif
    }
}