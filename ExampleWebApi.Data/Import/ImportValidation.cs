using ExampleWebApi.Data.Misc;
using ExampleWebApi.Data.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleWebApi.Data.Import
{
    public static class ImportValidation
    {
        public static BaseResultObject Validation(List<Company> companies, List<Employee> employees)
        {
            var result = new BaseResultObject();

            //Validation, no employee can have a manager with a different companyId
            var invalidEmployees = employees.Where(
                p => p.ManagerEmployeeNumber != null
                && p.CompanyId != employees.FirstOrDefault(
                        q => q.EmployeeNumber == p.ManagerEmployeeNumber
                        && q.CompanyId == p.CompanyId
                      )?.CompanyId);

            //Use this one if you want no invalid manager references overall
            //var invalidEmployees = employees.Where(
            //    p => p.ManagerEmployeeNumber != null
            //    && employees.FirstOrDefault(q => q.CompanyId==p.CompanyId && q.EmployeeNumber == p.ManagerEmployeeNumber)==null);

            if (!invalidEmployees.Any())
            {
                //Validation, no duplicate employee numbers within a single company
                var duplicateEmployeeNumbers = employees.GroupBy(
                    p => p.CompanyId).Where(
                        p => p.GroupBy(
                                p => p.EmployeeNumber)
                        .Any(q => q.Count() > 1)).SelectMany(p => p);

                if (!duplicateEmployeeNumbers.Any())
                {
                    result.Success = true;
                }
                else
                {
                    result.Message = $"Duplicate Employee Numbers : {JsonConvert.SerializeObject(duplicateEmployeeNumbers)}";
                }
            }
            else
            {
                result.Message = $"Invalid Managers : {JsonConvert.SerializeObject(invalidEmployees)}";
            }
            return result;
        }


    }
}
