namespace AnimeFeedManager.Shared.Charts.Static;

/// <summary>
/// Provides common chart colors as extension members on <see cref="ChartColor"/>.
/// Use these as reference values when creating custom colors.
/// </summary>
public static class ChartColorExtensionMembers
{
    extension(ChartColor)
    {
        // ──────────────────────────────────────────────────────────────────
        // Blues
        // ──────────────────────────────────────────────────────────────────

        public static ChartColor Blue => new(Red: 59, Green: 130, Blue: 246);
        public static ChartColor LightBlue => new(Red: 147, Green: 197, Blue: 253);
        public static ChartColor DarkBlue => new(Red: 30, Green: 64, Blue: 175);

        // ──────────────────────────────────────────────────────────────────
        // Greens
        // ──────────────────────────────────────────────────────────────────

        public static ChartColor Green => new(Red: 34, Green: 197, Blue: 94);
        public static ChartColor LightGreen => new(Red: 134, Green: 239, Blue: 172);
        public static ChartColor DarkGreen => new(Red: 22, Green: 163, Blue: 74);

        // ──────────────────────────────────────────────────────────────────
        // Reds
        // ──────────────────────────────────────────────────────────────────

        public static ChartColor Red => new(Red: 239, Green: 68, Blue: 68);
        public static ChartColor LightRed => new(Red: 252, Green: 165, Blue: 165);
        public static ChartColor DarkRed => new(Red: 185, Green: 28, Blue: 28);

        // ──────────────────────────────────────────────────────────────────
        // Yellows / Oranges
        // ──────────────────────────────────────────────────────────────────

        public static ChartColor Yellow => new(Red: 250, Green: 204, Blue: 21);
        public static ChartColor Orange => new(Red: 249, Green: 115, Blue: 22);
        public static ChartColor Amber => new(Red: 245, Green: 158, Blue: 11);

        // ──────────────────────────────────────────────────────────────────
        // Purples
        // ──────────────────────────────────────────────────────────────────

        public static ChartColor Purple => new(Red: 168, Green: 85, Blue: 247);
        public static ChartColor Violet => new(Red: 139, Green: 92, Blue: 246);

        // ──────────────────────────────────────────────────────────────────
        // Neutrals
        // ──────────────────────────────────────────────────────────────────

        public static ChartColor Gray => new(Red: 107, Green: 114, Blue: 128);
        public static ChartColor LightGray => new(Red: 209, Green: 213, Blue: 219);
        public static ChartColor DarkGray => new(Red: 55, Green: 65, Blue: 81);

        // ──────────────────────────────────────────────────────────────────
        // Semantic (for charts showing success/failure/warning states)
        // ──────────────────────────────────────────────────────────────────

        public static ChartColor Success => new(Red: 34, Green: 197, Blue: 94);
        public static ChartColor Error => new(Red: 239, Green: 68, Blue: 68);
        public static ChartColor Warning => new(Red: 245, Green: 158, Blue: 11);
        public static ChartColor Info => new(Red: 59, Green: 130, Blue: 246);
    }
}
