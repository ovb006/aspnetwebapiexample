using ExampleWebApi.Data.Misc;
using ExampleWebApi.Data.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.SqlServer;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ExampleWebApi.Data.Import
{
    public class DataImporter
    {
        private readonly ExampleWebApiDataContext _context;
        public DataImporter(ExampleWebApiDataContext context)
        {
            _context = context;
        }

        public async Task<ImportResult> ImportData(List<ImportFileRow> records)
        {
            var resultObject = new ImportResult();
            try
            {
                //If we got this far we read the file.
                var companies = records.Select(p => new Company
                {
                    CompanyId = p.CompanyId,
                    CompanyCode = p.CompanyCode,
                    CompanyDescription = p.CompanyDescription
                }).GroupBy(p => p.CompanyId).Select(q => q.First()).ToList();

                var employees = records.Select(p => new Employee
                {
                    EmployeeNumber = p.EmployeeNumber,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Email = p.Email,
                    Department = p.Department,
                    //NOTE: I'm assuming a null hire date here means they were hired before the system was created instead of them not being hired yet.
                    HireDate = p?.HireDate ?? DateOnly.MinValue,
                    ManagerEmployeeNumber = string.IsNullOrEmpty(p.ManagerEmployeeNumber)? null : p.ManagerEmployeeNumber,
                    CompanyId = p.CompanyId
                }).ToList();


                var validationResult = ImportValidation.Validation(companies, employees);
                if (validationResult.Success)
                {

                    //I'm assuming you want a transaction here since it's a clear and replace operation and a failure could leave you in an odd state.
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            //Clear Existing rows - I could have also done this with manual sql TRUNCATE, but these collections are small
                            _context.Companies.RemoveRange(_context.Companies);
                            _context.Employees.RemoveRange(_context.Employees);
                            _context.Companies.AddRange(companies);
                            _context.Employees.AddRange(employees);

                            await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Companies ON;");
                            await _context.SaveChangesAsync();
                            await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Companies OFF;");
                            await _context.Database.CommitTransactionAsync();

                            resultObject.Success = true;
                            resultObject.CompaniesImported = companies.Count();
                            resultObject.EmployeesImported = employees.Count();
                        }
                        catch (Exception updateException)
                        {
                            Log.Error(updateException, "An error occurred: {Message}", updateException.Message);
                            await _context.Database.RollbackTransactionAsync();
                            resultObject.Message = $"Exception while updating database: {updateException.Message}";
                            resultObject.InternalError = true;
                        }
                    }
                }
                else
                {
                    resultObject.Message = validationResult.Message;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred: {Message}", ex.Message);
                resultObject.Message = ex.Message;
            }
            return resultObject;
        }

    }
}
