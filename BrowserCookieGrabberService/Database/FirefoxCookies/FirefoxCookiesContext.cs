using Microsoft.EntityFrameworkCore;

namespace BrowserCookieGrabberService.Database.FirefoxCookies
{
    public partial class FirefoxCookiesContext : DbContext
    {
        private readonly string? path;
        
        public FirefoxCookiesContext()
        {
        }

        public FirefoxCookiesContext(DbContextOptions<FirefoxCookiesContext> options)
            : base(options)
        {
        }
        
        public FirefoxCookiesContext(DbContextOptions<FirefoxCookiesContext> options, string path)
            : this(options)
        {
            this.path = path;
        }

        public virtual DbSet<MozCookie> MozCookies { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlite("Data Source=/home/milleniumbug/.mozilla/firefox/dcsaxgd2.default/cookies.sqlite");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MozCookie>(entity =>
            {
                entity.ToTable("moz_cookies");

                entity.HasIndex(e => new { e.Name, e.Host, e.Path, e.OriginAttributes }, "IX_moz_cookies_name_host_path_originAttributes")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CreationTime).HasColumnName("creationTime");

                entity.Property(e => e.Expiry).HasColumnName("expiry");

                entity.Property(e => e.Host).HasColumnName("host");

                entity.Property(e => e.InBrowserElement)
                    .HasColumnName("inBrowserElement")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.IsHttpOnly).HasColumnName("isHttpOnly");

                entity.Property(e => e.IsSecure).HasColumnName("isSecure");

                entity.Property(e => e.LastAccessed).HasColumnName("lastAccessed");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.OriginAttributes)
                    .HasColumnName("originAttributes")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Path).HasColumnName("path");

                entity.Property(e => e.RawSameSite)
                    .HasColumnName("rawSameSite")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.SameSite)
                    .HasColumnName("sameSite")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.SchemeMap)
                    .HasColumnName("schemeMap")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public override void Dispose()
        {
            base.Dispose();
            if (path != null)
            {
                File.Delete(path);
            }
        }
    }
}
