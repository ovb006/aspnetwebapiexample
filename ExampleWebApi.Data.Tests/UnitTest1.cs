using ExampleWebApi.Data.Import;
using ExampleWebApi.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace ExampleWebApi.Data.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ValidDataShouldReturnSuccess()
        {
            var companies = new List<Company>
            {
                new Company { CompanyId = 1, },
                new Company { CompanyId = 2, }
            };

                    var employees = new List<Employee>
            {
                new Employee { EmployeeNumber = "E001", CompanyId = 1, ManagerEmployeeNumber = null },
                new Employee { EmployeeNumber = "E002", CompanyId = 1, ManagerEmployeeNumber = "E001" },
                new Employee { EmployeeNumber = "E003", CompanyId = 2, ManagerEmployeeNumber = null }
            };


            var result = ImportValidation.Validation(companies, employees);

            Assert.True(result.Success);
            Assert.Empty(result.Message);
        }

        [Fact]
        public void ManagerInOtherCompanyShouldReturnFailure()
        {
            var companies = new List<Company>
            {
                new Company { CompanyId = 1},
                new Company { CompanyId = 2}
            };

            var employees = new List<Employee>
            {
                new Employee { EmployeeNumber = "E001", CompanyId = 1, ManagerEmployeeNumber = null },
                new Employee { EmployeeNumber = "E002", CompanyId = 2, ManagerEmployeeNumber = "E001" } //Manager in other company
            };

            var result = ImportValidation.Validation(companies, employees);

            Assert.False(result.Success);
            Assert.Contains("Invalid Managers", result.Message);
        }

        [Fact]
        public void BogusManagerShouldReturnFailure()
        {
            var companies = new List<Company>
            {
                new Company { CompanyId = 1},
                new Company { CompanyId = 2}
            };

            var employees = new List<Employee>
            {
                new Employee { EmployeeNumber = "E001", CompanyId = 1, ManagerEmployeeNumber = null },
                new Employee { EmployeeNumber = "E002", CompanyId = 1, ManagerEmployeeNumber = "E004" } //Non-existant manager
            };

            var result = ImportValidation.Validation(companies, employees);

            Assert.False(result.Success);
            Assert.Contains("Invalid Managers", result.Message);
        }

        [Fact]
        public void DupeEmployeesInSameCompanyShouldReturnFailure()
        {
            var companies = new List<Company>
            {
                new Company { CompanyId = 1}
            };

            var employees = new List<Employee>
            {
                new Employee { EmployeeNumber = "E001", CompanyId = 1, ManagerEmployeeNumber = null },
                new Employee { EmployeeNumber = "E001", CompanyId = 1, ManagerEmployeeNumber = null } // Duplicate EmployeeNumber
            };

            var result = ImportValidation.Validation(companies, employees);

            Assert.False(result.Success);
            Assert.Contains("Duplicate Employee Numbers", result.Message);
        }

        [Fact]
        public void DupeEmployeesInDifferentCompaniesShouldReturnSuccess()
        {
            var companies = new List<Company>
            {
                new Company { CompanyId = 1},
                new Company { CompanyId = 2}
            };

            var employees = new List<Employee>
            {
                new Employee { EmployeeNumber = "E001", CompanyId = 1, ManagerEmployeeNumber = null },
                new Employee { EmployeeNumber = "E001", CompanyId = 2, ManagerEmployeeNumber = null } 
            };

            var result = ImportValidation.Validation(companies, employees);

            Assert.True(result.Success);
            Assert.Empty(result.Message);
        }


    }
}