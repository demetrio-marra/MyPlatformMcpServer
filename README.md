# MyPlatform MCP Server

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/demetrio-marra/MyPlatformMcpServer)](LICENSE)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)

A Model Context Protocol (MCP) server implementation for MyPlatform, providing AI agents with structured access to company information, performance statistics, chart generation, and file management capabilities.

## ?? Features

- **Model Context Protocol (MCP) Support**: Full implementation of the MCP specification for AI agent integration
- **Company Information Tools**: Retrieve and navigate company/family/product hierarchies
- **Performance Statistics**: Query and analyze performance metrics across products
- **Chart Generation**: Create bar, line, and pie charts with automatic color assignment
- **File Server Integration**: Upload and manage files with metadata
- **ACL (Access Control List)**: Permission management and validation
- **JWT Authentication**: Optional JWT-based authentication with Keycloak support
- **Docker Ready**: Containerized deployment with Kubernetes health probes
- **AutoMapper Integration**: Clean separation between DTOs and domain models

## ??? Architecture

The solution follows a layered architecture with clean separation of concerns:

```
MyPlatformMcpServer/
??? MyPlatform.Models/          # Domain models, interfaces, and business logic
??? MyPlatform.Infrastructure/  # Repository and external service implementations
??? MyPlatform.McpServer/       # MCP server, tools, DTOs, and API endpoints
```

### Key Components

- **Service Layer**: Core business logic with domain models and service interfaces
- **Data Layer**: Repository pattern implementation for data access
- **MCP Tools Layer**: MCP-compliant tool definitions for AI agent interaction
- **Infrastructure Layer**: External service clients (ACL, File Server)

## ?? Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (optional, for containerized deployment)
- MongoDB (for persistence repositories)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/demetrio-marra/MyPlatformMcpServer.git
cd MyPlatformMcpServer
```

2. Configure application settings:
```bash
cd MyPlatform.McpServer
cp appsettings.json appsettings.Development.json
```

3. Update `appsettings.Development.json` with your configuration:
```json
{
  "Jwt": {
    "Issuer": "your-keycloak-issuer",
    "Audience": "your-audience",
    "Enabled": false
  },
  "AclConfiguration": {
    "Endpoint": "https://your-acl-backend",
    "ApiKeyHeaderName": "X-API-Key",
    "ApiKeyValue": "your-api-key"
  },
  "FileServerConfiguration": {
    "Endpoint": "https://your-file-server",
    "ApiKeyHeaderName": "X-API-Key",
    "ApiKeyValue": "your-api-key"
  },
  "PerformanceStatisticsRepository": {
    "ConnectionString": "your-mongodb-connection-string"
  },
  "CompanyInfoRepository": {
    "ConnectionString": "your-mongodb-connection-string"
  }
}
```

4. Build and run:
```bash
dotnet build
dotnet run --project MyPlatform.McpServer
```

The server will start on `http://localhost:5000` (or the port specified in launchSettings.json).

### Docker Deployment

Build and run using Docker:

```bash
docker build -t myplatform-mcp-server -f MyPlatform.McpServer/Dockerfile .
docker run -p 8080:8080 myplatform-mcp-server
```

Or use with Kubernetes health probes:
- **Liveness probe**: `GET /health/liveness`
- **Readiness probe**: `GET /health/readiness`

## ?? Configuration

### JWT Authentication

Enable JWT authentication for production environments:

```json
{
  "Jwt": {
    "Issuer": "https://your-keycloak.com/realms/your-realm",
    "Audience": "your-client-id",
    "Enabled": true
  }
}
```

When enabled, all MCP endpoints require a valid JWT bearer token.

### Logging

Configure logging levels in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "ModelContextProtocol": "Debug"
    }
  }
}
```

## ?? MCP Tools

The server exposes the following MCP tools for AI agents:

### Company Information Tools

- **`MyPlatform_CompanyInfo_GetProductsHierarchy`**: Retrieve flattened company/family/product hierarchy
- **`MyPlatform_CompanyInfo_FindProductHierarchy`**: Find which company and family a specific product belongs to
- **`MyCompany_CompanyInfo_GetAllProductNames`**: List all available product names

### Statistics Tools

- **`MyPlatform_Statistics_GetPerformanceStatistics`**: Query performance metrics with filtering and grouping
- **`MyPlatform_Statistics_GetStatisticsRates`**: Calculate statistics rates and trends
- **`MyPlatform_Statistics_GetStatistics`**: Retrieve general statistics data

### Chart Tools

- **`MyPlatform_Chart_GenerateChart`**: Generate bar, line, or pie charts from data
  - Supports customizable titles, axis labels, and data points
  - Automatically uploads generated charts to file server
  - Returns file metadata for further use

### Permission Tools

- **`MyPlatform_Permissions_GetMyPermissions`**: Retrieve current agent's permissions

## ??? Development

### Project Structure

```
MyPlatform.McpServer/
??? Configuration/          # Configuration classes (JWT, ACL, FileServer)
??? DTOs/                   # Data Transfer Objects
??? Extensions/             # Extension methods
??? Mapping/                # AutoMapper profiles
??? Services/               # Application services
??? Tools/                  # MCP tool implementations
?   ??? Statistics/         # Statistics-related tools
??? Program.cs              # Application entry point
??? appsettings.json        # Configuration files
```

### Adding New Tools

1. Create a new tool class in the `Tools/` directory
2. Decorate with `[McpServerToolType]` attribute
3. Implement methods with `[McpServerTool]` attribute
4. Register in `Program.cs`:

```csharp
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<YourNewTool>();
```

### Coding Guidelines

Follow the directives in `.github/copilot-instructions.md`:

- **Service Layer**: Contains models, interfaces, and domain exceptions
- **Logging Rules**: 
  - Use `LogInformation` for successful operations in public service methods
  - Use `LogDebug` for detailed tracing
  - Use `LogWarning` for domain exceptions in controllers
  - Use `LogError` only for unhandled exceptions
- **Exception Handling**: Service layer throws domain exceptions; controller layer handles them
- **AutoMapper**: Use for mapping between entities and DTOs

## ?? Testing

Run tests using:

```bash
dotnet test
```

## ?? Dependencies

Key NuGet packages:

- **ModelContextProtocol** (0.3.0-preview.4): MCP server implementation
- **ModelContextProtocol.AspNetCore** (0.3.0-preview.4): ASP.NET Core integration
- **AutoMapper** (13.0.1): Object-to-object mapping
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT authentication
- **ScottPlot**: Chart generation library
- **MongoDB.Driver**: MongoDB database access

## ?? Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please ensure your code follows the project's coding guidelines and includes appropriate tests.

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ?? Links

- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [AutoMapper Documentation](https://docs.automapper.org/)

## ?? Contact

For questions or support, please open an issue on GitHub.

---

**Built with ?? using .NET 8 and Model Context Protocol**
