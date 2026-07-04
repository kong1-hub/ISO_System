namespace ISO11820.Helpers;

/// <summary>统一主题色管理 — 亮色主题</summary>
public static class ThemeColors
{
    // === 背景 ===
    public static Color BgDark        { get; } = SystemColors.Control;
    public static Color BgMedium      { get; } = Color.White;
    public static Color BgLight       { get; } = Color.FromArgb(235, 235, 235);
    public static Color BgLog         { get; } = Color.White;
    public static Color BgInput       { get; } = Color.White;
    public static Color BgLed         { get; } = Color.White;
    public static Color BgLedValue    { get; } = Color.FromArgb(248, 248, 248);

    // === 文字 ===
    public static Color TextPrimary   { get; } = Color.FromArgb(30, 30, 30);
    public static Color TextSecondary { get; } = Color.FromArgb(80, 80, 80);
    public static Color TextMuted     { get; } = Color.FromArgb(140, 140, 140);
    public static Color TextGray      { get; } = Color.FromArgb(180, 180, 180);

    // === 温度通道 ===
    public static Color TempFurnace1  { get; } = Color.FromArgb(220, 40, 40);
    public static Color TempFurnace2  { get; } = Color.FromArgb(220, 130, 30);
    public static Color TempSurface   { get; } = Color.FromArgb(20, 150, 60);
    public static Color TempCenter    { get; } = Color.FromArgb(20, 120, 200);
    public static Color TempCal       { get; } = Color.FromArgb(180, 150, 40);

    // === 功能色 ===
    public static Color StatusCyan    { get; } = Color.FromArgb(0, 130, 150);
    public static Color CalGold       { get; } = Color.FromArgb(180, 140, 20);

    // === 按钮 / 强调色 ===
    public static Color AccentBlue     { get; } = Color.FromArgb(60, 120, 200);
    public static Color AccentGreen    { get; } = Color.FromArgb(40, 160, 80);
    public static Color AccentOrange   { get; } = Color.FromArgb(200, 80, 60);
    public static Color AccentYellow   { get; } = Color.FromArgb(200, 140, 40);
    public static Color AccentPurple   { get; } = Color.FromArgb(140, 100, 180);
    public static Color AccentRed      { get; } = Color.FromArgb(200, 60, 60);
    public static Color AccentDarkGray { get; } = Color.FromArgb(100, 100, 110);
    public static Color LoginBlue      { get; } = Color.FromArgb(0, 122, 204);
    public static Color LoginGray      { get; } = Color.FromArgb(180, 180, 180);

    // === 表格 ===
    public static Color GridBg      { get; } = Color.White;
    public static Color GridAltBg   { get; } = Color.FromArgb(248, 248, 248);
    public static Color GridHeader  { get; } = Color.FromArgb(235, 235, 235);
    public static Color GridBorder  { get; } = Color.FromArgb(220, 220, 220);
    public static Color GridText    { get; } = Color.FromArgb(30, 30, 30);
    public static Color GridSelBg   { get; } = Color.FromArgb(0, 120, 215);
}
