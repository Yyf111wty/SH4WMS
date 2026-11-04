
using Microsoft.OpenApi.Models;
using test.webapi;

var builder = WebApplication.CreateBuilder(args);

//log4net 引入
builder.Logging.AddLog4Net("Config/log4net.config");
// Add services to the container.

builder.Services.AddControllers();

//注册支持跨域
#region  注册支持跨域
//builder.Services.AddCors(option => option.AddPolicy("allcors", corsBuilder => {

//    corsBuilder.AllowAnyHeader()
//                .AllowAnyOrigin()
//                .AllowAnyMethod();


//}));
builder.Services.AddCors(option => option.AddPolicy("allcors", corsBuilder =>
{
    corsBuilder.AllowAnyHeader()
        .AllowAnyOrigin()
        .AllowAnyMethod();
}));
#endregion
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
#region   swagger文档处理了部分

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WMS后台API接口文档",
        Version = "v1",
        Description = "WMS后台API接口文档"
    });
    var xml = Path.Combine(AppContext.BaseDirectory, $"{AppDomain.CurrentDomain.FriendlyName}.xml");
    if (File.Exists(xml))
    {
        option.IncludeXmlComments(xml, true);
    }
});
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(option => 
//{

//    //多版本注册
//    foreach (var version in typeof(ApiVersions).GetEnumNames())
//    {
//        option.SwaggerDoc(version, new OpenApiInfo()
//        {
//            Title = $"通用文档API",
//            Version = version,
//            Description = $"通用版本的coreapi版本{version}"

//        });
//        //option.SwaggerDoc("v1", new OpenApiInfo()
//        //{
//        //    Title = $"通用文档API",
//        //    Version = "v1",
//        //    Description = $"通用版本的coreapi版本v1"

//        //});
//    }

//    //xml文档绝对路径
//    var file = Path.Combine(AppContext.BaseDirectory, $"{AppDomain.CurrentDomain.FriendlyName}.xml");
//    option.IncludeXmlComments(file,true);
//    //是否显示注释
//    option.OrderActionsBy(option => option.RelativePath);

//    #region  支持jwt  token授权
//    //option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    //{

//    //    Description = "请输入token，格式为Bearer  xxxxxxxx (注意中间必须有空格)",
//    //    Name = "Authorization",
//    //    In = ParameterLocation.Header,
//    //    Type = SecuritySchemeType.ApiKey,
//    //    BearerFormat = "JWT",
//    //    Scheme = "Bearer"
//    //});

//    //option.AddSecurityRequirement(new OpenApiSecurityRequirement
//    //{
//    //    { 
//    //      new  OpenApiSecurityScheme 
//    //      {
//    //        Reference = new OpenApiReference()
//    //        { 
//    //        Type = ReferenceType.SecurityScheme,
//    //        Id ="Bearer"
//    //        }
//    //      }

//    //    }

//    //});
//    #endregion

//});
#endregion

//builder.Services.AddScoped<ISqlSugarClient>(p =>
//{
//    SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
//    {
//        DbType = DbType.MySql,
//        IsAutoCloseConnection = true,
//        ConnectionString = builder.Configuration.GetConnectionString("conn")
//    });
//    return db;
//});

builder.Services.AddSignalR();

var app = builder.Build();


//使用跨域
app.UseCors("allcors");
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "WMS后台API接口文档");
});
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwaggerExt("WMS后台API接口文档");
//    //app.UseSwagger();
//    //app.UseSwaggerUI(options => {
//    //    foreach (var version in typeof(ApiVersions).GetEnumNames())
//    //    {
//    //        options.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{version}");
//    //    }
//    //    //options.SwaggerEndpoint("/swagger/v1/swagger.json", "通用文档API[v1]");
//    //});
//}

app.UseAuthorization();


app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.Run();