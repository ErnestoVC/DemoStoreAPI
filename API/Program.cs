using API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var MyCorsPolicity = "_myCorsPolicity";

var builder = WebApplication.CreateBuilder(args);

//Add CorsPolicies
builder.Services.AddCors(options =>{
    options.AddPolicy(name: MyCorsPolicity, 
                                builder => {
                                    builder.WithOrigins("http://localhost:3000");
                                });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<StoreContext>(opt => {
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseAuthorization();

//app.MapControllers();

app.UseEndpoints(endpoints =>{
    endpoints.MapControllers().RequireCors(MyCorsPolicity);
});

/*
    Todo este código sirve para inicializar los datos de prueba,
    almacenar en memoria los cambios y ejecutar un mensaje de error.
    Antes de correr el servidor, limpia la memoria.
*/

var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<StoreContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

try
{
    context.Database.Migrate();
    DbInitializer.Initialize(context);
}
catch (Exception ex)
{
    logger.LogError(ex, "Problem migrating database");
}
finally{
    scope.Dispose(); // Clean up the cache memory
}

app.Run();
