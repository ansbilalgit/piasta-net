using Microsoft.EntityFrameworkCore;
using PiastaNet.API.Data;
using PiastaNet.API.Models;
using PiastaNet.API.Services;
using PiastaNet.API.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace PiastaNet.Tests
{
    public class ItemsServiceTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ItemsDbTest")
                .Options;
            var db = new AppDbContext(options);

            // Seed some data
            if (!db.Items.Any())
            {
                db.Items.AddRange(
                    new Item
                    {
                        Id = 1,
                        Name = "Catan",
                        Description = "Board game",
                        Type = ItemType.Boardgame,
                        Copies = 5,
                        Categories = new List<Category> { new Category { Name = "Strategy" } },
                        Boardgame = new Boardgame { MinPlayers = 3, MaxPlayers = 4 }
                    },
                    new Item
                    {
                        Id = 2,
                        Name = "FIFA",
                        Description = "Video game",
                        Type = ItemType.Videogame,
                        Copies = 10,
                        Categories = new List<Category> { new Category { Name = "Sports" } },
                        Videogame = new Videogame { MinPlayers = 1, MaxPlayers = 4 }
                    }
                );
                db.SaveChanges();
            }

            return db;
        }

        [Fact]
        public async Task GetAllAsync_Returns_All_Items()
        {
            // Arrange
            var db = GetDbContext();
            var service = new ItemsService(db);

            // Act
            var result = await service.GetAllAsync(
                q: null,
                type: null,
                category: null,
                sortBy: null,
                sortDir: null,
                page: 1,
                pageSize: 10,
                ct: CancellationToken.None);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Contains(result.Items, i => i.Name == "Catan");
            Assert.Contains(result.Items, i => i.Name == "FIFA");
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Correct_Item()
        {
            // Arrange
            var db = GetDbContext();
            var service = new ItemsService(db);

            // Act
            var item = await service.GetByIdAsync(1, CancellationToken.None);

            // Assert
            Assert.NotNull(item);
            Assert.Equal("Catan", item!.Name);
            Assert.Equal(ItemType.Boardgame, item.Type);
        }

        [Fact]
        public async Task DeleteAsync_Removes_Item()
        {
            // Arrange
            var db = GetDbContext();
            var service = new ItemsService(db);

            // Act
            var deleted = await service.DeleteAsync(1, CancellationToken.None);
            var item = await db.Items.FindAsync(1);

            // Assert
            Assert.True(deleted);
            Assert.Null(item);
        }

        [Fact]
        public async Task GetAllAsync_Filters_By_Type()
        {
            // Arrange
            var db = GetDbContext();
            var service = new ItemsService(db);

            // Act
            var result = await service.GetAllAsync(
                q: null,
                type: "videogame",
                category: null,
                sortBy: null,
                sortDir: null,
                page: 1,
                pageSize: 10,
                ct: CancellationToken.None);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("FIFA", result.Items[0].Name);
        }

        [Fact]
        public async Task GetAllAsync_Filters_By_Category()
        {
            // Arrange
            var db = GetDbContext();
            var service = new ItemsService(db);

            // Act
            var result = await service.GetAllAsync(
                q: null,
                type: null,
                category: "Strategy",
                sortBy: null,
                sortDir: null,
                page: 1,
                pageSize: 10,
                ct: CancellationToken.None);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("Catan", result.Items[0].Name);
        }
    }
}