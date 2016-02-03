namespace Entitas.Blueprints
{
    using System.Collections.Generic;

    public class Blueprint : IBlueprint
    {
        #region Properties

        /// <summary>
        ///   Ids of the components to add to the entity.
        /// </summary>
        public IEnumerable<int> ComponentTypes { get; set; }

        /// <summary>
        ///   Unique identifier of this blueprint.
        ///   Used in dictionaries to improve lookup performance.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///   Values to initialize the component properties with.
        /// </summary>
        public IDictionary<string, object> PropertyValues { get; set; }

        #endregion
    }
}