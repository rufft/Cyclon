using System.Text.Json.Serialization;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Cyclone.Common.SimpleSoftDelete.Abstractions;
using Measurement.Context;
using Measurement.GraphQL;
using Microsoft.EntityFrameworkCore;
using Query = Measurement.GraphQL.Query;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<MeasureDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString, b =>
        b.MigrationsAssembly(typeof(MeasureDbContext).Assembly.FullName));
    options.AddInterceptors(sp.GetRequiredService<SoftDeletePublishInterceptor>());
});

builder.Services.AddSimpleServices();
builder.Services.AddScoped<Query>();
builder.Services.AddScoped<Mutation>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddSubscriptionType<DeletionSubscription>()
    .AddInMemorySubscriptions()
    .ModifyRequestOptions(opts =>
    {
        opts.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    });
builder.Services
    .AddBatchClient()
    .ConfigureHttpClient(client => client.BaseAddress = new Uri("http://localhost:5299/graphql"));


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddScoped<IDeletionEventPublisher, HcDeletionEventPublisher>();
builder.Services.AddScoped<SoftDeletePublishInterceptor>(sp =>
    new SoftDeletePublishInterceptor(
        sp.GetRequiredService<IDeletionEventPublisher>(),
        originService: "Measurement"));

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


var app = builder.Build();


app.UseRouting();

app.UseCors();

app.MapGraphQL("/graphql");

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/graphql"));

app.Run();