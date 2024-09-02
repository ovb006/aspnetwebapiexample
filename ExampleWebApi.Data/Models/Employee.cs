using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExampleWebApi.Data.Models
{
    public class Employee
    {
        //FWIW, I generally steer away from string based ids
        public string EmployeeNumber { get; set; }
        public int CompanyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public DateOnly HireDate { get; set; }
        public string? ManagerEmployeeNumber { get; set; }

        
        [ForeignKey("ManagerEmployeeNumber, CompanyId")] 
        public virtual Employee Manager { get; set; }

        public virtual ICollection<Employee> ManagedEmployees { get; set; }

    }

}

