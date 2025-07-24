namespace Breakout.Game
{
    
    /// <summary>
    /// Manages level generation and difficulty progression
    /// </summary>
    public class LevelManager
    {
        #region Properties

        /// <summary>
        /// Current game level
        /// </summary>
        public int CurrentLevel { get; private set; } = 1;

        /// <summary>
        /// Current difficulty factor
        /// </summary>
        public float Difficulty { get; private set; } = 1.0f;

        /// <summary>
        /// Number of critical paths in path-based generations
        /// </summary>
        public int MaxPathsPerLevel { get; set; } = 3;

        /// <summary>
        /// Probability of reinforced bricks appearing
        /// </summary>
        public float ReinforcedBrickProbability { get; private set; } = 0.15f;

        /// <summary>
        /// Probability of obstacle bricks appearing
        /// </summary>
        public float ObstacleBrickProbability { get; private set; } = 0.05f;

        /// <summary>
        /// Probability of special bricks appearing
        /// </summary>
        public float SpecialBrickProbability { get; private set; } = 0.1f;

        /// <summary>
        /// Default brick height
        /// </summary>
        public float DefaultBrickHeight { get; set; } = 20f;

        /// <summary>
        /// Maximum columns in layout
        /// </summary>
        public int MaxColumns { get; set; } = BreakoutGame.MAX_BRICKS_COLUMNS;

        /// <summary>
        /// Maximum rows in layout
        /// </summary>
        public int MaxRows { get; set; } = BreakoutGame.MAX_BRICKS_ROWS;

        /// <summary>
        /// Horizontal spacing between bricks
        /// </summary>
        public float HorizontalSpacing { get; set; } = 4f;

        /// <summary>
        /// Vertical spacing between bricks
        /// </summary>
        public float VerticalSpacing { get; set; } = 6f;

        /// <summary>
        /// Space from top of play area
        /// </summary>
        public float TopMargin { get; set; } = 30f;

        /// <summary>
        /// Space from sides of play area
        /// </summary>
        public float SideMargin { get; set; } = 16f;

        /// <summary>
        /// Whether to allow variable brick sizes
        /// </summary>
        public bool AllowVariableBrickSizes { get; set; } = false;

        /// <summary>
        /// Base difficulty multiplier
        /// </summary>
        public float BaseDifficulty { get; set; } = 1.0f;

        /// <summary>
        /// How much difficulty increases per level
        /// </summary>
        public float DifficultyIncreasePerLevel { get; set; } = 0.15f;

        /// <summary>
        /// Base chance for indestructible bricks
        /// </summary>
        public float IndestructibleBrickBaseChance { get; set; } = 0.02f;

        /// <summary>
        /// Increase in indestructible chance per level
        /// </summary>
        public float IndestructibleBrickLevelScaling { get; set; } = 0.01f;

        /// <summary>
        /// Minimum level for indestructible bricks to appear
        /// </summary>
        public int MinimumLevelForIndestructible { get; set; } = 3;

        /// <summary>
        /// Maximum indestructible bricks per level
        /// </summary>
        public int MaxIndestructibleBricks { get; set; } = 5;

        /// <summary>
        /// Scale of noise function for organic shapes
        /// </summary>
        public float NoiseScale { get; set; } = 0.1f;

        /// <summary>
        /// Threshold for brick placement in noise-based generation
        /// </summary>
        public float NoiseThreshold { get; set; } = 0.5f;

        /// <summary>
        /// Smoothing iterations for organic shapes
        /// </summary>
        public int SmoothingPasses { get; set; } = 2;

        /// <summary>
        /// Random number generator
        /// </summary>
        private Random _random = new Random();

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new level manager
        /// </summary>
        public LevelManager()
        {
            // Default constructor
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Counts the number of breakable bricks in the generated level
        /// </summary>
        /// <param name="positions">The list of brick positions generated for the level</param>
        /// <returns>The number of bricks that can be destroyed</returns>
        public int CountBreakableBricks(List<BrickPosition> positions)
        {
            int breakableBricks = 0;

            foreach (var position in positions)
            {
                if (!string.IsNullOrEmpty(position.PresetId))
                {
                    var preset = BrickPresets.GetPreset(position.PresetId);

                    // Count as breakable if not undestructible and not special
                    if (preset != null && !preset.Undestructible)
                    {
                        breakableBricks++;
                    }
                }
            }

            return breakableBricks;
        }

        /// <summary>
        /// Generates a level with specified parameters
        /// </summary>
        public List<BrickPosition> GenerateLevel(
            int level,
            float availableWidth,
            float availableHeight,
            FormationType formation = FormationType.Grid,
            List<string> allowedPresets = null,
            Dictionary<string, float> presetProbabilityOverrides = null)
        {
            CurrentLevel = level;
            Difficulty = CalculateLevelDifficulty(level);

            // Calculate columns and rows based on level and available space
            int columns = DetermineColumns(level);
            int rows = DetermineRows(level);

            // Generate formation positions
            var positions = GenerateFormation(formation, columns, rows);

            // Filter allowed presets if specified
            var presets = FilterPresets(allowedPresets);

            // Apply difficulty and assign brick types
            AssignBrickTypes(positions, level, presets, presetProbabilityOverrides);

            // Return the positions (let the game create actual sprites)
            return positions;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculates level difficulty based on level number
        /// </summary>
        private float CalculateLevelDifficulty(int level)
        {
            return BaseDifficulty + (level) * DifficultyIncreasePerLevel;
        }

        /// <summary>
        /// Determines number of columns based on level
        /// </summary>
        private int DetermineColumns(int level)
        {
            return MaxColumns;
        }

        /// <summary>
        /// Determines number of rows based on level
        /// </summary>
        private int DetermineRows(int level)
        {
            return Math.Min(BreakoutGame.MIN_BRICKS_ROWS + level, MaxRows);
        }

        /// <summary>
        /// Calculates brick width based on available space and columns
        /// </summary>
        private float CalculateBrickWidth(float availableWidth, int columns)
        {
            float totalSpacing = HorizontalSpacing * (columns + 1);
            float availableWidthForBricks = availableWidth - totalSpacing - (SideMargin * 2);
            return availableWidthForBricks / columns;
        }

        /// <summary>
        /// Filters presets based on allowed list
        /// </summary>
        private List<BrickPreset> FilterPresets(List<string> allowedPresets)
        {
            if (allowedPresets == null || allowedPresets.Count == 0)
            {
                // If no filter, use all presets
                return BrickPresets.All.Values.ToList();
            }

            return BrickPresets.GetPresets(p => allowedPresets.Contains(p.Id));
        }

        /// <summary>
        /// Gets the chance for indestructible bricks based on level
        /// </summary>
        private float GetIndestructibleBrickChance(int level)
        {
            if (level < MinimumLevelForIndestructible)
                return 0;

            return Math.Min(
                IndestructibleBrickBaseChance + ((level - MinimumLevelForIndestructible) * IndestructibleBrickLevelScaling),
                0.25f // Cap at 25% chance for game balance
            );
        }

        /// <summary>
        /// Determines if a position is strategic for obstacle placement
        /// </summary>
        private bool IsStrategicPosition(float row, float col, int maxCols)
        {
            // Key pathways (middle columns on higher rows)
            bool isKeyPathway = (col > maxCols / 3 && col < 2 * maxCols / 3) && row < 2;

            // Border positions that block access
            bool isBorder = col == 0 || col == maxCols - 1 || row == 0;

            // "Chokepoints" where the ball must pass
            bool isChokepoint = (Math.Abs(col - maxCols / 2) < 1) && row % 2 == 0;

            return isKeyPathway || isBorder || isChokepoint;
        }

        /// <summary>
        /// Selects a brick preset based on difficulty and random factors
        /// </summary>
        private string SelectBrickPresetByDifficulty(List<BrickPreset> presets, float difficulty,
            Dictionary<string, float> probabilityOverrides)
        {
            // Group presets by type
            var standardPresets = presets.Where(p => p.SupplementaryHitsToDestroy == 0 && !p.IsSpecial && !p.Undestructible).ToList();
            var reinforcedPresets = presets.Where(p => p.SupplementaryHitsToDestroy > 0 && !p.IsSpecial && !p.Undestructible).ToList();
            var specialPresets = presets.Where(p => p.IsSpecial).ToList();
            var obstaclePresets = presets.Where(p => p.Undestructible).ToList();

            // If no presets of a certain type, return null or a default
            if (standardPresets.Count == 0)
                return null;

            float randomValue = (float)_random.NextDouble();

            // Apply difficulty scaling to probabilities
            float reinforcedChance = ReinforcedBrickProbability * difficulty;
            float specialChance = SpecialBrickProbability;
            float obstacleChance = ObstacleBrickProbability * difficulty;

            // Apply any overrides
            if (probabilityOverrides != null)
            {
                foreach (var preset in reinforcedPresets)
                {
                    if (probabilityOverrides.TryGetValue(preset.Id, out float overrideProb))
                    {
                        reinforcedChance = overrideProb;
                        break;
                    }
                }

                foreach (var preset in obstaclePresets)
                {
                    if (probabilityOverrides.TryGetValue(preset.Id, out float overrideProb))
                    {
                        obstacleChance = overrideProb;
                        break;
                    }
                }

                foreach (var preset in specialPresets)
                {
                    if (probabilityOverrides.TryGetValue(preset.Id, out float overrideProb))
                    {
                        specialChance = overrideProb;
                        break;
                    }
                }
            }

            // Select brick type based on probabilities
            if (obstaclePresets.Count > 0 && randomValue < obstacleChance)
            {
                return obstaclePresets[_random.Next(obstaclePresets.Count)].Id;
            }
            else if (reinforcedPresets.Count > 0 && randomValue < obstacleChance + reinforcedChance)
            {
                return reinforcedPresets[_random.Next(reinforcedPresets.Count)].Id;
            }
            else if (specialPresets.Count > 0 && randomValue < obstacleChance + reinforcedChance + specialChance)
            {
                return specialPresets[_random.Next(specialPresets.Count)].Id;
            }
            else
            {
                return standardPresets[_random.Next(standardPresets.Count)].Id;
            }
        }

        /// <summary>
        /// Assigns brick types to positions based on level difficulty
        /// </summary>
        private void AssignBrickTypes(List<BrickPosition> positions, int level, List<BrickPreset> presets,
            Dictionary<string, float> probabilityOverrides)
        {
            int indestructibleCount = 0;
            int maxCols = positions.Max(p => (int)p.Column) + 1;

            foreach (var position in positions)
            {
                // Check for strategic positions that might get indestructible bricks
                bool isStrategic = IsStrategicPosition(position.Row, position.Column, maxCols);

                // Calculate indestructible chance with strategic bonus
                float indestructibleChance = GetIndestructibleBrickChance(level);
                if (isStrategic)
                    indestructibleChance *= 2;

                // Assign indestructible if appropriate
                if (indestructibleCount < MaxIndestructibleBricks && _random.NextDouble() < indestructibleChance)
                {
                    var obstaclePresets = presets.Where(p => p.Undestructible).ToList();
                    if (obstaclePresets.Count > 0)
                    {
                        position.PresetId = obstaclePresets[_random.Next(obstaclePresets.Count)].Id;
                        indestructibleCount++;
                        continue;
                    }
                }

                // For other bricks, select by difficulty
                position.PresetId = SelectBrickPresetByDifficulty(presets, Difficulty, probabilityOverrides);
            }
        }



        /// <summary>
        /// Normalizes brick positions to ensure they start from column 0 (left-aligned)
        /// </summary>
        private void NormalizePositions(List<BrickPosition> positions)
        {
            if (positions == null || positions.Count == 0)
                return;

            float minColumn = positions.Min(p => p.Column);

            if (minColumn != 0)
            {
                float offset = -minColumn;
                foreach (var position in positions)
                {
                    position.Column += offset;
                }
            }
        }

        /// <summary>
        /// Adjusts the number of rows based on formation requirements
        /// </summary>
        private int AdjustRowsForFormation(FormationType formation, int requestedRows)
        {
            switch (formation)
            {
                case FormationType.Diamond:
                    // Diamond needs minimum 5 rows to form proper shape, max 12 for good proportions
                    return Math.Max(5, Math.Min(12, requestedRows));

                case FormationType.Pyramid:
                    // Pyramid needs minimum 4 rows to form triangle, max 10 for good gameplay
                    return Math.Max(4, Math.Min(10, requestedRows));

                case FormationType.Arch:
                    // Arch needs minimum 5 rows for proper arch shape, max 12
                    return Math.Max(5, Math.Min(12, requestedRows));

                case FormationType.Wave:
                    // Wave needs minimum 6 rows for visible wave pattern, max 10
                    return Math.Max(6, Math.Min(10, requestedRows));

                case FormationType.Maze:
                    // Maze needs minimum 6 rows for corridors, max 15 for complexity
                    return Math.Max(6, Math.Min(15, requestedRows));

                case FormationType.Organic:
                    // Organic can work with any size but looks better with minimum 5 rows
                    return Math.Max(5, requestedRows);

                case FormationType.Zigzag:
                    // Zigzag needs minimum 4 rows for pattern, max 12
                    return Math.Max(4, Math.Min(12, requestedRows));

                case FormationType.Grid:
                default:
                    // Grid can work with any number of rows
                    return requestedRows;
            }
        }

        /// <summary>
        /// Generates formation based on specified type
        /// </summary>
        private List<BrickPosition> GenerateFormation(FormationType formation, int columns, int rows)
        {
            List<BrickPosition> positions;

            // Adjust rows based on formation requirements
            rows = AdjustRowsForFormation(formation, rows);

            switch (formation)
            {
                case FormationType.Pyramid:
                    positions = GeneratePyramidFormation(columns, rows);
                    break;
                case FormationType.Arch:
                    positions = GenerateArchFormation(columns, rows);
                    break;
                case FormationType.Diamond:
                    positions = GenerateDiamondFormation(columns, rows);
                    break;
                case FormationType.Zigzag:
                    positions = GenerateZigzagFormation(columns, rows);
                    break;
                //case FormationType.Spiral:
                //    positions = GenerateSpiralFormation(columns, rows);
                //    break;
                case FormationType.Organic:
                    positions = GenerateOrganicFormation(columns, rows);
                    break;
                case FormationType.Wave:
                    positions = GenerateWaveFormation(columns, rows);
                    break;
                case FormationType.Maze:
                    positions = GenerateMazeFormation(columns, rows);
                    break;
                case FormationType.Grid:
                default:
                    positions = GenerateGridFormation(columns, rows);
                    break;
            }

            NormalizePositions(positions);
            return positions;
        }

        #endregion

        #region Formation Generators

        /// <summary>
        /// Generates a standard grid formation
        /// </summary>
        private List<BrickPosition> GenerateGridFormation(int columns, int rows)
        {
            var positions = new List<BrickPosition>();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    positions.Add(new BrickPosition
                    {
                        Column = col,
                        Row = row,
                        PresetId = null // Will be assigned later
                    });
                }
            }

            return positions;
        }

        /// <summary>
        /// Generates a pyramid formation
        /// </summary>
        private List<BrickPosition> GeneratePyramidFormation(int columns, int rows)
        {
            var positions = new List<BrickPosition>();
            int maxWidth = columns;

            // Center position offset
            float centerX = maxWidth / 2f;

            for (int row = 0; row < rows; row++)
            {
                // Calculate how many bricks in this row (wider at bottom, narrower at top)
                int rowWidth = maxWidth - row * 2;
                if (rowWidth <= 0) break;

                // Calculate starting position to center the row
                float startX = centerX - (rowWidth / 2f);

                for (int col = 0; col < rowWidth; col++)
                {
                    positions.Add(new BrickPosition
                    {
                        Column = startX + col,
                        Row = row,
                        PresetId = null // Will be assigned later
                    });
                }
            }

            return positions;
        }

        /// <summary>
        /// Generates an arch formation
        /// </summary>
        private List<BrickPosition> GenerateArchFormation(int columns, int rows)
        {
            var positions = new List<BrickPosition>();

            // Number of arches
            int numArches = Math.Max(1, columns / 6);

            // Width of each arch
            int archWidth = columns / numArches;

            // Height of arches
            int archHeight = Math.Min(rows, archWidth / 2);

            for (int archIdx = 0; archIdx < numArches; archIdx++)
            {
                float archCenterX = archIdx * archWidth + archWidth / 2f;

                // Create the arch
                for (int row = 0; row < archHeight; row++)
                {
                    // Calculate width at this row (wider at top)
                    float rowWidth = archWidth * (1 - (float)row / archHeight);

                    // Calculate starting position
                    float startX = archCenterX - rowWidth / 2;
                    float endX = archCenterX + rowWidth / 2;

                    // Add bricks along the row
                    for (float col = startX; col < endX; col++)
                    {
                        positions.Add(new BrickPosition
                        {
                            Column = col,
                            Row = row,
                            PresetId = null
                        });
                    }
                }

                // Add some supporting columns
                for (int row = archHeight; row < rows; row++)
                {
                    // Left support
                    positions.Add(new BrickPosition
                    {
                        Column = archCenterX - archWidth / 2,
                        Row = row,
                        PresetId = null
                    });

                    // Right support
                    positions.Add(new BrickPosition
                    {
                        Column = archCenterX + archWidth / 2 - 1,
                        Row = row,
                        PresetId = null
                    });
                }
            }

            return positions;
        }

        private List<BrickPosition> GenerateDiamondFormation(int columns, int rows)
        {
            var positions = new List<BrickPosition>();

            // Define the center. For an even number of columns/rows, 
            // the center is between cells, so we use (columns - 1) / 2f, etc.
            float centerX = (columns - 1) / 2f;
            float centerY = (rows - 1) / 2f;

            // Decide how large the diamond should be horizontally and vertically.
            // If you want it to span the full grid width/height, use columns-1 and rows-1 as below.
            float horizontalRadius = (columns - 1) / 2f;
            float verticalRadius = (rows - 1) / 2f;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    // Offsets from center
                    float dx = col - centerX;
                    float dy = row - centerY;

                    // Diamond equation in “centered” form:
                    //   |dx|/horizontalRadius + |dy|/verticalRadius <= 1
                    if ((Math.Abs(dx) / horizontalRadius) + (Math.Abs(dy) / verticalRadius) <= 1f)
                    {
                        positions.Add(new BrickPosition
                        {
                            Column = col,
                            Row = row,
                            PresetId = null // assigned later
                        });
                    }
                }
            }

            return positions;
        }


        /// <summary>
        /// Generates a zigzag formation
        /// </summary>
        private List<BrickPosition> GenerateZigzagFormation(int columns, int rows)
        {
            var positions = new List<BrickPosition>();

            // Number of zigzags
            int zigzagCount = Math.Max(2, rows / 3);

            // Width of each zigzag leg
            int legWidth = columns / 2;

            for (int zigzag = 0; zigzag < zigzagCount; zigzag++)
            {
                int rowStart = zigzag * 3;

                // Going right
                for (int col = 0; col < legWidth; col++)
                {
                    positions.Add(new BrickPosition
                    {
                        Column = col,
                        Row = rowStart,
                        PresetId = null
                    });
                }

                // Going left
                for (int col = 0; col < legWidth; col++)
                {
                    positions.Add(new BrickPosition
                    {
                        Column = columns - 1 - col,
                        Row = rowStart + 1,
                        PresetId = null
                    });
                }
            }

            return positions;
        }

        /// <summary>
        /// Generates an organic formation using noise
        /// </summary>
        private List<BrickPosition> GenerateOrganicFormation(int columns, int rows)
        {
            var positions = new List<BrickPosition>();

            // Create a noise seed
            float seedX = (float)_random.NextDouble() * 100;
            float seedY = (float)_random.NextDouble() * 100;

            // Create a boolean grid to track brick positions
            bool[,] hasBlock = new bool[columns, rows];

            // Fill using noise
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    // Generate perlin-like noise value
                    float noiseValue = GenerateNoiseValue(col * NoiseScale + seedX, row * NoiseScale + seedY);

                    // If noise value exceeds threshold, place a brick
                    if (noiseValue > NoiseThreshold)
                    {
                        hasBlock[col, row] = true;
                    }
                }
            }

            // Apply smoothing if needed
            if (SmoothingPasses > 0)
            {
                for (int pass = 0; pass < SmoothingPasses; pass++)
                {
                    hasBlock = SmoothOrganicGrid(hasBlock, columns, rows);
                }
            }

            // Convert grid to positions
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    if (hasBlock[col, row])
                    {
                        positions.Add(new BrickPosition
                        {
                            Column = col,
                            Row = row,
                            PresetId = null
                        });
                    }
                }
            }

            return positions;
        }

        /// <summary>
        /// Generates a wave formation
        /// </summary>
        private List<BrickPosition> GenerateWaveFormation(int columns, int rows)
        {
            var positions = new List<BrickPosition>();

            // Wave parameters
            float amplitude = rows / 5f;
            float frequency = 2 * (float)Math.PI / columns;
            int waves = 2;  // Number of complete waves

            for (int col = 0; col < columns; col++)
            {
                // Calculate wave height at this column
                float waveY = amplitude * (float)Math.Sin(frequency * waves * col);

                // Center wave vertically
                float centerY = rows / 2f;
                float baseY = centerY + waveY;

                // Place bricks above and below the wave curve
                for (int offset = -2; offset <= 2; offset++)
                {
                    float y = baseY + offset;
                    if (y >= 0 && y < rows)
                    {
                        positions.Add(new BrickPosition
                        {
                            Column = col,
                            Row = y,
                            PresetId = null
                        });
                    }
                }
            }

            return positions;
        }

        /// <summary>
        /// Generates a simple noise value (simple hash-based noise)
        /// </summary>
        private float GenerateNoiseValue(float x, float y)
        {
            // A simple hash-based noise function
            float n = x + y * 57;
            n = (n * 21.5453f) % 1.0f;
            return n;
        }

        /// <summary>
        /// Smooths an organic grid using cellular automata rules
        /// </summary>
        private bool[,] SmoothOrganicGrid(bool[,] grid, int width, int height)
        {
            bool[,] newGrid = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Count alive neighbors
                    int neighbors = 0;
                    for (int nx = -1; nx <= 1; nx++)
                    {
                        for (int ny = -1; ny <= 1; ny++)
                        {
                            if (nx == 0 && ny == 0) continue;

                            int checkX = x + nx;
                            int checkY = y + ny;

                            if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                            {
                                if (grid[checkX, checkY])
                                    neighbors++;
                            }
                        }
                    }

                    // Apply cellular automata rules
                    if (grid[x, y])
                    {
                        // Cell is alive
                        newGrid[x, y] = neighbors >= 3;
                    }
                    else
                    {
                        // Cell is dead
                        newGrid[x, y] = neighbors >= 5;
                    }
                }
            }

            return newGrid;
        }

        /// <summary>
        /// Generates a maze-like formation with corridors and walls
        /// </summary>
        private List<BrickPosition> GenerateMazeFormation(int columns, int rows)
        {
            var positions = new List<BrickPosition>();

            // Create maze using simple algorithm
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    bool placeBrick = false;

                    // Create maze walls - every 3rd column and row creates corridors
                    if (row % 3 == 0 || col % 3 == 0)
                    {
                        placeBrick = true;
                    }

                    // Add some random openings in walls (30% chance)
                    if (placeBrick && _random.NextDouble() < 0.3)
                    {
                        placeBrick = false;
                    }

                    // Add some random blocks in corridors (20% chance)
                    if (!placeBrick && _random.NextDouble() < 0.2)
                    {
                        placeBrick = true;
                    }

                    if (placeBrick)
                    {
                        positions.Add(new BrickPosition
                        {
                            Column = col,
                            Row = row,
                            PresetId = null
                        });
                    }
                }
            }

            return positions;
        }

        #endregion
    }
}