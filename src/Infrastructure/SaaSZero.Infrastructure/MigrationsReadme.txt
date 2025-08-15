Migrations are created from Infrastructure.
Use:
  dotnet tool run dotnet-ef migrations add InitialCreate --project src/Infrastructure/SaaSZero.Infrastructure/SaaSZero.Infrastructure.csproj --startup-project src/API/SaaSZero.API/SaaSZero.API.csproj --output-dir Persistence/Migrations
