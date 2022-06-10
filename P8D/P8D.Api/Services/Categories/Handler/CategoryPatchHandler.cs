namespace P8D.Api.Services.Categories.Handler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json.Linq;
    using P8D.Infrastructure.Common.Constants;
    using P8D.Domain.Entities.Contexts;
    using P8D.Infrastructure.Common.Models;
    using P8D.Domain.Entities;
    using P8D.Infrastructure.Mvc.Utilities;
    using P8D.Infrastructure.Caching;
    using P8D.Api.Services.Categories.Request;
    using P8D.Api.Services.Categories.Response;

    public class CategoryPatchHandler : IRequestHandler<CategoryPatchRequest, ResponseModel>
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICacheManager _cacheManager;

        private readonly List<string> patchAllowedFields = ReflectionUtilities.GetAllPropertyNamesOfType(typeof(CategoryEditRequest));

        /// <summary>
        ///   Initializes a new instance of the <see cref="CategoryPatchHandler" /> class.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="mapper">The auto mapper configuration.</param>
        public CategoryPatchHandler(
            AppDbContext db,
            IMapper mapper,
            ICacheManager cacheManager)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        }

        public async Task<ResponseModel> Handle(CategoryPatchRequest request, CancellationToken cancellationToken)
        {
            var packageObj = request.CategoryPatchModel;

            if (!validState(packageObj))
            {
                return new ResponseModel()
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Message = AppMessageConstants.CATEGORY_PATCH_UPDATE_NO_FIELD
                };
            }

            var category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (category == null)
            {
                return new ResponseModel()
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    Message = AppMessageConstants.CATEGORY_NOT_FOUND
                };
            }

            foreach (var obj in packageObj)
            {
                var key = patchAllowedFields.SingleOrDefault(_ => _.Equals(obj.Key, StringComparison.InvariantCultureIgnoreCase));
                if (key == null)
                {
                    return new ResponseModel()
                    {
                        StatusCode = System.Net.HttpStatusCode.Forbidden,
                        Message = AppMessageConstants.CATEGORY_PATCH_UPDATE_NOT_MATCH_FIELD
                    };
                }

                var jValue = obj.Value;
                if (jValue == null) continue;

                var propertyName = key;
                var targetType = typeof(Category);
                var myPropInfo = targetType.GetProperty(propertyName);

                dynamic newValue = "";
                if (myPropInfo.PropertyType == typeof(decimal) || myPropInfo.PropertyType == typeof(decimal?))
                    newValue = jValue.Value<decimal?>();
                else if (myPropInfo.PropertyType == typeof(int) || myPropInfo.PropertyType == typeof(int?))
                    newValue = jValue.Value<int?>();
                else if (myPropInfo.PropertyType == typeof(string))
                    newValue = jValue.Value<string>()?.Trim();

                if (targetType == typeof(Category))
                    myPropInfo.SetValue(category, newValue);
            }

            _db.Categories.Update(category);
            await _db.SaveChangesAsync(cancellationToken);

            // remove cache after Category has been patched successfully
            var cacheKey = $"CATEGORY_{request.Id}";
            _cacheManager.Remove(cacheKey);

            return new ResponseModel()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Message = AppMessageConstants.CATEGORY_UPDATED_SUCCESSFULLY,
                Data = _mapper.Map<CategoryResponse>(category)
            };
        }

        private bool validState(JObject jObj)
        {
            return jObj != null && jObj.Properties().Count() > 0;
        }
    }
}
