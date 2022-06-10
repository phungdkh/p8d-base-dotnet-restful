namespace P8D.Api.Services.Categories.Request
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using MediatR;
    using P8D.Infrastructure.Common.Models;

    public class CategoryCreateRequest : IRequest<ResponseModel>
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        public List<Guid> ProductIds { get; set; }
    }
}
