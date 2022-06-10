using P8D.Infrastructure.Common.Attributes;
using System;

namespace P8D.Api.Services.Categories.Response
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }

        [ExportExcel(DisplayName = "Name", Priority = 1)]
        public string Name { get; set; } = string.Empty;

        [ExportExcel(DisplayName = "Email", Priority = 2)]
        public string Desciption { get; set; } = string.Empty;
    }
}
