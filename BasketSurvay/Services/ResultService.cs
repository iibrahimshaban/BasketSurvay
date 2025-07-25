
using BasketSurvay.Contracts;

namespace BasketSurvay.Services
{
    public class ResultService(ApplicationDbContext context) : IResultService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<Result<PollVotesResponse>> GetPollVoteAsync(int PollId, CancellationToken cancellationToken = default)
        {
            var PollVotes = await _context.Polls
                .Where(x => x.Id == PollId)
                .Select(p => new PollVotesResponse(
                p.Title,
                p.Votes.Select(v => new VoteResponse(
                    v.User.FirstName + ' ' + v.User.LastName,
                    v.SubmittedOn,
                    v.Answers.Select(a => new QuestionAnswerResponse(
                       a.Question.Content,
                       a.Answer.Content
                       ))
                    ))
                )).SingleOrDefaultAsync(cancellationToken);

            return PollVotes is null
                ? Result.Failure<PollVotesResponse>(PollErrors.PollNotFound)
                : Result.Success(PollVotes);
        }

        public async Task<Result<IEnumerable<VotesPerDayResponse>>> GetVotePerDayAsync(int PollId, CancellationToken cancellationToken = default)
        {
            var PollExists = await _context.Polls.AnyAsync(x => x.Id == PollId, cancellationToken);

            if (!PollExists)
                return Result.Failure<IEnumerable<VotesPerDayResponse>>(PollErrors.PollNotFound);

            var VotesPerDay = await _context.Votes
                .Where(v => v.PollId == PollId)
                .GroupBy(v => new { Date = DateOnly.FromDateTime(v.SubmittedOn) })
                .Select(x => new VotesPerDayResponse(
                    x.Key.Date,
                    x.Count()
                    )).ToListAsync(cancellationToken);

            return Result.Success<IEnumerable<VotesPerDayResponse>>(VotesPerDay);
        }

        public async Task<Result<IEnumerable<VotePerQuestionResponse>>> GetVotePerQuestionAsync(int PollId,
            CancellationToken cancellationToken = default)
        {
            var PollExists = await _context.Polls.AnyAsync(x => x.Id == PollId, cancellationToken);

            if (!PollExists)
                return Result.Failure<IEnumerable<VotePerQuestionResponse>>(PollErrors.PollNotFound);

            var VotePerQuestion = await _context.VoteAnswers
                .Where(x => x.Vote.PollId == PollId)
                .Select(x => new VotePerQuestionResponse(
                    x.Question.Content,
                    x.Question.VoteAnswers
                       .GroupBy(x => new { content = x.Answer.Content, Id = x.AnswerId })
                       .Select(g => new VotesPerAnswerResponse(
                           g.Key.content,
                           g.Count()
                           ))
                    )).ToListAsync(cancellationToken);

            return Result.Success<IEnumerable<VotePerQuestionResponse>>(VotePerQuestion);
        }
    }
}
