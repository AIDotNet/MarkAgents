using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// 系统MCP工具定义实体 - 基于MCP协议规范
/// </summary>
public class SystemMcpTool : BaseEntity<Guid>
{
    /// <summary>
    /// 工具名称（唯一标识）
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ToolName { get; set; } = string.Empty;
    
    /// <summary>
    /// 工具显示名称
    /// </summary>
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// 工具描述
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// MCP工具类型：Tool, Resource, Prompt
    /// </summary>
    [Required]
    public McpToolType ToolType { get; set; } = McpToolType.Tool;
    
    /// <summary>
    /// 工具分类（基于MCP标准分类）
    /// </summary>
    [StringLength(50)]
    public string? Category { get; set; }
    
    /// <summary>
    /// 工具版本
    /// </summary>
    [StringLength(50)]
    public string? Version { get; set; }
    
    /// <summary>
    /// 工具作者
    /// </summary>
    [StringLength(100)]
    public string? Author { get; set; }
    
    /// <summary>
    /// 工具图标URL或路径
    /// </summary>
    [StringLength(500)]
    public string? IconUrl { get; set; }
    
    /// <summary>
    /// 是否为系统内置工具
    /// </summary>
    public bool IsBuiltIn { get; set; } = false;
    
    /// <summary>
    /// 是否启用（系统管理员控制）
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 输入参数Schema（JSON Schema格式）
    /// </summary>
    public string? InputSchema { get; set; }
    
    /// <summary>
    /// 输出Schema（JSON Schema格式）
    /// </summary>
    public string? OutputSchema { get; set; }
    
    /// <summary>
    /// 工具实现代码或配置（JSON格式）
    /// </summary>
    public string? Implementation { get; set; }
    
    /// <summary>
    /// 默认配置（JSON格式）
    /// </summary>
    public string? DefaultConfiguration { get; set; }
    
    /// <summary>
    /// 必需的权限列表（JSON格式）
    /// </summary>
    public string? RequiredPermissions { get; set; }
    
    /// <summary>
    /// 工具文档URL
    /// </summary>
    [StringLength(500)]
    public string? DocumentationUrl { get; set; }
    
    /// <summary>
    /// 仓库或源码URL
    /// </summary>
    [StringLength(500)]
    public string? RepositoryUrl { get; set; }
    
    /// <summary>
    /// 标签（用于搜索和分类）
    /// </summary>
    public List<string>? Tags { get; set; } = new List<string>();
    
    /// <summary>
    /// 排序权重
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    /// <summary>
    /// 总使用次数
    /// </summary>
    public long TotalUsageCount { get; set; } = 0;
    
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime? LastUpdateTime { get; set; }
    
    /// <summary>
    /// 安全级别
    /// </summary>
    public McpToolSecurityLevel SecurityLevel { get; set; } = McpToolSecurityLevel.Safe;
    
    /// <summary>
    /// 执行超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// 是否支持批量执行
    /// </summary>
    public bool SupportsBatch { get; set; } = false;
    
    /// <summary>
    /// 依赖的其他工具
    /// </summary>
    public List<string>? Dependencies { get; set; } = new List<string>();
}

/// <summary>
/// MCP工具类型枚举（基于MCP协议规范）
/// </summary>
public enum McpToolType
{
    /// <summary>
    /// 可执行工具 - 执行特定操作并返回结果
    /// </summary>
    Tool = 1,
    
    /// <summary>
    /// 资源 - 提供静态或动态数据
    /// </summary>
    Resource = 2,
    
    /// <summary>
    /// 提示模板 - 结构化的AI提示模板
    /// </summary>
    Prompt = 3
}

/// <summary>
/// MCP工具分类枚举（基于MCP标准分类）
/// </summary>
public enum McpToolCategory
{
    /// <summary>
    /// 文件操作 - 读写、创建、删除文件
    /// </summary>
    FileOperations,
    
    /// <summary>
    /// 网络请求 - HTTP、API调用、网页抓取
    /// </summary>
    NetworkRequests,
    
    /// <summary>
    /// 数据处理 - 数据转换、分析、计算
    /// </summary>
    DataProcessing,
    
    /// <summary>
    /// 文本处理 - 文本分析、格式化、翻译
    /// </summary>
    TextProcessing,
    
    /// <summary>
    /// 图像处理 - 图像编辑、分析、生成
    /// </summary>
    ImageProcessing,
    
    /// <summary>
    /// 代码分析 - 代码审查、重构、生成
    /// </summary>
    CodeAnalysis,
    
    /// <summary>
    /// 系统工具 - 系统信息、进程管理
    /// </summary>
    SystemTools,
    
    /// <summary>
    /// 第三方集成 - 外部服务集成
    /// </summary>
    ThirdPartyIntegration,
    
    /// <summary>
    /// 人工智能 - AI模型调用、提示工程
    /// </summary>
    ArtificialIntelligence,
    
    /// <summary>
    /// 数据库操作 - 查询、更新、管理数据库
    /// </summary>
    DatabaseOperations,
    
    /// <summary>
    /// 其他
    /// </summary>
    Other
}

/// <summary>
/// MCP工具安全级别
/// </summary>
public enum McpToolSecurityLevel
{
    /// <summary>
    /// 安全 - 只读操作，无副作用
    /// </summary>
    Safe = 1,
    
    /// <summary>
    /// 中等 - 有限的写操作
    /// </summary>
    Moderate = 2,
    
    /// <summary>
    /// 危险 - 可能影响系统或数据
    /// </summary>
    Dangerous = 3,
    
    /// <summary>
    /// 受限 - 需要特殊权限
    /// </summary>
    Restricted = 4
} 