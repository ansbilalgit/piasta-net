using Microsoft.EntityFrameworkCore;
using PiastaNet.API.Models;

namespace PiastaNet.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Item> Items => Set<Item>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Boardgame> Boardgames => Set<Boardgame>();
        public DbSet<Videogame> Videogames => Set<Videogame>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>().ToTable("items");
            modelBuilder.Entity<Category>().ToTable("categories");
            modelBuilder.Entity<Boardgame>().ToTable("boardgames");
            modelBuilder.Entity<Videogame>().ToTable("videogames");

            modelBuilder.Entity<Item>(e =>
            {
                e.HasKey(x => x.Id);

                // IMPORTANT for sqlite import: preserve original ids
                e.Property(x => x.Id).ValueGeneratedNever();

                e.HasIndex(x => x.Name).IsUnique();

                // Store item.type as lowercase text to match sqlite ('boardgame','videogame')
                e.Property(x => x.Type).HasConversion(
                    v => v == ItemType.Boardgame ? "boardgame" : "videogame",
                    v => v == "boardgame" ? ItemType.Boardgame : ItemType.Videogame
                );

                e.Property(x => x.Description).HasDefaultValue("No description provided");
                e.Property(x => x.Thumbnail).HasDefaultValue("https://i.imgur.com/OJhoTqu.png");
                e.Property(x => x.Copies).HasDefaultValue(1);
            });

            modelBuilder.Entity<Category>(e =>
            {
                // sqlite: PRIMARY KEY (id, category)
                e.HasKey(x => new { x.ItemId, x.Name });

                // map column names to match sqlite schema
                e.Property(x => x.ItemId).HasColumnName("id");
                e.Property(x => x.Name).HasColumnName("category");

                e.HasOne(x => x.Item)
                    .WithMany(i => i.Categories)
                    .HasForeignKey(x => x.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Boardgame>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedNever();

                e.HasOne(x => x.Item)
                    .WithOne(i => i.Boardgame)
                    .HasForeignKey<Boardgame>(x => x.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Videogame>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedNever();

                e.HasOne(x => x.Item)
                    .WithOne(i => i.Videogame)
                    .HasForeignKey<Videogame>(x => x.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}