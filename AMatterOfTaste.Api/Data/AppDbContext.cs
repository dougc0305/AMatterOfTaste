using AMatterOfTaste.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AMatterOfTaste.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<RecipeCategory> RecipeCategories => Set<RecipeCategory>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
    public DbSet<RecipePhoto> RecipePhotos => Set<RecipePhoto>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();
    public DbSet<SiteVisit> SiteVisits => Set<SiteVisit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // All table names lowercase, no quoted identifiers
        modelBuilder.Entity<RecipeCategory>(e =>
        {
            e.ToTable("recipecategory");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.SortOrder).HasDefaultValue(0);
        });

        modelBuilder.Entity<Recipe>(e =>
        {
            e.ToTable("recipe");
            e.Property(x => x.Title).HasMaxLength(255).IsRequired();
            e.Property(x => x.Attribution).HasMaxLength(255);
            e.HasOne(x => x.Category).WithMany(c => c.Recipes).HasForeignKey(x => x.CategoryId);
        });

        modelBuilder.Entity<RecipeIngredient>(e =>
        {
            e.ToTable("recipeingredient");
            e.Property(x => x.Quantity).HasMaxLength(50);
            e.Property(x => x.Unit).HasMaxLength(50);
            e.Property(x => x.Name).HasMaxLength(255).IsRequired();
            e.Property(x => x.SortOrder).HasDefaultValue(0);
            e.HasOne(x => x.Recipe).WithMany(r => r.Ingredients).HasForeignKey(x => x.RecipeId);
        });

        modelBuilder.Entity<RecipeStep>(e =>
        {
            e.ToTable("recipestep");
            e.Property(x => x.Instruction).IsRequired();
            e.HasOne(x => x.Recipe).WithMany(r => r.Steps).HasForeignKey(x => x.RecipeId);
        });

        modelBuilder.Entity<RecipePhoto>(e =>
        {
            e.ToTable("recipephoto");
            e.Property(x => x.Filename).HasMaxLength(255).IsRequired();
            e.Property(x => x.IsPrimary).HasDefaultValue(false);
            e.Property(x => x.SortOrder).HasDefaultValue(0);
            e.HasOne(x => x.Recipe).WithMany(r => r.Photos).HasForeignKey(x => x.RecipeId);
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("appuser");
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(255).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.CookbookSlug).HasMaxLength(100);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.CookbookSlug).IsUnique().HasFilter("cookbookslug IS NOT NULL");
        });

        modelBuilder.Entity<UserFavorite>(e =>
        {
            e.ToTable("userfavorite");
            e.HasOne(x => x.User).WithMany(u => u.Favorites).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Recipe).WithMany(r => r.Favorites).HasForeignKey(x => x.RecipeId);
            e.HasIndex(x => new { x.UserId, x.RecipeId }).IsUnique();
        });

        modelBuilder.Entity<SiteVisit>(e =>
        {
            e.ToTable("sitevisit");
            e.HasIndex(x => x.VisitDate).IsUnique();
        });

        // Map all column names to lowercase via snake_case convention
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.Name.ToLower());
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(key.GetName()?.ToLower());
            }

            foreach (var fk in entity.GetForeignKeys())
            {
                fk.SetConstraintName(fk.GetConstraintName()?.ToLower());
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName()?.ToLower());
            }
        }

        // Set default values for audit fields on all entities
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IntIdentityBase).IsAssignableFrom(entity.ClrType))
            {
                modelBuilder.Entity(entity.ClrType).Property("CreatedById").HasDefaultValue(1);
                modelBuilder.Entity(entity.ClrType).Property("ModifiedById").HasDefaultValue(1);
                modelBuilder.Entity(entity.ClrType).Property("CreatedDate").HasDefaultValueSql("now()");
                modelBuilder.Entity(entity.ClrType).Property("ModifiedDate").HasDefaultValueSql("now()");
                modelBuilder.Entity(entity.ClrType).Property("IsActive").HasDefaultValue(true);
            }
        }

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed admin user — password will be reset on first login
        // Pre-computed BCrypt hash of "ChangeMe123!" — static so EF doesn't see model changes each build
        const string adminPasswordHash = "$2a$11$AUuv/yUUlaVfZx4zMYpP1OMfZndwAx4vmVRNzAltu4JnSR3bI4SWq";
        modelBuilder.Entity<AppUser>().HasData(new AppUser
        {
            Id = 1,
            Name = "Cheryl Charbonneau",
            Email = "admin@amatteroftaste.us",
            PasswordHash = adminPasswordHash,
            IsAdmin = true,
            IsActive = true,
            CreatedById = 1,
            ModifiedById = 1,
            CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Seed all 18 recipe categories
        var categories = new[]
        {
            "Main Course", "Dessert", "Cookies", "Breakfast", "Side Dishes", "Soups",
            "Bread", "Entertaining", "Gifts", "Appetizer", "Salads", "Beverages",
            "Casserole", "Dinner", "Snacks", "Condiments", "Drinks", "Other"
        };

        for (var i = 0; i < categories.Length; i++)
        {
            modelBuilder.Entity<RecipeCategory>().HasData(new RecipeCategory
            {
                Id = i + 1,
                Name = categories[i],
                SortOrder = i,
                IsActive = true,
                CreatedById = 1,
                ModifiedById = 1,
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ModifiedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
