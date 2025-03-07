namespace testtask.sorter
{
    internal class ParsedStringComparer : IComparer<ParsedString>
    {
        public int Compare(ParsedString x, ParsedString y)
        {            
            int stringComparison = x.StringPart.Span.CompareTo(y.StringPart.Span, StringComparison.Ordinal);

            return stringComparison != 0 ? stringComparison : x.NumberPart.CompareTo(y.NumberPart);
        }
    }
}