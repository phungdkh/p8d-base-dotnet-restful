using AutoMapper;
using P8D.Api.Services.Categories.Request;
using P8D.Api.Services.Categories.Response;
using P8D.Domain.Entities;

namespace P8D.Api.Handlers
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryResponse>();
            CreateMap<CategoryCreateRequest, Category>();
            CreateMap<CategoryEditRequest, Category>()
                .ForMember(x => x.Id, opt => opt.Ignore());
        }
    }
}
