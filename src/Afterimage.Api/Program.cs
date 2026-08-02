using Afterimage.Api.Mapping;
using Afterimage.Domain;

var builder = WebApplication.CreateBuilder(args);

// Connection string comes from the host's configuration (appsettings → env vars →
// user-secrets); the actual creds live in env/user-secrets, never in appsettings.
// Domain receives only the resolved string (ADR-0003).
builder.Services.AddAfterimagePersistence(
    builder.Configuration.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException(
            "Missing connection string 'Postgres'. Set ConnectionStrings__Postgres " +
            "(env var) or a user-secret."));

builder.Services.AddSingleton<PhotoMapper>();

var app = builder.Build();

app.Run();
