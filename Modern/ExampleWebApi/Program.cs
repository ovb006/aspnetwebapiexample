using CsvHelper;
using ExampleWebApi.Data;
using ExampleWebApi.Data.Import;
using ExampleWebApi.Data.Models;
using ExampleWebApi.Models;
using ExampleWebApi.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ExampleConnStr");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ExampleWebApiDataContext>(p=>p.UseSqlServer(connectionString));
builder.Services.AddScoped<DataImporter>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ExampleWebApiDataContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/DataStore", async (ExampleWebApiDataContext context, [FromServices]DataImporter importer, IFormFile csvFile) =>
{
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
                return Results.Ok($"{importResult.CompaniesImported} Companies and {importResult.EmployeesImported} Employees were imported");
            }
            else
            {
                if (importResult.InternalError)
                {
                    return Results.Problem(importResult.Message, statusCode: 500);
                } 
                else
                {
                    return Results.BadRequest(importResult.Message);
                }
            }
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"An error occurred: {ex.Message}", statusCode: 500);
    }


}).DisableAntiforgery();

//All companies as CompanyHeader
app.MapGet("/Companies/", async (ExampleWebApiDataContext context) =>
{
    try
    {
        var companies = await context.Companies.ToListAsync();
        if (companies.Count > 0)
        {
            var results = companies.Select(p => new CompanyHeader(p, context.Employees.Count(q => q.CompanyId == p.CompanyId)));
            return Results.Ok(results);
        }
        else
        {
            return Results.NotFound();
        }
    } catch (Exception ex)
    {
        return Results.Problem($"An error occurred: {ex.Message}",statusCode:500);
    }
});

//Companies by id
app.MapGet("/Companies/{companyId}", async (ExampleWebApiDataContext context, int companyId) =>
{
    try { 
        var company = await context.Companies.FirstOrDefaultAsync(p => p.CompanyId == companyId);
        if (company != null)
        {
            var result = new ExampleWebApi.Models.DTO.Company(company, context.Employees.Where(q => q.CompanyId == companyId).ToList());
            return Results.Ok(result);
        }
        else
        {
            return Results.NotFound();
        }
    } catch (Exception ex)
    {
        return Results.Problem($"An error occurred: {ex.Message}", statusCode: 500);
    }
});

//Employee by company and employee id
app.MapGet("/Companies/{companyId}/Employees/{employeeNumber}", async (ExampleWebApiDataContext context, int companyId, string employeeNumber) =>
{
    try
    {
        var employee = await context.Employees.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.EmployeeNumber == employeeNumber);
        if (employee != null)
        {
            var managerHierarchy = context.GetManagerHierarchy(employeeNumber, companyId).Select(p => new EmployeeHeader(p)).ToArray();
            var result = new ExampleWebApi.Models.DTO.Employee(employee, managerHierarchy);
            return Results.Ok(result);
        }
        else
        {
            return Results.NotFound();
        }

    }
    catch (Exception ex)
    {
        return Results.Problem($"An error occurred: {ex.Message}", statusCode: 500);
    }
});


app.Run();

