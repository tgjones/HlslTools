using System.Xml;
using System.Xml.Serialization;

namespace SyntaxGenerator.Model
{
    public class Comment
    {
        [XmlAnyElement]
        public XmlElement[] Body;
    }
}
