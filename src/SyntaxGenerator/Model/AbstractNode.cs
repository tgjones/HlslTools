using System.Collections.Generic;
using System.Xml.Serialization;

namespace SyntaxGenerator.Model
{
    public class AbstractNode : TreeType
    {
        [XmlElement(ElementName = "Field", Type = typeof(Field))]
        public List<Field> Fields;
    }
}
