using P8D.Domain.Entities;
using System.Collections.Generic;
using System.Dynamic;

namespace P8D.Infrastructure.Common.Constants
{
    public static class RoleConstants
    {
        public const string SYSTEM_ADMIN = "System Administrator";

        public const string MEMBER_1 = "Member 1";

        public const string MEMBER_2 = "Member 2";

        public static List<ApplicationRole> GetListRoles()
        {
            return new List<ApplicationRole>() {
                new ApplicationRole{
                    Name = "System Administrator",
                    NormalizedName = "System Administrator"
                },
                new ApplicationRole {
                    Name = "Member 1",
                    NormalizedName = "Normal Member 001"
                },
                new ApplicationRole{
                    Name = "Member 2",
                    NormalizedName = "Normal Member 002"
                }
            };
        }
    }
}
