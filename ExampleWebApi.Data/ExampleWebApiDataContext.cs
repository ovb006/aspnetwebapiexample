using ExampleWebApi.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleWebApi.Data
{
    public class ExampleWebApiDataContext : DbContext
    {
        public ExampleWebApiDataContext(DbContextOptions<ExampleWebApiDataContext> options) : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //There's a newer way to do this with annotations, but this is the way I remember from Raco
            modelBuilder.Entity<Employee>()
                .HasKey(p => new { p.EmployeeNumber, p.CompanyId });

            //There's probably a cleaner way to define this many:1 relationship
            modelBuilder.Entity<Employee>().HasOne(p=>p.Manager).WithMany(q => q.ManagedEmployees)
                .HasForeignKey(p=>new {p.ManagerEmployeeNumber, p.CompanyId})
                .HasPrincipalKey(q=>new {q.EmployeeNumber, q.CompanyId})
                .OnDelete(DeleteBehavior.NoAction);
                
                
            modelBuilder.Entity<Company>().HasKey(p => p.CompanyId);
            new DbInitializer(modelBuilder).Initialize();
        }

        //There's probably some slick way to do this with just linq instead of this while loop
        public List<Employee> GetManagerHierarchy(string employeeNumber, int companyId)
        {
            var managers = new List<Employee>();
            var currentEmployee = Employees
                                    .Include(p => p.Manager)
                                    .FirstOrDefault(q => q.EmployeeNumber == employeeNumber && q.CompanyId == companyId);

            //Avoiding infinite loops with the second clause here
            while (currentEmployee?.Manager != null && !managers.Contains(currentEmployee.Manager))
            {
                managers.Add(currentEmployee.Manager);
                currentEmployee = currentEmployee.Manager;
            }
            return managers;
        }


    }
}
