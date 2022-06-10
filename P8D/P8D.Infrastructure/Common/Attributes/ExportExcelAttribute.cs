using System;

namespace P8D.Infrastructure.Common.Attributes
{
    public class ExportExcelAttribute : Attribute
    {
        public string DisplayName { get; set; }

        public int Priority { get; set; }
    }
}
