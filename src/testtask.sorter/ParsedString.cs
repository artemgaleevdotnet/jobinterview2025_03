namespace testtask.sorter
{
    internal readonly struct ParsedString
    {
        public ParsedString(string value, string delimiter)
        {
            int dotIndex = value.IndexOf(delimiter);

            NumberPart = string.Intern(value.Substring(0, dotIndex));
            StringPart = value.AsMemory(dotIndex + delimiter.Length);
        }

        public ReadOnlyMemory<char> StringPart { get; }
        public string NumberPart { get; }
    }
}