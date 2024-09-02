using CsvHelper;
using ExampleWebApi.Data;
using ExampleWebApi.Data.Import;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Globalization;

namespace UsingControllers.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class DataStoreController : ControllerBase
    {

        private readonly ILogger<DataStoreController> _logger;
        private readonly ExampleWebApiDataContext _context;
        private readonly DataImporter _importer;

        public DataStoreController(ExampleWebApiDataContext context, DataImporter importer, ILogger<DataStoreController> logger)
        {
            _logger = logger;
            _context = context;
            _importer = importer;
        }

        [HttpPost("/")]
        public async Task<IResult> Import(IFormFile csvFile)
        {
            var response = Results.Problem("An unknown error occurred", statusCode: 500);
            var records = new List<ImportFileRow>();
            try
            {
                using (var reader = new StreamReader(csvFile.OpenReadStream()))
                {

                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        records.AddRange(csv.GetRecords<ImportFileRow>());
                    }

                    var importResult = await _importer.ImportData(records);

                    if (importResult.Success)
                    {
                        var resultMessage = $"{importResult.CompaniesImported} Companies and {importResult.EmployeesImported} Employees were imported";
                        Log.Information(resultMessage);
                        response = Results.Ok(resultMessage);
                    }
                    else
                    {
                        if (importResult.InternalError)
                        {
                            return Results.Problem(importResult.Message, statusCode: 500);
                        }
                        else
                        {
                            response = Results.BadRequest(importResult.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = Results.Problem($"An error occurred: {ex.Message}", statusCode: 500);
            }
            return response;
        }
    }
}
