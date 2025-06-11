using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.Models;

public class Customer
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Email { get; set; } = null!;
    
    public virtual ICollection<CustomerBook> CustomerBooks { get; set; } = new List<CustomerBook>();

}