using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleWebApi.Data.Misc
{
    public class BaseResultObject
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = "";
    }
}
