namespace E_commerce.Models
{
    public class ProductFilterRequest
    {
        public string? Category { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public string? Color { get; set; }
        public int? NumberOfItemAvaiable { get; set; }
        public string? Name { get; set; }
    }
}
