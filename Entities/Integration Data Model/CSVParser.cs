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
        public List<DonationModel> ParseDonationRecordsFromString(string csvContent)
        {
            try
            {
                using (var reader = new StringReader(csvContent))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    // Set up CSV configuration if needed
                    var records = csv.GetRecords<DonationModel>();
                    return new List<DonationModel>(records);
                }
            }
            catch (Exception ex)
            {
                // Handle parsing exceptions
                Console.WriteLine($"Error occurred while parsing: {ex.Message}");
                return null;
            }
        }
    }
}
