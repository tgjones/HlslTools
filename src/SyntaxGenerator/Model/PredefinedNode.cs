using System.Collections.Generic;
using System.Xml.Serialization;

namespace SyntaxGenerator.Model
{
    public class PredefinedNode : TreeType
    {
        [XmlElement(ElementName = "Field", Type = typeof(Argument))]
        public List<Argument> Fields;
    }
}
