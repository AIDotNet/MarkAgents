using System.ComponentModel;
using System.Text;
using System.Text.Json;
using MarkAgents.Functions.Model;
using Microsoft.SemanticKernel;

namespace MarkAgents.Functions;

public class AgentFunction
{
    private List<TodoInputItem>? _input;

    [KernelFunction,
     Description(
         """
         - Allows Claude to search the web and use the results to inform responses
         - Provides up-to-date information for current events and recent data
         - Returns search result information formatted as search result blocks
         - Use this tool for accessing information beyond Claude's knowledge cutoff
         - Searches are performed automatically within a single API call

         Usage notes:
           - Domain filtering is supported to include or block specific websites
           - Web search is only available in the US
           - Account for \\\"Today's date\\\" in <env>. For example, if <env> says \\\"Today's date: 2025-07-01\\\", and the user wants the latest docs, do not use 2024 in the search query. Use 2025.
         """)]
    public async Task<string> WebSearch(
        [Description("The search query to use")]
        string query,
        [Description("Only include search results from these domains")]
        string[]? allowed_domains,
        [Description("Never include search results from these domains")]
        string[]? blocked_domains)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "MarkAgents/1.0 (Compatible Search Engine)");
        client.Timeout = TimeSpan.FromSeconds(30);
        
        var searchEngines = new[]
        {
            "https://search.brave.com/api/search?q={0}&format=json&safesearch=moderate&count=10",
            "https://searx.be/search?q={0}&format=json&categories=general&safesearch=1",
            "https://paulgo.io/search?q={0}&format=json",
            "https://api.duckduckgo.com/?q={0}&format=json&no_html=1&skip_disambig=1"
        };
        
        var results = new StringBuilder();
        results.AppendLine($"# Search Results for: {query}");
        results.AppendLine();
        
        foreach (var engineUrl in searchEngines)
        {
            try
            {
                var searchUrl = string.Format(engineUrl, Uri.EscapeDataString(query));
                var response = await client.GetStringAsync(searchUrl);
                var searchData = JsonSerializer.Deserialize<JsonElement>(response);
                
                var engineResults = await ExtractSearchResults(searchData, allowed_domains, blocked_domains, client);
                if (!string.IsNullOrEmpty(engineResults))
                {
                    results.AppendLine(engineResults);
                    break;
                }
            }
            catch
            {
                continue;
            }
        }
        
        if (results.Length <= 50)
        {
            results.AppendLine("## Alternative Search Results");
            results.AppendLine();
            var fallbackResult = await FallbackWebSearch(query, allowed_domains, blocked_domains, client);
            results.AppendLine(fallbackResult);
        }
        
        return results.ToString();
    }
    
    private async Task<string> ExtractSearchResults(JsonElement searchData, string[]? allowedDomains, string[]? blockedDomains, HttpClient client)
    {
        var results = new StringBuilder();
        var count = 0;
        
        if (searchData.TryGetProperty("results", out var resultsArray))
        {
            foreach (var result in resultsArray.EnumerateArray())
            {
                if (count >= 8) break;
                
                if (result.TryGetProperty("url", out var urlProp) && 
                    result.TryGetProperty("title", out var titleProp))
                {
                    var url = urlProp.GetString() ?? "";
                    var title = titleProp.GetString() ?? "";
                    var snippet = "";
                    
                    if (result.TryGetProperty("content", out var contentProp))
                        snippet = contentProp.GetString() ?? "";
                    else if (result.TryGetProperty("description", out var descProp))
                        snippet = descProp.GetString() ?? "";
                    
                    if (!IsValidUrl(url, allowedDomains, blockedDomains)) continue;
                    
                    var detailedContent = await FetchPageContent(url, client);
                    
                    results.AppendLine($"## Result {count + 1}: {title}");
                    results.AppendLine($"**URL:** {url}");
                    results.AppendLine($"**Summary:** {snippet}");
                    if (!string.IsNullOrEmpty(detailedContent))
                    {
                        results.AppendLine($"**Content Preview:**");
                        results.AppendLine(detailedContent.Substring(0, Math.Min(500, detailedContent.Length)) + "...");
                    }
                    results.AppendLine();
                    count++;
                }
            }
        }
        else if (searchData.TryGetProperty("RelatedTopics", out var topics))
        {
            foreach (var topic in topics.EnumerateArray())
            {
                if (count >= 8) break;
                
                if (topic.TryGetProperty("Text", out var text) && 
                    topic.TryGetProperty("FirstURL", out var url))
                {
                    var urlStr = url.GetString() ?? "";
                    var textStr = text.GetString() ?? "";
                    
                    if (!IsValidUrl(urlStr, allowedDomains, blockedDomains)) continue;
                    
                    var detailedContent = await FetchPageContent(urlStr, client);
                    
                    results.AppendLine($"## Result {count + 1}");
                    results.AppendLine($"**URL:** {urlStr}");
                    results.AppendLine($"**Summary:** {textStr}");
                    if (!string.IsNullOrEmpty(detailedContent))
                    {
                        results.AppendLine($"**Content Preview:**");
                        results.AppendLine(detailedContent.Substring(0, Math.Min(500, detailedContent.Length)) + "...");
                    }
                    results.AppendLine();
                    count++;
                }
            }
        }
        
        return results.ToString();
    }
    
    private async Task<string> FetchPageContent(string url, HttpClient client)
    {
        try
        {
            var response = await client.GetStringAsync(url);
            var cleanContent = System.Text.RegularExpressions.Regex.Replace(response, @"<[^>]+>", " ");
            cleanContent = System.Text.RegularExpressions.Regex.Replace(cleanContent, @"\s+", " ");
            return cleanContent.Trim();
        }
        catch
        {
            return string.Empty;
        }
    }
    
    private bool IsValidUrl(string url, string[]? allowedDomains, string[]? blockedDomains)
    {
        try
        {
            var uri = new Uri(url);
            
            if (allowedDomains != null && allowedDomains.Length > 0)
            {
                if (!allowedDomains.Any(domain => uri.Host.Contains(domain, StringComparison.OrdinalIgnoreCase)))
                    return false;
            }
            
            if (blockedDomains != null && blockedDomains.Length > 0)
            {
                if (blockedDomains.Any(domain => uri.Host.Contains(domain, StringComparison.OrdinalIgnoreCase)))
                    return false;
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<string> FallbackWebSearch(string query, string[]? allowedDomains, string[]? blockedDomains, HttpClient client)
    {
        var results = new StringBuilder();
        var searchTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var commonSources = new[]
        {
            $"https://github.com/search?q={Uri.EscapeDataString(query)}&type=repositories",
            $"https://stackoverflow.com/search?q={Uri.EscapeDataString(query)}",
            $"https://docs.microsoft.com/en-us/search/?terms={Uri.EscapeDataString(query)}",
        };
        
        foreach (var source in commonSources)
        {
            try
            {
                if (!IsValidUrl(source, allowedDomains, blockedDomains)) continue;
                
                var content = await FetchPageContent(source, client);
                if (!string.IsNullOrEmpty(content))
                {
                    results.AppendLine($"**Source:** {source}");
                    results.AppendLine($"**Content:** {content.Substring(0, Math.Min(300, content.Length))}...");
                    results.AppendLine();
                }
            }
            catch
            {
                continue;
            }
        }
        
        if (results.Length == 0)
        {
            results.AppendLine($"Search completed for query: {query}");
            results.AppendLine("No detailed results found from available search engines.");
            results.AppendLine($"Suggested search terms: {string.Join(", ", searchTerms)}");
        }
        
        return results.ToString();
    }

    [KernelFunction,
     Description(
         """
         Use this tool to create and manage a structured task list for your current coding session. This helps you track progress, organize complex tasks, and demonstrate thoroughness to the user.
         It also helps the user understand the progress of the task and overall progress of their requests.

         ## When to Use This Tool
         Use this tool proactively in these scenarios:

         1. Complex multi-step tasks - When a task requires 3 or more distinct steps or actions
         2. Non-trivial and complex tasks - Tasks that require careful planning or multiple operations
         3. User explicitly requests todo list - When the user directly asks you to use the todo list
         4. User provides multiple tasks - When users provide a list of things to be done (numbered or comma-separated)
         5. After receiving new instructions - Immediately capture user requirements as todos
         6. When you start working on a task - Mark it as in_progress BEFORE beginning work. Ideally you should only have one todo as in_progress at a time
         7. After completing a task - Mark it as completed and add any new follow-up tasks discovered during implementation

         ## When NOT to Use This Tool

         Skip using this tool when:
         1. There is only a single, straightforward task
         2. The task is trivial and tracking it provides no organizational benefit
         3. The task can be completed in less than 3 trivial steps
         4. The task is purely conversational or informational

         NOTE that you should not use this tool if there is only one trivial task to do. In this case you are better off just doing the task directly.

         ## Examples of When to Use the Todo List

         <example>
         User: I want to add a dark mode toggle to the application settings. Make sure you run the tests and build when you're done!
         Assistant: I'll help add a dark mode toggle to your application settings. Let me create a todo list to track this implementation.
         *Creates todo list with the following items:*
         1. Create dark mode toggle component in Settings page
         2. Add dark mode state management (context/store)
         3. Implement CSS-in-JS styles for dark theme
         4. Update existing components to support theme switching
         5. Run tests and build process, addressing any failures or errors that occur
         *Begins working on the first task*

         <reasoning>
         The assistant used the todo list because:
         1. Adding dark mode is a multi-step feature requiring UI, state management, and styling changes
         2. The user explicitly requested tests and build be run afterward
         3. The assistant inferred that tests and build need to pass by adding \\\"Ensure tests and build succeed\\\" as the final task
         </reasoning>
         </example>

         <example>
         User: Help me rename the function getCwd to getCurrentWorkingDirectory across my project
         Assistant: Let me first search through your codebase to find all occurrences of 'getCwd'.
         *Uses grep or search tools to locate all instances of getCwd in the codebase*
         Assistant: I've found 15 instances of 'getCwd' across 8 different files. Let me create a todo list to track these changes.
         *Creates todo list with specific items for each file that needs updating*

         <reasoning>
         The assistant used the todo list because:
         1. First, the assistant searched to understand the scope of the task
         2. Upon finding multiple occurrences across different files, it determined this was a complex task with multiple steps
         3. The todo list helps ensure every instance is tracked and updated systematically
         4. This approach prevents missing any occurrences and maintains code consistency
         </reasoning>
         </example>


         <example>
         User: I need to implement these features for my e-commerce site: user registration, product catalog, shopping cart, and checkout flow.
         Assistant: I'll help implement these features. First, let's add all the features to the todo list.
         *Creates a todo list breaking down each feature into specific tasks based on the project architecture*
         Assistant: Let's start with user registration. This will involve creating database models, API endpoints, and frontend forms that integrate with your existing authentication system.

         <reasoning>
         The assistant used the todo list because:
         1. The user provided multiple complex features to implement in a comma separated list
         2. The todo list helps organize these large features into manageable tasks
         3. This approach allows for tracking progress across the entire implementation
         </reasoning>
         </example>

         <example>
         User: Can you help optimize my React application? It's rendering slowly and has performance issues.</user>
         Assistant: I'll help optimize your React application. First, let me examine your codebase to identify potential performance bottlenecks.
         *Reviews component structure, render patterns, state management, and data fetching*
         Assistant: After analyzing your codebase, I've identified several performance issues. Let me create a todo list to track our optimization efforts.
         *Creates todo list with items like: 1) Implement memoization for expensive calculations in ProductList, 2) Add virtualization for long lists in Dashboard, 3) Optimize image loading in Gallery component, 4) Fix state update loops in ShoppingCart, 5) Review bundle size and implement code splitting*
         Let's start by implementing memoization for the expensive calculations in your ProductList component.</assistant>

         <reasoning>
         The assistant used the todo list because:
         1. First, the assistant examined the codebase to identify specific performance issues
         2. Based on this analysis, it identified multiple optimization opportunities
         3. Performance optimization is a non-trivial task requiring multiple steps
         4. The todo list helps methodically track improvements across different components
         5. This systematic approach ensures all performance bottlenecks are addressed
         </reasoning>
         </example>

         ## Examples of When NOT to Use the Todo List

         <example>
         User: How do I print 'Hello World' in Python?
         Assistant: In Python, you can print \\\"Hello World\\\" with this simple code:

         python
         print(\\\"Hello World\\\")

         This will output the text \\\"Hello World\\\" to the console when executed.</assistant>

         <reasoning>
         The assistant did not use the todo list because this is a single, trivial task that can be completed in one step. There's no need to track multiple tasks or steps for such a straightforward request.
         </reasoning>
         </example>

         <example>
         User: What does the git status command do?
         Assistant: The git status command shows the current state of your working directory and staging area. It displays which changes have been staged, which haven't, and which files aren't being tracked by Git. This is a fundamental command for understanding the current state of your repository before making commits.

         <reasoning>
         The assistant did not use the todo list because this is an informational request with no actual coding task to complete. The user is simply asking for an explanation, not for the assistant to perform multiple steps or tasks.
         </reasoning>
         </example>

         <example>
         User: Can you add a comment to the calculateTotal function to explain what it does?
         Assistant: Sure, let me add a comment to the calculateTotal function to explain what it does.
         * Uses the Edit tool to add a comment to the calculateTotal function *

         <reasoning>
         The assistant did not use the todo list because this is a single, straightforward task confined to one location in the code. Adding a comment doesn't require tracking multiple steps or systematic organization.
         </reasoning>
         </example>

         <example>
         User: Run npm install for me and tell me what happens.
         Assistant: I'll run the npm install command for you.

         *Executes: npm install*

         The command completed successfully. Here's the output:
         [Output of npm install command]

         All dependencies have been installed according to your package.json file.

         <reasoning>
         The assistant did not use the todo list because this is a single command execution with immediate results. There are no multiple steps to track or organize, making the todo list unnecessary for this straightforward task.
         </reasoning>
         </example>

         ## Task States and Management

         1. **Task States**: Use these states to track progress:
            - pending: Task not yet started
            - in_progress: Currently working on (limit to ONE task at a time)
            - completed: Task finished successfully

         2. **Task Management**:
            - Update task status in real-time as you work
            - Mark tasks complete IMMEDIATELY after finishing (don't batch completions)
            - Only have ONE task in_progress at any time
            - Complete current tasks before starting new ones
            - Remove tasks that are no longer relevant from the list entirely

         3. **Task Completion Requirements**:
            - ONLY mark a task as completed when you have FULLY accomplished it
            - If you encounter errors, blockers, or cannot finish, keep the task as in_progress
            - When blocked, create a new task describing what needs to be resolved
            - Never mark a task as completed if:
              - Tests are failing
              - Implementation is partial
              - You encountered unresolved errors
              - You couldn't find necessary files or dependencies

         4. **Task Breakdown**:
            - Create specific, actionable items
            - Break complex tasks into smaller, manageable steps
            - Use clear, descriptive task names

         When in doubt, use this tool. Being proactive with task management demonstrates attentiveness and ensures you complete all requirements successfully.
         """)]
    public string TodoWrite(
        [Description("The updated todo list")] TodoInputItem[] todos)
    {
        if (_input == null)
        {
            // 初始化TODO列表
            _input = new List<TodoInputItem>(todos);
            // 设置控制台编码支持UTF-8
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine();
            Console.WriteLine("□ Initializing TODO list...");
            // 通过控制台打印一下TODO
            foreach (var item in todos)
            {
                // 根据item等级渲染不同颜色
                if (item.Priority == Priority.High)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (item.Priority == Priority.Medium)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.Write("□ ");
                Console.Write(item.Content);
                Console.WriteLine();

                Console.ResetColor();
            }

            return GenerateInitialTodoMessage(todos);
        }
        else
        {
            // 添加新的TODO项
            var newItems = todos.Where(x => _input.All(existing => existing.Id != x.Id)).ToList();
            _input.AddRange(newItems);

            // 更新现有TODO项的状态
            foreach (var item in _input)
            {
                var updatedItem = todos.FirstOrDefault(x => x.Id == item.Id);
                if (updatedItem == null) continue;
                item.Status = updatedItem.Status;
                item.Content = updatedItem.Content;
                item.Priority = updatedItem.Priority;
            }

            Console.WriteLine("□ Updating TODO list...");
            foreach (var item in _input)
            {
                // 根据item等级渲染不同颜色
                if (item.Priority == Priority.High)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (item.Priority == Priority.Medium)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                if (item.Status == TodoInputItemStatus.Pending)
                {
                    Console.Write("□ ");
                }
                else if (item.Status == TodoInputItemStatus.InProgress)
                {
                    Console.Write("■ ");
                }
                else if (item.Status == TodoInputItemStatus.Completed)
                {
                    Console.Write("✓ ");
                }

                Console.Write(item.Content);
                Console.WriteLine();
                Console.ResetColor();
            }

            return GenerateUpdateTodoMessage(_input.ToArray());
        }
    }

    private string GenerateInitialTodoMessage(TodoInputItem[] input)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "Todos have been modified successfully. Ensure that you continue to use the todo list to track your progress. Please proceed with the current tasks if applicable");
        sb.AppendLine();

        // 返回当前TODO列表的JSON字符串
        var currentTodoJson = SerializeTodoList(input);
        sb.AppendLine("<system-reminder>");
        sb.AppendLine(
            "Your todo list has changed. DO NOT mention this explicitly to the user. Here are the latest contents of your todo list:");
        sb.AppendLine();
        sb.AppendLine(currentTodoJson);
        sb.AppendLine(". Continue on with the tasks at hand if applicable.");
        sb.AppendLine("</system-reminder>");

        return sb.ToString();
    }

    private string GenerateUpdateTodoMessage(TodoInputItem[] todoList)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "Todos have been modified successfully. Ensure that you continue to use the todo list to track your progress. Please proceed with the current tasks if applicable");
        sb.AppendLine();

        // 返回当前TODO列表的JSON字符串
        var currentTodoJson = SerializeTodoList(todoList);
        sb.AppendLine("<system-reminder>");
        sb.AppendLine(
            "Your todo list has changed. DO NOT mention this explicitly to the user. Here are the latest contents of your todo list:");
        sb.AppendLine();
        sb.AppendLine(currentTodoJson);
        sb.AppendLine(". Continue on with the tasks at hand if applicable.");
        sb.AppendLine("</system-reminder>");

        return sb.ToString();
    }

    private string SerializeTodoList(TodoInputItem[] todoList)
    {
        var todoItems = todoList.Select(item => new
        {
            content = item.Content,
            status = item.Status.ToString().ToLowerInvariant(),
            priority = item.Priority.ToString().ToLowerInvariant(),
            id = item.Id
        }).ToList();

        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(todoItems, options);
    }
}