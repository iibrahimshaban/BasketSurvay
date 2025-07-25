
namespace BasketSurvay.Persistence.EntitiesConfigurations
{
    public class AplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.OwnsMany(x => x.RefreshTokens)
                .ToTable("RefreshTokens")
                .WithOwner()
                .HasForeignKey("UserId");

            builder
                .Property(x => x.FirstName)
                .HasMaxLength(150);
            builder
                .Property(x => x.LastName)
                .HasMaxLength(150);

            var appUser = new ApplicationUser
            {
                Id = DefaultUsers.AdminId,
                FirstName = "Ibrahim",
                LastName = "khaled",
                Email = DefaultUsers.AdminEmail,
                NormalizedEmail = DefaultUsers.AdminEmail.ToUpper(),
                UserName = "iibrahim",
                NormalizedUserName = "IIBRAHIM",
                SecurityStamp = DefaultUsers.AdminSequrityStamp,
                ConcurrencyStamp = DefaultUsers.AdminConcurrencyStamp,
                EmailConfirmed = true,
                PasswordHash = DefaultUsers.AdminHashedPassword
            };

            builder.HasData(appUser);
        }
    }
}
