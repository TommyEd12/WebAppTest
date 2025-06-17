namespace WebAppTest.ViewModels
{
    public class AccidentsViewModel
    {
        public IEnumerable<Accident> Accidents { get; set; } = Enumerable.Empty<Accident>();
        public string? Error { get; set; } = null!;
    }
}
