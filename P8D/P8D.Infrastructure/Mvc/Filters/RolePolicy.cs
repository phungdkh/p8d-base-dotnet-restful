namespace P8D.Infrastructure.Mvc.Filters
{
    public class Policy
    {
        public static readonly string[] Policies =
        {
            ADMIN_ACCESS, BASIC_ACCESS
        };

        public const string ADMIN_ACCESS = "AdminAccess";
        public const string BASIC_ACCESS = "BasicAccess";
    }
}
