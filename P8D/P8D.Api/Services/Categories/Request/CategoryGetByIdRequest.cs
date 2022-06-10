﻿namespace P8D.Api.Services.Categories.Request
{
    using System;
    using MediatR;
    using P8D.Infrastructure.Common.Models;

    public class CategoryGetByIdRequest : IRequest<ResponseModel>
    {
        public Guid Id { get; set; } = Guid.Empty;
    }
}
