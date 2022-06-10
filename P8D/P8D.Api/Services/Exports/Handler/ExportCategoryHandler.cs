namespace P8D.Api.Services.Exports.Handler
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using P8D.Domain.Entities.Contexts;
    using P8D.Infrastructure.Common.Models;
    using AutoMapper.QueryableExtensions;
    using P8D.Infrastructure.Mvc.Utilities;
    using System.IO;
    using P8D.Api.Services.Exports.Request;
    using P8D.Api.Services.Categories.Response;

    public class ExportCategoryHandler : IRequestHandler<ExportCategoryRequest, ResponseModel>
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        /// <summary>
        ///   Initializes a new instance of the <see cref="CategoryPageListHandler" /> class.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="mapper">The auto mapper configuration.</param>
        public ExportCategoryHandler(
            AppDbContext db,
            IMapper mapper)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ResponseModel> Handle(ExportCategoryRequest request, CancellationToken cancellationToken)
        {
            var list = await _db.Categories.Where(
                x => (string.IsNullOrEmpty(request.Query))
                    || (x.Name.Contains(request.Query)))
                .ProjectTo<CategoryResponse>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            MemoryStream stream;

            if (request.ExportType == ExportType.All)
            {
                stream = ExportExcelUtilities.ExportExcel<CategoryResponse>(list);
            }
            else
            {
                var viewModelProperties = ReflectionUtilities.GetAllPropertyNamesOfType(typeof(CategoryResponse));
                var sortPropertyName = !string.IsNullOrEmpty(request.SortName) ? request.SortName.ToLower() : string.Empty;
                string matchedPropertyName = viewModelProperties.FirstOrDefault(x => x.ToLower() == sortPropertyName);

                if (string.IsNullOrEmpty(matchedPropertyName))
                {
                    matchedPropertyName = "Name";
                }

                var type = typeof(CategoryResponse);
                var sortProperty = type.GetProperty(matchedPropertyName);

                list = request.IsDesc ? list.OrderByDescending(x => sortProperty.GetValue(x, null)).ToList() : list.OrderBy(x => sortProperty.GetValue(x, null)).ToList();

                var pageList = new PagedList<CategoryResponse>(list, request.Offset ?? CommonConstants.Config.DEFAULT_SKIP, request.Limit ?? CommonConstants.Config.DEFAULT_TAKE);

                stream = ExportExcelUtilities.ExportExcel<CategoryResponse>(pageList.Sources.ToList());
            }

            return new ResponseModel()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Data = stream
            };
        }
    }
}
