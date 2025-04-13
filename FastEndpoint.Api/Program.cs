using ApiPlayground.Data.Context;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

var bld = WebApplication.CreateBuilder();
bld.Services.AddFastEndpoints();

bld.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(bld.Configuration.GetConnectionString("DefaultConnection")));

bld.Services.AddMemoryCache();

var app = bld.Build();
app.UseFastEndpoints();
app.Run();