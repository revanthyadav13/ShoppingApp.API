using System.Globalization;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.API.Models;

namespace ShoppingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UploadController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (Path.GetExtension(file.FileName).ToLower() != ".csv")
                return BadRequest("Only CSV files are allowed.");

            var records = new List<Item>();

            // Create CsvConfiguration for handling CSV settings
            var csvConfig = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,  // Set HasHeaderRecord here
                Delimiter = ",",         // Specify the delimiter (usually comma)
            };

            var reader = new StreamReader(file.OpenReadStream());
            var csv = new CsvReader(reader, csvConfig);
            
                // Reading records from the CSV
                var csvRecords = csv.GetRecords<Item>().ToList();

                // Validate records if necessary
                foreach (var record in csvRecords)
                {
                    if (string.IsNullOrEmpty(record.Name) || record.Price <= 0)
                    {
                        return BadRequest($"Invalid record data: {record.Name}, {record.Price}");
                    }
                }
                records = csvRecords;
            

            // Store data in the database
            await _context.AddRangeAsync(records);
            await _context.SaveChangesAsync();

            return Ok(new { message = "File uploaded and data saved successfully." });
        }
    }
}
