using System.ComponentModel.DataAnnotations;

namespace RazorPagesUser.Models;

public class User
{
    public int Id { get; set; }
    public string? Name { get; set; }
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }
}   