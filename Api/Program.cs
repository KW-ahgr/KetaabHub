using Api.Security;
using App.Books;
using App.Validation;
using Dom.Repo;
using Domin;
using FluentValidation;
using Infra.Persist;
using Infra.RepositoryImpl;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = "Api.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    var xmlFile2 = "App.xml";
    var xmlPath2 = Path.Combine(AppContext.BaseDirectory, xmlFile2);
    c.IncludeXmlComments(xmlPath2);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<BoooooooooksDBContext>(options =>
     options.UseSqlServer(connectionString));

builder.Services.AddDbContext<BoooooooooksDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwt = builder.Configuration.GetSection("Jwt");
var issuer = jwt["Issuer"];
var audience = jwt["Audience"];
var key = jwt["Key"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                var jti = ctx.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(jti)) return;
                var store = ctx.HttpContext.RequestServices.GetRequiredService<IRefreshTokenStore>();
                var revoked = store.IsAccessJtiRevokedAsync(jti);
                if (revoked) ctx.Fail("Access token revoked.");
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
builder.Services.AddScoped<UserService>();


builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddScoped<IGenericRepository<Book>, BookRepository>();
builder.Services.AddScoped<IGenericRepository<BookCat>, BookCatRepository>();
builder.Services.AddScoped<IGenericRepository<BookPic>, BookPicRepository>();
builder.Services.AddScoped<IGenericRepository<Comment>, CommentRepository>();
builder.Services.AddScoped<IGenericRepository<Person>, PersonRepository>();
builder.Services.AddScoped<IGenericRepository<User>, UserRepository>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(BooksAccMrk).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CatsAccMrk).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CommentsAccMrk).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(PersonsAccMrk).Assembly);
});

builder.Services.AddValidatorsFromAssembly(typeof(ValdMRK).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));


var app = builder.Build();

app.UseMiddleware<ValidationExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Book-API v1");
        c.RoutePrefix = string.Empty; 
    });
}

//app.UseHttpsRedirection();


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
