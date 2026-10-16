using InterviewPrep.Domain.Entities;
using InterviewPrep.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InterviewPrep.Infrastructure.Persistence;

/// <summary>
/// Applies pending EF Core migrations and seeds one admin login + a few demo members/purchases,
/// so the Angular/React login pages have something real to authenticate against on first run
/// without any manual setup. Called once from Program.cs on startup.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(InterviewPrepDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync())
        {
            var admin = User.Create("admin", BCrypt.Net.BCrypt.HashPassword("Admin123!"), UserRole.Admin);
            var staff = User.Create("staff", BCrypt.Net.BCrypt.HashPassword("Staff123!"), UserRole.Staff);
            context.Users.AddRange(admin, staff);
        }

        if (!await context.Members.AnyAsync())
        {
            var alice = Member.Create("Alice Tan", 29, Gender.Female, MemberTier.Gold);
            var bob = Member.Create("Bob Lim", 41, Gender.Male, MemberTier.Silver);
            context.Members.AddRange(alice, bob);
            await context.SaveChangesAsync();

            alice.RecordPurchase(250m, 18m);
            bob.RecordPurchase(620m, 13m);
            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();
    }
}
