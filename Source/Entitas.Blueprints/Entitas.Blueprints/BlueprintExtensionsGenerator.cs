namespace Entitas.Blueprints
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Entitas.CodeGenerator;

    public class BlueprintExtensionsGenerator : IComponentCodeGenerator
    {
        const string CLASS_SUFFIX = "GeneratedBlueprintsExtension";

        public CodeGenFile[] Generate(Type[] components)
        {
            var codeGenFiles = components
                    .Where(shouldGenerate)
                    .Aggregate(new List<CodeGenFile>(), (files, type) => {
                        files.Add(new CodeGenFile
                        {
                            fileName = type + CLASS_SUFFIX,
                            fileContent = addDefaultPoolCode(type).ToUnixLineEndings()
                        });
                        return files;
                    }).ToList();

            codeGenFiles.Add(new CodeGenFile
            {
                fileName = "BlueprintPoolExtensions",
                fileContent = addBlueprintPoolCode(components
                    .Where(shouldGenerate)).ToUnixLineEndings()
            });

            return codeGenFiles.ToArray();
        }

        static bool shouldGenerate(Type type)
        {
            return !Attribute.GetCustomAttributes(type).Any(attr => attr is DontGenerateAttribute)
                   && !isSingletonComponent(type);
        }

        static string addDefaultPoolCode(Type type)
        {
            var code = addNamespace();
            code += addEntityClassHeader();
            code += addBlueprintMethod(type);
            code += addCloseClass();
            code += closeNamespace();
            return code;
        }

        static string addBlueprintPoolCode(IEnumerable<Type> types)
        {
            var code = addNamespace();
            code += addPoolExtensionClassHeader();
            code += addCreateEntityMethod(types);
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

        static string addBlueprintMethod(Type type)
        {
            var name = type.RemoveComponentSuffix();
            var nameLowercaseFirst = name.LowercaseFirst();
            var lookupTags = type.ComponentLookupTags();
            var ids = lookupTags.Length == 0 ? string.Empty : lookupTags[0];

            var memberNameInfos = getFieldInfos(type);
            var assignments = fieldAssignments(memberNameInfos);

            return "\n" + string.Format(@"        public Entity Add{0}(System.Collections.Generic.IDictionary<string, object> properties) {{
            var component = _{1}ComponentPool.Count > 0 ? _{1}ComponentPool.Pop() : new {2}();
{3}
            return AddComponent({4}.{0}, component);
        }}
", name, nameLowercaseFirst, type, assignments, ids);
        }

        static string addCreateEntityMethod(IEnumerable<Type> types)
        {
            var cases = new StringBuilder();

            foreach (var type in types)
            {
                var name = type.RemoveComponentSuffix();

                cases.Append(string.Format(@"                   case ComponentIds.{0}:
                        entity.Add{0}(blueprint.PropertyValues);
                        break;
", name));
            }
            
            return "\n" + string.Format(@"        public static Entity CreateEntity(this Pool pool, Entitas.Blueprints.IBlueprint blueprint) {{
            var entity = pool.CreateEntity();

            foreach (var componentType in blueprint.ComponentTypes) {{
                switch (componentType) {{
{0}
                }}
            }}

            return entity;
        }}
", cases);
        }

        static string fieldAssignments(MemberTypeNameInfo[] infos)
        {
            const string format = "            component.{0} = ({1})properties[\"{0}\"];";
            var assignments = infos.Select(info => {
                return string.Format(format, info.name, info.type);
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

        static MemberTypeNameInfo[] getFieldInfos(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Select(field => new MemberTypeNameInfo { name = field.Name, type = field.FieldType })
                .ToArray();
        }

        static bool isSingletonComponent(Type type)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            return type.GetFields(bindingFlags).Length == 0;
        }
    }
}