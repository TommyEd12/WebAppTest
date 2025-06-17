using System;
using System.Collections.Generic;

public partial class Brand
{
    /// <summary>
    /// Класс для брендов автомобилей
    /// </summary> <summary>
    /// 
    /// </summary>
    /// <value></value>
    public string Title { get; set; } = null!;

    public string FullTitle { get; set; } = null!;

    public string Country { get; set; } = null!;

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}
