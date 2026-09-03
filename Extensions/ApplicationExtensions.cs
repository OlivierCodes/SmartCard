using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SmartCard.Data;
using SmartCard.Models;

namespace SmartCard.Extensions
{
    public static class ApplicationExtensions
    {
        public static async Task UseDatabaseInitializer(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                try
                {
                    // Create database and tables if they don't exist
                    var databaseCreator = context.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
                    if (!databaseCreator.Exists())
                    {
                        databaseCreator.Create();
                    }
                    if (!databaseCreator.HasTables())
                    {
                        databaseCreator.CreateTables();
                    }

                    // Create default roles
                    await CreateDefaultRolesAsync(roleManager, userManager);

                    // Create default admin user if it doesn't exist
                    await CreateDefaultAdminUserAsync(userManager, roleManager);
                }
                catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.Contains("Departments") || ex.Message.Contains("invalid object name"))
                {
                    // If migration fails due to table issues, try raw SQL as fallback
                    try
                    {
                        // Execute raw SQL to create the Departments table if it doesn't exist
                        context.Database.ExecuteSqlRaw(@"
                            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Departments' AND xtype='U')
                            BEGIN
                                CREATE TABLE Departments (
                                    Id int IDENTITY(1,1) PRIMARY KEY,
                                    Name nvarchar(100) NOT NULL,
                                    CreatedDate datetime2 DEFAULT GETDATE()
                                );
                            END
                        ");

                        // Add DepartmentId column to Employees if it doesn't exist
                        context.Database.ExecuteSqlRaw(@"
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Employees') AND name = 'DepartmentId')
                            BEGIN
                                ALTER TABLE Employees ADD DepartmentId int NULL;
                            END
                        ");
                    }
                    catch
                    {
                        // If raw SQL also fails, continue - the app will handle missing features gracefully
                    }
                }
            }
        }

        private static async Task CreateDefaultRolesAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            // Liste des rôles cibles
            var targetRoles = new[] { "Admin", "Pompiste", "Employé" };

            // 1. S'assurer que les nouveaux rôles existent
            foreach (var roleName in targetRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Migrer les utilisateurs des anciens rôles et les supprimer
            var migrationMap = new Dictionary<string, string>
            {
                { "PumpAttendant", "Pompiste" },
                { "Employee", "Employé" }
            };

            foreach (var mapping in migrationMap)
            {
                var oldRoleName = mapping.Key;
                var newRoleName = mapping.Value;

                var oldRole = await roleManager.FindByNameAsync(oldRoleName);
                if (oldRole != null)
                {
                    // Récupérer tous les utilisateurs ayant l'ancien rôle
                    var usersInOldRole = await userManager.GetUsersInRoleAsync(oldRoleName);
                    foreach (var user in usersInOldRole)
                    {
                        // Ajouter le nouveau rôle s'ils ne l'ont pas déjà
                        if (!await userManager.IsInRoleAsync(user, newRoleName))
                        {
                            await userManager.AddToRoleAsync(user, newRoleName);
                        }
                        // Retirer l'ancien rôle
                        await userManager.RemoveFromRoleAsync(user, oldRoleName);
                    }

                    // Supprimer l'ancien rôle de la base de données
                    await roleManager.DeleteAsync(oldRole);
                }
            }
        }

        private static async Task CreateDefaultAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var adminEmail = "admin@smartcard.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}