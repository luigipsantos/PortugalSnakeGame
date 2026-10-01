using System;
using UnityEngine;

namespace SnakeJam
{
    /// All tweakable numbers in one place. Shows up in the Inspector on the Game object.
    [Serializable]
    public class SnakeSettings
    {
        [Header("Grid")]
        public int width = 12;
        public int height = 12;

        [Header("Speed (seconds per move, lower = faster)")]
        public float startStepTime = 0.35f;
        public float minStepTime = 0.07f;
        [Tooltip("How much the step time drops each time the snake eats")]
        public float speedUpPerFood = 0.01f;

        [Header("Icons (Resources folder)")]
        [Tooltip("Folder inside any 'Resources' folder")]
        public string iconsFolder = "SnakeIcons";

        [Header("Animated Icons")]
        [Tooltip("Frames per second for animated sprites. 0 = static.")]
        public float iconFps = 8f;
        [Tooltip("If true, all icons in the folder are treated as one looping animation.")]
        public bool iconsAreAnimation = false;

        [Header("Colors")]
        public Color boardColor = new Color(0.1f, 0.1f, 0.14f);
        public Color headColor = new Color(0.4f, 1f, 0.5f);
        public Color bodyColor = new Color(0.2f, 0.75f, 0.35f);
        public Color foodColor = new Color(1f, 0.35f, 0.35f);
        public Color bonusFoodColor = new Color(1f, 0.9f, 0.2f);
        public Color deathColor = new Color(1f, 0.15f, 0.15f);

        [Header("Grid Lines")]
        public bool showGrid = true;
        public Color gridColor = new Color(1f, 1f, 1f, 0.08f);
        [Tooltip("World-space thickness of grid lines (slim = ~0.02)")]
        public float gridLineWidth = 0.02f;
        public int gridSortingOrder = -1;

        [Header("Bonus Food Timer")]
        public Color bonusTimerColor = new Color(1f, 1f, 1f, 0.9f);

        [Header("Food Sprites")]
        [Tooltip("Folder inside any 'Resources' folder. Leave empty to use colored squares.")]
        public string foodIconsFolder = "SnakeFood";

        [Tooltip("How strongly the bonus food pulses its color.")]
        [Range(0f, 1f)] public float bonusPulseStrength = 0.35f;

        [Tooltip("Pulses per second for the bonus food color.")]
        public float bonusPulseSpeed = 3f;

        [Header("Audio")]
        [Tooltip("Played when the snake eats the normal food.")]
        public AudioClip eatSound;

        [Tooltip("Played when the snake eats the bonus food. Leave empty to fall back to eatSound.")]
        public AudioClip bonusEatSound;

        [Tooltip("Played on death. Optional.")]
        public AudioClip deathSound;

        [Range(0f, 1f)] public float eatVolume = 0.8f;
        [Range(0f, 1f)] public float bonusEatVolume = 1f;
        [Range(0f, 1f)] public float deathVolume = 0.8f;

        [Tooltip("Pitch applied to the normal eat sound. 1 = unchanged.")]
        public float eatPitch = 1f;

        [Tooltip("Pitch applied to the bonus eat sound. Higher = weirder/more rewarding.")]
        public float bonusEatPitch = 1.35f;

        [Header("Ambient")]
        public AudioClip ambientLoop;
        [Range(0f, 1f)] public float ambientVolume = 0.35f;
    }
}