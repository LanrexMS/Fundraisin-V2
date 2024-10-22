using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace Fundraising_Engagement.Plugins.Entities.Integration_Data_Model
{
    public class CSVParser
    {
        public List<DonationModel> ParseDonationRecords(string csvFilePath)
        {
            try
            {
                using (var reader = new StreamReader(csvFilePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    // Set up CSV configuration here if necessary (e.g., Delimiter, IgnoreBlankLines, etc.)
                    var records = csv.GetRecords<DonationModel>();
                    return new List<DonationModel>(records);
                }
            }
            catch (Exception ex)
            {
                // Handle any parsing exceptions (e.g., log or rethrow)
                Console.WriteLine($"Error occurred while parsing: {ex.Message}");
                return null;
            }
        }
    }
}
