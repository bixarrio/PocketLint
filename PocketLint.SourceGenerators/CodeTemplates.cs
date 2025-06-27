namespace PocketLint.SourceGenerators
{
    internal static class CodeTemplates
    {
        public static string SceneRegistryGeneratedTemplate => @"using PocketLint.Core.Entities;
namespace {{ assembly_name }}.Generated;
public static class SceneRegistryGenerated
{
    #region Public Methods

    public static void RegisterAllScenes()
    {
{{- for scene in scenes }}
        SceneRegistry.Register(""{{ scene.scene_name }}"", {{ scene.class_name }}.Setup);
{{- end #for scenes }}
    }

    #endregion
}";

        public static string SceneSetupTemplate => @"using PocketLint.Core.Components;
using PocketLint.Core.Entities;
using PocketLint.Core.Logging;
using PocketLint.Tools;
using System.Reflection;
namespace {{ assembly_name }}.Generated;
public static class {{ class_name }}
{
    #region Public Methods

    public static void Setup()
    {
        var entityManager = Scene.Current.EntityManager;

        var entryAssembly = Assembly.GetEntryAssembly();
        using (var stream = entryAssembly?.GetManifestResourceStream(""PocketLintAOT.{{ sprite_sheet.name }}""))
        {
            if (stream == null)
            {
                Logger.Error(""SpriteSheet resource not found"");
                var resources = entryAssembly?.GetManifestResourceNames() ?? Array.Empty<string>();
                Logger.Log($""Resources available: {string.Join("", "", resources)}"");
            }
            else
            {
                SpriteSheetImporter.Import(stream, Scene.SpriteSheet);
            }
        }

        {{~ for entity in entities ~}}
        var entity{{ entity.id }} = Scene.CreateEntity(""{{ entity.name }}"", x: {{ entity.x }}f, y: {{ entity.y }}f{{ if entity.parent_id }}, parentId: entity{{ entity.parent_id }}{{ end #if parent_id }}{{ if entity.tag }}, tag: ""{{ entity.tag }}""{{ end #if tag }});

        {{~ for component in entity.components ~}}
        {{~ if component.type == ""EntityTransform""
                continue
            end ~}}
        var comp{{ entity.id }}{{ component.type | string.replace ""."" """" }}{{ component.index }} = Scene.AddComponent<{{ component.type }}>(entity{{ entity.id }});
        {{~ for prop in component.properties ~}}
        comp{{ entity.id }}{{ component.type | string.replace ""."" """" }}{{ component.index }}.{{ prop.key }} = {{ prop.value }};
        {{~ end #prop ~}}

        {{~ end #component ~}}
        {{~ end #for entities ~}}
    }

    #endregion

    #region DebugLog

    /* Debug Log
    {{ debug_log }}
    */

    #endregion
}";
    }
}
