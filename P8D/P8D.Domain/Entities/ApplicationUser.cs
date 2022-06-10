using Microsoft.AspNetCore.Identity;
using System;

namespace P8D.Domain.Entities
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string Name { get; set; }

        public string Avatar { get; set; }

        public string Address { get; set; }
    }
}
