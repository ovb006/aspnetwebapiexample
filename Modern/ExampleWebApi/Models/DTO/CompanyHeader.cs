using ExampleWebApi.Data.Models;

namespace ExampleWebApi.Models.DTO
{
    public class CompanyHeader
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int EmployeeCount { get; set; }  


        public CompanyHeader()
        {

        }

        public CompanyHeader(ExampleWebApi.Data.Models.Company company, int employeeCount)
        {
            Id = company.CompanyId;
            Code = company.CompanyCode;
            Description = company.CompanyDescription;
            EmployeeCount = employeeCount;
        }
    }
}
