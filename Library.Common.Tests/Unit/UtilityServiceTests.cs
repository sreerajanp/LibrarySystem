using Library.Shared.Services;

namespace Library.Common.Tests.Unit
{
    public class CommonUtilityServiceTests
    {
        private readonly CommonUtilityService _utilityService;

        public CommonUtilityServiceTests()
        {
            _utilityService = new CommonUtilityService();
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, false)]
        [InlineData(4, true)]
        [InlineData(0, false)]
        [InlineData(-2, false)]
        public void IsPowerOfTwo_ShouldReturnCorrectResult(int bookId, bool expected)
        {
            var result = _utilityService.IsPowerOfTwo(bookId);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Book", "kooB")]
        [InlineData("12345", "54321")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void ReverseTitle_ShouldReturnReversedString(string input, string expected)
        {
            var result = _utilityService.ReverseTitle(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Hi", 3, "HiHiHi")]
        [InlineData("A", 5, "AAAAA")]
        [InlineData("X", 0, "")]
        [InlineData(null, 3, "")]
        public void RepeatTitle_ShouldReturnRepeatedString(string title, int count, string expected)
        {
            var result = _utilityService.RepeatTitle(title, count);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, false)]
        [InlineData(99, true)]
        [InlineData(100, false)]
        public void CheckTheGivenBookIdOdd_ShouldReturnTrueIfOdd(int input, bool expected)
        {
            var result = _utilityService.CheckTheGivenBookIdOdd(input);
            Assert.Equal(expected, result);
        }
    }
}
