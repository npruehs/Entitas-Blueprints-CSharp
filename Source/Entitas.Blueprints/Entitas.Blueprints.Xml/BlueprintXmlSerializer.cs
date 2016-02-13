namespace Entitas.Blueprints.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Serialization;

    public class BlueprintXmlSerializer
    {
        #region Constants

        private const string BlueprintElementName = "Blueprint";

        private const string BlueprintIdAttributeName = "Id";

        private const string ComponentElementName = "Component";

        private const string ComponentsElementName = "Components";

        private const string PropertyElementName = "Property";

        private const string PropertyKeyAttributeName = "Key";

        private const string PropertyTypeAttributeName = "Type";

        private const string PropertyValueElementName = "Value";

        private const string PropertyValuesElementName = "Properties";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///   Reads the next blueprint from the specified reader.
        /// </summary>
        /// <param name="reader">Reader to read the next blueprint from.</param>
        /// <returns>Read blueprint.</returns>
        public Blueprint Read(XmlReader reader)
        {
            var blueprintId = reader[BlueprintIdAttributeName];
            var blueprint = new Blueprint(blueprintId);

            // Read blueprint id.
            reader.ReadStartElement(BlueprintElementName);
            {
                // Read property values.
                reader.ReadStartElement(PropertyValuesElementName);
                {
                    while (reader.IsStartElement(PropertyElementName))
                    {
                        var key = reader[PropertyKeyAttributeName];
                        var typeName = reader[PropertyTypeAttributeName];
                        var type = Type.GetType(typeName);

                        reader.ReadStartElement(PropertyElementName);
                        {
                            var value =
                                new XmlSerializer(type, new XmlRootAttribute(PropertyValueElementName)).Deserialize(
                                    reader);

                            // Add to blueprint.
                            blueprint.PropertyValues.Add(key, value);
                        }
                        reader.ReadEndElement();
                    }
                }
                reader.ReadEndElement();

                // Read components.
                reader.ReadStartElement(ComponentsElementName);
                {
                    while (reader.IsStartElement(ComponentElementName))
                    {
                        var component = reader.ReadElementContentAsString();

                        // Add to blueprint.
                        blueprint.ComponentTypes.Add(component);
                    }
                }
                reader.ReadEndElement();
            }
            reader.ReadEndElement();

            return blueprint;
        }

        /// <summary>
        ///   Reads all following blueprints from the specified reader.
        /// </summary>
        /// <param name="xmlReader">Reader to read the blueprints from.</param>
        /// <returns>Read blueprints.</returns>
        public List<Blueprint> ReadBlueprints(XmlReader xmlReader)
        {
            var blueprints = new List<Blueprint>();

            xmlReader.ReadToFollowing(BlueprintElementName);
            while (xmlReader.Name == BlueprintElementName)
            {
                var blueprint = this.Read(xmlReader);
                blueprints.Add(blueprint);
            }

            return blueprints;
        }

        /// <summary>
        ///   Writes the passed blueprint to the specified writer.
        /// </summary>
        /// <param name="writer">Writer to write the blueprint to.</param>
        /// <param name="blueprint">Blueprint to write.</param>
        public void Write(XmlWriter writer, IBlueprint blueprint)
        {
            writer.WriteStartElement(BlueprintElementName);
            {
                // Write blueprint id.
                writer.WriteAttributeString(BlueprintIdAttributeName, blueprint.Id);

                // Write property values.
                writer.WriteStartElement(PropertyValuesElementName);
                {
                    foreach (var property in blueprint.PropertyValues)
                    {
                        var propertyType = property.Value.GetType();

                        writer.WriteStartElement(PropertyElementName);
                        {
                            writer.WriteAttributeString(PropertyKeyAttributeName, property.Key);
                            writer.WriteAttributeString(PropertyTypeAttributeName, propertyType.FullName);
                            new XmlSerializer(propertyType, new XmlRootAttribute(PropertyValueElementName)).Serialize(
                                writer,
                                property.Value);
                        }
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();

                // Write components.
                writer.WriteStartElement(ComponentsElementName);
                {
                    foreach (var component in blueprint.ComponentTypes)
                    {
                        writer.WriteElementString(ComponentElementName, component);
                    }
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        #endregion
    }
}