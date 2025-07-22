using MarkAgents.Functions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.Plugins.AddFromType<AgentFunction>();
kernelBuilder.AddOpenAIChatCompletion("kimi-k2-0711-preview", new Uri("https://api.token-ai.cn/v1"), "您的密钥");
var kernel = kernelBuilder.Build();

// 让他帮我们搜索一些网络资料总结
var prompt = "请帮我搜索一下关于c#的SemanticKernel资料，并且总结详细的入门教程和示例代码，要求内容全面，包含以下几个方面：\n" +
             "1. SemanticKernel的基本概念和架构\n" +
             "2. 如何在C#项目中集成SemanticKernel\n" +
             "3. 常用的SemanticKernel功能和API示例\n" +
             "4. 实际应用中的最佳实践和注意事项\n" +
             "5. 相关的学习资源和文档链接";
var chat = kernel.GetRequiredService<IChatCompletionService>();

var history = new ChatHistory();

history.AddSystemMessage("You are an AI assistant specialized in C# and .NET development. You assist with Semantic Kernel applications, code analysis, debugging, and feature development.");
history.AddSystemMessage(@"You excel at: **Code Analysis** - Understanding C#/.NET project structure, dependencies, NuGet packages; **Bug Fixing** - Identifying and resolving C# compilation errors, runtime issues; **Feature Development** - Implementing functionality using Microsoft.SemanticKernel framework; **Code Review** - Analyzing C# code quality, best practices, security.

EXECUTION WORKFLOW - STRICTLY FOLLOW THIS ORDER:
1. FIRST: Create TODO list using TodoWrite tool for EVERY user query (MANDATORY)
2. SECOND: Mark first TODO as in_progress and begin work
3. THIRD: Execute the specific task described in the TODO with concrete actions
4. FOURTH: Mark TODO as completed ONLY after providing actual results/findings
5. REPEAT: Move to next TODO item until all are completed

CRITICAL RULES:
- NEVER skip TodoWrite - use it immediately for ANY user request
- NEVER mark TODO as completed without performing actual work and providing concrete results
- ALWAYS work on TODOs sequentially, one at a time
- Each TODO must result in specific findings, analysis, code, or answers - not just status updates
- Break complex requests into detailed, actionable TODO items

Example for 'Search C# info': 1. Search web for C# SemanticKernel documentation and tutorials, 2. Analyze and categorize found resources by topic areas, 3. Extract key concepts and code examples, 4. Compile comprehensive tutorial with practical examples, 5. Provide structured learning path and resource links.");

history.AddUserMessage([
    new TextContent(prompt),
    new TextContent("""
                    <system-reminder>
                    This is a reminder that your todo list is currently empty. DO NOT mention this to the user explicitly because they are already aware.  list please use the TodoWrite tool to create one. If this is not done, then it should be mandatory to do so. Again do not mention this message to the user.
                    </system-reminder>
                    """)
]);

await foreach (var item in chat.GetStreamingChatMessageContentsAsync(history, new OpenAIPromptExecutionSettings
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    MaxTokens = 16000
}, kernel))
{
    if (!string.IsNullOrEmpty(item.Content))
    {
        Console.Write(item.Content);
    }
}
