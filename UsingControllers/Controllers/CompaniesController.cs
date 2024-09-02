using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExampleWebApi.Data;
using ExampleWebApi.Common.Models;
using ExampleWebApi.Common.Models.DTO;
using Serilog;

namespace ExampleWebApi.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly ExampleWebApiDataContext _context;

        public CompaniesController(ExampleWebApiDataContext context)
        {
            _context = context;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<IResult> GetAllCompanies()
        {
            var result = Results.Problem("An unknown error occurred", statusCode: 500);
            try
            {
                var companies = await _context.Companies.ToListAsync();
                if (companies.Any())
                {
                    var results = companies.Select(p => new CompanyHeader(p, _context.Employees.Count(q => q.CompanyId == p.CompanyId)));
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
        }

        // GET: api/Companies/{companyId}
        [HttpGet("{companyId}")]
        public async Task<IResult> GetCompanyById(int companyId)
        {
            var result = Results.Problem("An unknown error occurred", statusCode: 500);
            try
            {
                var company = await _context.Companies.FirstOrDefaultAsync(p => p.CompanyId == companyId);
                if (company != null)
                {
                    var boxedCompany = new Company(company, _context.Employees.Where(q => q.CompanyId == companyId).ToList());
                    result = Results.Ok(boxedCompany);
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
        }

        // GET: api/Companies/{companyId}/Employees/{employeeNumber}
        [HttpGet("{companyId}/Employees/{employeeNumber}")]
        public async Task<IResult> GetEmployeeByCompanyIdAndEmployeeNumber(int companyId, string employeeNumber)
        {
            var result = Results.Problem("An unknown error occurred", statusCode: 500);
            try
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.EmployeeNumber == employeeNumber);
                if (employee != null)
                {
                    var managerHierarchy = _context.GetManagerHierarchy(employeeNumber, companyId).Select(p => new EmployeeHeader(p)).ToArray();
                    var boxedEmployees = new ExampleWebApi.Common.Models.DTO.Employee(employee, managerHierarchy);
                    result = Results.Ok(boxedEmployees);
                }
                else
                {
                    Log.Warning("Employee not found: {CompanyId}.{EmployeeNumber}", companyId, employeeNumber);
                    result = Results.NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred: {Message}", ex.Message);
                result = Results.Problem($"An error occurred: {ex.Message}", statusCode: 500);
            }
            return result;
        }
    }
}
