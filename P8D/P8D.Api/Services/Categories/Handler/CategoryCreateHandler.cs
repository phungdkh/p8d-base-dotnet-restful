namespace P8D.Api.Services.Categories.Handler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using MediatR;
    using P8D.Domain.Entities;
    using P8D.Domain.Entities.Contexts;
    using P8D.Infrastructure.Common.Models;
    using P8D.Infrastructure.Common.Constants;
    using Microsoft.AspNetCore.Http;
    using System.Security.Claims;
    using P8D.Api.Services.Categories.Request;

    public class CategoryCreateHandler : IRequestHandler<CategoryCreateRequest, ResponseModel>
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        ///   Initializes a new instance of the <see cref="CategoryCreateHandler" /> class.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="mapper">The auto mapper configuration.</param>
        public CategoryCreateHandler(
            AppDbContext db,
            IMapper mapper,
            IHttpContextAccessor contextAccessor)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public async Task<ResponseModel> Handle(CategoryCreateRequest request, CancellationToken cancellationToken)
        {
            var category = _mapper.Map<Category>(request);

            // current user id logged in
            var userId = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // assign current user id logged in to created by
            category.CreatedBy = new Guid(userId);

            request.ProductIds.ForEach(productId =>
            {
                _db.ProductInCategories.Add(new ProductInCategory()
                {
                    CategoryId = category.Id,
                    ProductId = productId
                });
            });

            _db.Categories.Add(category);

            await _db.SaveChangesAsync(cancellationToken);

            return new ResponseModel()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Message = AppMessageConstants.CATEGORY_CREATED_SUCCESSFULLY
            };
        }
    }
}
