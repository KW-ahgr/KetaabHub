using Domin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infra.Persist
{
    public class BoooooooooksDBContext : DbContext
    {

        public BoooooooooksDBContext(DbContextOptions<BoooooooooksDBContext> options) : base(options) {   }

        public BoooooooooksDBContext() {  }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<BookCat> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<BookPic> Pics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<BookCat>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasAlternateKey(c => c.Gid);
                entity.Property(x => x.Gid)
                     .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<BookPic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasAlternateKey(c => c.Gid);
                entity.Property(x => x.Gid)
                     .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasAlternateKey(c => c.Gid);
                entity.Property(x => x.Gid)
                     .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasAlternateKey(c => c.Gid);
                entity.Property(x => x.Gid)
                     .ValueGeneratedOnAdd();
            });
            var stringListConverter = new ValueConverter<ICollection<string>, string>(
                v => string.Join(";", v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.HasAlternateKey(b => b.Gid);
                entity.Property(b => b.Title)
                     .IsRequired()
                     .HasMaxLength(200);
                entity.HasOne(b => b.Category)
                    .WithMany(c => c.Books)
                    .HasForeignKey(b => b.CategoryId)
                    .HasPrincipalKey(c => c.Gid)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(b => b.Authors)
                    .WithMany() 
                    .UsingEntity(j => j.ToTable("BookAuthors"));
                entity.HasMany(b => b.Translators)
                    .WithMany()
                    .UsingEntity(j => j.ToTable("BookTranslators"));
                entity.Property(b => b.Pics)
                    .HasConversion(stringListConverter);
                entity.Property(x => x.Gid)
                    .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasAlternateKey(c => c.Gid);
                entity.HasOne(c => c.Book)
                    .WithMany(b => b.Comments)
                    .HasForeignKey(c => c.BookId)
                    .HasPrincipalKey(b => b.Gid)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .HasPrincipalKey(u => u.Gid)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(x => x.Gid)
                    .ValueGeneratedOnAdd();
            });

            /*
            modelBuilder.Entity<Book>()
                .Property(b => b.Authors)
                .HasConversion(stringListConverter);
            modelBuilder.Entity<Book>()
                .Property(b => b.Translators)
                .HasConversion(stringListConverter);
            */
                                  
            base.OnModelCreating(modelBuilder);
        }
    }

    public class BoooooooooksDBContextFactory : IDesignTimeDbContextFactory<BoooooooooksDBContext>
    {
        public BoooooooooksDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BoooooooooksDBContext>();
            optionsBuilder.UseSqlServer("Server=.;Database=BooksDB;Trusted_Connection=True;TrustServerCertificate=True;");

            return new BoooooooooksDBContext(optionsBuilder.Options);
        }
    }
}
