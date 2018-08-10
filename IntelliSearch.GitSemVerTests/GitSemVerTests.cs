//using Moq;
//using NUnit.Framework;

//namespace IntelliSearch.GitSemVerTests
//{
//    using GitSemVer;

//    [TestFixture]
//    public class GitSemVerTests
//    {
    
//        private MockRepository mockRepository;



//    [SetUp]
//        public void SetUp()
//        {
//            this.mockRepository = new MockRepository(MockBehavior.Strict);


//        }

//        [TearDown]
//        public void TearDown()
//        {
//            this.mockRepository.VerifyAll();
//        }


//        [Test]
//        public void TestDefaultRepoDefaultSettings()
//        {
//            // Arrange
//            var gitSemVer = new GitSemVer();
//            // Act
//            var version = gitSemVer.Analyze(); 

//            // Assert
//            Assert.That(version, Is.EqualTo("1.0.0.0"));
//        }

//    }
//}
