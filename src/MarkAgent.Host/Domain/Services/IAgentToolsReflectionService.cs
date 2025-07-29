using System.ComponentModel;

namespace MarkAgent.Host.Domain.Services;

/// <summary>
/// AgentTools反射服务接口
/// </summary>
public interface IAgentToolsReflectionService
{
    /// <summary>
    /// 获取所有MCP工具方法的详细信息
    /// </summary>
    /// <returns>工具方法信息列表</returns>
    Task<List<McpToolInfo>> GetAllMcpToolsAsync();

    /// <summary>
    /// 根据方法名获取特定工具的详细信息
    /// </summary>
    /// <param name="methodName">方法名</param>
    /// <returns>工具方法信息</returns>
    Task<McpToolInfo?> GetMcpToolByNameAsync(string methodName);
}

/// <summary>
/// MCP工具信息
/// </summary>
public class McpToolInfo
{
    /// <summary>
    /// 方法名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 工具描述（从Description特性获取）
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 方法参数信息
    /// </summary>
    public List<ParameterInfo> Parameters { get; set; } = new();

    /// <summary>
    /// 返回类型名称
    /// </summary>
    public string ReturnType { get; set; } = string.Empty;

    /// <summary>
    /// 返回类型的详细信息（如果是复杂类型）
    /// </summary>
    public TypeInfo? ReturnTypeInfo { get; set; }

    /// <summary>
    /// 是否为异步方法
    /// </summary>
    public bool IsAsync { get; set; }

    /// <summary>
    /// 工具类别（基于方法名称或描述推断）
    /// </summary>
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// 参数信息
/// </summary>
public class ParameterInfo
{
    /// <summary>
    /// 参数名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 参数类型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 参数描述（从Description特性获取）
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 是否为必需参数
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 默认值（如果有）
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// 是否为复杂类型
    /// </summary>
    public bool IsComplexType { get; set; }

    /// <summary>
    /// 复杂类型的详细信息
    /// </summary>
    public TypeInfo? TypeInfo { get; set; }
}

/// <summary>
/// 类型信息
/// </summary>
public class TypeInfo
{
    /// <summary>
    /// 类型名称
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// 类型的完整名称（包含命名空间）
    /// </summary>
    public string FullTypeName { get; set; } = string.Empty;

    /// <summary>
    /// 是否为枚举类型
    /// </summary>
    public bool IsEnum { get; set; }

    /// <summary>
    /// 枚举值（如果是枚举类型）
    /// </summary>
    public List<EnumValueInfo>? EnumValues { get; set; }

    /// <summary>
    /// 属性信息（如果是复杂对象）
    /// </summary>
    public List<PropertyInfo>? Properties { get; set; }

    /// <summary>
    /// 是否为数组类型
    /// </summary>
    public bool IsArray { get; set; }

    /// <summary>
    /// 数组元素类型（如果是数组）
    /// </summary>
    public string? ElementType { get; set; }
}

/// <summary>
/// 枚举值信息
/// </summary>
public class EnumValueInfo
{
    /// <summary>
    /// 枚举值名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 枚举值
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// 枚举值描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 属性信息
/// </summary>
public class PropertyInfo
{
    /// <summary>
    /// 属性名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 属性类型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 属性描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 是否为必需属性
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 是否可为空
    /// </summary>
    public bool IsNullable { get; set; }
}