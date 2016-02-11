namespace Entitas.Blueprints.CodeGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Entitas.CodeGenerator;

    public class BlueprintExtensionsGenerator : IComponentCodeGenerator
    {
        const string CLASS_SUFFIX = "GeneratedBlueprintsExtension";

        public CodeGenFile[] Generate(ComponentInfo[] componentInfos)
        {
            var codeGenFiles = componentInfos
                    .Where(info => info.generateMethods && !info.isSingletonComponent)
                    .Select(
                        info =>
                            new CodeGenFile
                            {
                                fileName = info.fullTypeName + CLASS_SUFFIX,
                                fileContent = addDefaultPoolCode(info).ToUnixLineEndings()
                            })
                    .ToList();

            codeGenFiles.Add(
                new CodeGenFile
                {
                    fileName = "BlueprintPoolExtensions",
                    fileContent =
                        addBlueprintPoolCode(componentInfos.Where(info => info.generateMethods)).ToUnixLineEndings()
                });

            return codeGenFiles.ToArray();
        }

        static string addDefaultPoolCode(ComponentInfo info)
        {
            var code = addNamespace();
            code += addEntityClassHeader();

            if (!info.isSingletonComponent)
            {
                code += addBlueprintMethod(info);
            }

            code += addCloseClass();
            code += closeNamespace();
            return code;
        }

        static string addBlueprintPoolCode(IEnumerable<ComponentInfo> infos)
        {
            var code = addNamespace();
            code += addPoolExtensionClassHeader();
            code += addCreateEntityMethod(infos);
            code += addComponentDictionary(infos);
            code += addCloseClass();
            code += closeNamespace();
            return code;
        }

        static string addNamespace()
        {
            return @"namespace Entitas {";
        }

        static string addEntityClassHeader()
        {
            return "\n    public partial class Entity {";
        }

        static string addPoolExtensionClassHeader()
        {
            return "\n    public static class BlueprintPoolExtension {";
        }

        static string addBlueprintMethod(ComponentInfo info)
        {
            var name = info.typeName.RemoveComponentSuffix();
            var nameLowercaseFirst = name.LowercaseFirst();
            var lookupTags = info.ComponentLookupTags();
            var ids = lookupTags.Length == 0 ? string.Empty : lookupTags[0];

            var memberNameInfos = info.fieldInfos;
            var assignments = fieldAssignments(name, memberNameInfos);

            return "\n" + string.Format(@"        public Entity Add{0}(System.Collections.Generic.IDictionary<string, object> properties) {{
            var componentPool = GetComponentPool(ComponentIds.{0});
            var component = ({2})(componentPool.Count > 0 ? componentPool.Pop() : new {2}());
{3}
            return AddComponent({4}.{0}, component);
        }}
", name, nameLowercaseFirst, info.typeName, assignments, ids);
        }

        static string addCreateEntityMethod(IEnumerable<ComponentInfo> infos)
        {
            var cases = new StringBuilder();

            foreach (var info in infos)
            {
                var name = info.typeName.RemoveComponentSuffix();

                cases.AppendLine(string.Format(@"                   case ComponentIds.{0}:", name));

                if (info.isSingletonComponent)
                {
                    cases.AppendLine(string.Format(@"                        entity.Is{0}(true);", name));
                }
                else
                {
                    cases.AppendLine(string.Format(@"                        entity.Add{0}(blueprint.PropertyValues);", name));
                }

                cases.AppendLine("                        break;");
            }
            
            return "\n" + string.Format(@"        public static Entity CreateEntity(this Pool pool, Entitas.Blueprints.IBlueprint blueprint) {{
            var entity = pool.CreateEntity();

            foreach (var componentTypeName in blueprint.ComponentTypes) {{
                var componentId = ComponentNameToId[componentTypeName];

                switch (componentId) {{
{0}
                }}
            }}

            return entity;
        }}
", cases);
        }

        static string addComponentDictionary(IEnumerable<ComponentInfo> infos)
        {
            var cases = new StringBuilder();

            foreach (var info in infos)
            {
                var name = info.typeName.RemoveComponentSuffix();

                cases.AppendLine(string.Format("            {{ \"{0}\", ComponentIds.{0} }},", name));
            }

            return "\n" + string.Format(@"        private static System.Collections.Generic.Dictionary<string, int> ComponentNameToId = new System.Collections.Generic.Dictionary<string, int> {{
{0}
        }};
", cases);
        }

        static string fieldAssignments(string componentName, ComponentFieldInfo[] infos)
        {
            const string format = "            component.{0} = ({1})properties[\"{2}.{0}\"];";
            var assignments = infos.Select(info => {
                return string.Format(format, info.name, info.type, componentName);
            }).ToArray();

            return string.Join("\n", assignments);
        }

        static string addCloseClass()
        {
            return "    }\n";
        }

        static string closeNamespace()
        {
            return "}\n";
        }
    }
}