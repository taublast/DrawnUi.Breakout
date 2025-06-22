using BreakoutGame.Game;
using Microsoft.Maui.Graphics;

namespace BreakoutGame.Game
{
    /// <summary>
    /// Defines the properties of a brick preset
    /// </summary>
    public class BrickPreset
    {
        /// <summary>
        /// Unique identifier for the preset
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Visual color of the brick
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Additional hits required beyond the first hit
        /// </summary>
        public int SupplementaryHitsToDestroy { get; set; }

        /// <summary>
        /// Whether the brick cannot be destroyed
        /// </summary>
        public bool Undestructible { get; set; }

        /// <summary>
        /// Score value when destroyed
        /// </summary>
        public int ScoreValue { get; set; }

        /// <summary>
        /// Human-readable description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Base probability of appearance
        /// </summary>
        public float Probability { get; set; }

        /// <summary>
        /// Whether this brick type has special effects
        /// </summary>
        public bool IsSpecial { get; set; }

        /// <summary>
        /// Type of power-up this brick might drop
        /// </summary>
        public PowerUpType? PowerUpType { get; set; }
    }
}