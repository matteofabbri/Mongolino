using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maddalena.Mongo
{
    internal class ConfigurationSection
    {
        public string ConnectionString { get; set; }

        public string Database { get; set; }

        public string Collection { get; set; }
    }
}
