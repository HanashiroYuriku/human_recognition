# human_recognition - .NET 8 BE Template
A modular backend template utilizing Clean Architecture, Vertical Slicing, and CQRS approaches on C# .NET.

## Technical Specifications

* **Language:** [C# .NET](https://learn.microsoft.com/en-us/dotnet/)
* **Web Framework:** [ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-10.0)
* **Database:** MySQL
* **ORM:** [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) (via Pomelo.EntityFrameworkCore.MySql)
* **Architecture & Pattern:** Clean Architecture & CQRS via [Cortex.Mediator](https://medium.com/@eneshoxha_65350/cortex-mediator-a-free-open-source-alternative-to-mediatr-for-cqrs-in-net-59534e1305c7)
* **Configuration:** Native .NET appsettings.json
* **Validation:** [FluentValidation](https://docs.fluentvalidation.net/en/latest/)
* **Security & Auth:** JWT Bearer & BCrypt.Net
* **Logging & Monitoring:** Serilog & ASP.NET Core HealthChecks

## Core Features
1. **Architecture:** Implementation of Clean Architecture combined with Vertical Slice Architecture (feature-oriented development).
2. **Separation of Concerns (CQRS):** Application of the Command and Query Responsibility Segregation pattern using the Cortex.Mediator library to strictly separate data modification operations (`Commands`) from data retrieval operations (`Queries`).
3. **Data Validation:** Utilization of FluentValidation integrated into Cortex.Mediator Pipeline Behaviors to ensure all validation processes are executed automatically and centrally.
4. **Centralized Error Handling:** Implementation of a Global Exception Handler compliant with the RFC 7807 international standard (Problem Details) to generate uniform and informative error response formats.
5. **Database Management:** Utilization of the Entity Framework Core Object-Relational Mapping (ORM) alongside the Pomelo MySQL data provider, supported by database schema definitions via Fluent API configurations.
6. **Design Pattern:** Application of the Repository Pattern, serving as an abstraction layer to isolate business logic from direct data access operations.
7. **System Security:** Securing credentials and sensitive data through cryptographic hashing mechanisms using the BCrypt.Net-Next library.
8. **Activity Logging:** Implementation of a comprehensive logging system encompassing inbound request logging, as well as behavior monitoring for every Command and Query execution.
9. **Upcoming Developments:** Planned integration of component testing (Unit Testing) and container-based infrastructure management utilizing Docker.

## Template Structure
```text
human_recognition/
    ├── human_recognition.sln
    ├── image/
    │   ├── Human Detected/ 
    │   ├── Human Not Detected/ 
    ├── image_source/
    ├── models/
    ├── src/
    │   │
    │   ├── human_recognition.Domain/                        # [Domain Layer]
    │   │   ├── Common/                              # BaseEntity.cs (Now using Guid. You can change to UUIDNext, ULID or UUIDv7 [.NET 9+] to optimize performance)
    │   │   ├── Entities/                            # User.cs (Pure Domain/Database Models without dependencies)
    │   │   ├── Enums/                               # RoleEnum.cs
    │   │   ├── Exceptions/                          # Domain-specific custom exceptions under human_recognitionException class(e.g., NotFoundException, UnauthorizedException)
    │   │   └── human_recognition.Domain.csproj
    │   │
    │   ├── human_recognition.Application/                   # [Application Layer: Use Cases (CQRS & Vertical Slicing)]
    │   │   ├── Common/                              
    │   │   │   ├── Behaviors/                       # Cortex.Mediator Pipeline Behaviors (e.g., ValidationBehavior, LoggingBehavior)
    │   │   │   ├── Interfaces/                      
    │   │   │   │   ├── Authentication/              # IPasswordHasher.cs, IJwtProvider.cs
    │   │   │   │   ├── Caching/                     # ICacheService.cs (Not Implemented)
    │   │   │   │   ├── ExternalServices/            # External Services
    │   │   │   │   └── Repositories/                # IUserRepository.cs
    │   │   │   └── Models/                          # ApiResponse.cs, PagedList.cs, PaginationParams.cs
    │   │   │
    │   │   ├── Features/
    │   │   │   └── Auth/                            # All Use Cases related to Auth
    │   │   │       └── Commands/                    # Login and Refresh Token                            
    │   │   │   └── Users/                           # All Use Cases related to Users
    │   │   │       ├── Commands/                    # Request DTO / Command Object, Business logic handler, Command FluentValidation rules
    │   │   │       │   └── CreateUser.cs            # e.g., for Commands
    │   │   │       └── Queries/                     # Response DTO / Query Object, Business logic handler              
    │   │   │           └── GetUserById              # e.g., for Queries
    │   │   │
    │   │   ├── DependencyInjection.cs               # Cortex.Mediator & FluentValidation Service Registration
    │   │   └── human_recognition.Application.csproj
    │   │
    │   ├── human_recognition.Infrastructure/                # [Infrastructure Layer: External Services, Database, & Tools]
    │   │   ├── Authentication/                      # BcryptPasswordHasher.cs, JwtProvider.cs (Access and Refresh Token)
    │   │   ├── Caching/                             # RedisCacheService.cs or other caching services (Not implemented)
    │   │   ├── Data/                                
    │   │   │   ├── Configurations/                  # EF Core Entity Configurations
    │   │   │   ├── Repositories/                    # UserRepository.cs (IUserRepository Implementation)
    │   │   │   ├── Extensions/                      # Queryable Extensions for convert to Paged List.
    │   │   │   ├── DbTransactionManager.cs          # Database Transaction Manager
    │   │   │   └── ApplicationDbContext.cs          # Main EF Core DbContext
    │   │   ├── ExternalServices/                    # EmailService.cs
    │   │   ├── Logging/                             # Custom Logging like Elasticsearch and Custom Serilog
    │   │   ├── Migration/                           # Auto-generated EF Core Migrations
    │   │   ├── human_recognition.Infrastructure.csproj
    │   │   └── DependencyInjection.cs               # DB, Redis, and Infrastructure implementations registration
    │   │
    │   └── human_recognition.API/                           # [Presentation Layer / Entry Point]
    │       ├── Controllers/v1/                      # UsersController.cs
    │       ├── Extensions/                          # SwaggerSetup.cs, SerilogSetup.cs, AuthSetup.cs, CorsSetup.cs, VersioningSetup.cs, HealthCheck.cs
    │       ├── Health/                              # Database, Cache, Server, and others unit health check
    │       ├── Middlewares/                         # RequestLoggingMiddleware.cs, JwtMiddleware.cs
    │       ├── Properties/                          # launchSettings.json
    │       ├── logs/                                # Logs result
    │       ├── appsettings.json                     # Main configuration (Connection strings, Logging levels)
    │       ├── appsettings.Development.json         # Development environment overrides
    │       └── Program.cs                           # Composition Root: Dependency Injection & Middleware Pipeline
    │
    ├── tests/
    │   ├── human_recognition.UnitTests/                     # Tests for Domain & Application logic (No I/O dependencies)
    │   ├── human_recognition.IntegrationTests/              # Tests for API Endpoints & actual Database integration
    │   └── human_recognition.ArchitectureTests/             # NetArchTest (Ensures architectural boundaries are not violated)
    ├── .gitignore
    ├── global.json
    ├── .template.config
    ├── LICENSE
    └── README.md
```

## Dependency
````mermaid
graph TD
    API[human_recognition.API<br>Presentation / Entry Point] --> App[human_recognition.Application<br>Use Cases / CQRS]
    API --> Infrastructure[human_recognition.Infrastructure<br>External Services / DB]
    Infrastructure --> App
    App --> Domain[human_recognition.Domain<br>Entities / Core Logic]
    Infrastructure --> Domain

    %% Custom Colors with Black Text (color:#333) for visibility
    style Domain fill:#ffd966,stroke:#333,stroke-width:2px,color:#333
    style App fill:#f4cccc,stroke:#333,stroke-width:2px,color:#333
    style Infrastructure fill:#cfe2f3,stroke:#333,stroke-width:2px,color:#333
    style API fill:#d9ead3,stroke:#333,stroke-width:2px,color:#333
````

## Getting Started (Local Development)
### Preparation
```bash
    # Clone repository by following command.
    git clone [https://github.com/HanashiroYuriku/human_recognition.git](https://github.com/HanashiroYuriku/human_recognition.git)
    
    # 2. Change directory to root project.
    cd human_recognition
```

### 1. Database Setup
Ensure MySQL is installed and running on your local machine. Create a new, empty database (e.g., `human_recognition_db`).

### 2. `appsettings` Setup
Navigate to `src/human_recognition.API` and configure `appsettings.json` (or `appsettings.Development.json`) with your database credentials and JWT Secret.

### 3. Package Install
Run the following command to install all package needed.
```bash
dotnet restore
```

### 4. Run the Server
Use the following command to start the server. Makesure you are on `human_recognition/src/human_recognition.API` directory
```bash
dotnet run
# Your server will run on port: 5098 (or as configured in launchSettings.json)
```
### 5. Healty Check 
You can check your *server* and *database* health by accessing:
```
GET http://localhost:5098/api/health
```

### 6. Access Swagger
View the API documentation and test endpoints via Swagger at:
```text
http://localhost:5098/swagger
```

### Other CLI Commands
#### Database Migrations (Must be run from the Root Project):
```bash
# 1. Create table blueprints
dotnet ef migrations add InitialCreate --project src/human_recognition.Infrastructure --startup-project src/human_recognition.API

# 2. Execute table creation to MySQL
dotnet ef database update --project src/human_recognition.Infrastructure --startup-project src/human_recognition.API
```

## Prepush Checklist
Before committing and push your code, you are encouraged to run the verification suite locally:
```bash
# 1. Format code according to .editorconfig standards
dotnet format

# 2. Compile the application and run static analysis
dotnet build

# 3. Ensure all unit tests pass perfectly (Unit Testing not implemented yet)
dotnet test
```

## Standardized Pagination

This template includes a built-in, reusable pagination system for both the API consumers and the backend developers. It uses `PaginationParams` to keep queries clean and `PagedList<T>` to provide comprehensive metadata in the API response.

### 1. How to Use (API Consumer)
Pagination parameters are passed via the URL query string. If no parameters are provided, the system defaults to `pageNumber=1` and `pageSize=10`.

```http
# Get default (Page 1, 10 items)
GET /api/v1/users

# Get specific page and size
GET /api/v1/users?pageNumber=2&pageSize=5
```
Expected JSON Response:
```json
{
  "items": [
    { "id": "...", "name": "John Doe" },
    { "id": "...", "name": "Jane Doe" }
  ],
  "pageNumber": 2,
  "pageSize": 5,
  "totalCount": 45,
  "totalPages": 9,
  "hasNextPage": true,
  "hasPreviousPage": true
}
```

### 2. How to Use (Backend Developer)
To implement pagination in your CQRS flow, simply inherit from the PaginationParams record in your Query.

The Query:
```C#
// Inheriting PaginationParams automatically adds PageNumber and PageSize properties
public record GetAllUsersQuery : PaginationParams, IQuery<PagedList<UserResponse>>;
```
The Handler:
```C#
public async Task<PagedList<UserResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
{
    // 1. Pass the entire 'request' to the repository
    var pagedData = await _repository.GetAllAsync(request, cancellationToken);

    // 2. Map your Entity to DTO
    var dtoItems = pagedData.Items.Select(x => new UserResponse(x.Id, x.Name)).ToList();

    // 3. Return the mapped PagedList
    return new PagedList<UserResponse>(dtoItems, pagedData.TotalCount, pagedData.PageNumber, pagedData.PageSize);
}
```
* In your infrastructure layer, use the provided `ToPagedListAsync()` EF Core extension method to easily convert any `IQueryable` into a `PagedList<T>`

## Base Entity & Audit Trails

To ensure consistency across the database, all domain entities should inherit from `BaseEntity`. It provides built-in standardized primary keys, audit logging, and soft-delete mechanisms.

### Key Features
- **Audit Trails:** Automatically tracks `CreatedAt`, `CreatedBy`, `UpdatedAt`, and `UpdatedBy`.
- **Soft Delete Pattern:** Includes an `IsDeleted` flag and a `MarkAsDeleted()` method to safely archive records without hard-deleting them from the database.
- **Flexible Primary Keys:** Defaults to `Guid`, but supports any data type (e.g., `int`, `string`) via generics.

### How to Use

**1. Creating a Standard Entity (Guid as Primary Key)**
Simply inherit from `BaseEntity`. The `Id` will automatically be generated as a new `Guid`.
```csharp
public class Product : BaseEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

**2. Creating an Entity with a Custom Primary Key**
If you need an int or string primary key, inherit from `BaseEntity<TId>`.
```csharp
public class Category : BaseEntity<int>
{
    public string Name { get; set; }
}
```

**3. Updating & Soft Deleting in Handlers**
The base class provides clean methods to encapsulate audit logic, keeping your Application layer neat.
```csharp
// Updating an entity
product.Name = "New Laptop";
product.SetUpdateAudit(currentUser.Id); // Sets UpdatedAt and UpdatedBy

// Soft deleting an entity
product.MarkAsDeleted(currentUser.Id);  // Sets IsDeleted = true, UpdatedAt, and UpdatedBy
```

## Credits & Acknowledgements

* **Hanashiro Yuriku | Dionisius Geovanni Caesario**
* **Gemini** - *AI Pair Programmer*
  Served as a technical consultant and coding assistant, providing architectural insights and collaborative debugging throughout the development journey.
* #### This project was generated using the [C&#111;lumbina Template](https://github.com/HanashiroYuriku/C&#111;lumbina).