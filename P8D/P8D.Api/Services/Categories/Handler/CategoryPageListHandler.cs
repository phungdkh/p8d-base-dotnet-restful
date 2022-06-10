namespace P8D.Api.Services.Categories.Handler
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using MediatR;
    using P8D.Domain.Entities.Contexts;
    using P8D.Infrastructure.Common.Models;
    using P8D.Api.Services.Categories.Request;
    using P8D.Api.Services.Categories.Response;
    using P8D.Infrastructure.Helpes;

    public class CategoryPageListHandler : IRequestHandler<CategoryPageListRequest, ResponseModel>
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        /// <summary>
        ///   Initializes a new instance of the <see cref="CategoryPageListHandler" /> class.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="mapper">The auto mapper configuration.</param>
        public CategoryPageListHandler(
            AppDbContext db,
            IMapper mapper)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ResponseModel> Handle(CategoryPageListRequest request, CancellationToken cancellationToken)
        {
            var list = _db.Categories.Where(
                x => (string.IsNullOrEmpty(request.Query))
                    || (x.Name.Contains(request.Query)))
                .Select(x => new CategoryResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Desciption = x.Description
                });

            var sortAndPaginationModel = new SortAndPaginationModel()
            {
                DefaulPropNameForOrder = "Name",
                PropToOrder = request.SortName,
                IsDesc = request.IsDesc,
                Limit = request.Limit,
                Offset = request.Offset
            };

            var pageList = QueryHelper.SortAndPaginationDynamic(list, sortAndPaginationModel);

            return new ResponseModel()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Data = pageList
            };
        }
    }
}
