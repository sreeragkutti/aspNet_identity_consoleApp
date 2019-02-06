using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IdentityFrameworkSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var username = "sree@gmail.com";
            var password = "sree@123.";

            // first creat userstore and usermanager
            //var userStore = new UserStore<IdentityUser>();
            //var userManager = new UserManager<IdentityUser>(userStore);

            // 1. creation of database tables for Implementing  Identity Server

            //var creationMsg = userManager.Create(new IdentityUser("sreerag@gmail.com"), "sree@123.");
            //Console.Write("Message {0}", creationMsg.Succeeded);

            // 2. Finding user from the identity server

            //var user = userManager.FindByName(username);
            //var clientResult = userManager.AddClaim(user.Id, new Claim("Sreerag K", "Kannur"));

            //Console.WriteLine("Claim {0}", clientResult.Succeeded);

            // 3. for checking user is valid

            //var isValid = userManager.CheckPassword(user, password);
            //Console.Write("isValid {0}", isValid);

            // 4. create custom table based on the dbcontext , custom user and custom store

            var userStore = new CustomUserStore(new CustomUserDbContext());
            var userManager = new UserManager<CustomUser,int>(userStore);


            var createUser = userManager.Create(new CustomUser { UserName = username }, password);
            Console.WriteLine("User created  {0} ", createUser.Succeeded);

            Console.ReadKey();

        }
    }

    public class CustomUser : IUser<int>
    {
        public int Id { get; set; }

        public string UserName { get; set; }
        public string PasswordHash { get; set; }
    }


    public class CustomUserDbContext : DbContext
    {
        public CustomUserDbContext(): base("DefaultConnection")
        {

        }

        public DbSet<CustomUser> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var user = modelBuilder.Entity<CustomUser>();
            user.ToTable("Users");
            user.HasKey(t => t.Id);
            user.Property(p => p.Id).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            user.Property(p => p.UserName).IsRequired().HasMaxLength(256)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UsernameIndex") { IsUnique = true }));



            base.OnModelCreating(modelBuilder);

        }
    }


    public class CustomUserStore : IUserPasswordStore<CustomUser, int>
    {
        private readonly CustomUserDbContext dbContext;
        public CustomUserStore(CustomUserDbContext _dbContext)
        {
            dbContext = _dbContext;
        }
        public Task CreateAsync(CustomUser user)
        {
            dbContext.Users.Add(user);
            return dbContext.SaveChangesAsync();
        }

        public Task DeleteAsync(CustomUser user)
        {
            dbContext.Users.Attach(user);
            return dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        public Task<CustomUser> FindByIdAsync(int userId)
        {
            var user = dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            return user;
        }

        public Task<CustomUser> FindByNameAsync(string userName)
        {
            var user = dbContext.Users.FirstOrDefaultAsync(n => n.UserName == userName);
            return user;
        }

        public Task<string> GetPasswordHashAsync(CustomUser user)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(CustomUser user)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public Task SetPasswordHashAsync(CustomUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(CustomUser user)
        {
            dbContext.Users.Attach(user);
            return dbContext.SaveChangesAsync();
        }
    }

}
