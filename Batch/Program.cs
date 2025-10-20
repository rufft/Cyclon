using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Batch.Context;
using Batch.GraphQL;
using Batch.Models.Displays;
using Cyclone.Common.SimpleDatabase;
using Cyclone.Common.SimpleLogger.Extensions;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString("DefaultConnection");

builder.Host.UseSimpleLogging(
    serviceName: "Batch",
    connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
    logFilePath: "logs/batch-.txt"
);

builder.Services.AddSimpleServices();
builder.Services.AddScoped<Query>();
builder.Services.AddScoped<Mutation>();

// builder.Services.AddRabbitMqSoftDelete(opt => 
//     builder.Configuration.GetSection("RabbitMQ").Bind(opt));

//builder.Services.AddRabbitMqSoftDelete();

builder.Services
    .AddGraphQLServer()
    .AddSimpleGraphQlLogging()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .ModifyRequestOptions(opts =>
    {
        opts.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    });


SoftDeletePolicyRegistry.RegisterCollection<Batch.Models.Batch, Display>(b => b.Displays);
SoftDeletePolicyRegistry.RegisterCollection<DisplayType, Batch.Models.Batch>(dt => dt.Batches);


builder.Services.AddSimpleDbContext<BatchDbContext>(options =>
    options.UseNpgsql(connectionString, b =>
        b.MigrationsAssembly(typeof(BatchDbContext).Assembly.FullName)));


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.WebHost.UseUrls("http://0.0.0.0:5299");

var app = builder.Build();

app.UseSimpleRequestLogging();

app.UseCors();

if (app.Environment.IsDevelopment())
{
}

app.UseRouting();

app.MapGraphQL("/graphql");

//app.UseWebSockets();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/graphql"));

try
{
    Log.Information("Starting application {ServiceName}", "MyMicroservice");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}