using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Domain.Model.Configuration
{
    public class DatabaseConfiguration
    {
        public const string prefix = "database_config_";

        public string DatabasePath { get; set; }
        public string DatabasePassword { get; set; }
    }
}
