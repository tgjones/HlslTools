using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SyntaxGenerator.Model
{
    public class Argument
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Type;
    }
}
