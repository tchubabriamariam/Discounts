// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Persistance.Seed
{
    public static class DiscountsSeed
    {
        // Fixed GUIDs so migrations don't regenerate them every time
        private const string AdminRoleId = "a1b2c3d4-0001-0000-0000-000000000001";
        private const string MerchantRoleId = "a1b2c3d4-0002-0000-0000-000000000001";
        private const string CustomerRoleId = "a1b2c3d4-0003-0000-0000-000000000001";

        private const string AdminUserId = "b1c2d3e4-0001-0000-0000-000000000001";
        private const string MerchantUserId = "b1c2d3e4-0002-0000-0000-000000000001";
        private const string CustomerUserId = "b1c2d3e4-0003-0000-0000-000000000001";

        public static void Seed(ModelBuilder builder)
        {
            SeedRoles(builder);
            SeedUsers(builder);
            SeedUserRoles(builder);
            SeedCategories(builder);
            SeedGlobalSettings(builder);
            SeedMerchant(builder);
            SeedOffers(builder);
        }

        private static void SeedRoles(ModelBuilder builder) =>
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = AdminRoleId,
                    Name = Roles.Admin,
                    NormalizedName = Roles.Admin.ToUpperInvariant(),
                    ConcurrencyStamp = AdminRoleId // to not egenerate extra migration file
                },
                new IdentityRole
                {
                    Id = MerchantRoleId,
                    Name = Roles.Merchant,
                    NormalizedName = Roles.Merchant.ToUpperInvariant(),
                    ConcurrencyStamp = MerchantRoleId
                },
                new IdentityRole
                {
                    Id = CustomerRoleId,
                    Name = Roles.Customer,
                    NormalizedName = Roles.Customer.ToUpperInvariant(),
                    ConcurrencyStamp = CustomerRoleId
                }
            );

        private static void SeedUsers(ModelBuilder builder)
        {
            var hasher = new PasswordHasher<ApplicationUser>();

            var adminUser = new ApplicationUser
            {
                Id = AdminUserId,
                UserName = "admin@discounts.ge",
                NormalizedUserName = "ADMIN@DISCOUNTS.GE",
                Email = "admin@discounts.ge",
                NormalizedEmail = "ADMIN@DISCOUNTS.GE",
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin",
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                SecurityStamp = Guid.NewGuid().ToString()
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123!");

            var merchantUser = new ApplicationUser
            {
                Id = MerchantUserId,
                UserName = "merchant@demo.ge",
                NormalizedUserName = "MERCHANT@DEMO.GE",
                Email = "merchant@demo.ge",
                NormalizedEmail = "MERCHANT@DEMO.GE",
                EmailConfirmed = true,
                FirstName = "Demo",
                LastName = "Merchant",
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                SecurityStamp = Guid.NewGuid().ToString()
            };
            merchantUser.PasswordHash = hasher.HashPassword(merchantUser, "Merchant@123!");

            var customerUser = new ApplicationUser
            {
                Id = CustomerUserId,
                UserName = "customer@demo.ge",
                NormalizedUserName = "CUSTOMER@DEMO.GE",
                Email = "customer@demo.ge",
                NormalizedEmail = "CUSTOMER@DEMO.GE",
                EmailConfirmed = true,
                FirstName = "Demo",
                LastName = "Customer",
                Balance = 500.00m,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                SecurityStamp = Guid.NewGuid().ToString()
            };
            customerUser.PasswordHash = hasher.HashPassword(customerUser, "Customer@123!");

            builder.Entity<ApplicationUser>().HasData(adminUser, merchantUser, customerUser);
        }

        private static void SeedUserRoles(ModelBuilder builder) =>
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = AdminUserId, RoleId = AdminRoleId },
                new IdentityUserRole<string> { UserId = MerchantUserId, RoleId = MerchantRoleId },
                new IdentityUserRole<string> { UserId = CustomerUserId, RoleId = CustomerRoleId }
            );

        private static void SeedCategories(ModelBuilder builder) =>
            builder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Restaurants",
                    Description = "Food and Drinks",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Category
                {
                    Id = 2,
                    Name = "Tourism",
                    Description = "Travel and Hotels",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Category
                {
                    Id = 3,
                    Name = "Beauty",
                    Description = "Salons and Spa",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Category
                {
                    Id = 4,
                    Name = "Sports",
                    Description = "Gyms and Training",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Category
                {
                    Id = 5,
                    Name = "Entertainment",
                    Description = "Cinema, Theater, Events",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Category
                {
                    Id = 6,
                    Name = "Healthcare",
                    Description = "Clinics and Pharmacy",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Category
                {
                    Id = 7,
                    Name = "Education",
                    Description = "Courses and Trainings",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Category
                {
                    Id = 8,
                    Name = "Automotive",
                    Description = "Car Services",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

        private static void SeedGlobalSettings(ModelBuilder builder) =>
            builder.Entity<GlobalSettings>().HasData(
                new GlobalSettings
                {
                    Id = 1,
                    ReservationDurationMinutes = 30,
                    MerchantEditWindowHours = 24,
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedByAdminId = AdminUserId
                }
            );

        private static void SeedMerchant(ModelBuilder builder) =>
            builder.Entity<Merchant>().HasData(
                new Merchant
                {
                    Id = 1,
                    UserId = MerchantUserId,
                    CompanyName = "Demo Restaurant Group",
                    Description = "Demo merchant account for testing",
                    ContactEmail = "merchant@demo.ge",
                    PhoneNumber = "+995 599 000 001",
                    Address = "Tbilisi, Georgia",
                    IsVerified = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

        private static void SeedOffers(ModelBuilder builder) =>
            builder.Entity<Offer>().HasData(
                new Offer
                {
                    Id = 1,
                    MerchantId = 1,
                    CategoryId = 2,
                    Title = "Miller’s Planet Escape: 1 Hour Here, 7 Years Savings",
                    Description = "Take a leap through the wormhole. Includes a gravity-defying stay at our orbital lounge. Tars and Case robots included for navigation.",
                    ImageUrl = "https://www.oberlin.edu/sites/default/files/styles/width_760/public/content/news/image/wormhole.1_0.png?itok=ARTDejwP",
                    OriginalPrice = 80.00m,
                    DiscountedPrice = 40.00m,
                    TotalCoupons = 50,
                    RemainingCoupons = 50,
                    StartDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                    Status = OfferStatus.Approved,
                    ApprovedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                    ApprovedByAdminId = AdminUserId,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                new Offer
                {
                    Id = 2,
                    MerchantId = 1,
                    CategoryId = 7,
                    Title = "Heptapod Linguistics: Learn to See the Future",
                    Description = "Master the non-linear orthography of the 'Abbott and Costello' dialect. Guaranteed to change your perception of time.",
                    ImageUrl = "https://cdn.prod.website-files.com/639281d335ff5f86b30762e7/66fc20b9f3b199dfac4d4fec_66dc6b9f567f3167369a70a4_AD_4nXfU8j7VSRdch-FEQ5oA6pNRZ3xETFikaUUFCmcSD9osvn8X-lk5i5CZvrKTzE0zjglWsBRV77M3n8Thwp6cuhCHI9RyuyZaTwnvYL_y283XwUx0wAI_mzKBNTrMj86nmYwuXLh9zYHjcIO9K9PyFzRS4Ms.png",
                    OriginalPrice = 120.00m,
                    DiscountedPrice = 84.00m,
                    TotalCoupons = 20,
                    RemainingCoupons = 20,
                    StartDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2025, 6, 30, 23, 59, 59, DateTimeKind.Utc),
                    Status = OfferStatus.Approved,
                    ApprovedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                    ApprovedByAdminId = AdminUserId,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
    }
}
