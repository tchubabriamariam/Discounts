using Discounts.Application.IRepositories;
using Discounts.Domain.Entity;
using Discounts.Infrustructure.Repositories;
using Discounts.Persistance.Context;
using Discounts.Worker.BackgroundWorkers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

try
{
    await CreateHostBuilder(args).Build().RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Worker failed to start: {ex.Message}");
    throw;
}

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseWindowsService()
        .ConfigureServices((hostContext, services) =>
        {
            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    hostContext.Configuration.GetConnectionString("DefaultConnection")));

            // Identity (needed for UserManager if workers need it)
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Infrastructure (UnitOfWork, Repositories)

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IOfferRepository, OfferRepository>();
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IMerchantRepository, MerchantRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IGlobalSettingsRepository, GlobalSettingsRepository>();
            // Background Workers
            services.AddHostedService<ReservationCleanupWorker>();
            services.AddHostedService<OfferExpirationWorker>();
        });