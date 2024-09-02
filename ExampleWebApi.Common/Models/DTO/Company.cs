namespace ExampleWebApi.Common.Models.DTO
{
    public class Company : CompanyHeader
    {
        public EmployeeHeader[] Employees { get; set; }
        public Company()
        {
        }

        public Company(ExampleWebApi.Data.Models.Company company, List<ExampleWebApi.Data.Models.Employee> employees) : base(company, employees.Count)
        {
            Employees = employees.Select(e => new EmployeeHeader(e)).ToArray();
        }
    }
}
