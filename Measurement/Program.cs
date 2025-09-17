using System.Text.Json.Serialization;
using Cyclone.Common.SimpleClient;
using Cyclone.Common.SimpleDatabase;
using Cyclone.Common.SimpleDatabase.FileSystem;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Cyclone.Common.SimpleSoftDelete.Extensions;
using Measurement.Context;
using Measurement.GraphQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Query = Measurement.GraphQL.Query;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString("DefaultConnection");

builder.Services.AddSimpleServices();
builder.Services.AddScoped<Query>();
builder.Services.AddScoped<Mutation>();

// builder.Services.AddRabbitMqSoftDelete(config =>
//     builder.Configuration.GetSection("RabbitMQ").Bind(config));

// builder.Services.AddRabbitMqSoftDelete();

// builder.Services.AddDeletionSubscription("OnDisplayDelete", async (ev, sp, ct) =>
// {
//     using var scope = sp.CreateScope();
//     var db = scope.ServiceProvider.GetRequiredService<MeasureDbContext>();
//     await db.CieMeasures
//         .Where(m => m.DisplayId == ev.EntityId && !m.IsDeleted)
//         .ExecuteUpdateAsync(u => u.SetProperty(x => x.IsDeleted, true), ct);
// });

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .ModifyRequestOptions(opts =>
    {
        opts.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    });

builder.Services.Configure<GraphQlClientOptions>(
    "Batch",
    builder.Configuration.GetSection("GraphQlClients:Batch"));

builder.Services.AddHttpClient<SimpleClient>("Batch", (sp, http) =>
{
    var opts = sp.GetRequiredService<
            IOptionsMonitor<GraphQlClientOptions>>()
        .Get("Batch");

    http.BaseAddress = new Uri(opts.Endpoint);
    http.Timeout = opts.Timeout;
});

builder.Services.AddSimpleDbContext<MeasureDbContext>(options =>
{
    options.UseNpgsql(connectionString, b =>
        b.MigrationsAssembly(typeof(MeasureDbContext).Assembly.FullName));
    options.UseUploadedFiles();
});



builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


var app = builder.Build();


app.UseRouting();

app.UseCors();

//app.UseWebSockets();

app.MapGraphQL("/graphql");

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/graphql"));

app.Run();