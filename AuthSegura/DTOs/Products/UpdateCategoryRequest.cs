﻿namespace AuthSegura.DTOs.Products
{
    public class UpdateCategoryRequest
    {
        public required int Id { get; set; }
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}
