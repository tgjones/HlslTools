using System.Collections.Generic;
using System.Xml.Serialization;

namespace SyntaxGenerator.Model
{
    [XmlRoot]
    public class Tree
    {
        [XmlAttribute]
        public string Namespace;

        [XmlAttribute]
        public string LanguageName;

        [XmlAttribute]
        public string Root;

        [XmlElement(ElementName = "Node", Type = typeof(Node))]
        [XmlElement(ElementName = "AbstractNode", Type = typeof(AbstractNode))]
        [XmlElement(ElementName = "PredefinedNode", Type = typeof(PredefinedNode))]
        public List<TreeType> Types;

        [XmlElement(ElementName = "Using", Type = typeof(Using))]
        public List<Using> Usings;
    }
}
