using System.Text.RegularExpressions;

namespace Network.Services
{
    public class Validation
    {
        //validate and convert price string into decimal
        public decimal ValidProduct(string price)
        {
            // Remove whitespaces and validate conversion to decimal
            var cleanedPrice = RemoveWhitespacesUsingRegex(price);

            // Try parsing the cleaned price and handle any exceptions
            if (decimal.TryParse(cleanedPrice, out decimal validProduct))
            {
                return validProduct;
            }
            else
            {
                throw new FormatException("Invalid price format.");
            }
        }

        public string ValidYear(string year)
        {
            return RemoveWhitespacesUsingRegex(year);
        }


        private string RemoveWhitespacesUsingRegex(string source)
        {
            return Regex.Replace(source, @"\s", string.Empty);
        }
    }
}