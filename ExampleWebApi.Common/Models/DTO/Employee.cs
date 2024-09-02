namespace ExampleWebApi.Common.Models.DTO
{
    public class Employee : EmployeeHeader
    {
        public string Email { get; set; }
        public string Department { get; set; }
        public DateOnly HireDate { get; set; }
        public EmployeeHeader[] Managers{ get; set; }

        public Employee()
        {

        }

        public Employee(ExampleWebApi.Data.Models.Employee employee, EmployeeHeader[] managers) : base(employee)
        {
            Email = employee.Email;
            Department = employee.Department;
            HireDate = employee.HireDate;
            Managers = managers;
        }
    }
    
}
