using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using SAPO.utilities;
using zkemkeeper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy
            .AllowAnyOrigin()        // If you need credentials, replace with .SetIsOriginAllowed(_ => true) and use .AllowCredentials()
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
});

// Dependency Injection
builder.Services.AddSingleton<ZkemClient>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<ZkemClient>>();
    // Simple event handler (replace with dispatcher if needed)
    return new ZkemClient(logger, (sender, evt) =>
    {
        logger.LogInformation("Device event: {Event}", evt);
    });
});
builder.Services.AddSingleton<IZKEM>(sp => sp.GetRequiredService<ZkemClient>()); // optional alias
builder.Services.AddSingleton<DeviceManipulator>(); // stateless helper
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.WriteIndented = true;
    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;   
});


var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Order: CORS before endpoints that use it
app.UseCors("AllowAll");

// (Optional) auth / static / etc. here later

// Minimal probe / health
app.MapGet("/", () => Results.Ok("API OK"));

// Versioned or plain API controllers
app.MapControllers()
    .WithMetadata(); // or [EnableCors("AllowAll")] on controllers;;

app.Run();