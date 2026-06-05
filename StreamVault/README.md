# StreamVault — README

Project overview
- StreamVault is an ASP.NET Core web API (target: .NET 10) consisting of a Web project (StreamVault) and companion class library projects: SV.Common, SV.Service, SV.Store, and SV.Data. The solution uses a stores/services separation: "Store" components access/persist data (data-layer), "Service" components implement business logic and orchestrate stores, and controllers expose HTTP endpoints.

Repository layout (important folders)
- /StreamVault/StreamVault (Web API)
  - Program.cs — application bootstrap, middleware, DI registrations and Swagger.
  - Controllers/ — HTTP controllers for each feature (Auth, User, Movie, Genre, Plan, Subscription, Payment, WatchHistory, Watchlist, Review, Role, Dashboard, etc.).
  - Controllers/Middleware/ExceptionMiddlewareImpl.cs — global exception middleware used by the app.
  - appsettings.json / appsettings.Development.json — configuration (JWT settings, DB connection string, etc.).

- /SV.Common
  - DTOs/ — request and response DTOs used across projects (CreateMovieRequest, CreatePlanRequest, UpdateUserDto, UserResponseDto, etc.).
  - Validators/ — FluentValidation validators for request models.
  - Utilities/ — JWT helper and other shared utilities.

- /SV.Service
  - Abstractions/ — service interfaces (IUserService, IMovieService, IPlanService, IWatchHistoryService, etc.).
  - Implementations/ — service implementations that call stores (currently many implementations are minimal stubs to permit compilation and run).

- /SV.Store
  - Abstractions/ — store interfaces that encapsulate persistence operations (IUserStore, IMovieStore, IPlanStore, etc.).
  - Implementations/ — concrete store implementations (currently minimal/no-op stubs; replace with DB access code as needed).

- /SV.Data
  - (Data-layer helpers / DB scripts) — connection factories and SQL helpers (IDbConnectionFactory, SqlConnectionFactory, etc.).

Key concepts and how the app works
- Controller -> Service -> Store pattern:
  - Controllers receive HTTP requests and validate/authorize them.
  - Controllers call into services (I*Service) which implement business rules.
  - Services call stores (I*Store) to persist or read data.

- Dependency Injection (Program.cs):
  - All services and stores are registered in Program.cs via builder.Services.AddScoped<>. The DI graph is resolved at runtime by ASP.NET Core.
  - Jwt authentication is configured via Microsoft.AspNetCore.Authentication.JwtBearer using settings in configuration.

- DTOs and validation:
  - Shared DTOs live in SV.Common and are public so they can be used across projects. FluentValidation validators are used to validate requests automatically.

Implemented features (current state)
- Authentication scaffolding (JWT configuration and Auth controller present).
- User management endpoints (controllers exist); service and store layers stubbed to compile.
- Movies: listing and creation endpoints (controllers + services + stores present; core logic is stubbed).
- Genres: listing and create endpoint (stubbed service/store).
- Plans: listing and create endpoints (uses CreatePlanRequest DTO; stubs provided).
- Subscriptions: creation and listing endpoints (stubs).
- Watch history: insert and paged listing endpoints (stubs).
- Watchlist: add/remove/get endpoints (stubs).
- Reviews: add and list reviews (stubs).
- Payments: insert payment endpoint (stubbed).
- Roles & Dashboard: management and reporting endpoints (stubbed).

Important implementation notes (what is stubbed)
- Many service/store implementations were intentionally added as minimal stubs (returning Task.CompletedTask, empty lists, or simple objects) so the solution compiles and runs. These are located under SV.Service/Implementations and SV.Store/Implementations.
- Some original files from another codebase (Demo.*) were replaced or removed where their namespaces conflicted with the expected SV.* namespaces. If you have real data access code, restore it into SV.Store implementations.
- A few service/store method signatures were temporarily relaxed (e.g., some Create* methods take object) to avoid transient cross-project type resolution issues during the repair work. You may revert those signatures to concrete DTO types (SV.Common.DTOs.*) once you implement the real methods.

How to build and run
1. Ensure .NET 10 SDK is installed.
2. From solution root (where StreamVault.csproj lives), run:
   - dotnet build
   - dotnet run --project StreamVault.csproj
3. Open Swagger (in Development) at https://localhost:{port}/swagger to explore available endpoints.

Development workflow & next steps
- Replace stubs with real implementations:
  - Implement data access in SV.Store.Implementations (use Dapper/EF/ADO.NET as needed) and return concrete DTOs from SV.Common.
  - Implement business rules in SV.Service.Implementations and use typed DTOs in service signatures.
  - Revert any relaxed method signatures to the typed DTO versions.

- Tests: add unit tests for services and integration tests for stores (not included currently).
- Database: wire connection string in appsettings.json and implement the SQL / ORM code in SV.Store implementations.

Notes & caveats
- The current repository state is intentionally made to compile quickly by using no-op/stub implementations. This is a temporary repair to get a working build and allow iterative development.
- Keep shared models/DTOs in SV.Common and make them public so services and controllers can depend on stable types.
- Use the Program.cs registration area to add or remove DI registrations as you implement/remove features.

Branching and commits
- Work was performed on branch: cleanup/move-to-root. Keep using branches for large changes and small commits with clear messages.

Contact & contribution
- Replace stubbed methods with real implementations, run the app and tests, then open a PR on branch cleanup/move-to-root or create feature branches per area (users, movies, payments, subscriptions).

If you want, I can:
- Generate a prioritized TODO list for replacing stubs with real logic.
- Create example implementation for one feature (e.g., Movie store using Dapper + SQL) to show how to wire data access end-to-end.
