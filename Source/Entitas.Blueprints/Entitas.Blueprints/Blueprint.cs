namespace Entitas.Blueprints
{
    using System.Collections.Generic;

    public class Blueprint : IBlueprint
    {
        #region Constructors and Destructors

        public Blueprint()
            : this(string.Empty)
        {
        }

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