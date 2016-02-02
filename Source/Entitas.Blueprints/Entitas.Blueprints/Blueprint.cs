namespace Entitas.Blueprints
{
    using System;
    using System.Collections.Generic;

    public class Blueprint
    {
        #region Properties

        /// <summary>
        ///   Ids of the components to add to the entity.
        /// </summary>
        public List<int> ComponentTypes { get; set; }

        /// <summary>
        ///   Values to initialize the component properties with.
        /// </summary>
        public Dictionary<string, object> PropertyValues { get; set; }

        #endregion
    }
}