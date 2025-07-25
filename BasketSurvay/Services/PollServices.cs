using Hangfire;
namespace BasketSurvay.Services
{
    public class PollServices(ApplicationDbContext Context, INotificationService notificationService) : IPollServices
    {
        private readonly ApplicationDbContext _context = Context;
        private readonly INotificationService _notificationService = notificationService;

        public async Task<Result<IEnumerable<PollResponse>>> GetAllAsync(CancellationToken cancellationToken)
        {
            var polls = await _context.Polls
                .AsNoTracking()
                .ProjectToType<PollResponse>()
                .ToListAsync(cancellationToken);

            return polls.Count > 0 ?
                Result.Success<IEnumerable<PollResponse>>(polls) :
                Result.Failure<IEnumerable<PollResponse>>(PollErrors.NoPoll);
        }
        public async Task<Result<IEnumerable<PollResponse>>> GetCurrentAsyncV1(CancellationToken cancellationToken = default)
        {
            var Polls = await GetCurrentPolls(cancellationToken);

            return !Polls.Any()
                ? Result.Failure<IEnumerable<PollResponse>>(PollErrors.NoPoll)
                : Result.Success(Polls);
        }
        public async Task<Result<IEnumerable<PollResponseV2>>> GetCurrentAsyncV2(CancellationToken cancellationToken = default)
        {
            var Polls = await GetCurrentPolls(cancellationToken);

            return !Polls.Any()
                ? Result.Failure<IEnumerable<PollResponseV2>>(PollErrors.NoPoll)
                : Result.Success(Polls.Adapt<IEnumerable<PollResponseV2>>());
        }

        public async Task<Result<PollResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var poll = await _context.Polls.FindAsync(id, cancellationToken);

            return poll is null
                ? Result.Failure<PollResponse>(PollErrors.PollNotFound)
                : Result.Success(poll.Adapt<PollResponse>());
        }
        public async Task<OneOf<PollResponse, Error>> CreateAsync(CreatePollRequest request,
            CancellationToken cancellationToken = default)
        {
            var myPoll = request.Adapt<Poll>();
            try
            {
                await _context.AddAsync(myPoll, cancellationToken);
                var result = await _context.SaveChangesAsync(cancellationToken);
                return myPoll.Adapt<PollResponse>();
            }
            catch
            {
                return PollErrors.DuplicatedPoll;
            }
        }
        public async Task<Result> UpdateAsync(int id, CreatePollRequest poll, CancellationToken cancellationToken)
        {
            var ExistingPoll = await _context.Polls.AnyAsync(x => x.Title == poll.Title && x.Id != id, cancellationToken);

            if (ExistingPoll)
                return Result.Failure(PollErrors.DuplicatedPoll);

            var CurrentPoll = await _context.Polls.FindAsync(id, cancellationToken);

            if (CurrentPoll == null)
                return Result.Failure(PollErrors.PollNotFound);

            CurrentPoll = poll.Adapt(CurrentPoll);


            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();

        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var poll = await _context.Polls.FindAsync(id, cancellationToken);

            if (poll is null)
                return Result.Failure(PollErrors.PollNotFound);

            _context.Polls.Remove(poll);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken)
        {
            var UpdatedPoll = await _context.Polls.FindAsync(id, cancellationToken);

            if (UpdatedPoll == null)
                return Result.Failure(PollErrors.PollNotFound);


            UpdatedPoll.IsPublished = !UpdatedPoll.IsPublished;
            await _context.SaveChangesAsync(cancellationToken);

            if (UpdatedPoll.IsPublished && UpdatedPoll.StartsAt == DateOnly.FromDateTime(DateTime.UtcNow))
            {
                BackgroundJob.Enqueue(() => _notificationService.SendNewPollNotification(UpdatedPoll.Id));
            }

            return Result.Success();

        }
        private async Task<IEnumerable<PollResponse>> GetCurrentPolls(CancellationToken cancellationToken = default)
        {
            var Polls = await _context.Polls
                .Where(
                p => p.IsPublished &&
                p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) &&
                p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow))
                .AsNoTracking()
                .ProjectToType<PollResponse>()
                .ToListAsync(cancellationToken);

            return Polls;

        }
    }
}
