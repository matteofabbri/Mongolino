using System;
using Mongolino;

namespace Examples
{
    public class ApplicationUser : DBObject<ApplicationUser>
    {
        [FullTextIndex]
        public string Name { get; set; }
        public string MiddleName { get; set; }
        public string FamilyName { get; set; }

        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string SecurityStamp { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string AuthenticatorKey { get; set; }
        public string PasswordHash { get; set; }
    }
}
