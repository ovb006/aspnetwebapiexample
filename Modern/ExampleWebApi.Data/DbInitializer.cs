using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleWebApi.Data
{
    public class DbInitializer
    {
        private readonly ModelBuilder _modelBuilder;

        public DbInitializer(ModelBuilder modelBuilder) {
            _modelBuilder = modelBuilder;
        }


        public void Initialize()
        {
            //Now seeding from web api

        }
    }
}
