using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Batch.Context;
using Batch.GraphQL;
using Batch.Models.Displays;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Cyclone.Common.SimpleSoftDelete.Abstractions;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<BatchDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString, b =>
        b.MigrationsAssembly(typeof(BatchDbContext).Assembly.FullName));
    options.AddInterceptors(sp.GetRequiredService<SoftDeletePublishInterceptor>());
});

builder.Services.AddSimpleServices();
builder.Services.AddScoped<Query>();
builder.Services.AddScoped<Mutation>();

SoftDeletePolicyRegistry.RegisterCollection<Batch.Models.Batch, Display>(b => b.Displays);
SoftDeletePolicyRegistry.RegisterCollection<DisplayType, Batch.Models.Batch>(dt => dt.Batches);

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

builder.Services.AddScoped<IDeletionEventPublisher, HcDeletionEventPublisher>();
builder.Services.AddScoped<SoftDeletePublishInterceptor>(sp =>
    new SoftDeletePublishInterceptor(
        sp.GetRequiredService<IDeletionEventPublisher>(),
        originService: "Batch"));


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

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
}

app.UseRouting();

app.MapGraphQL("/graphql");

app.UseWebSockets();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/graphql"));

app.Run();