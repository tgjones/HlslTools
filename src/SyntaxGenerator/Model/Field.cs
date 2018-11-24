using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SyntaxGenerator.Model
{
    public class Field
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Type;

        [XmlAttribute]
        public string Optional;

        [XmlAttribute]
        public string Override;

        [XmlAttribute]
        public string New;

        [XmlAttribute]
        public string Abstract;

        [XmlElement(ElementName = "Kind", Type = typeof(Kind))]
        public List<Kind> Kinds;

        [XmlElement]
        public Comment PropertyComment;

        [XmlElement(ElementName = "Getter", Type = typeof(Field))]
        public List<Field> Getters;
    }
}
