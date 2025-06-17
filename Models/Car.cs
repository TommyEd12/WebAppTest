using System;
using System.Collections.Generic;

public partial class Car
{
    public string Number { get; set; } = null!;

    public string Brand { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string Color { get; set; } = null!;

    public virtual Brand BrandNavigation { get; set; } = null!;
}
