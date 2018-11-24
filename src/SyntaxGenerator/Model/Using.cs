using System.Xml.Serialization;

namespace SyntaxGenerator.Model
{
    public class Using
    {
        [XmlAttribute]
        public string Namespace;
    }
}
