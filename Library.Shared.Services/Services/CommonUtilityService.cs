namespace Library.Shared.Services
{
    public class CommonUtilityService
    {
        //bitwise trick to check a Book ID is a Power of Two
        public bool IsPowerOfTwo(int bookId)
        {
            return bookId > 0 && (bookId & (bookId - 1)) == 0;
        }

        //Reverse a Book Title
        public string ReverseTitle(string title)
        {
            if (string.IsNullOrEmpty(title)) return title;
            char[] chars = title.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        //Generate Book Title Replicas
        public string RepeatTitle(string title, int count)
        {
            if (count <= 0 || title == null) return string.Empty;
            return string.Concat(Enumerable.Repeat(title, count));
        }

        //List Odd-Numbered Book IDs from 0 to 100
        public bool CheckTheGivenBookIdOdd(int input)
        {
            return input % 2 != 0;
        }

        //List Odd-Numbered Book IDs from 0 to 100
        public void PrintOddBookIds()
        {
            for (int i = 1; i < 100; i += 2)
            {
                Console.WriteLine(i);
            }
        }
    }
}
