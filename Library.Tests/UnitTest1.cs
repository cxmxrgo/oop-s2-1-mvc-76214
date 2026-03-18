namespace Library.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ThisTestShouldFail()
        {
            string name = "Carecaio";
            Assert.Equal("Carecaio", name);
        }
    }
}
