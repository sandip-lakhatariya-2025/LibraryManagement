namespace LibraryManagement.Models.Models;

public class EndPointPermission
{
    public int Id { get; set; }
    public string EndpointName { get; set; } = null!;
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanMultipleUpdate { get; set; }
    public bool CanMultipleDelete { get; set; }
}