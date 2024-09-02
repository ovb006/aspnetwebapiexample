using ExampleWebApi.Data.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleWebApi.Data.Import
{
    public class ImportResult : BaseResultObject
    {
        public int CompaniesImported { get; set; } = 0;
        public int EmployeesImported { get; set; } = 0;

        public bool InternalError = false;

        public ImportResult() : base()
        {

        }
    }
}
