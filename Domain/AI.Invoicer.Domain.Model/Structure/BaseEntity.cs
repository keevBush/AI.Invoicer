using SQLite;

namespace AI.Invoicer.Domain.Model.Structure;

public abstract class BaseEntity
{
    [PrimaryKey]
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
