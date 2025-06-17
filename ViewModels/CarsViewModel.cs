namespace WebAppTest.ViewModels
{
    public class CarsViewModel
    {
        public IEnumerable<Car> Cars { get; set; } = Enumerable.Empty<Car>();
        public IEnumerable<Brand> Brands { get; set; } = Enumerable.Empty<Brand>();
        public string? Error { get; set; } = null!;
    }
}