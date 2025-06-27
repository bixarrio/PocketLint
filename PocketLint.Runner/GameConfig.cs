using PocketLint.Core.Logging;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PocketLint.Runner;

public partial class GameConfig
{
    #region Properties and Fields

    private readonly string _title;
    public string Title => _title;

    private readonly string _startScene;
    public string StartScene => _startScene;

    #endregion

    #region ctor

    public GameConfig(string title, string startScene)
    {
        _title = title;
        _startScene = startScene;
    }

    #endregion

    #region Public Methods

    public static GameConfig Load(string configName)
    {
        try
        {
            // Load embedded game.plgame
            using var stream = Assembly.GetEntryAssembly()?.GetManifestResourceStream($"PocketLintAOT.{configName}");
            if (stream == null)
            {
                Logger.Error("No embedded resources found");
                var resources = Assembly.GetEntryAssembly()?.GetManifestResourceNames() ?? Array.Empty<string>();
                Logger.Log($"Resources available: {string.Join(", ", resources)}");
                return GameConfig.Default;
            }

            using var reader = new StreamReader(stream);
            string json = reader.ReadToEnd();
            var options = new JsonSerializerOptions { TypeInfoResolver = GameConfigDataContext.Default };
            var data = JsonSerializer.Deserialize(json, GameConfigDataContext.Default.GameConfigData);
            if (data == null) return GameConfig.Default;
            return new GameConfig(data.Title, data.StartScene);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load embedded config: {ex.Message}");
            return GameConfig.Default;
        }
    }

    public static GameConfig Default => new GameConfig("PocketLint Runner", "");

    #endregion

    #region Classes and Structs

    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(GameConfigData))]
    internal partial class GameConfigDataContext : JsonSerializerContext
    {
    }

    internal partial class GameConfigData
    {
        [JsonPropertyName("title")] public string? Title { get; init; }
        [JsonPropertyName("startScene")] public string? StartScene { get; init; }
    }

    #endregion
}