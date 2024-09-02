namespace ExampleWebApi.Common.Models.DTO
{
    public class EmployeeHeader
    {
        public string EmployeeNumber { get; set; }
        public string FullName { get; set; }

        public EmployeeHeader()
        {

        }
        public EmployeeHeader(ExampleWebApi.Data.Models.Employee employee)
        {
            EmployeeNumber = employee.EmployeeNumber;
            FullName = $"{employee.FirstName} {employee.LastName}";
        }
    }
}
