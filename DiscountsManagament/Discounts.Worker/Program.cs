// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.IRepositories;
using Discounts.Domain.Entity;
using Discounts.Infrustructure.Repositories;
using Discounts.Persistance.Context;
using Discounts.Worker.BackgroundWorkers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .Build();

try
{
    await CreateHostBuilder(args).Build().RunAsync().ConfigureAwait(false);
}
catch (Exception ex)
{
    Console.WriteLine($"Worker failed to start: {ex.Message}");
    throw;
}
static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseWindowsService() //can run in the background even when no one is logged in to the pc
        .ConfigureServices((hostContext, services) =>
        {
            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    hostContext.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            // Unitofwork and repositories
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
