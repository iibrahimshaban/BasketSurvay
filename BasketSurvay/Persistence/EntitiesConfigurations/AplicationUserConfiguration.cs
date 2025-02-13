
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
        }
    }
}
