namespace Entitas.Blueprints
{
    using System.Collections.Generic;

    public interface IBlueprint
    {
        /// <summary>
        ///   Ids of the components to add to the entity.
        /// </summary>
        IEnumerable<int> ComponentTypes { get; }

        /// <summary>
        ///   Values to initialize the component properties with.
        /// </summary>
        IDictionary<string, object> PropertyValues { get; }
    }
}