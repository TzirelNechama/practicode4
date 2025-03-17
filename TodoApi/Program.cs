using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
    new MySqlServerVersion(new Version(8, 0, 21))));
// builder.Services.AddSingleton<ToDoDbContext>();
builder.Services.AddOpenApi();
// builder.Services.AddEndpointsApiExplorer();
// swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
        Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});
var app = builder.Build();
app.UseCors("AllowSpecificOrigin");

builder.Services.AddEndpointsApiExplorer();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
object CreateJwt(User user)
{
    var claims = new List<Claim>(){
        new Claim("id",user.Id.ToString()),
        new Claim("name",user.Name),
        new Claim("email", user.Email)

    };
    var secretKeyValue = builder.Configuration.GetValue<string>("JWT:Key") ?? throw new InvalidOperationException("JWT Key is not configured.");
    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyValue)); var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
    var tokeOptions = new JwtSecurityToken(
        issuer: builder.Configuration.GetValue<string>("JWT:Issuer"),
        audience: builder.Configuration.GetValue<string>("JWT:Audience"),
        claims: claims,
        expires: DateTime.Now.AddDays(30),
        signingCredentials: signinCredentials
    );
    var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
    return new { Token = tokenString };

}

app.MapPost("/login", ([FromBody] Login user, ToDoDbContext service) =>
{

    var uu = service.Users.FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);
    if (uu != null)
    {
        var jwt = CreateJwt(uu);
        return Results.Ok(jwt);
    }

    return Results.NotFound("User not registered.");
});
app.MapPost("/register", async ([FromBody] Login user, ToDoDbContext service) =>
{
    var name = user.Email.Split("@")[0];
    var newUser = new User { Name = name, Email = user.Email, Password = user.Password };
    await service.Users.AddAsync(newUser); // Use AddAsync for async operation
    await service.SaveChangesAsync(); // Save changes to the database
    var jwt = CreateJwt(newUser);
    return Results.Ok(jwt);
});

// app.MapGet("/public", (Application options) =>
// {
//     var application = options.Value;
//     return Results.Ok(application);
// });
app.MapGet("/{id}", [Authorize] ([FromRoute] int id, ToDoDbContext service) =>
{
    return service.Items.FirstOrDefault(x => x.Id == id);
});

app.MapGet("/getTasks", [Authorize] async (ToDoDbContext service) => { return await service.Items.ToListAsync(); });
app.MapGet("/allUsers", (ToDoDbContext service) => { return service.Users.ToListAsync(); });

app.MapPost("/addItem", [Authorize] async (Item item, ToDoDbContext db) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/{item.Id}", item);
});


app.MapPut("/{id}", [Authorize] async (int id, bool IsComplete, ToDoDbContext db) =>
{
    var item = db.Items.FirstOrDefault(x => x.Id == id);
    if (item != null)
    {
        item.IsComplete = IsComplete;
        await db.SaveChangesAsync();
    }
    return item;
});

app.MapDelete("/{id}", [Authorize] ([FromRoute] int id, ToDoDbContext service) =>
{
    var item = service.Items.FirstOrDefault(x => x.Id == id);
    if (item != null)
        service.Items.Remove(item);
    service.SaveChanges();
});

app.Run();
