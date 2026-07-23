using System.Xml;

namespace Geo.Gps.Serialization.Xml;

// Decorates an XmlReader and reports a fixed namespace for elements that appear
// in no namespace, so documents whose root element is missing its default xmlns
// (a common defect in real-world GPX exports) can still be deserialized against
// the namespace-qualified serialization models. Attributes and nodes that are
// already namespaced are passed through unchanged.
internal sealed class NamespaceCoercingXmlReader : XmlReader
{
    private readonly XmlReader _inner;
    private readonly string _namespace;

    public NamespaceCoercingXmlReader(XmlReader inner, string @namespace)
    {
        _inner = inner;
        _namespace = @namespace;
    }

    public override string NamespaceURI =>
        (_inner.NodeType == XmlNodeType.Element || _inner.NodeType == XmlNodeType.EndElement)
        && string.IsNullOrEmpty(_inner.NamespaceURI)
            ? _namespace
            : _inner.NamespaceURI;

    public override int AttributeCount => _inner.AttributeCount;
    public override string BaseURI => _inner.BaseURI;
    public override int Depth => _inner.Depth;
    public override bool EOF => _inner.EOF;
    public override bool IsEmptyElement => _inner.IsEmptyElement;
    public override string LocalName => _inner.LocalName;
    public override XmlNameTable NameTable => _inner.NameTable;
    public override XmlNodeType NodeType => _inner.NodeType;
    public override string Prefix => _inner.Prefix;
    public override ReadState ReadState => _inner.ReadState;
    public override string Value => _inner.Value;

    public override string GetAttribute(int i) => _inner.GetAttribute(i);

    public override string GetAttribute(string name) => _inner.GetAttribute(name);

    public override string GetAttribute(string name, string namespaceURI) =>
        _inner.GetAttribute(name, namespaceURI);

    public override string LookupNamespace(string prefix) => _inner.LookupNamespace(prefix);

    public override bool MoveToAttribute(string name) => _inner.MoveToAttribute(name);

    public override bool MoveToAttribute(string name, string ns) =>
        _inner.MoveToAttribute(name, ns);

    public override bool MoveToElement() => _inner.MoveToElement();

    public override bool MoveToFirstAttribute() => _inner.MoveToFirstAttribute();

    public override bool MoveToNextAttribute() => _inner.MoveToNextAttribute();

    public override bool Read() => _inner.Read();

    public override bool ReadAttributeValue() => _inner.ReadAttributeValue();

    public override void ResolveEntity() => _inner.ResolveEntity();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _inner.Dispose();
    }
}
