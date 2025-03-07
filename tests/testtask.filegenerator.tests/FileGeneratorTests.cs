using Microsoft.Extensions.DependencyInjection;

namespace testtask.filegenerator.tests
{
    [TestClass]
    public sealed class FileGeneratorTests
    {
        private const string FilePath = "testFile";
        private IFileGenerator? _fileGenerator;

        [TestInitialize]
        public void TestInit()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddFileGenerator();

            _fileGenerator = serviceCollection?.BuildServiceProvider().GetService<IFileGenerator>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(FilePath))
            { 
                File.Delete(FilePath); 
            }
        }

        [TestMethod]
        public async Task TestFileGenerator_WhenFileIsNotEmpty_FileSizeIsCorrect()
        {
            if (_fileGenerator == null)
            {
                Assert.Fail("DI is broken");
            }

            var fileSize = new Random().Next(short.MaxValue, ushort.MaxValue);

            await _fileGenerator.GenerateFile(FilePath, fileSize, CancellationToken.None);

            if (!File.Exists(FilePath))
            {
                Assert.Fail("File wasn't created");
            }

            Assert.AreEqual(new FileInfo(FilePath).Length, fileSize);
        }

        [TestMethod]
        public async Task TestFileGenerator_WhenFileIsNotEmpty_FormatOfDataIsCorrect()
        {
            if (_fileGenerator == null)
            {
                Assert.Fail("DI is broken");
            }

            await _fileGenerator.GenerateFile(FilePath, short.MaxValue, CancellationToken.None);

            if (!File.Exists(FilePath))
            {
                Assert.Fail("File wasn't created");
            }

            var text = File.ReadAllText(FilePath);

            var isFormatCorrect = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).All(line =>
            {
                var index = line.IndexOf(". ");

                return index > 0 && int.TryParse(line[..index], out _);
            });

            Assert.IsTrue(isFormatCorrect);
        }
    }
}
