using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using AspNetCorePostgreSQLDockerApp.Models;

namespace AspNetCorePostgreSQLDockerApp.Repository
{
    public class DatabaseDbSeeder
    {
        private readonly ILogger _logger;

        public DatabaseDbSeeder(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("DatabaseDbSeederLogger");
        }

        public async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var databaseDb = serviceScope.ServiceProvider.GetService<DatabaseDbContext>();

            // Pokreće migracije ako su dostupne
            _logger.LogInformation("Applying migrations...");
            await databaseDb.Database.MigrateAsync();

            // Proverava da li postoje podaci u tabeli "Users"
            if (!await databaseDb.Users.AnyAsync())
            {
                _logger.LogInformation("Inserting sample data into the database...");
                await InsertDatabaseSampleData(databaseDb);
            }
            else
            {
                _logger.LogInformation("Database already contains data. Skipping seeding.");
            }
        }

        private async Task InsertDatabaseSampleData(DatabaseDbContext db)
        {
            var users = GetUsers();
            db.Users.AddRange(users);

            // Dodajemo slike
            var images = GetImages(users);
            db.Images.AddRange(images);

            // Dodajemo komentare
            var comments = GetComments(users, images);
            db.Comments.AddRange(comments);

            // Dodajemo lajkove
            var likes = GetLikes(users, images);
            db.Likes.AddRange(likes);

            try
            {
                await db.SaveChangesAsync();
                _logger.LogInformation("Sample data successfully added to the database.");
            }
            catch (Exception exp)
            {
                _logger.LogError($"Error in {nameof(DatabaseDbSeeder)}: {exp.Message}");
                throw;
            }
        }

        private List<User> GetUsers()
        {
            var random = new Random();
            var userNames = new[] { "Alice", "Bob", "Charlie", "David", "Eva", "Frank", "Grace", "Hannah", "Ivy", "Jack" };
            var emailProviders = new[] { "gmail.com", "yahoo.com", "outlook.com", "acmecorp.com" };

            var users = new List<User>();
            for (int i = 0; i < userNames.Length; i++)
            {
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Name = userNames[i],
                    Email = $"{userNames[i].ToLower()}@{emailProviders[random.Next(emailProviders.Length)]}",
                    Password = HashPassword("Password123"), // Hashirana lozinka
                    ProfilePicture = $"https://example.com/images/{userNames[i].ToLower()}.jpg", // URL za profilnu sliku
                    Bio = $"Hello, I am {userNames[i]}.", // Kratak opis korisnika
                    CreatedAt = DateTime.UtcNow // Datum kada je korisnik kreiran
                };
                users.Add(user);
            }
            return users;
        }

        private List<Image> GetImages(List<User> users)
        {
            var random = new Random();
            var images = new List<Image>();

            foreach (var user in users)
            {
                for (int i = 0; i < 3; i++) // Svaki korisnik postavlja 3 slike
                {
                    var image = new Image
                    {
                        ImageId = Guid.NewGuid(),
                        UserId = user.UserId,
                        ImageUrl = $"https://example.com/images/{user.Name.ToLower()}_image{i}.jpg",
                        Description = $"Image {i + 1} by {user.Name}",
                        CreatedAt = DateTime.UtcNow
                    };
                    images.Add(image);
                }
            }
            return images;
        }

        private List<Comment> GetComments(List<User> users, List<Image> images)
        {
            var random = new Random();
            var comments = new List<Comment>();

            foreach (var user in users)
            {
                foreach (var image in images.Where(i => i.UserId != user.UserId)) // Korisnici ne komentarišu svoje slike
                {
                    for (int i = 0; i < 2; i++) // Svaki korisnik ostavlja 2 komentara na slike drugih
                    {
                        var comment = new Comment
                        {
                            CommentId = Guid.NewGuid(),
                            UserId = user.UserId,
                            ImageId = image.ImageId,
                            Content = $"Great photo, {image.Description}!",
                            CreatedAt = DateTime.UtcNow
                        };
                        comments.Add(comment);
                    }
                }
            }
            return comments;
        }

        private List<Like> GetLikes(List<User> users, List<Image> images)
        {
            var random = new Random();
            var likes = new List<Like>();

            foreach (var user in users)
            {
                foreach (var image in images.Where(i => i.UserId != user.UserId)) // Korisnici ne lajkaju svoje slike
                {
                    var like = new Like
                    {
                        UserId = user.UserId,
                        ImageId = image.ImageId,
                        CreatedAt = DateTime.UtcNow
                    };
                    likes.Add(like);
                }
            }
            return likes;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password); // Hashiranje lozinke
        }
    }
}
