using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ExampleWebApi.Data.Import
{
    public class ImportFileRow
    {
        [Index(0)]
        public int CompanyId { get; set; } = 0;
        [Index(1)]
        public string CompanyCode { get; set; } = string.Empty;
        [Index(2)]
        public string CompanyDescription { get; set; } = string.Empty;
        [Index(3)]
        public string EmployeeNumber { get; set; } = string.Empty;
        [Index(4)]
        public string FirstName { get; set; } = string.Empty;
        [Index(5)]
        public string LastName { get; set; } = string.Empty;
        [Index(6)]
        public string Email { get; set; } = string.Empty;
        [Index(7)]
        public string Department { get; set; } = string.Empty;
        [Index(8)]
        public DateOnly? HireDate { get; set; } = DateOnly.MinValue;
        [Index(9)]
        public string ManagerEmployeeNumber { get; set; } = string.Empty;
    }
}
