using LM.API.Commons;
using LM.API.Infrastructure;
using LM.Models.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// longtran 20240120 Phần này mình đăng ký interface nào thì mình đăng ký ở đây 
//<
builder.Services.AddSingleton<IDateTimeService, VietnamDateTimeService>();
builder.Services.AddScoped<IBMDbContext, LMDbContext>(); // đăng ký kết nối db
builder.Services.AddRegisterServices(); // đăng ký service


//AddCors -> cho client nào được gọi. hiện tại là ALL
builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                                                                    .AllowAnyMethod()
                                                                     .AllowAnyHeader())); // 

// longtran 20240120 add service Authen
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
{
    option.SaveToken = true;
    option.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration.GetSection("Jwt:JwtIssuer").Value,
        ValidAudience = builder.Configuration.GetSection("Jwt:JwtAudience").Value,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:JwtSecurityKey").Value + "")),
        ClockSkew = TimeSpan.Zero
    };
});
//>



var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//>
app.Run();
