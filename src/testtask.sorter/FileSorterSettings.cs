namespace testtask.sorter
{
    public class FileSorterSettings
    {
        public int WriteBufferSize { get; set; }
        public int ReadBufferSize { get; set; }
        public double MemoryUsageFactor { get; set; }
        public int MaxStringSize { get; set; }
    }
}