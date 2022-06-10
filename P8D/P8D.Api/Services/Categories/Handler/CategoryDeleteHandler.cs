namespace P8D.Api.Services.Categories.Handler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using P8D.Infrastructure.Common.Constants;
    using P8D.Domain.Entities.Contexts;
    using P8D.Infrastructure.Common.Models;
    using P8D.Infrastructure.Caching;
    using P8D.Api.Services.Categories.Request;

    public class CategoryDeleteHandler : IRequestHandler<CategoryDeleteRequest, ResponseModel>
    {
        private readonly AppDbContext _db;
        private readonly ICacheManager _cacheManager;

        /// <summary>
        ///   Initializes a new instance of the <see cref="CategoryDeleteHandler" /> class.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="cacheManager">The cache manager.</param>
        public CategoryDeleteHandler(AppDbContext db, ICacheManager cacheManager)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        }

        public async Task<ResponseModel> Handle(CategoryDeleteRequest request, CancellationToken cancellationToken)
        {
            var Category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (Category == null)
            {
                return new ResponseModel()
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Message = AppMessageConstants.CATEGORY_DELETED_SUCCESSFULLY
                };
            }

            _db.Categories.Remove(Category);
            await _db.SaveChangesAsync(cancellationToken);

            // remove cache after Category has been deleted successfully
            var cacheKey = $"CATEGORY_{request.Id}";
            _cacheManager.Remove(cacheKey);


            return new ResponseModel()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Message = AppMessageConstants.CATEGORY_DELETED_SUCCESSFULLY
            };
        }
    }
}
