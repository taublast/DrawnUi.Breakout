namespace Breakout.Game;

/// <summary>
/// Manages brick presets and their applications
/// </summary>
public static class BrickPresets
{
    /// <summary>
    /// Collection of all available brick presets
    /// </summary>
    public static Dictionary<string, BrickPreset> Presets;

    /// <summary>
    /// Static constructor to initialize presets
    /// </summary>
    static BrickPresets()
    {
        InitializePresets();
    }

    /// <summary>
    /// Gets all available presets
    /// </summary>
    public static IReadOnlyDictionary<string, BrickPreset> All => Presets;

    /// <summary>
    /// Initializes the standard brick presets
    /// </summary>
    private static void InitializePresets()
    {
        Presets = new Dictionary<string, BrickPreset>
        {
            // Standard bricks (1 hit)
            { "Standard_Red", new BrickPreset {
                Id = "Standard_Red",
                BackgroundColor = Colors.Magenta,
                SupplementaryHitsToDestroy = 0,
                ScoreValue = 50,
                Description = "Standard brick, 1 hit",
                Probability = 0.2f
            }},

            { "Standard_Blue", new BrickPreset {
                    Id = "Standard_Blue",
                    BackgroundColor = Colors.CornflowerBlue,
                    SupplementaryHitsToDestroy = 0,
                    ScoreValue = 10,
                    Description = "Standard brick, 1 hit",
                    Probability = 0.30f
            }},

            { "Standard_Green", new BrickPreset {
                Id = "Standard_Green",
                BackgroundColor = Colors.HotPink,
                SupplementaryHitsToDestroy = 0,
                ScoreValue = 10,
                Description = "Standard brick, 1 hit",
                Probability = 0.30f
            }},

            { "Standard_Orange", new BrickPreset {
                Id = "Standard_Orange",
                BackgroundColor = Colors.Orange,
                SupplementaryHitsToDestroy = 0,
                ScoreValue = 15,
                PowerUpType = PowerupType.Destroyer,
                Description = "Standard brick, 1 hit",
                Probability = 0.20f
            }},

            { "Standard_Yellow", new BrickPreset {
                Id = "Standard_Yellow",
                BackgroundColor = Colors.Yellow,
                SupplementaryHitsToDestroy = 0,
                ScoreValue = 15,
                Description = "Standard brick, 1 hit",
                Probability = 0.15f
            }},

            // Reinforced bricks (2 hits)
            { "Reinforced_Brown", new BrickPreset {
                Id = "Reinforced_Brown",
                BackgroundColor = Colors.GreenYellow,
                SupplementaryHitsToDestroy = 0,
                ScoreValue = 20,
                Description = "???",
                PowerUpType = PowerupType.StickyBall,
                Probability = 0.15f
            }},

            // Hard bricks (3 hits)
            { "Hard_DarkGray", new BrickPreset {
                Id = "Hard_DarkGray",
                BackgroundColor = Colors.DarkGreen,
                SupplementaryHitsToDestroy = 1,
                ScoreValue = 30,
                PowerUpType = PowerupType.ExtraLife,
                Description = "Hard brick, 3 hits",
                Probability = 0.10f
            }},

            // Obstacle bricks (indestructible)
            { "Obstacle_Black", new BrickPreset {
                Id = "Obstacle_Black",
                BackgroundColor = Colors.DarkGray,
                Undestructible = true,
                ScoreValue = 0,
                Description = "Indestructible obstacle",
                Probability = 0.05f
            }},

            // Special bricks (with power-ups)
            { "Special_Green", new BrickPreset {
                Id = "Special_Green",
                BackgroundColor = Colors.LightGreen,
                SupplementaryHitsToDestroy = 0,
                ScoreValue = 25,
                IsSpecial = true,
                PowerUpType = PowerupType.ExpandPaddle,
                Description = "Drops paddle expander power-up",
                Probability = 0.07f
            }},

            { "Special_Blue", new BrickPreset {
                Id = "Special_Blue",
                BackgroundColor = Colors.LightBlue,
                SupplementaryHitsToDestroy = 0,
                ScoreValue = 25,
                IsSpecial = true,
                PowerUpType = PowerupType.SlowBall,
                Description = "Drops slow ball power-up",
                Probability = 0.07f
            }},

            { "Special_Purple", new BrickPreset {
                Id = "Special_Purple",
                BackgroundColor = Colors.Purple,
                SupplementaryHitsToDestroy = 0,
                ScoreValue = 35,
                IsSpecial = true,
                PowerUpType = PowerupType.ExtraLife,
                Description = "Drops extralife power-up",
                Probability = 0.04f
            }},

            { "Special_Gold", new BrickPreset {
                Id = "Special_Gold",
                BackgroundColor = Colors.Gold,
                SupplementaryHitsToDestroy = 1,
                ScoreValue = 50,
                IsSpecial = true,
                PowerUpType = PowerupType.Destroyer,
                Description = "Drops extra life power-up (rare)",
                Probability = 0.02f
            }}
        };
    }

    /// <summary>
    /// Applies a preset to a brick
    /// </summary>
    public static void ApplyPreset(BrickSprite brick, string presetId)
    {
        if (brick != null)
        {
            brick.ResetAnimationState();
            if (Presets.TryGetValue(presetId, out var preset))
            {
                brick.BackgroundColor = preset.BackgroundColor;
                brick.SupplementaryHitsToDestroy = preset.SupplementaryHitsToDestroy;
                brick.Undestructible = preset.Undestructible;

                // Additional properties could be set here if BrickSprite supports them
            }
        }
    }

    /// <summary>
    /// Gets a preset by ID
    /// </summary>
    public static BrickPreset GetPreset(string presetId)
    {
        return Presets.TryGetValue(presetId, out var preset) ? preset : null;
    }

    /// <summary>
    /// Gets all presets matching a filter
    /// </summary>
    public static List<BrickPreset> GetPresets(Func<BrickPreset, bool> filter)
    {
        return Presets.Values.Where(filter).ToList();
    }
}