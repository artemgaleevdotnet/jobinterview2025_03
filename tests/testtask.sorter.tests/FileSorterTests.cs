using Microsoft.Extensions.DependencyInjection;
using testtask.filegenerator;

namespace testtask.sorter.tests
{
    [TestClass]
    public sealed class FileSorterTests
    {
        private const string FilePath = "testFile";
        private const string SortedFile = "sortedTestFile";
        private readonly ushort FileSize = ushort.MaxValue;
        private ServiceProvider? _serviceProvider;

        [TestInitialize]
        public async Task TestInit()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddFileGenerator();
            serviceCollection.AddFileSorter();

            _serviceProvider = serviceCollection.BuildServiceProvider();

            var generator = _serviceProvider.GetService<IFileGenerator>();

            if (generator == null)
            {
                Assert.Fail("File wasn't generated");
            }

            await generator.GenerateFile(FilePath, FileSize, CancellationToken.None);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }

            if (File.Exists(SortedFile))
            {
                File.Delete(SortedFile);
            }
        }

        [TestMethod]
        public async Task SortFile_WhenUnsortedFileIsExists_SortedFileIsCreated()
        {
            var fileSorter = _serviceProvider?.GetService<IFileSorter>();

            if (fileSorter == null)
            {
                Assert.Fail("Di is broken");
            }

            await fileSorter.SortFile(FilePath, SortedFile, CancellationToken.None);

            Assert.IsTrue(File.Exists(SortedFile));

            Assert.AreEqual(new FileInfo(FilePath).Length, new FileInfo(SortedFile).Length);
        }

        [TestMethod]
        public async Task SortFile_WhenFileExists_SortingIsCorrect()
        {
            var fileSorter = _serviceProvider?.GetService<IFileSorter>();

            if (fileSorter == null)
            {
                Assert.Fail("Di is broken");
            }

            await fileSorter.SortFile(FilePath, SortedFile, CancellationToken.None);

            var lines = await File.ReadAllLinesAsync(SortedFile);

            var prevLine = lines[0];

            for (int i = 1; i < lines.Length; i++) 
            {
                var compareValue = string.Compare(prevLine[prevLine.IndexOf(". ")..], lines[i][lines[i].IndexOf(". ")..]);

                if (compareValue > 0)
                {
                    Assert.Fail($"{prevLine} and {lines[i]} has wrong order");
                }

                if (compareValue == 0 && int.Parse(prevLine[..prevLine.IndexOf(". ")]) > int.Parse(lines[i][..lines[i].IndexOf(". ")]))
                {
                    Assert.Fail($"{prevLine} and {lines[i]} has wrong order");
                }

                prevLine = lines[i];
            }
        }
    }
}
