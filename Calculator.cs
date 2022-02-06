namespace IPSuiteCalculator
{
    public class Calculator
    {
        public static decimal Median(IEnumerable<int> source)
        {
            // copy source to a sorted array
            int[] temp = source.ToArray();
            Array.Sort(temp);

            int count = temp.Length;
            if (count == 0)
            {
                throw new InvalidOperationException("Empty collection");
            }
            else if (count % 2 == 0)
            {
                // count is even, average two middle elements
                int a = temp[count / 2 - 1];
                int b = temp[count / 2];
                return (a + b) / 2m;
            }
            else
            {
                // count is odd, return the middle element
                return temp[count / 2];
            }
        }

        // Nine Star Ki number
        public static int Number(int year)
        {
            double sum = 0;

            for (int i = 0; i <= 3; i++)
            {
                sum += Modulus(Math.Floor(Math.Abs(year) / Math.Pow(10, i)), 10);
            }

            int number = SumOfDigits(11 - SumOfDigits(Math.Sign(year) * sum));

            return number;
        }

        // mathematical modulus
        public static double Modulus(double a, double n)
        {
            double result = a % n;
            if ((a < 0 && n > 0 || a > 0 && n < 0) & result != 0)
            {
                result += n;
            }

            return result;
        }

        // integeral sum of digits in a two-digit integer
        public static int SumOfDigits(double integer)
            => (int)(Math.Floor(integer / 10) + Modulus(integer, 10));
    }
}
