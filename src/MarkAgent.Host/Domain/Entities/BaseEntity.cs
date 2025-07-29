using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// 领域实体基类
/// </summary>
public abstract class BaseEntity<TKey> : ICreationAudited, IModificationAudited, ISoftDelete where TKey : notnull
{
    [Key] public TKey Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 软删除标记
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }
}

public interface ICreationAudited
{
    DateTime CreatedAt { get; set; }
}

public interface IModificationAudited
{
    DateTime UpdatedAt { get; set; }
}

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}