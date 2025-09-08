using System.Text.Json.Serialization;
using Cyclone.Common.SimpleService;
using Cyclone.Common.SimpleSoftDelete;
using Cyclone.Common.SimpleSoftDelete.Extensions;
using Measurement.Context;
using Microsoft.EntityFrameworkCore;
using Query = Measurement.GraphQL.Query;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString("DefaultConnection");

builder.Services.AddSimpleServices();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    //.AddMutationType<Mutation>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddDeletionSubscriptions()
    .ModifyRequestOptions(opts =>
    {
        opts.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    });

builder.Services.AddSoftDeleteEventSystem(() =>
{
}, originServiceName: "Measurement");

builder.Services.AddDbContextFactory<MeasureDbContext>(options =>
{
    options.UseNpgsql(connectionString, b =>
        b.MigrationsAssembly(typeof(MeasureDbContext).Assembly.FullName));
    options.AddInterceptors(new SoftDeletePublishInterceptor("Measurement"));
});

builder.Services.AddSubscription("OnDisplayDelete", async (ev, sp, ct) =>
{
    using var scope = sp.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MeasureDbContext>();
    await db.CieMeasures
        .Where(m => m.DisplayId == ev.EntityId && !m.IsDeleted)
        .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true), ct);
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

app.UseWebSockets();

app.MapGraphQL("/graphql");

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/graphql"));

app.Run();