namespace WebAppTest.ViewModels
{
    public class BrandsViewModel
    {
        public IEnumerable<Brand> Brands { get; set; } = Enumerable.Empty<Brand>();
        public string? Error { get; set; } = null!;
    }
}
