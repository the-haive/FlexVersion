//using Moq;
//using NUnit.Framework;

//namespace IntelliSearch.FlexVersionTests
//{
//    using FlexVersion;

//    [TestFixture]
//    public class FlexVersionTests
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
//            var flexVersion = new FlexVersion();
//            // Act
//            var version = flexVersion.Analyze(); 

//            // Assert
//            Assert.That(version, Is.EqualTo("1.0.0.0"));
//        }

//    }
//}
