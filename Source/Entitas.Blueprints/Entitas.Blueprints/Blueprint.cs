namespace Entitas.Blueprints
{
    using System.Collections.Generic;

    /// <summary>
    ///   Template for creating an entity with all of its components.
    /// </summary>
    public class Blueprint : IBlueprint
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Creates a new default blueprint.
        /// </summary>
        public Blueprint()
            : this(string.Empty)
        {
        }

        /// <summary>
        ///   Creates a blueprint with the specified id.
        /// </summary>
        /// <param name="id">Id of the blueprint to create.</param>
        public Blueprint(string id)
        {
            this.Id = id;

            this.ComponentTypes = new List<string>();
            this.PropertyValues = new Dictionary<string, object>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Types of the components to add to the entity.
        /// </summary>
        public IList<string> ComponentTypes { get; set; }

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