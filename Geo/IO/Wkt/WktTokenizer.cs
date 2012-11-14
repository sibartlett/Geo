using System.Runtime.Serialization;
using System.Text;
using System.IO;

namespace Geo.IO.Wkt
{
	internal class WktTokenizer
    {
        public WktTokenQueue Tokenize(string text)
        {
            using (var reader = new StringReader(text))
			    return Tokenize(reader);
		}

        public WktTokenQueue Tokenize(TextReader reader)
        {
            var queue = new WktTokenQueue();
            var builder = new StringBuilder();

            var nextCh = reader.Peek();
            while (nextCh != -1)
            {
                var ch = (char)reader.Read();
                var type = GetTokenType(ch);

                nextCh = reader.Peek();
                var nextType = WktTokenType.None;
                if (nextCh != -1)
                    nextType = GetTokenType((char) nextCh);

                if (type != WktTokenType.Whitespace)
                {
                    builder.Append(ch);
                    if (type != nextType ||
                        type == WktTokenType.Comma ||
                        type == WktTokenType.LeftParenthesis ||
                        type == WktTokenType.RightParenthesis)
                    {
                        if (type != WktTokenType.Whitespace)
                            queue.Enqueue(new WktToken(type, builder.ToString()));
                        builder.Remove(0, builder.Length);
                    }
                }
            }
            return queue;
		}

		private static WktTokenType GetTokenType(char ch)
        {
            if (char.IsWhiteSpace(ch))
                return WktTokenType.Whitespace;
		    if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z')
                return WktTokenType.String;
            if (ch == '-' || ch == '.' || ch >= '0' && ch <= '9')
                return WktTokenType.Number;
		    if (ch == ',')
		        return WktTokenType.Comma;
		    if (ch == '(')
		        return WktTokenType.LeftParenthesis;
		    if (ch == ')')
		        return WktTokenType.RightParenthesis;
		    throw new SerializationException("Invalid WKT string.");
		}
	}
}
