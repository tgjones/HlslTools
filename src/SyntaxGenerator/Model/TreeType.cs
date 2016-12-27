using System.Xml.Serialization;

namespace SyntaxGenerator.Model
{
    public class TreeType
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Base;

        [XmlElement]
        public Comment TypeComment;

        [XmlElement]
        public Comment FactoryComment;
    }
}
