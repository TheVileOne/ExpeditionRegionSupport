using LogUtils.Helpers.FileHandling;
using System.IO;
using RainWorldPath = LogUtils.Helpers.Paths.RainWorld;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal sealed class TempFolderTests : TestCase, ITestable
    {
        internal const string TEST_NAME = "Test - Temp Folder";

        public TempFolderTests() : base(TEST_NAME)
        {
        }

        public void Test()
        {
            testMapPathToFolder();
        }

        private void testMapPathToFolder()
        {
            const string TEST_FILENAME = "SomeFile.txt";
            const string TEST_DIRECTORY = "SomeFolder";

            //Empty path tests
            testEmptyPathsReturnCorrectPath();
            //Filename tests
            testFilenameReturnsCorrectPath();
            //File path tests
            testFilePathsReturnCorrectPath();
            //Directory tests
            testDirectoryReturnsCorrectPath();
            //Directory path tests
            testDirectoryPathsReturnCorrectPath();
            //Path truncation tests
            testPathTruncationByFolderSelectionLimit();
            testPathTruncationIsConsistent();

            void testEmptyPathsReturnCorrectPath()
            {
                foreach (string input in TestInput.Strings.EmptyPathStrings)
                    testPath(input, TempFolder.Path);
            }

            void testFilenameReturnsCorrectPath()
            {
                testPath(TEST_FILENAME, Path.Combine(TempFolder.Path, TEST_FILENAME));
            }

            void testFilePathsReturnCorrectPath()
            {
                string partialPath, //Use for testing a path not associated with any path root.
                       gamePath,    //Use for testing a path inside the Rain World root folder.
                       tempPath,    //Use for testing a path inside the temp folder.
                       foreignPath; //Use for testing a path not inside Rain World root folder.

                //Arrange paths to test
                partialPath = @"SomeFolder\" + TEST_FILENAME;
                gamePath = Path.Combine(RainWorldPath.RootPath, partialPath); //Assigned subfolder is unimportant to result
                tempPath = Path.Combine(TempFolder.Path, partialPath);
                foreignPath = Path.Combine(@"C:\A\B\C", TEST_FILENAME);

                //Test each path against the expected output
                testPath(partialPath, Path.Combine(TempFolder.Path, partialPath));                            //Path will be mirrored within temp folder
                testPath(gamePath, Path.Combine(TempFolder.Path, RainWorldPath.ROOT_DIRECTORY, partialPath)); //Path will be mirrored within temp folder starting at Rain World root
                testPath(tempPath, tempPath);                                                                 //Path will be unchanged when processing a temp folder path
                testPath(foreignPath, Path.Combine(TempFolder.Path, @"A\B\C", TEST_FILENAME));                //Path will be mirrored within the temp folder
            }

            void testDirectoryReturnsCorrectPath()
            {
                testPath(TEST_DIRECTORY, Path.Combine(TempFolder.Path, TEST_DIRECTORY));
            }

            void testDirectoryPathsReturnCorrectPath()
            {
                string partialPath, //Use for testing a path not associated with any path root.
                       gamePath,    //Use for testing a path inside the Rain World root folder.
                       tempPath,    //Use for testing a path inside the temp folder.
                       foreignPath; //Use for testing a path not inside Rain World root folder.

                //Arrange paths to test
                partialPath = @"SomeFolder\" + TEST_DIRECTORY;
                gamePath = Path.Combine(RainWorldPath.RootPath, partialPath); //Assigned subfolder is unimportant to result
                tempPath = Path.Combine(TempFolder.Path, partialPath);
                foreignPath = @"C:\A\B\C"; //It will affect testing behavior if the number of folders in this path is changed for this test

                //Test each path against the expected output
                testPath(partialPath, Path.Combine(TempFolder.Path, partialPath));                            //Path will be mirrored within temp folder.
                testPath(gamePath, Path.Combine(TempFolder.Path, RainWorldPath.ROOT_DIRECTORY, partialPath)); //Path will be mirrored within temp folder starting at Rain World root.
                testPath(tempPath, tempPath);                                                                 //Path will be unchanged when processing a temp folder path.
                testPath(foreignPath, Path.Combine(TempFolder.Path, @"A\B\C"));                               //Path will be mirrored within the temp folder.
            }

            void testPathTruncationByFolderSelectionLimit()
            {
                //Test will break if limit value changes
                string testInput = @"C:\A\B\C\D\E";
                string expectedResult = Path.Combine(TempFolder.Path, @"B\C\D\E"); //When working with a directory path, selection maximum is increased by 1

                testPath(testInput, expectedResult);
            }

            void testPathTruncationIsConsistent() //The last path segment whether it be a filename or directory shouldn't be affected by folder selection limits 
            {
                //Test will break if limit value changes
                string testInput = @"C:\A\B\C\D\E";
                string expectedResult = Path.Combine(TempFolder.Path, @"B\C\D\E"); //When working with a directory path, selection maximum is increased by 1

                string directoryPathResult = TempFolder.MapPathToFolder(testInput);
                string filePathResult = TempFolder.MapPathToFolder(testInput + ".txt");

                string[] directoryPathSegments = PathUtils.SplitPath(directoryPathResult);
                string[] filePathSegments = PathUtils.SplitPath(filePathResult);

                //Increasing selection maximum by 1 should mean that these results should share the same parent path
                AssertThat(directoryPathSegments.Length).IsEqualTo(filePathSegments.Length);
            }

            void testPath(string testInput, string expectedOutput)
            {
                string pathResult = TempFolder.MapPathToFolder(testInput);
                AssertPathsAreEqual(expectedPath: expectedOutput,
                                      actualPath: pathResult);
            }
        }

        [PostTest]
        public void ShowResults()
        {
            TestLogger.LogDebug(CreateReport());
        }
    }
}
