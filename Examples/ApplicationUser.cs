using System;
using Mongolino;
using Mongolino.Attributes;

namespace Examples
{
    public class ApplicationUser : DBObject<ApplicationUser>
    {
        public string Name { get; set; }

        public string MiddleName { get; set; }

        [FullTextIndex]
        public string FamilyName { get; set; }

        [AscendingIndex]
        public string UserName { get; set; }

        public int LogInNumber { get; set; }

        public DateTime LastLogin { get; set; }
    }
}
