using System;
using System.Collections.Generic;

public partial class Owner
{
    /// <summary>
    /// Классс для владельцев
    /// </summary> <summary>
    /// 
    /// </summary>
    /// <value></value>
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string SecondName { get; set; }

    public string? Surname { get; set; }
    public string? Login { get; set; }


    public virtual Car NumberNavigation { get; set; } = null!;
}
