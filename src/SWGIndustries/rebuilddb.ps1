dotnet ef database drop -f
dotnet ef migrations remove
dotnet ef migrations add InitialMigration -o "data/migrations"
dotnet ef database update