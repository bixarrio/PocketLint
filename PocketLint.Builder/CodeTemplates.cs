namespace PocketLint.Builder;
internal static class CodeTemplates
{
    public static string TemporaryProjectTemplate => @"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <OutputType>Exe</OutputType>
        <PublishAot>true</PublishAot>
        <SelfContained>true</SelfContained>
        <TrimMode>none</TrimMode>
        <OutputPath>{{ output_path }}</OutputPath>
        <AssemblyName>{{ game_assembly_name }}</AssemblyName>
        <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
        <Nullable>enable</Nullable>
    </PropertyGroup>
    <ItemGroup>
        <Reference Include=""PocketLint.Core"">
            <HintPath>PocketLint.Core.dll</HintPath>
        </Reference>
        <Reference Include=""PocketLint.Tools"">
            <HintPath>PocketLint.Tools.dll</HintPath>
        </Reference>
        <Reference Include=""PocketLint.Runner"">
            <HintPath>PocketLint.Runner.dll</HintPath>
        </Reference>
        <Reference Include=""{{ game_assembly_name }}.Core"">
            <HintPath>{{ game_assembly_name }}.Core.dll</HintPath>
        </Reference>
    </ItemGroup>
    <ItemGroup>
        <PackageReference Include=""OpenTK"" Version=""4.9.4"" />
        <PackageReference Include=""System.Reflection.Metadata"" Version=""8.0.0"" />
        <PackageReference Include=""SixLabors.ImageSharp"" Version=""3.1.10"" />
    </ItemGroup>
    <ItemGroup>
        <Compile Include=""Program.cs"" />
    </ItemGroup>
    <ItemGroup>
        <EmbeddedResource Include=""game.plgame"" />
{{~ for resource in game_resources ~}}
        <EmbeddedResource Include=""{{ resource }}"" />
{{~ end ~}}
    </ItemGroup>
</Project>
";

    public static string ProgramTemplate => @"#nullable enable
using System;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PocketLint.Core.Components;
using PocketLint.Core.Logging;
using PocketLint.Runner;
using {{ paths.game_assembly_name }}.Generated;
namespace {{ paths.game_assembly_name }}.Runner;
public static class Program
{
    #region Public Methods

    public static void Main()
    {
        Logger.Register(new FileLogger(""{{ paths.output_exe }}.log""));
        GLFW.Init();
        try
        {
            var config = GameConfig.Load(""{{ config_name }}"");
            Logger.Log(""Registering scenes..."");
            SceneRegistryGenerated.RegisterAllScenes();
            Logger.Log(""Scenes registered"");

            var runner = new GameRunner(config);
            runner.Run();
        }
        catch (Exception ex)
        {
            Logger.Error($""Startup failed: {ex.Message}"");
        }
        finally
        {
            GLFW.Terminate();
        }
    }

    #endregion
}
";
}
