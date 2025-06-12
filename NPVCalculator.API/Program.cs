using NPVCalculator.Application;

var builder = WebApplication.CreateBuilder(args);

// Services
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

builder.Services.AddApplication();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                     ?? new[] { "https://localhost:5002" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddResponseCompression();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NPV Calculator API V1");
        c.RoutePrefix = string.Empty;
    });

    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("{Method} {Path}", context.Request.Method, context.Request.Path);
        await next();
    });
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseResponseCompression();
app.UseCors("AllowClient");
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();