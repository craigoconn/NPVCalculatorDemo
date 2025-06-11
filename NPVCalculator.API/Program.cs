using NPVCalculator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "NPV Calculator API",
        Version = "v1",
        Description = "API for calculating Net Present Value across multiple discount rates"
    }); 
});

builder.Services.AddInfrastructure();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins("https://localhost:5002") 
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Request/response compression for better performance
builder.Services.AddResponseCompression();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NPV Calculator API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseResponseCompression();
app.UseCors("AllowClient");
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        await next();
    });
}

app.MapControllers();

app.Run();