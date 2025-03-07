namespace testtask.sorter
{
    internal readonly struct ParsedString
    {
        private const string Delimiter = ". ";
        public ParsedString(string value)
        {
            int dotIndex = value.IndexOf(Delimiter);

            NumberPart = string.Intern(value.Substring(0, dotIndex));
            StringPart = value.AsMemory(dotIndex + Delimiter.Length);
        }

        public ReadOnlyMemory<char> StringPart { get; }
        public string NumberPart { get; }
    }
}