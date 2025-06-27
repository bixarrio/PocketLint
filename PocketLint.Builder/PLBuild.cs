using Mono.Cecil;
using PocketLint.Core.Logging;
using PocketLint.Runner;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PocketLint.Builder;

public static class PLBuild
{
    #region Properties and Fields

    private const string CORE_DLL_PATH = @"..\PocketLint.Core\bin\Debug\net8.0\PocketLint.Core.dll";
    private const string TOOLS_DLL_PATH = @"..\PocketLint.Tools\bin\Debug\net8.0\PocketLint.Tools.dll";
    private const string RUNNER_DLL_PATH = @"..\PocketLint.Runner\bin\Debug\net8.0\PocketLint.Runner.dll";

    #endregion

    #region Public Methods

    public static void Build(string[] args)
    {
        Logger.Register(new ConsoleLogger());
        Logger.Register(new FileLogger("plbuild.log"));

        string? gamePath = null;
        string? outputExe = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--game" && i + 1 < args.Length)
                gamePath = args[i + 1];
            else if (args[i] == "--output" && i + 1 < args.Length)
                outputExe = args[i + 1];
        }

        if (string.IsNullOrEmpty(gamePath) || string.IsNullOrEmpty(outputExe))
        {
            Logger.Error("Usage: plbuild build --game <path/to/MyDemoGame.csproj> --output <MyDemoGame.exe>");
            return;
        }

        try
        {
            CompileRunner(gamePath, outputExe);
        }
        catch (Exception ex)
        {
            Logger.Error($"Build failed: {ex.Message}");
        }
    }

    #endregion

    #region Private Methods

    private static void CompileRunner(string gamePath, string outputExe)
    {
        Logger.Log("Compiling runner...");

        string tempDir = SetupTempDirectory();
        var paths = new Paths(gamePath, outputExe, tempDir);
        if (!ValidateInputFiles(gamePath, paths))
            return;

        BuildGameProject(gamePath, paths.GameDllDir);
        if (!File.Exists(paths.GameDll))
        {
            Logger.Error($"Game DLL not found: {paths.GameDll}");
            return;
        }

        LogAssemblyTypes(paths.GameDll);

        List<string> scenePaths = ReadConfigFile(paths.ConfigFile, paths.ProjectDir);
        var gameResources = EnumerateResources(paths.ResourcesDir);

        if (!CopyFiles(paths, gameResources))
            return;
        if (!CopySceneFiles(scenePaths, paths.ProjectDir, tempDir))
            return;

        GenerateProjectFiles(tempDir, paths, gameResources);
        if (!RunRestore(tempDir))
            return;

        if (!RunPublish(tempDir, paths.OutputDir))
            return;

        Cleanup(tempDir, paths.GameDllDir);
    }

    private static string SetupTempDirectory()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "PocketLintAOT");
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }

    private static bool ValidateInputFiles(string gamePath, Paths paths)
    {
        if (!File.Exists(gamePath))
        {
            Logger.Error($"Game project not found: {gamePath}");
            return false;
        }
        if (!File.Exists(paths.PlGame))
        {
            Logger.Error($"Mandatory game.plgame not found: {paths.PlGame}");
            return false;
        }
        if (!File.Exists(paths.CoreDll))
        {
            Logger.Error($"Core DLL not found: {paths.CoreDll}");
            return false;
        }
        if (!File.Exists(paths.ToolsDll))
        {
            Logger.Error($"Tools DLL not found: {paths.ToolsDll}");
            return false;
        }
        if (!File.Exists(paths.RunnerDll))
        {
            Logger.Error($"Runner DLL not found: {paths.RunnerDll}");
            return false;
        }
        if (!File.Exists(paths.ConfigFile))
        {
            Logger.Error($"Config file not found: {paths.ConfigFile}");
            return false;
        }
        return true;
    }

    private static List<string> ReadConfigFile(string configFile, string projectDir)
    {
        try
        {
            string json = File.ReadAllText(configFile);
            var config = JsonSerializer.Deserialize<BuildConfig>(json) ?? throw new Exception("Failed to deserialize build.plconfig");
            Logger.Log($"Read {config.Scenes.Count} scenes from {configFile}: {string.Join(", ", config.Scenes)}");
            return config.Scenes;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to read config file: {ex.Message}");
            throw;
        }
    }

    private static List<string> EnumerateResources(string resourcesDir)
    {
        var resources = new List<string>();
        foreach (var resource in Directory.EnumerateFiles(resourcesDir))
            resources.Add(Path.GetFileName(resource));
        return resources;
    }

    private static bool CopySceneFiles(List<string> scenePaths, string projectDir, string tempDir)
    {
        try
        {
            foreach (string scenePath in scenePaths)
            {
                string fullScenePath = Path.Combine(projectDir, scenePath);
                string destPath = Path.Combine(tempDir, Path.GetFileName(scenePath));
                if (!File.Exists(fullScenePath))
                {
                    Logger.Error($"Scene file not found: {fullScenePath}");
                    return false;
                }
                File.Copy(fullScenePath, destPath, true);
                Logger.Log($"Copied {fullScenePath} to {destPath}, size: {new FileInfo(destPath).Length} bytes");
            }
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Scene file copy failed: {ex.Message}");
            return false;
        }
    }

    private static void BuildGameProject(string gamePath, string gameDllDir)
    {
        Logger.Log($"Building game project: {gamePath}");
        var gameAssemblyName = Path.GetFileNameWithoutExtension(gamePath);
        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{gamePath}\" -c Release -o \"{gameDllDir}\" -p:AssemblyName={gameAssemblyName}.Core",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
        buildProcess.Start();
        string buildOutput = buildProcess.StandardOutput.ReadToEnd();
        string buildError = buildProcess.StandardError.ReadToEnd();
        buildProcess.WaitForExit();
        Logger.Log(buildOutput);
        if (buildProcess.ExitCode != 0)
        {
            Logger.Error($"Game build failed: {buildError}");
        }
    }

    private static void LogAssemblyTypes(string dllPath)
    {
        Logger.Log($"Inspecting types in {dllPath}");
        try
        {
            using var module = ModuleDefinition.ReadModule(dllPath);
            Logger.Log($"Found {module.Types.Count} types:");
            foreach (var type in module.Types)
            {
                Logger.Log($"  {type.FullName}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to inspect {dllPath}: {ex.Message}");
        }
    }

    private static bool CopyFiles(Paths paths, List<string> gameResources)
    {
        Directory.CreateDirectory(paths.OutputDir);
        try
        {
            File.Copy(paths.PlGame, paths.TempPlGame, true);
            File.Copy(paths.GameDll, paths.TempGameDll, true);
            File.Copy(paths.CoreDll, paths.TempCoreDll, true);
            File.Copy(paths.ToolsDll, paths.TempToolsDll, true);
            File.Copy(paths.RunnerDll, paths.TempRunnerDll, true);

            foreach (var file in gameResources)
                File.Copy(Path.Combine(paths.ResourcesDir, file), Path.Combine(paths.TempDir, file), true);

            Logger.Log($"Copied {paths.PlGame}, size: {new FileInfo(paths.TempPlGame).Length} bytes");
            Logger.Log($"Contents of {paths.PlGame}: {File.ReadAllText(paths.TempPlGame)}");
            Logger.Log($"Copied DLLs: {paths.GameDll}, {paths.CoreDll}, {paths.ToolsDll}, {paths.RunnerDll}");
            Logger.Log($"Copied Resources: {string.Join(", ", gameResources.Select(s => Path.Combine(paths.ResourcesDir, s)))}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"File copy failed: {ex.Message}");
            return false;
        }
    }

    private static void GenerateProjectFiles(string tempDir, Paths paths, List<string> gameResources)
    {
        string aotCsproj = Path.Combine(tempDir, $"PocketLintAOT.csproj");
        string programCs = Path.Combine(tempDir, "Program.cs");
        string gameAssemblyName = Path.GetFileNameWithoutExtension(paths.GameDll);
        File.WriteAllText(aotCsproj, GenerateAotCsproj(paths.OutputDir, Path.GetFileNameWithoutExtension(paths.GameDll), gameResources));
        File.WriteAllText(programCs, GenerateProgramCs(paths));
        Logger.Log($"Generated {aotCsproj}, {programCs}");
    }

    private static bool RunRestore(string tempDir)
    {
        string aotCsproj = Path.Combine(tempDir, "PocketLintAOT.csproj");
        var restoreProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"restore \"{aotCsproj}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
        restoreProcess.Start();
        string restoreOutput = restoreProcess.StandardOutput.ReadToEnd();
        string restoreError = restoreProcess.StandardError.ReadToEnd();
        restoreProcess.WaitForExit();
        Logger.Log(restoreOutput);
        if (restoreProcess.ExitCode != 0)
        {
            Logger.Error($"NuGet restore failed: {restoreError}");
            return false;
        }
        Logger.Log("NuGet restore completed");
        return true;
    }

    private static bool RunPublish(string tempDir, string outputDir)
    {
        string aotCsproj = Path.Combine(tempDir, "PocketLintAOT.csproj");
        var publishProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"publish \"{aotCsproj}\" -c Release -o \"{outputDir}\" -v normal",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
        publishProcess.Start();
        string publishOutput = publishProcess.StandardOutput.ReadToEnd();
        string publishError = publishProcess.StandardError.ReadToEnd();
        publishProcess.WaitForExit();
        Logger.Log(publishOutput);
        if (publishProcess.ExitCode != 0)
        {
            Logger.Error($"AOT compilation failed: {publishError}");
            return false;
        }
        Logger.Log($"Executable generated: {outputDir}");
        return true;
    }

    private static void Cleanup(string tempDir, string gameDllDir)
    {
        if (Directory.Exists(gameDllDir))
        {
            Directory.Delete(gameDllDir, true);
        }
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static string GenerateAotCsproj(string outputPath, string gameProjectPath, List<string> gameResources)
    {
        var gameAssemblyName = Path.GetFileNameWithoutExtension(gameProjectPath);

        var template = Scriban.Template.Parse(CodeTemplates.TemporaryProjectTemplate);
        return template.Render(new { gameAssemblyName = gameAssemblyName, outputPath = outputPath, gameResources });
    }

    private static string GenerateProgramCs(Paths paths)
    {
        var template = Scriban.Template.Parse(CodeTemplates.ProgramTemplate);
        return template.Render(new { paths = paths, configName = "game.plgame" });
    }

    #endregion

    #region Classes and Structs

    private sealed class BuildConfig
    {
        [JsonPropertyName("scenes")] public List<string> Scenes { get; set; } = new();
    }

    private sealed class Paths
    {
        #region Properties and Fields

        public string ProjectDir { get; private set; }
        public string ResourcesDir { get; private set; }
        public string OutputDir { get; private set; }
        public string GameDllDir { get; private set; }
        public string GameDll { get; private set; }
        public string PlGame { get; private set; }
        public string CoreDll { get; private set; }
        public string ToolsDll { get; private set; }
        public string RunnerDll { get; private set; }
        public string TempDir { get; private set; }
        public string TempPlGame { get; private set; }
        public string TempGameDll { get; private set; }
        public string TempCoreDll { get; private set; }
        public string TempToolsDll { get; private set; }
        public string TempRunnerDll { get; private set; }
        public string ConfigFile { get; private set; }
        public string OutputExe { get; private set; }
        public string GameAssemblyName { get; private set; }

        #endregion

        #region ctor

        public Paths(string gamePath, string outputExe, string tempDir)
        {
            var gameAssemblyName = Path.GetFileNameWithoutExtension(gamePath);
            TempDir = tempDir;
            ProjectDir = Path.GetDirectoryName(Path.GetFullPath(gamePath)) ?? throw new Exception("Invalid game path");
            ResourcesDir = Path.Combine(ProjectDir, "Resources");
            OutputDir = Path.Combine(ProjectDir, "build");
            GameDllDir = Path.Combine(ProjectDir, "bin", "Release", "net8.0");
            GameDll = Path.Combine(GameDllDir, $"{gameAssemblyName}.Core.dll");
            PlGame = Path.Combine(ProjectDir, "Manifest", "game.plgame");
            CoreDll = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), CORE_DLL_PATH));
            ToolsDll = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), TOOLS_DLL_PATH));
            RunnerDll = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RUNNER_DLL_PATH));
            TempPlGame = Path.Combine(tempDir, "game.plgame");
            TempGameDll = Path.Combine(tempDir, $"{gameAssemblyName}.Core.dll");
            TempCoreDll = Path.Combine(tempDir, "PocketLint.Core.dll");
            TempToolsDll = Path.Combine(tempDir, "PocketLint.Tools.dll");
            TempRunnerDll = Path.Combine(tempDir, "PocketLint.Runner.dll");
            ConfigFile = Path.Combine(ProjectDir, "Config", "build.plconfig");
            OutputExe = Path.GetFileNameWithoutExtension(outputExe);
            GameAssemblyName = Path.GetFileNameWithoutExtension(GameDll);
        }

        #endregion
    }

    #endregion
}