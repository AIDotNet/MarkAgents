using System.ComponentModel;
using System.Reflection;
using MarkAgent.Host.Domain.Services;
using MarkAgent.Host.Tools;
using ModelContextProtocol.Server;

namespace MarkAgent.Host.Infrastructure.Services;

/// <summary>
/// AgentTools反射服务实现
/// </summary>
public class AgentToolsReflectionService : IAgentToolsReflectionService
{
    private readonly ILogger<AgentToolsReflectionService> _logger;
    private List<McpToolInfo>? _cachedTools;

    public AgentToolsReflectionService(ILogger<AgentToolsReflectionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 获取所有MCP工具方法的详细信息
    /// </summary>
    public async Task<List<McpToolInfo>> GetAllMcpToolsAsync()
    {
        if (_cachedTools != null)
        {
            return _cachedTools;
        }

        var tools = new List<McpToolInfo>();

        try
        {
            var agentToolsType = typeof(AgentTools);
            var methods = agentToolsType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in methods)
            {
                // 检查是否有McpServerTool特性
                var mcpToolAttribute = method.GetCustomAttribute<McpServerToolAttribute>();
                if (mcpToolAttribute == null)
                    continue;

                var toolInfo = await CreateMcpToolInfoAsync(method);
                if (toolInfo != null)
                {
                    tools.Add(toolInfo);
                }
            }

            _cachedTools = tools;
            _logger.LogInformation("Successfully reflected {Count} MCP tools", tools.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reflecting MCP tools");
            throw;
        }

        return tools;
    }

    /// <summary>
    /// 根据方法名获取特定工具的详细信息
    /// </summary>
    public async Task<McpToolInfo?> GetMcpToolByNameAsync(string methodName)
    {
        var allTools = await GetAllMcpToolsAsync();
        return allTools.FirstOrDefault(t => t.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 创建MCP工具信息
    /// </summary>
    private async Task<McpToolInfo?> CreateMcpToolInfoAsync(MethodInfo method)
    {
        try
        {
            var toolInfo = new McpToolInfo
            {
                Name = method.Name,
                IsAsync = method.ReturnType.IsGenericType && 
                         method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)
            };

            // 获取描述信息
            var descriptionAttribute = method.GetCustomAttribute<DescriptionAttribute>();
            toolInfo.Description = descriptionAttribute?.Description ?? "";

            // 确定返回类型
            var returnType = method.ReturnType;
            if (toolInfo.IsAsync && returnType.IsGenericType)
            {
                returnType = returnType.GetGenericArguments()[0];
            }
            
            toolInfo.ReturnType = GetFriendlyTypeName(returnType);
            toolInfo.ReturnTypeInfo = await CreateTypeInfoAsync(returnType);

            // 获取参数信息
            var parameters = method.GetParameters();
            foreach (var param in parameters)
            {
                // 跳过IMcpServer参数，这是框架注入的参数
                if (param.ParameterType.Name == "IMcpServer")
                    continue;

                var paramInfo = await CreateParameterInfoAsync(param);
                if (paramInfo != null)
                {
                    toolInfo.Parameters.Add(paramInfo);
                }
            }

            // 推断工具类别
            toolInfo.Category = InferToolCategory(method.Name, toolInfo.Description);

            return toolInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tool info for method {MethodName}", method.Name);
            return null;
        }
    }

    /// <summary>
    /// 创建参数信息
    /// </summary>
    private async Task<Domain.Services.ParameterInfo?> CreateParameterInfoAsync(System.Reflection.ParameterInfo param)
    {
        try
        {
            var paramInfo = new Domain.Services.ParameterInfo
            {
                Name = param.Name ?? "",
                Type = GetFriendlyTypeName(param.ParameterType),
                IsRequired = !param.HasDefaultValue && !IsNullableType(param.ParameterType),
                DefaultValue = param.HasDefaultValue ? param.DefaultValue : null,
                IsComplexType = IsComplexType(param.ParameterType)
            };

            // 获取参数描述
            var descriptionAttribute = param.GetCustomAttribute<DescriptionAttribute>();
            paramInfo.Description = descriptionAttribute?.Description ?? "";

            // 如果是复杂类型，创建类型信息
            if (paramInfo.IsComplexType)
            {
                paramInfo.TypeInfo = await CreateTypeInfoAsync(param.ParameterType);
            }

            return paramInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating parameter info for {ParameterName}", param.Name);
            return null;
        }
    }

    /// <summary>
    /// 创建类型信息
    /// </summary>
    private async Task<Domain.Services.TypeInfo?> CreateTypeInfoAsync(Type type)
    {
        try
        {
            var typeInfo = new Domain.Services.TypeInfo
            {
                TypeName = GetFriendlyTypeName(type),
                FullTypeName = type.FullName ?? type.Name,
                IsEnum = type.IsEnum,
                IsArray = type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            };

            if (typeInfo.IsArray)
            {
                var elementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
                typeInfo.ElementType = elementType != null ? GetFriendlyTypeName(elementType) : "unknown";
            }

            if (typeInfo.IsEnum)
            {
                typeInfo.EnumValues = GetEnumValues(type);
            }
            else if (IsComplexType(type) && !typeInfo.IsArray)
            {
                typeInfo.Properties = GetProperties(type);
            }

            return typeInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating type info for {TypeName}", type.Name);
            return null;
        }
    }

    /// <summary>
    /// 获取枚举值
    /// </summary>
    private List<EnumValueInfo> GetEnumValues(Type enumType)
    {
        var enumValues = new List<EnumValueInfo>();
        
        foreach (var enumValue in Enum.GetValues(enumType))
        {
            var enumValueInfo = new EnumValueInfo
            {
                Name = enumValue.ToString() ?? "",
                Value = (int)enumValue,
                Description = GetEnumDescription(enumType, enumValue.ToString() ?? "")
            };
            enumValues.Add(enumValueInfo);
        }

        return enumValues;
    }

    /// <summary>
    /// 获取枚举描述
    /// </summary>
    private string GetEnumDescription(Type enumType, string enumValueName)
    {
        var field = enumType.GetField(enumValueName);
        if (field == null) return "";

        var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();
        return descriptionAttribute?.Description ?? "";
    }

    /// <summary>
    /// 获取属性信息
    /// </summary>
    private List<Domain.Services.PropertyInfo> GetProperties(Type type)
    {
        var properties = new List<Domain.Services.PropertyInfo>();
        
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propInfo = new Domain.Services.PropertyInfo
            {
                Name = prop.Name,
                Type = GetFriendlyTypeName(prop.PropertyType),
                IsRequired = !IsNullableType(prop.PropertyType),
                IsNullable = IsNullableType(prop.PropertyType)
            };

            // 获取属性描述
            var descriptionAttribute = prop.GetCustomAttribute<DescriptionAttribute>();
            propInfo.Description = descriptionAttribute?.Description ?? "";

            properties.Add(propInfo);
        }

        return properties;
    }

    /// <summary>
    /// 获取友好的类型名称
    /// </summary>
    private string GetFriendlyTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "long";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(double)) return "double";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(DateTime)) return "DateTime";
        if (type == typeof(object)) return "object";

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            return elementType != null ? $"{GetFriendlyTypeName(elementType)}[]" : "array";
        }

        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(List<>))
            {
                var elementType = type.GetGenericArguments()[0];
                return $"List<{GetFriendlyTypeName(elementType)}>";
            }
            
            if (genericTypeDef == typeof(Dictionary<,>))
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];
                return $"Dictionary<{GetFriendlyTypeName(keyType)}, {GetFriendlyTypeName(valueType)}>";
            }

            if (genericTypeDef == typeof(Nullable<>))
            {
                var underlyingType = type.GetGenericArguments()[0];
                return $"{GetFriendlyTypeName(underlyingType)}?";
            }
        }

        return type.Name;
    }

    /// <summary>
    /// 判断是否为复杂类型
    /// </summary>
    private bool IsComplexType(Type type)
    {
        if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || 
            type == typeof(decimal) || type.IsEnum)
        {
            return false;
        }

        if (type.IsArray)
        {
            return true;
        }

        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(List<>) || genericTypeDef == typeof(Dictionary<,>))
            {
                return true;
            }
        }

        return type.IsClass && type != typeof(object);
    }

    /// <summary>
    /// 判断是否为可空类型
    /// </summary>
    private bool IsNullableType(Type type)
    {
        return !type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
    }

    /// <summary>
    /// 推断工具类别
    /// </summary>
    private string InferToolCategory(string methodName, string description)
    {
        var name = methodName.ToLower();
        var desc = description.ToLower();

        if (name.Contains("think") || desc.Contains("think") || desc.Contains("思考"))
            return "思维工具";
        
        if (name.Contains("todo") || desc.Contains("todo") || desc.Contains("任务"))
            return "任务管理";
        
        if (name.Contains("mental") || name.Contains("model") || desc.Contains("model"))
            return "思维模型";

        if (name.Contains("data") || desc.Contains("data") || desc.Contains("数据"))
            return "数据处理";

        if (name.Contains("analysis") || desc.Contains("analysis") || desc.Contains("分析"))
            return "分析工具";

        return "通用工具";
    }
}