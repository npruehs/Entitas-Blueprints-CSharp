﻿namespace Entitas.Blueprints.CodeGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Entitas.CodeGenerator;

    /// <summary>
    ///   Generates extension methods for creating entities and their components
    ///   from blueprints.
    /// </summary>
    public class BlueprintExtensionsGenerator : IComponentCodeGenerator
    {
        #region Constants

        /// <summary>
        ///   Declaration and opening bracket for the entity extension class body.
        /// </summary>
        private const string BeginEntityClass = "\n    public partial class Entity {";

        /// <summary>
        ///   Declaration and opening bracket for namespace.
        /// </summary>
        private const string BeginNamespace = @"namespace Entitas {";

        /// <summary>
        ///   Declaration and opening bracket for the pool extension class body.
        /// </summary>
        private const string BeginPoolExtensionClass = "\n    public static class BlueprintPoolExtension {";

        /// <summary>
        ///   Suffix of the generated component extension class file names.
        /// </summary>
        private const string ClassFileNameSuffix = "GeneratedBlueprintsExtension";

        /// <summary>
        ///   Closing bracket for the class body.
        /// </summary>
        private const string EndClass = "    }\n";

        private const string EndNamespace = "}\n";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///   Generates the contents of the source code files to write.
        /// </summary>
        /// <param name="componentInfos">Data about all available entity components.</param>
        /// <returns>Contents and names of the source code files to write.</returns>
        public CodeGenFile[] Generate(ComponentInfo[] componentInfos)
        {
            // Generate methods for initializing components from dictionaries.
            var codeGenFiles =
                componentInfos.Where(info => info.generateMethods && !info.isSingletonComponent)
                    .Select(
                        info =>
                            new CodeGenFile
                            {
                                fileName = info.fullTypeName + ClassFileNameSuffix,
                                fileContent = AddDefaultPoolCode(info).ToUnixLineEndings()
                            })
                    .ToList();

            // Generate methods for creating entities and adding components from blueprints.
            codeGenFiles.Add(
                new CodeGenFile
                {
                    fileName = "BlueprintPoolExtensions",
                    fileContent =
                        AddBlueprintPoolCode(componentInfos.Where(info => info.generateMethods)).ToUnixLineEndings()
                });

            return codeGenFiles.ToArray();
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Returns the method for initializing a component from a dictionary.
        /// </summary>
        /// <param name="info">Data about the component to generate the code for.</param>
        /// <returns>Method for initializing a component from a dictionary.</returns>
        private static string AddBlueprintMethod(ComponentInfo info)
        {
            var name = info.typeName.RemoveComponentSuffix();
            var lookupTags = info.ComponentLookupTags();
            var ids = lookupTags.Length == 0 ? string.Empty : lookupTags[0];
            var assignments = FieldAssignments(name, info.fieldInfos);

            return "\n"
                   + string.Format(@"        public Entity Add{0}(System.Collections.Generic.IDictionary<string, object> properties) {{
            var componentPool = GetComponentPool(ComponentIds.{0});
            var component = ({1})(componentPool.Count > 0 ? componentPool.Pop() : new {1}());
{2}
            return AddComponent({3}.{0}, component);
        }}
",
                       name,
                       info.typeName,
                       assignments,
                       ids);
        }

        /// <summary>
        ///   Returns the extension class body for creating entities and adding components from blueprints.
        /// </summary>
        /// <param name="infos">Data about all available entity components.</param>
        /// <returns>Extension class body for creating entities and adding components from blueprints.</returns>
        private static string AddBlueprintPoolCode(IEnumerable<ComponentInfo> infos)
        {
            var componentInfos = infos.ToList();

            var code = BeginNamespace;
            code += BeginPoolExtensionClass;
            code += AddCreateEntityMethod(componentInfos);
            code += AddComponentDictionary(componentInfos);
            code += EndClass;
            code += EndNamespace;
            return code;
        }

        /// <summary>
        ///   Generates a map from component type names to component ids.
        /// </summary>
        /// <param name="infos">Data about all available entity components.</param>
        /// <returns>Map from component type names to component ids.</returns>
        private static string AddComponentDictionary(IEnumerable<ComponentInfo> infos)
        {
            var cases = new StringBuilder();

            // Generate key-value-pairs for all components.
            foreach (var info in infos)
            {
                var name = info.typeName.RemoveComponentSuffix();

                cases.AppendLine(string.Format("            {{ \"{0}\", ComponentIds.{0} }},", name));
            }

            // Generate dictionary.
            return "\n"
                   + string.Format(@"        private static System.Collections.Generic.Dictionary<string, int> ComponentNameToId = new System.Collections.Generic.Dictionary<string, int> {{
{0}
        }};
",
                       cases);
        }

        /// <summary>
        ///   Returns the method for creating entities and adding components from blueprints.
        /// </summary>
        /// <param name="infos">Data about all available entity components.</param>
        /// <returns>Method for creating entities and adding components from blueprints.</returns>
        private static string AddCreateEntityMethod(IEnumerable<ComponentInfo> infos)
        {
            var cases = new StringBuilder();

            // Generate initialization cases for all components.
            foreach (var info in infos)
            {
                var name = info.typeName.RemoveComponentSuffix();

                // Begin new component case.
                cases.AppendLine(string.Format(@"                   case ComponentIds.{0}:", name));

                if (info.isSingletonComponent)
                {
                    // Add singleton component.
                    cases.AppendLine(string.Format(@"                        entity.Is{0}(true);", name));
                }
                else
                {
                    // Add and initialize component.
                    cases.AppendLine(
                        string.Format(@"                        entity.Add{0}(blueprint.PropertyValues);", name));
                }

                cases.AppendLine("                        break;");
            }

            // Generate method body.
            return "\n"
                   + string.Format(@"        public static Entity CreateEntity(this Pool pool, Entitas.Blueprints.IBlueprint blueprint) {{
            var entity = pool.CreateEntity();

            foreach (var componentTypeName in blueprint.ComponentTypes) {{
                var componentId = ComponentNameToId[componentTypeName];

                switch (componentId) {{
{0}
                }}
            }}

            return entity;
        }}
",
                       cases);
        }

        /// <summary>
        ///   Generates the extension class body for initializing a component from a dictionary.
        /// </summary>
        /// <param name="info">Data about the component to generate the class body for.</param>
        /// <returns>Extension class body for initializing a component from a dictionary.</returns>
        private static string AddDefaultPoolCode(ComponentInfo info)
        {
            var code = BeginNamespace;
            code += BeginEntityClass;

            if (!info.isSingletonComponent)
            {
                code += AddBlueprintMethod(info);
            }

            code += EndClass;
            code += EndNamespace;
            return code;
        }

        /// <summary>
        ///   Generates the code for assigning all fields of a component from data from a dictionary.
        /// </summary>
        /// <param name="componentName">Name of the component to generate the code for.</param>
        /// <param name="infos">Data about all fields to assign.</param>
        /// <returns>Code for assigning all fields of a component from data from a dictionary.</returns>
        private static string FieldAssignments(string componentName, ComponentFieldInfo[] infos)
        {
            const string Format = "            component.{0} = ({1})properties[\"{2}.{0}\"];";
            var assignments =
                infos.Select(info => { return string.Format(Format, info.name, info.type, componentName); }).ToArray();

            return string.Join("\n", assignments);
        }

        #endregion
    }
}