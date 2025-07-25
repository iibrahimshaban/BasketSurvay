
namespace BasketSurvay.Persistence.EntitiesConfigurations
{
    public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> builder)
        {
            builder.HasIndex(x => new { x.Content, x.QuestionId }).IsUnique();
            builder.Property(x => x.Content).HasMaxLength(1000);
        }
    }
}
