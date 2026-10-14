using FluentJalium.Localization;

namespace FluentJalium.Launcher;

/// <summary>
/// Every string the launcher shows, in both languages.
/// </summary>
/// <remarks>
/// The window's own markup strings are registered here rather than in a name table, because
/// <see cref="TextSweeper"/> matches the authored English: registering the same literal the <c>.jalxaml</c>
/// declares is the whole contract, and a string that leaves the markup without leaving here shows up in
/// <see cref="UiText.MissingChinese"/>.
/// </remarks>
internal static class LauncherText
{
    /// <summary>Registers every launcher string. Idempotent, so a host may call it defensively.</summary>
    public static void Register()
    {
        // Header.
        Add("Fluent Design System in Jalium.UI", "Fluent Design System in Jalium.UI");
        Add("Pick a renderer and a language, then launch.", "选择渲染器与语言，然后启动。");

        // Window text.
        Add("Rendering quality", "渲染质量");
        Add("Theme", "主题");
        Add("Reduce motion", "减少动效");
        Add("Renderer", "渲染器");
        Add("Path engine", "实现器");
        Add("Language", "语言");
        Add("Start", "开始启动");

        // Dropdown labels. "Auto" is shared by the renderer and the engine, and the catalog keys on the English,
        // so one registration serves every list that offers it.
        Add("Auto", "自动");
        Add("Vulkan", "Vulkan");
        Add("DirectX (D3D12)", "DirectX (D3D12)");
        Add("Vello", "Vello");
        Add("Impeller", "Impeller");
        Add("Simplified Chinese", "简体中文");
        Add("English", "English");
        Add("Analytic coverage", "解析抗锯齿");
        Add("MSAA 4x", "MSAA 4×");
        Add("MSAA 8x", "MSAA 8×");
        Add("MSAA 16x", "MSAA 16×");
        Add("Follow system", "跟随系统");
        Add("Light", "浅色");
        Add("Dark", "深色");

        // Status line templates.
        Add("Running {0} with {1}.", "当前运行：渲染器 {0}，实现器 {1}。");
        Add("The main window opens with the same settings.", "主界面将以相同设置打开。");
        Add("The main window opens on {0} with {1}.", "主界面将以渲染器 {0}、实现器 {1} 打开。");

        // Handoff.
        Add("Switching renderer...", "正在切换渲染后端……");
        Add("{0} is not available on this machine; staying on {1}.", "本机不支持 {0}，继续使用 {1}。");
        Add("Could not save preferences to {0}; they will not survive a restart.", "无法把偏好保存到 {0}，重启后将丢失。");
    }

    private static void Add(string english, string simplifiedChinese) =>
        UiText.Add(english, simplifiedChinese);
}
