namespace ExampleWebApi.Models.DTO
{
    //Why did I not take the lazy way out and use the public DTOs as my data model?
    //Two reasons - 1 it's inefficient to structure the data that way, it's overly normalized
    // 2, I wanted to demonstrate how I'd add convenience methods to the DTOs to make them easier to work with
    // I realize the argument could be made here that this couples the api to the data layer.
    // I'd argue that the DTOs are a contract between the api and the client, and that the api should be the one to define that contract.
    // but that internally data should be structured in the way that is most efficient.

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
