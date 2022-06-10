
using MediatR;
using P8D.Infrastructure.Common.Models;

namespace P8D.Api.Services.Exports.Request
{
    public class ExportCategoryRequest : BaseRequestModel, IRequest<ResponseModel>
    {
        public ExportType ExportType { get; set; } = ExportType.All;
    }

    public enum ExportType
    {
        All = 1,
        CurrentPage = 2
    }
}
