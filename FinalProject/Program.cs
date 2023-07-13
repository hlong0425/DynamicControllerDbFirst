using API;
using Domain;
using FinalProject;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

DomainsBuilder.GenerateObjectFromTable(builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddControllers(o =>
{
    o.Conventions.Add(new RouteConvention());
}).ConfigureApplicationPartManager(m => m.FeatureProviders.Add(new GenericTypeControllerFeatureProvider()));


builder.Services.AddDbContext<AppDbContext>(o =>
{   
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("API"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
