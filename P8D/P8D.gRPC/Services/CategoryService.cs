using Grpc.Core;
using Microsoft.Extensions.Logging;
using P8D.Domain.Entities.Contexts;
using P8D.Infrastructure.Common.Models;
using P8D.Infrastructure.Helpes;
using System.Linq;
using System.Threading.Tasks;

namespace P8D.gRPC
{
    public class CategoryService : Category.CategoryBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(AppDbContext db, ILogger<CategoryService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public override Task<ResponseModel> CategoryPagedList(CategoryPageListRequest request, ServerCallContext context)
        {
            var list = _db.Categories.Where(
                x => (string.IsNullOrEmpty(request.Query))
                    || (x.Name.Contains(request.Query)))
                .Select(x => new CategoryResponse()
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Description = x.Description
                });

            var sortAndPaginationModel = new SortAndPaginationModel()
            {
                DefaulPropNameForOrder = "Name",
                PropToOrder = request.Sortname,
                IsDesc = request.Isdesc,
                Limit = request.Limit,
                Offset = request.Offset
            };

            var pageList = QueryHelper.SortAndPaginationDynamic<CategoryResponse>(list, sortAndPaginationModel);

            var responseModel = new ResponseModel
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK
            };
            responseModel.Data.AddRange(pageList.Sources);
            return Task.FromResult(responseModel);
        }
    }
}
