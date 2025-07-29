using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Apis.Dtos;

/// <summary>
/// 发送验证码请求DTO
/// </summary>
public class SendVerificationCodeRequest
{
    /// <summary>
    /// 邮箱地址
    /// </summary>
    [Required(ErrorMessage = "邮箱地址不能为空")]
    [EmailAddress(ErrorMessage = "邮箱地址格式不正确")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// 验证码类型
    /// </summary>
    [Required(ErrorMessage = "验证码类型不能为空")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// 验证验证码请求DTO
/// </summary>
public class VerifyCodeRequest
{
    /// <summary>
    /// 邮箱地址
    /// </summary>
    [Required(ErrorMessage = "邮箱地址不能为空")]
    [EmailAddress(ErrorMessage = "邮箱地址格式不正确")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// 验证码
    /// </summary>
    [Required(ErrorMessage = "验证码不能为空")]
    [StringLength(10, MinimumLength = 4, ErrorMessage = "验证码长度不正确")]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 验证码类型
    /// </summary>
    [Required(ErrorMessage = "验证码类型不能为空")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// 忘记密码请求DTO
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// 邮箱地址
    /// </summary>
    [Required(ErrorMessage = "邮箱地址不能为空")]
    [EmailAddress(ErrorMessage = "邮箱地址格式不正确")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// 重置密码请求DTO
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// 重置令牌
    /// </summary>
    [Required(ErrorMessage = "重置令牌不能为空")]
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// 新密码
    /// </summary>
    [Required(ErrorMessage = "新密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100字符之间")]
    public string NewPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// 确认密码
    /// </summary>
    [Required(ErrorMessage = "确认密码不能为空")]
    [Compare("NewPassword", ErrorMessage = "两次输入的密码不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// 通用响应DTO
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 数据
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// 错误代码
    /// </summary>
    public string? ErrorCode { get; set; }

    public static ApiResponse<T> SuccessResult(T? data = default, string message = "操作成功")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResult(string message, string? errorCode = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode
        };
    }
} 