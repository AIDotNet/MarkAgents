namespace MarkAgent.Shared;

/// <summary>
/// Prompts for various tools and thinking processes used by the MarkAgent
/// </summary>
public static class Prompts
{
    /// <summary>
    /// Prompt for the Todo tool
    /// </summary>
    public const string TodoPrompt = 
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
""";

    public const string DeepThinkingPrompt = 
"""
Use this tool to engage in deep, structured thinking about complex problems, user requirements, or challenging decisions. This tool helps you process information systematically and provides your thought process back to enhance understanding and decision-making.

## When to Use This Tool
Use this tool proactively in these scenarios:

1. **Complex Problem Analysis** - When facing multi-faceted problems that require careful consideration
2. **Requirement Clarification** - When user requests are ambiguous and need deeper exploration
3. **Decision Points** - When multiple approaches exist and you need to evaluate trade-offs
4. **Architecture Planning** - When designing systems or making technical decisions
5. **Risk Assessment** - When considering potential issues or complications
6. **Learning from Context** - When analyzing existing code or systems to understand patterns

## Core Thinking Principles

1. **Question Assumptions** - Challenge initial interpretations and explore alternatives
2. **Break Down Complexity** - Decompose complex problems into manageable components
3. **Consider Multiple Perspectives** - Look at problems from different angles
4. **Evaluate Trade-offs** - Weigh pros and cons of different approaches
5. **Anticipate Consequences** - Think through potential implications and side effects
6. **Build on Context** - Use existing knowledge and patterns to inform decisions

## Thinking Process Structure

Your thought process should follow this pattern:

1. **Initial Understanding** - What is the core problem or requirement?
2. **Context Analysis** - What relevant information do we have?
3. **Assumption Identification** - What assumptions am I making?
4. **Alternative Exploration** - What other approaches could work?
5. **Trade-off Evaluation** - What are the pros and cons of each option?
6. **Decision Rationale** - Why is this the best approach?
7. **Implementation Considerations** - What practical factors matter?
8. **Risk Assessment** - What could go wrong and how to mitigate?

## Examples of Deep Thinking Scenarios

<example>
User: "I want to add real-time notifications to my app"
Thought Process:
- Initial Understanding: User wants real-time notifications, but what type? Push notifications, in-app notifications, or both?
- Context Analysis: Need to examine existing tech stack, user base size, notification frequency
- Assumptions: Assuming they want both types, but should clarify the specific use cases
- Alternatives: WebSockets, Server-Sent Events, Push API, third-party services
- Trade-offs: WebSockets offer full duplex but require more infrastructure; SSE is simpler but one-way
- Decision: Recommend starting with requirements clarification, then suggest appropriate technology based on their specific needs
- Implementation: Consider scalability, reliability, user preferences
- Risks: Notification fatigue, performance impact, complexity overhead
</example>

<example>
User: "This code is running slowly, can you help optimize it?"
Thought Process:
- Initial Understanding: Performance issue exists, but need to identify bottlenecks
- Context Analysis: Need to examine the code, understand data volumes, usage patterns
- Assumptions: Assuming it's algorithmic complexity, but could be I/O, memory, or network
- Alternatives: Algorithm optimization, caching, database indexing, parallel processing
- Trade-offs: Code complexity vs performance gains, memory usage vs speed
- Decision: Profile first to identify actual bottlenecks before optimizing
- Implementation: Measure performance, implement targeted optimizations
- Risks: Premature optimization, breaking existing functionality, over-engineering
</example>

## Guidelines for Effective Thinking

1. **Be Thorough** - Don't rush to conclusions; explore the problem space fully
2. **Stay Objective** - Consider evidence and logic over preferences
3. **Embrace Uncertainty** - It's okay to acknowledge when you need more information
4. **Think Practically** - Consider real-world constraints and limitations
5. **Document Reasoning** - Clearly explain your thought process and rationale
6. **Iterate and Refine** - Be prepared to revise your thinking as new information emerges

The goal is to provide well-reasoned, thoughtful analysis that leads to better outcomes and helps others understand complex problems more clearly.
""";
    
    public const string SequentialThinkingPrompt = 
"""
A detailed tool for dynamic and reflective problem-solving through thoughts.
This tool helps analyze problems through a flexible thinking process that can adapt and evolve.
Each thought can build on, question, or revise previous insights as understanding deepens.

When to use this tool:
- Breaking down complex problems into steps
- Planning and design with room for revision
- Analysis that might need course correction
- Problems where the full scope might not be clear initially
- Problems that require a multi-step solution
- Tasks that need to maintain context over multiple steps
- Situations where irrelevant information needs to be filtered out

You should:
1. Start with an initial estimate of needed thoughts, but be ready to adjust
2. Feel free to question or revise previous thoughts
3. Don't hesitate to add more thoughts if needed, even at the "end"
4. Express uncertainty when present
5. Mark thoughts that revise previous thinking or branch into new paths
6. Ignore information that is irrelevant to the current step
7. Generate a solution hypothesis when appropriate
8. Verify the hypothesis based on the Chain of Thought steps
9. Repeat the process until satisfied with the solution
10. Provide a single, ideally correct answer as the final output
11. Only set next_thought_needed to false when truly done and a satisfactory answer is reached
""";
    
    public const string MentalModelPrompt =
"""
A tool for applying structured mental models to problem-solving.
Supports various mental models including:
- First Principles Thinking
- Opportunity Cost Analysis
- Error Propagation Understanding
- Rubber Duck Debugging
- Pareto Principle
- Occam's Razor

Each model provides a systematic approach to breaking down and solving problems.            
""";
    
    public const string WebQueryPrompt =
"""
Search the web for current information, research materials, and real-time data. This tool performs web searches to retrieve up-to-date content from across the internet, including news articles, technical documentation, product information, and other online resources.

Use this tool when you need:
- Current or recent information that may not be in your training data
- Real-time data (prices, weather, news, etc.)
- Technical documentation and tutorials
- Product reviews and comparisons
- Academic papers and research materials
- Fact verification from authoritative sources

Query optimization tips:
- Keep queries concise and specific (2-6 words work best)
- Use relevant keywords rather than full questions
- For technical topics, include specific terms or version numbers
- For current events, include time indicators like 'latest' or '2025'
- Avoid special characters unless searching for exact phrases
The tool returns search results with titles, URLs, and content snippets that you can analyze and synthesize to provide comprehensive answers.
""";
}