# MCP 服务器

本README文档是使用C# MCP服务器项目模板创建的。它演示了如何使用C#轻松创建MCP服务器并将其发布为NuGet包。

有关完整指南，请参阅 [aka.ms/nuget/mcp/guide](https://aka.ms/nuget/mcp/guide)。

请注意，此模板目前处于早期预览阶段。如果您有反馈意见，请参与[简短调查](http://aka.ms/dotnet-mcp-template-survey)。

## 发布到NuGet.org之前的检查清单

- 使用以下步骤在本地测试MCP服务器。
- 更新.csproj文件中的包元数据，特别是`<PackageId>`。
- 更新`.mcp/server.json`以声明您的MCP服务器的输入。
  - 有关更多详细信息，请参阅[配置输入](https://aka.ms/nuget/mcp/guide/configuring-inputs)。
- 使用`dotnet pack`打包项目。

`bin/Release`目录将包含包文件(.nupkg)，该文件可以[发布到NuGet.org](https://learn.microsoft.com/nuget/nuget-org/publish-a-package)。

## 本地开发

要从源代码（本地）测试此MCP服务器而不使用构建的MCP服务器包，您可以配置IDE使用`dotnet run`直接运行项目。

```json
{
  "servers": {
    "MarkAgent.Local": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "<项目目录路径>"
      ]
    }
  }
}
```

## 测试MCP服务器

配置完成后，您可以向Copilot Chat请求随机数，例如，`给我3个随机数`。它应该提示您在`MarkAgent.Local` MCP服务器上使用`get_random_number`工具并显示结果。

## 发布到NuGet.org

1. 运行`dotnet pack -c Release`创建NuGet包
2. 使用`dotnet nuget push bin/Release/*.nupkg --api-key <your-api-key> --source https://api.nuget.org/v3/index.json`发布到NuGet.org

## 从NuGet.org使用MCP服务器

一旦MCP服务器包发布到NuGet.org，您就可以在首选的IDE中配置它。VS Code和Visual Studio都使用`dnx`命令从NuGet.org下载和安��MCP服务器包。

- **VS Code**: 创建`<工作区目录>/.vscode/mcp.json`文件
- **Visual Studio**: 创建`<解决方案目录>\.mcp.json`文件

对于VS Code和Visual Studio，配置文件使用以下服务器定义：

```json
{
  "servers": {
    "MarkAgent.Local": {
      "type": "stdio",
      "command": "dnx",
      "args": [
        "MarkAgent",
        "--version",
        "0.1.0",
        "--yes"
      ]
    }
  }
}
```

# MarkAgent.Local

## Claude Desktop配置

```json
{
  "mcpServers": {
    "MarkAgent.Local": {
      "command": "dnx",
      "args": [
        "MarkAgent",
        "--version",
        "0.1.0",
        "--yes"
      ]
    }
  }
}
```

## Trae接入配置

```json
{
  "mcpServers": {
    "MarkAgent.Local": {
      "command": "dnx",
      "args": [
        "MarkAgent",
        "--version",
        "0.1.0",
        "--yes"
      ]
    }
  }
}
```

## 更多信息

.NET MCP服务器使用[ModelContextProtocol](https://www.nuget.org/packages/ModelContextProtocol) C# SDK。有关MCP的更多信息：

- [官方文档](https://modelcontextprotocol.io/)
- [协议规范](https://spec.modelcontextprotocol.io/)
- [GitHub组织](https://github.com/modelcontextprotocol)

请参阅VS Code或Visual Studio文档，了解有关配置和使用MCP服务器的更多信息：

- [在VS Code中使用MCP服务器（预览版）](https://code.visualstudio.com/docs/copilot/chat/mcp-servers)
- [在Visual Studio中使用MCP服务器（预览版）](https://learn.microsoft.com/visualstudio/ide/mcp-servers)
