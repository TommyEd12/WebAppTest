public partial class Accident
{   
    /// <summary>
    /// Класс для автомобильных аварий
    /// </summary> <summary>
    /// 
    /// </summary>
    /// <value></value>
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public DateOnly Date { get; set; }

    public string Departureaddress { get; set; } = null!; 

    public string Destinationaddress { get; set; } = null!; 

    public decimal? Sum { get; set; } 

    public string Login { get; set; } = null!; 

    public virtual Car NumberNavigation { get; set; } = null!;

    public virtual Owner LoginNavigation { get; set; } = null!;
}