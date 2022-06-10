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
    using System.Linq;
    using P8D.Domain.Entities;

    public class CategoryEditHandler : IRequestHandler<CategoryEditRequest, ResponseModel>
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICacheManager _cacheManager;

        /// <summary>
        ///   Initializes a new instance of the <see cref="CategoryEditHandler" /> class.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="mapper">The auto mapper configuration.</param>
        public CategoryEditHandler(
            AppDbContext db,
            IMapper mapper,
            ICacheManager cacheManager)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        }

        public async Task<ResponseModel> Handle(CategoryEditRequest request, CancellationToken cancellationToken)
        {
            var category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (category == null)
            {
                return new ResponseModel()
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    Message = AppMessageConstants.CATEGORY_NOT_FOUND
                };
            }

            _mapper.Map(request, category);

            var removeProducts = _db.ProductInCategories.Where(x => !request.ProductIds.Contains(x.ProductId) && x.CategoryId == category.Id);
            var addProductIds = request.ProductIds.Where(productId => !removeProducts.Select(x => x.ProductId).Contains(productId)).ToList();

            _db.ProductInCategories.RemoveRange(removeProducts);

            addProductIds.ForEach(productId =>
            {
                _db.ProductInCategories.Add(new ProductInCategory()
                {
                    CategoryId = category.Id,
                    ProductId = productId
                });
            });

            await _db.SaveChangesAsync(cancellationToken);

            // remove cache after Category has been updated successfully
            var cacheKey = $"CATEGORY_{request.Id}";
            _cacheManager.Remove(cacheKey);

            return new ResponseModel()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Message = AppMessageConstants.CATEGORY_UPDATED_SUCCESSFULLY
            };
        }
    }
}
