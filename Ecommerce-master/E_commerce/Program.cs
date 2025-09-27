using E_commerce.Models;
using Microsoft.AspNetCore.Authorization;
using E_commerce.Services;
using E_commerce.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace E_commerce
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.Configure<MongoDbContextSetting>
                (builder.Configuration.GetSection("E_commerceDatabase"));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularDev", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var rabbitSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>();
            builder.Services.AddSingleton(rabbitSettings);

            builder.Services.AddSingleton<IEventPublisher, EventPublisher>();

            builder.Services.AddSingleton(typeof(IMongoDbContext<>), typeof(MongoDbContext<>));
            builder.Services.AddSingleton(typeof(IDatabaseService<>), typeof(DatabaseService<>));

            builder.Services.AddSingleton<IRoleService, RoleService>();
            builder.Services.AddSingleton<IUserRoleService, UserRoleService>();
            builder.Services.AddSingleton<IProductService, ProductServices>();
            builder.Services.AddSingleton<ICategoryService, CategoryServices>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<ICartService, CartService>();

         


            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
         .AddJwtBearer(options =>
        {
        options.TokenValidationParameters = new TokenValidationParameters
        {

            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully.");
                return Task.CompletedTask;
            }
        };
    });

            builder.Services.AddAuthorization();



            // Add services to the container.

            builder.Services.AddControllers()
           .AddJsonOptions(
            options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            app.UseCors("AllowAngularDev");
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
