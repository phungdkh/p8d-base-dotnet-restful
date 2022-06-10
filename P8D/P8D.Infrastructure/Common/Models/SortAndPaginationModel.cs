namespace P8D.Infrastructure.Common.Models
{
    public class SortAndPaginationModel
    {
        public string PropToOrder { get; set; }

        public bool IsDesc { get; set; }

        public int? Offset { get; set; }

        public int? Limit { get; set; }

        public string DefaulPropNameForOrder { get; set; }
    }
}
