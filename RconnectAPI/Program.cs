using RconnectAPI.Models;
using RconnectAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));
//Scoped
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<HobbyService>();
builder.Services.AddSingleton<HostService>();
builder.Services.AddSingleton<MeetingService>();

builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddPolicy("DEV", builder =>
{
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DEV");
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();