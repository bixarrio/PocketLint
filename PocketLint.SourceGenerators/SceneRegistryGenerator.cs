namespace PocketLint.SourceGenerators
{
    using Microsoft.CodeAnalysis;
    using Scriban;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    [Generator]
    public class SceneRegistryGenerator : IIncrementalGenerator
    {
        #region Public Methods
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var buildConfigProvider = context.AdditionalTextsProvider
                .Where(file => Path.GetFileName(file.Path).Equals("build.plconfig", StringComparison.OrdinalIgnoreCase))
                .Select((file, ct) => new { Path = file.Path, Content = file.GetText(ct)?.ToString() ?? string.Empty });

            var sceneFilesProvider = context.AdditionalTextsProvider
                .Where(file => Path.GetExtension(file.Path).Equals(".plscene", StringComparison.OrdinalIgnoreCase))
                .Select((file, ct) => new AnonymousSceneFile { Name = Path.GetFileNameWithoutExtension(file.Path), Path = file.Path, Content = file.GetText(ct)?.ToString() ?? string.Empty })
                .Collect();

            var combinedProvider = buildConfigProvider.Combine(sceneFilesProvider)
                .Select((tuple, ctx) => Tuple.Create(tuple.Left.Path, tuple.Left.Content, tuple.Right));

            context.RegisterSourceOutput(combinedProvider, (ctx, input) => GenerateSceneRegistry(ctx, input));
            context.RegisterSourceOutput(context.CompilationProvider, GenerateDebugScene);
        }
        #endregion

        #region Private Methods
        private static void GenerateDebugScene(SourceProductionContext context, Compilation comp)
        {
            context.AddSource("DebugSceneGenerator.cs", @"namespace PocketLint.Core.Generated
{
    public class DebugSceneGenerator
    {
        public static string Test = ""Generator is running"";
    }
}");
        }

        private static void GenerateSceneRegistry(SourceProductionContext context, Tuple<string, string, ImmutableArray<AnonymousSceneFile>> input)
        {
            StringBuilder debugLog = new StringBuilder();
            debugLog.AppendLine(@"namespace PocketLint.Core.Generated { public class DebugLog { public static string Log = @""");

            try
            {
                if (!LogConfigInfo(debugLog, input.Item1, input.Item2)) return;
                if (!TryParseConfig(input.Item2, debugLog, out BuildConfigData buildConfig)) return;

                IReadOnlyList<AnonymousSceneFile> sceneFiles = input.Item3;
                LogSceneFilesInfo(debugLog, sceneFiles);

                var sceneNames = buildConfig.Scenes.Select(Path.GetFileNameWithoutExtension).Select(n => n.ToLowerInvariant()).ToList();
                debugLog.AppendLine($"Scenes Found: {string.Join(" ", sceneNames.Select(i => i + ".plscene"))}");

                List<SceneInfo> sceneInfoList = new List<SceneInfo>();
                foreach (string sceneName in sceneNames)
                {
                    AnonymousSceneFile sceneFile = sceneFiles.FirstOrDefault(f => f.Name.ToLowerInvariant() == sceneName);
                    debugLog.AppendLine($"Looking for scene: {sceneName}.plscene, Found: {(sceneFile != null ? $"{sceneFile.Name}.plscene (Path: {sceneFile.Path.Replace("\"", "\"\"")})" : "null")}");

                    if (sceneFile == null || string.IsNullOrEmpty(sceneFile.Content))
                    {
                        debugLog.AppendLine($"Warning: Scene {sceneName}.plscene not found or empty");
                        continue;
                    }

                    debugLog.AppendLine($"Scene Content ({sceneName}.plscene): {sceneFile.Content.Replace("\"", "\"\"")}");
                    if (!TryParseScene(sceneFile.Content, sceneName, debugLog, out SceneData sceneData)) continue;

                    sceneInfoList.Add(new SceneInfo { Name = sceneName, Data = sceneData });
                }

                if (sceneInfoList.Count == 0)
                {
                    debugLog.AppendLine("Warning: No valid scenes found to generate");
                }
                else
                {
                    foreach (SceneInfo scene in sceneInfoList)
                    {
                        string className = char.ToUpper(scene.Name[0]) + scene.Name.Substring(1) + "Scene";
                        string code = GenerateSceneSetupClass(className, scene.Name, scene.Data);
                        debugLog.AppendLine($"Generating {className}.cs for scene {scene.Name}");
                        context.AddSource($"{className}.cs", code);
                    }

                    string registryCode = GenerateSceneRegistryClass(sceneInfoList);
                    debugLog.AppendLine("Generating SceneRegistryGenerated.cs");
                    context.AddSource("SceneRegistryGenerated.cs", registryCode);
                }
            }
            catch (Exception ex)
            {
                debugLog.AppendLine($"Unexpected error in GenerateSceneRegistry: {ex.Message.Replace("\"", "\"\"")}");
                debugLog.AppendLine($"Stack Trace: {ex.StackTrace?.Replace("\"", "\"\"") ?? "null"}");
            }
            finally
            {
                debugLog.AppendLine(@"""; } }");
                context.AddSource("DebugLog.cs", GenerateDebugLog(debugLog.ToString()));
            }
        }

        private static string GenerateDebugLog(string logContent)
        {
            return $@"namespace PocketLint.Core.Generated
{{
    public class DebugLog
{{
        public static string Log = @""{logContent.Replace(@"""", @"""""")}"";
    }}
}}";
        }

        private static bool LogConfigInfo(StringBuilder debugLog, string configPath, string configContent)
        {
            debugLog.AppendLine($"Config Path: {configPath.Replace("\"", "\"\"")}");
            debugLog.AppendLine($"Config Content Length: {configContent.Length}");
            debugLog.AppendLine($"Config Content: {(string.IsNullOrEmpty(configContent) ? "empty" : configContent.Replace("\"", "\"\""))}");
            if (string.IsNullOrEmpty(configContent))
            {
                debugLog.AppendLine("Error: Could not read Config/build.plconfig");
                return false;
            }
            return true;
        }

        private static bool TryParseConfig(string configContent, StringBuilder debugLog, out BuildConfigData buildConfig)
        {
            buildConfig = null;
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                buildConfig = JsonSerializer.Deserialize<BuildConfigData>(configContent, options);
                if (buildConfig == null || buildConfig.Scenes == null || buildConfig.Scenes.Length == 0)
                {
                    debugLog.AppendLine($"Error: build.plconfig is empty or invalid. Content: {configContent.Replace("\"", "\"\"")}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                debugLog.AppendLine($"Error parsing build.plconfig: {ex.Message.Replace("\"", "\"\"")}");
                debugLog.AppendLine($"Stack Trace: {ex.StackTrace?.Replace("\"", "\"\"") ?? "null"}");
                return false;
            }
        }

        private static void LogSceneFilesInfo(StringBuilder debugLog, IReadOnlyList<AnonymousSceneFile> sceneFiles)
        {
            debugLog.AppendLine($"Scene Files Count: {sceneFiles.Count}");
            debugLog.AppendLine($"Scene Files: {string.Join(" ", sceneFiles.Select(f => $"{f.Name}.plscene (Path: {f.Path.Replace("\"", "\"\"")})"))}");
            debugLog.AppendLine($"Scene File Contents: {string.Join(" ", sceneFiles.Select(f => $"\"{f.Name}: {(string.IsNullOrEmpty(f.Content) ? "empty" : f.Content.Length + " chars")}\""))}");
        }

        private static bool TryParseScene(string content, string sceneName, StringBuilder debugLog, out SceneData sceneData)
        {
            sceneData = null;
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                sceneData = JsonSerializer.Deserialize<SceneData>(content, options);
                if (sceneData == null || sceneData.Entity == null)
                {
                    debugLog.AppendLine($"Warning: Scene {sceneName}.plscene is empty or invalid. Content: {content.Replace("\"", "\"\"")}");
                    return false;
                }
                debugLog.AppendLine($"Deserialized SceneData for {sceneName}.plscene: Entity has {sceneData.Entity.Children?.Length ?? 0} children");
                return true;
            }
            catch (Exception ex)
            {
                debugLog.AppendLine($"Error parsing {sceneName}.plscene: {ex.Message.Replace("\"", "\"\"")}");
                debugLog.AppendLine($"Stack Trace: {ex.StackTrace?.Replace("\"", "\"\"") ?? "null"}");
                return false;
            }
        }

        private static string GenerateSceneSetupClass(string className, string sceneName, SceneData sceneData)
        {
            var entities = new List<object>();
            var debugLog = new StringBuilder();
            debugLog.AppendLine($"Processing {sceneName}: Root entity has {sceneData.Entity.Children?.Length ?? 0} children");

            uint entityIdCounter = 1;
            var idMap = new Dictionary<string, uint>();

            foreach (var child in sceneData.Entity.Children ?? Array.Empty<EntityData>())
            {
                debugLog.AppendLine($"Processing child: {(child.Entity?.Name ?? "null")}");
                ProcessEntity(child, null, ref entityIdCounter, idMap, entities, debugLog);
            }

            var template = Template.Parse(CodeTemplates.SceneSetupTemplate);
            var result = template.Render(new
            {
                className,
                entities,
                debugLog = debugLog.ToString()
            });

            System.Diagnostics.Debug.WriteLine(debugLog.ToString());
            return result;

            void ProcessEntity(EntityData entity, uint? parentId, ref uint idCounter, Dictionary<string, uint> entityIdMap, List<object> entityList, StringBuilder log)
            {
                if (entity?.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
                {
                    log.AppendLine($"Skipping invalid entity: Entity is {(entity == null ? "null" : "not null")}, Name is {(entity?.Entity == null ? "null" : entity.Entity.Name ?? "empty")}. Children will not be processed.");
                    return;
                }

                uint entityId = idCounter++;
                entityIdMap[entity.Entity.Name] = entityId;
                log.AppendLine($"Generating entity: {entity.Entity.Name}, ID: {entityId}, ParentID: {(parentId.HasValue ? parentId.ToString() : "none")}");

                float x = 0f;
                float y = 0f;
                var components = new List<object>();
                int componentIndex = 0;

                foreach (var component in entity.Entity.Components ?? Array.Empty<ComponentData>())
                {
                    if (component.Type == "EntityTransform" && component.Properties != null)
                    {
                        foreach (var prop in component.Properties)
                        {
                            if (prop.Key == "LocalX" && prop.Value.Value.TryGetSingle(out float xVal)) x = xVal;
                            if (prop.Key == "LocalY" && prop.Value.Value.TryGetSingle(out float yVal)) y = yVal;
                        }
                    }

                    var properties = new List<object>();
                    if (component.Properties != null)
                    {
                        foreach (var prop in component.Properties)
                        {
                            string value = FormatPropertyValue(prop.Value);
                            properties.Add(new { key = prop.Key, value });
                            log.AppendLine($"Setting property: {prop.Key} = {value} for {component.Type}");
                        }
                    }

                    components.Add(new { type = component.Type, properties, index = componentIndex++ });
                    log.AppendLine($"Adding component: {component.Type} to {entity.Entity.Name}");
                }

                entityList.Add(new
                {
                    id = entityId,
                    name = entity.Entity.Name,
                    tag = entity.Entity.Tag,
                    x = x.ToString("F2", CultureInfo.InvariantCulture),
                    y = y.ToString("F2", CultureInfo.InvariantCulture),
                    parentId = parentId.HasValue ? parentId.ToString() : null,
                    components
                });

                foreach (var child in entity.Entity.Children ?? Array.Empty<EntityData>())
                {
                    log.AppendLine($"Processing child of {entity.Entity.Name}: {(child.Entity?.Name ?? "null")}");
                    ProcessEntity(child, entityId, ref idCounter, entityIdMap, entityList, log);
                }
            }

            string FormatPropertyValue(PropertyData propData)
            {
                var value = propData.Value;
                var type = propData.Type?.ToLowerInvariant();

                if (value.ValueKind == JsonValueKind.Number)
                {
                    if (type == "uint" || type == "int")
                        return value.GetInt32().ToString(CultureInfo.InvariantCulture);
                    if (type == "float" || type == "double")
                        return $"{value.GetSingle().ToString("F2", CultureInfo.InvariantCulture)}f";
                    return value.GetInt32().ToString(CultureInfo.InvariantCulture);
                }
                if (value.ValueKind == JsonValueKind.True) return "true";
                if (value.ValueKind == JsonValueKind.False) return "false";
                if (value.ValueKind == JsonValueKind.String) return $"\"{value.GetString()?.Replace("\"", "\"\"")}\"";
                return "null";
            }
        }

        private static string GenerateSceneRegistryClass(List<SceneInfo> sceneDataList)
        {
            var scenes = sceneDataList.Select(s => new
            {
                sceneName = s.Name,
                className = char.ToUpper(s.Name[0]) + s.Name.Substring(1) + "Scene"
            }).ToList();

            var template = Template.Parse(CodeTemplates.SceneRegistryGeneratedTemplate);
            return template.Render(new { scenes });
        }
        #endregion

        #region Classes
        private class BuildConfigData
        {
            [JsonPropertyName("scenes")]
            public string[] Scenes { get; set; }
        }

        private class SceneData
        {
            [JsonPropertyName("entity")]
            public EntityData Entity { get; set; }
        }

        private class EntityData
        {
            [JsonPropertyName("entity")]
            public EntityWrapper Entity { get; set; }

            [JsonPropertyName("components")]
            public ComponentData[] Components { get; set; }

            [JsonPropertyName("children")]
            public EntityData[] Children { get; set; }
        }

        private class EntityWrapper
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("tag")]
            public string Tag { get; set; }

            [JsonPropertyName("components")]
            public ComponentData[] Components { get; set; }

            [JsonPropertyName("children")]
            public EntityData[] Children { get; set; }
        }

        private class ComponentData
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("properties")]
            public Dictionary<string, PropertyData> Properties { get; set; }
        }

        private class PropertyData
        {
            [JsonPropertyName("value")]
            public JsonElement Value { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }
        }

        private class AnonymousSceneFile
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public string Content { get; set; }
        }

        private class SceneInfo
        {
            public string Name { get; set; }
            public SceneData Data { get; set; }
        }
        #endregion
    }
}