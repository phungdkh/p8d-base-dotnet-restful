namespace P8D.Api.Services.Categories.Request
{
    using System;
    using System.Text.Json.Serialization;
    using MediatR;
    using Newtonsoft.Json.Linq;
    using P8D.Infrastructure.Common.Models;

    public class CategoryPatchRequest : IRequest<ResponseModel>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public JObject CategoryPatchModel { get; set; }
    }
}
