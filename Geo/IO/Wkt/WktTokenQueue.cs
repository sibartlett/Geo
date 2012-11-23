using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Geo.IO.Wkt
{
    internal class WktTokenQueue : Queue<WktToken>
    {
        public WktTokenQueue()
        {
        }

        public WktTokenQueue(IEnumerable<WktToken> tokens) : base(tokens)
        {
        }

        public bool NextTokenIs(WktTokenType type)
        {
            if (Count == 0)
                return false;
            var token = Peek();
            return token.Type == type;
        }

        public bool NextTokenIs(string value)
        {
            if (Count == 0)
                return false;
            var token = Peek();
            return token.Type == WktTokenType.String && token.Value.ToUpperInvariant() == value;
        }

        public WktToken Dequeue(WktTokenType type)
        {
            var t = Dequeue();
            if (t.Type != type)
                throw new SerializationException("Invalid WKT string.");
            return t;
        }

        public WktToken Dequeue(string value)
        {
            var token = Dequeue();
            if (token.Type != WktTokenType.String || !string.Equals(value, token.Value, StringComparison.OrdinalIgnoreCase))
                throw new SerializationException("Invalid WKT string.");
            return token;
        }
    }
}
