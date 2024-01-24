using System.Text.RegularExpressions;

namespace RentCheckAU
{
    internal class Program
    {
        static HttpClient? client;

        static async Task Main(string[] args)
        {
            bool postCodeValid = false;
            int postCode = 0;

            while (!postCodeValid)
            {
                Console.WriteLine("Enter an Australian postcode.");
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (!int.TryParse(input, out var code))
                {
                    Console.WriteLine("Invalid postcode.");
                    continue;
                }

                postCode = code;
                postCodeValid = true;
            }

            client = new HttpClient();
            string? content;

            try
            {
                content = await client.GetStringAsync($"https://sqmresearch.com.au/weekly-rents.php?postcode={postCode}&t=1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch rent information for postcode '{postCode}'.");
                Console.WriteLine($"Error: {ex.Message}");
                await Main(args);
                return;
            }

            if (content is null)
            {
                Console.WriteLine("Postcode content was null.");
                await Main(args);
                return;
            }

            content = content.Replace("\r", "");
            content = content.Replace("\n", "");

            var matches = Regex.Match(content, "<.*All Houses.*?<div>(.*?)<\\/div>.*?All Units.*?<div>(.*?)<\\/div>");

            if (!matches.Success ||
                matches.Groups.Count < 2)
            {
                Console.WriteLine("Failed to find any regex matches for house/unit pricing.");
                return;
            }

            string? rentAvgHouses = matches.Groups[1].Value.Trim();

            Console.WriteLine($"The average rent for houses in the postcode '{postCode}' is '{rentAvgHouses}' AUD/wk.");

            if (matches.Groups.Count > 2)
            {
                string? rentAvgUnits = matches.Groups[2].Value.Trim();

                Console.WriteLine($"The average rent for units in the postcode '{postCode}' is '{rentAvgUnits}' AUD/wk.");
            }

            Console.WriteLine("Press ENTER to search another postcode.");
            Console.ReadLine();

            await Main(args);
        }
    }
}