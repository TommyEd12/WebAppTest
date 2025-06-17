namespace WebAppTest.ViewModels
{
    public class OwnersViewModel
    {
        public IEnumerable<Owner> Owners { get; set; } = Enumerable.Empty<Owner>();
        public string? Error { get; set; } = null!;
    }
}