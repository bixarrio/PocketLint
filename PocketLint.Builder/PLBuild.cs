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
        var paths = ResolvePaths(gamePath, outputExe, tempDir);
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
        if (!CopyFiles(paths))
            return;
        if (!CopySceneFiles(scenePaths, paths.ProjectDir, tempDir))
            return;

        List<string> sceneFiles = scenePaths.Select(Path.GetFileName).ToList();
        GenerateProjectFiles(tempDir, paths, sceneFiles);
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

    private static (string ProjectDir, string OutputDir, string GameDllDir, string GameDll, string PlGame, string CoreDll, string ToolsDll, string RunnerDll, string TempPlGame, string TempGameDll, string TempCoreDll, string TempToolsDll, string TempRunnerDll, string TempTrimmerRoots, string TempILLinkSuppressions, string ConfigFile) ResolvePaths(string gamePath, string outputExe, string tempDir)
    {
        string projectDir = Path.GetDirectoryName(Path.GetFullPath(gamePath)) ?? throw new Exception("Invalid game path");
        string outputDir = Path.Combine(projectDir, "build");
        string gameDllDir = Path.Combine(projectDir, "bin", "Release", "net8.0");
        string gameAssemblyName = Path.GetFileNameWithoutExtension(gamePath);
        string gameDll = Path.Combine(gameDllDir, $"{gameAssemblyName}.Core.dll");
        string plgamePath = Path.Combine(projectDir, "Manifest", "game.plgame");
        string coreDll = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), CORE_DLL_PATH));
        string toolsDll = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), TOOLS_DLL_PATH));
        string runnerDll = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), RUNNER_DLL_PATH));
        string tempPlgame = Path.Combine(tempDir, "game.plgame");
        string tempGameDll = Path.Combine(tempDir, $"{gameAssemblyName}.Core.dll");
        string tempCoreDll = Path.Combine(tempDir, "PocketLint.Core.dll");
        string tempToolsDll = Path.Combine(tempDir, "PocketLint.Text.dll");
        string tempRunnerDll = Path.Combine(tempDir, "PocketLint.Runner.dll");
        string tempTrimmerRoots = Path.Combine(tempDir, "TrimmerRoots.xml");
        string tempILLinkSuppressions = Path.Combine(tempDir, "ILLinkSuppressions.xml");
        string configFile = Path.Combine(projectDir, "Config", "build.plconfig");
        return (projectDir, outputDir, gameDllDir, gameDll, plgamePath, coreDll, toolsDll, runnerDll, tempPlgame, tempGameDll, tempCoreDll, tempToolsDll, tempRunnerDll, tempTrimmerRoots, tempILLinkSuppressions, configFile);
    }

    private static bool ValidateInputFiles(string gamePath, (string ProjectDir, string OutputDir, string GameDllDir, string GameDll, string PlGame, string CoreDll, string ToolsDll, string RunnerDll, string TempPlGame, string TempGameDll, string TempCoreDll, string TempToolsDll, string TempRunnerDll, string TempTrimmerRoots, string TempILLinkSuppressions, string ConfigFile) paths)
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

    private static bool CopyFiles((string ProjectDir, string OutputDir, string GameDllDir, string GameDll, string PlGame, string CoreDll, string ToolsDll, string RunnerDll, string TempPlGame, string TempGameDll, string TempCoreDll, string TempToolsDll, string TempRunnerDll, string TempTrimmerRoots, string TempILLinkSuppressions, string ConfigFile) paths)
    {
        Directory.CreateDirectory(paths.OutputDir);
        try
        {
            File.Copy(paths.PlGame, paths.TempPlGame, true);
            File.Copy(paths.GameDll, paths.TempGameDll, true);
            File.Copy(paths.CoreDll, paths.TempCoreDll, true);
            File.Copy(paths.ToolsDll, paths.TempToolsDll, true);
            File.Copy(paths.RunnerDll, paths.TempRunnerDll, true);
            Logger.Log($"Copied {paths.PlGame}, size: {new FileInfo(paths.TempPlGame).Length} bytes");
            Logger.Log($"Contents of {paths.PlGame}: {File.ReadAllText(paths.TempPlGame)}");
            Logger.Log($"Copied DLLs: {paths.GameDll}, {paths.CoreDll}, {paths.ToolsDll}, {paths.RunnerDll}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"File copy failed: {ex.Message}");
            return false;
        }
    }

    private static void GenerateProjectFiles(string tempDir, (string ProjectDir, string OutputDir, string GameDllDir, string GameDll, string PlGame, string CoreDll, string ToolsDll, string RunnerDll, string TempPlGame, string TempGameDll, string TempCoreDll, string TempToolsDll, string TempRunnerDll, string TempTrimmerRoots, string TempILLinkSuppressions, string ConfigFile) paths, List<string> sceneFiles)
    {
        string aotCsproj = Path.Combine(tempDir, "PocketLintAOT.csproj");
        string programCs = Path.Combine(tempDir, "Program.cs");
        string gameAssemblyName = Path.GetFileNameWithoutExtension(paths.GameDll);
        File.WriteAllText(aotCsproj, GenerateAotCsproj(paths.OutputDir, Path.GetFileNameWithoutExtension(paths.GameDll), sceneFiles));
        File.WriteAllText(programCs, GenerateProgramCs(gameAssemblyName));
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

    private static string GenerateAotCsproj(string outputPath, string gameProjectPath, List<string> sceneFiles)
    {
        var gameAssemblyName = Path.GetFileNameWithoutExtension(gameProjectPath);
        string sceneResources = string.Join("\n    ", sceneFiles.Select(s => $"<EmbeddedResource Include=\"{s}\" />"));
        return $"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <OutputType>Exe</OutputType>
            <PublishAot>true</PublishAot>
            <SelfContained>true</SelfContained>
            <TrimMode>none</TrimMode>
            <OutputPath>{outputPath}</OutputPath>
            <AssemblyName>{gameAssemblyName}</AssemblyName>
            <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
            <Nullable>enable</Nullable>
          </PropertyGroup>
          <ItemGroup>
            <Reference Include="PocketLint.Core">
              <HintPath>PocketLint.Core.dll</HintPath>
            </Reference>
            <Reference Include="PocketLint.Text">
              <HintPath>PocketLint.Text.dll</HintPath>
            </Reference>
            <Reference Include="PocketLint.Runner">
              <HintPath>PocketLint.Runner.dll</HintPath>
            </Reference>
            <Reference Include="{gameAssemblyName}.Core">
              <HintPath>{gameAssemblyName}.Core.dll</HintPath>
            </Reference>
          </ItemGroup>
          <ItemGroup>
            <PackageReference Include="OpenTK" Version="4.9.4" />
            <PackageReference Include="System.Reflection.Metadata" Version="8.0.0" />
          </ItemGroup>
          <ItemGroup>
            <Compile Include="Program.cs" />
          </ItemGroup>
          <ItemGroup>
            <EmbeddedResource Include="game.plgame" />
            {sceneResources}
          </ItemGroup>
          <Target Name="ValidateResource" BeforeTargets="Build">
            <Message Importance="high" Text="[DEBUG] EmbeddedResource items: @(EmbeddedResource)" />
            <Error Condition="!Exists('%(EmbeddedResource.Identity)')" 
              Text="Missing embedded resource file: %(EmbeddedResource.Identity)" />
          </Target>
        </Project>
        """;
    }

    private static string GenerateProgramCs(string gameAssemblyName)
    {
        return $@"#nullable enable
using System;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PocketLint.Core.Components;
using PocketLint.Core.Generated;
using PocketLint.Core.Logging;
using PocketLint.Runner;

namespace {gameAssemblyName}.AOT;

public static class Program
{{
    #region Public Methods

    public static void Main()
    {{
        Logger.Register(new FileLogger(""runner.log""));
        GLFW.Init();
        try
        {{
            // Debugging:
            ListEmbeddedResources();
            var config = GameConfig.Load();
            ComponentRegistry.Initialize();
            Logger.Log(""Registering components..."");
            ComponentRegistryGenerated.RegisterAll();
            Logger.Log(""Components registered"");
            Logger.Log(""Registering scenes..."");
            SceneRegistryGenerated.RegisterAll();
            Logger.Log(""Scenes registered"");
            var runner = new GameRunner(config);
            runner.Run();
        }}
        catch (Exception ex)
        {{
            Logger.Error($""Startup failed: {{ex.Message}}"");
        }}
        finally
        {{
            GLFW.Terminate();
        }}
    }}

    #endregion

    #region Private Methods

    private static void ListEmbeddedResources()
    {{
        var resources = System.Reflection.Assembly.GetEntryAssembly()?.GetManifestResourceNames() ?? Array.Empty<string>();
        Logger.Log($""Resources available: {{string.Join("", "", resources)}}"");
    }}

    #endregion
}}";
    }

    #endregion

    #region Classes and Structs

    private class BuildConfig
    {
        [JsonPropertyName("scenes")] public List<string> Scenes { get; set; } = new();
    }

    #endregion
}