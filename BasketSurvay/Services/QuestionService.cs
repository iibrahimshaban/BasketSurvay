using BasketSurvay.Contracts.Common;
using Microsoft.Extensions.Caching.Hybrid;
using System.Linq.Dynamic.Core;

namespace BasketSurvay.Services
{
    public class QuestionService(ApplicationDbContext context, HybridCache hybridCache) : IQuestionService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly HybridCache _hybridCache = hybridCache;

        private const string _cachePrefix = "AvilableQuestions";

        public async Task<Result<PaginationList<QuestionResponse>>> GetAllAsync(int PollId, RequestFilter requestFilter, CancellationToken cancellationToken)
        {
            var ExistingPoll = await _context.Polls.AnyAsync(x => x.Id == PollId, cancellationToken);

            if (!ExistingPoll)
                return Result.Failure<PaginationList<QuestionResponse>>(PollErrors.PollNotFound);

            if (requestFilter.PageSize > 20)
                return Result.Failure<PaginationList<QuestionResponse>>(FiltersErrors.InvalidPageSize);

            var query = _context.Questions
                .Where(x => x.PollId == PollId);

            if (!string.IsNullOrEmpty(requestFilter.SearchValue))
            {
                query = query.Where(x => x.Content.Contains(requestFilter.SearchValue, StringComparison.CurrentCultureIgnoreCase));
            }

            if (!string.IsNullOrEmpty(requestFilter.SortColumn))
            {
                query = query.OrderBy($"{requestFilter.SortColumn} {requestFilter.SortDirection}");
            }

            var source = query
                            .Include(x => x.Answers)
                            .ProjectToType<QuestionResponse>()
                            .AsNoTracking();

            var questions = await PaginationList<QuestionResponse>
                .CreateAsync(source, requestFilter.PageNumber, requestFilter.PageSize);

            return Result.Success<PaginationList<QuestionResponse>>(questions);

        }
        public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int PollId, string UserId, CancellationToken cancellationToken)
        {
            var VoteExistis = await _context.Votes
                .AnyAsync(v => v.UserId == UserId && v.PollId == PollId, cancellationToken);

            if (VoteExistis)
                return Result.Failure<IEnumerable<QuestionResponse>>(VoteErrors.Existis);

            var PollExistis = await _context.Polls
                .AnyAsync(p =>
                p.Id == PollId &&
                p.IsPublished &&
                p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) &&
                p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow)
                , cancellationToken);

            if (!PollExistis)
                return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

            var CacheKey = $"{_cachePrefix}-{PollId}";

            var Questions = await _hybridCache.GetOrCreateAsync<IEnumerable<QuestionResponse>>(CacheKey, async cacheEntry => await _context.Questions
                            .Where(x => x.PollId == PollId && x.IsActivated)
                            .Include(x => x.Answers)
                            .Select(q => new QuestionResponse(
                                q.Id,
                                q.Content,
                                q.Answers.Where(a => a.IsActivated).Select(a => new AnswerResponse(a.Id, a.Content))
                             ))
                            .AsNoTracking()
                            .ToListAsync(cancellationToken)
                            , new HybridCacheEntryOptions
                            {
                                Expiration = TimeSpan.FromMinutes(5),
                            }
                            , cancellationToken: cancellationToken);

            return Result.Success(Questions);

        }
        public async Task<Result<QuestionResponse>> GetByQuestionIdAsync(int PollId, int QuestionId, CancellationToken cancellationToken)
        {
            var ExistingPoll = await _context.Polls.AnyAsync(x => x.Id == PollId, cancellationToken);

            if (!ExistingPoll)
                return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

            var question = _context.Questions
                .Where(q => q.Id == QuestionId && q.PollId == PollId)
                .Include(q => q.Answers)
                .ProjectToType<QuestionResponse>()
                .AsNoTracking()
                .FirstOrDefault();

            if (question is null)
                return Result.Failure<QuestionResponse>(QuestionErrors.NotFound);

            return Result.Success(question)!;

        }
        public async Task<Result<QuestionResponse>> CreateAsync(int PollId, QuestionRequest request, CancellationToken cancellationToken = default)
        {
            var ExistingPoll = await _context.Polls.AnyAsync(x => x.Id == PollId, cancellationToken);

            if (!ExistingPoll)
                return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

            var DoublicatedQuestion = await _context.Questions
                .AnyAsync(x => x.Content == request.Content && x.PollId == PollId, cancellationToken);

            if (DoublicatedQuestion)
                return Result.Failure<QuestionResponse>(QuestionErrors.DoublicatedQuestionContent);

            var question = request.Adapt<Question>();
            question.PollId = PollId;

            await _context.AddAsync(question, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _hybridCache.RemoveAsync($"{_cachePrefix}-{PollId}", cancellationToken);

            return Result.Success(question.Adapt<QuestionResponse>());
        }

        public async Task<Result<QuestionResponse>> UpdateAsync(int PollId, int id, QuestionRequest request, CancellationToken cancellationToken = default)
        {
            var DoublicatedQuestion = await _context.Questions
                .AnyAsync(
                x => x.PollId == PollId && x.Id != id && x.Content == request.Content
                , cancellationToken
                );

            if (DoublicatedQuestion)
                return Result.Failure<QuestionResponse>(QuestionErrors.DoublicatedQuestionContent);

            var question = await _context.Questions.Include(x => x.Answers)
                        .FirstOrDefaultAsync(x => x.Id == id && x.PollId == PollId, cancellationToken);

            if (question is null)
                return Result.Failure<QuestionResponse>(QuestionErrors.NotFound);

            question.PollId = PollId;
            question.Content = request.Content;
            //current answers 
            var DBAnswers = question.Answers.Select(x => x.Content).ToList();

            //add new answers
            var NewAnswers = request.Answers.Except(DBAnswers).ToList();

            NewAnswers.ForEach(ans =>
            {
                question.Answers.Add(new Answer { Content = ans });
            });
            //dis-activate the current answers 
            question.Answers.ToList().ForEach(ans =>
            {
                ans.IsActivated = request.Answers.Contains(ans.Content);
            });

            await _context.SaveChangesAsync(cancellationToken);

            await _hybridCache.RemoveAsync($"{_cachePrefix}-{PollId}", cancellationToken);
            return Result.Success(question.Adapt<QuestionResponse>());

        }
        public async Task<Result<bool>> ToggleStatusAsync(int PollId, int id, CancellationToken cancellationToken = default)
        {
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.PollId == PollId && q.Id == id, cancellationToken);

            if (question == null)
                return Result.Failure<bool>(QuestionErrors.NotFound);


            question.IsActivated = !question.IsActivated;

            await _context.SaveChangesAsync(cancellationToken);

            await _hybridCache.RemoveAsync($"{_cachePrefix}-{PollId}", cancellationToken);
            return Result.Success(question.IsActivated);
        }


    }
}
