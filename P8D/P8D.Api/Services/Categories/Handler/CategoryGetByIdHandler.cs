namespace P8D.Api.Services.Categories.Handler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using P8D.Infrastructure.Common.Constants;
    using P8D.Domain.Entities.Contexts;
    using P8D.Infrastructure.Common.Models;
    using P8D.Infrastructure.Caching;
    using P8D.Api.Services.Categories.Request;
    using P8D.Api.Services.Categories.Response;

    public class CategoryGetByIdHandler : IRequestHandler<CategoryGetByIdRequest, ResponseModel>
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICacheManager _cacheManager;

        /// <summary>
        ///   Initializes a new instance of the <see cref="CategoryGetByIdHandler" /> class.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="mapper">The auto mapper configuration.</param>
        /// <param name="cacheManager">The cache manager.</param>
        public CategoryGetByIdHandler(
            AppDbContext db,
            IMapper mapper, ICacheManager cacheManager)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        }

        public async Task<ResponseModel> Handle(CategoryGetByIdRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"CATEGORY_{request.Id}";

            var Category = await _cacheManager.GetAndSetAsync(cacheKey, 1, () =>
            {
                return _db.Categories
                .FirstOrDefaultAsync(x => x.Id == request.Id);
            });

            if (Category == null)
            {
                return new ResponseModel()
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    Message = AppMessageConstants.CATEGORY_NOT_FOUND
                };
            }
            return new ResponseModel()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Data = _mapper.Map<CategoryResponse>(Category)
            };
        }
    }
}
