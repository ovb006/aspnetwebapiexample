using CsvHelper;
using ExampleWebApi.Data;
using ExampleWebApi.Data.Import;
using ExampleWebApi.Data.Models;
using ExampleWebApi.Common.Models;
using ExampleWebApi.Common.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ExampleConnStr");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ExampleWebApiDataContext>(p=>p.UseSqlServer(connectionString));
builder.Services.AddScoped<DataImporter>();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code)
    .CreateLogger();

builder.Host.UseSerilog();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ExampleWebApiDataContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/DataStore", async ([FromServices] ExampleWebApiDataContext context, [FromServices]DataImporter importer, IFormFile csvFile) =>
{
    var result = Results.Problem("An unknown error occurred", statusCode: 500);
    var records = new List<ImportFileRow>();
    try
    {
        using (var reader = new StreamReader(csvFile.OpenReadStream()))
        {

            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records.AddRange(csv.GetRecords<ImportFileRow>());
            }

            var importResult = await importer.ImportData(records);

            if (importResult.Success)
            {
                var resultMessage = $"{importResult.CompaniesImported} Companies and {importResult.EmployeesImported} Employees were imported";
                Log.Information(resultMessage);
                result = Results.Ok(resultMessage);
            }
            else
            {
                result = (importResult.InternalError)?Results.Problem(importResult.Message, statusCode: 500) : result = Results.BadRequest(importResult.Message);
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred: {Message}", ex.Message);
        result = Results.Problem($"An error occurred: {ex.Message}", statusCode: 500);
    }
    return result;
}).DisableAntiforgery();

//All companies as CompanyHeader
app.MapGet("/Companies/", async ([FromServices] ExampleWebApiDataContext context) =>
{
    var result = Results.Problem("An unknown error occurred", statusCode: 500);
    try
    {
        var companies = await context.Companies.ToListAsync();
        if (companies.Any())
        {
            var results = companies.Select(p => new CompanyHeader(p, context.Employees.Count(q => q.CompanyId == p.CompanyId)));
            result = Results.Ok(results);
        }
        else
        {
            Log.Warning("Company not found: {CompanyId}");
            result = Results.NotFound();
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred: {Message}", ex.Message);
        result = Results.Problem($"An error occurred: {ex.Message}", statusCode: 500);
    }
    return result;
});

//Companies by id
app.MapGet("/Companies/{companyId}", async ([FromServices] ExampleWebApiDataContext context, int companyId) =>
{
    var result = Results.Problem("An unknown error occurred", statusCode: 500);
    try
    {
        var company = await context.Companies.FirstOrDefaultAsync(p => p.CompanyId == companyId);
        if (company != null)
        {
            var boxedCompany = new ExampleWebApi.Common.Models.DTO.Company(company, context.Employees.Where(q => q.CompanyId == companyId).ToList());
            result = Results.Ok(boxedCompany);
        }
        else
        {
            Log.Warning("Company not found: {CompanyId}", companyId);
            result = Results.NotFound();
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred: {Message}", ex.Message);
        result = Results.Problem($"An error occurred: {ex.Message}", statusCode: 500);
    }
    return result;
});

//Employee by company and employee id
app.MapGet("/Companies/{companyId}/Employees/{employeeNumber}", async ([FromServices] ExampleWebApiDataContext context, int companyId, string employeeNumber) =>
{
    var result = Results.Problem("An unknown error occurred", statusCode: 500);
    try
    {
        var employee = await context.Employees.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.EmployeeNumber == employeeNumber);
        if (employee != null)
        {
            var managerHierarchy = context.GetManagerHierarchy(employeeNumber, companyId).Select(p => new EmployeeHeader(p)).ToArray();
            var boxedEmployees = new ExampleWebApi.Common.Models.DTO.Employee(employee, managerHierarchy);
            result = Results.Ok(boxedEmployees);
        }
        else
        {
            Log.Warning("Employee not found: {CompanyId}.{EmployeeNumber}", companyId,employeeNumber);
            result = Results.NotFound();
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred: {Message}", ex.Message);
        result = Results.Problem($"An error occurred: {ex.Message}", statusCode: 500);
    }
    return result;
});


app.Run();

