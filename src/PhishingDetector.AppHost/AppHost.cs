var builder = DistributedApplication.CreateBuilder(args);

var postgresUser = builder.AddParameter("postgres-username", "postgresUser");
var postgresPassword = builder.AddParameter("postgres-password", "postgresPW", secret: true);

var postgres = builder.AddPostgres(
 "postgres",
 postgresUser,
 postgresPassword
)
    .WithPgAdmin()
    .WithHostPort(5432)
.WithImage("postgres:15")
.WithDataVolume("phishing-postgres-data");

var database = postgres.AddDatabase("phishingdetectordb");
var phishingDetectorApi = builder.AddProject<Projects.PhishingDetector_API>("phishing-detector-api")
    .WithReference(database)
    .WaitFor(database)
    .WithHttpHealthCheck("/health");


var phishingDetectorWeb = builder.AddProject<Projects.PhishingDetector_Web>("phishing-detector-web")
    .WithReference(phishingDetectorApi)
    .WaitFor(phishingDetectorApi)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
