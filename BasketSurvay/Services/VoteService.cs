using BasketSurvay.Contracts.Votes;

namespace BasketSurvay.Services
{
    public class VoteService(ApplicationDbContext context) : IVoteService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<Result> AddVoteAsync(int PollId, string UserId, VoteRequest request, CancellationToken cancellationToken = default)
        {
            var VoteExistis = await _context.Votes
                .AnyAsync(v => v.UserId == UserId && v.PollId == PollId, cancellationToken);

            if (VoteExistis)
                return Result.Failure(VoteErrors.Existis);

            var PollExistis = await _context.Polls
                .AnyAsync(p =>
                p.Id == PollId &&
                p.IsPublished &&
                p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) &&
                p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow)
                , cancellationToken);

            if (!PollExistis)
                return Result.Failure(PollErrors.PollNotFound);

            var CurrentQuestions = await _context.Questions
                .Where(q => q.PollId == PollId && q.IsActivated)
                .Select(q => q.Id)
                .ToListAsync(cancellationToken);

            if (!request.Answers.Select(a => a.QuestionId).SequenceEqual(CurrentQuestions))
                return Result.Failure(VoteErrors.Invalid);

            var Vote = new Vote
            {
                UserId = UserId,
                PollId = PollId,
                Answers = request.Answers.Adapt<IEnumerable<VoteAnswer>>().ToList()
            };

            await _context.AddAsync(Vote, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
