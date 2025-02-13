
using BasketSurvay.Persistence;
using System.Runtime.InteropServices;

namespace BasketSurvay.Services
{
    public class PollServices(ApplicationDbContext Context) : IPollServices
    {
        private readonly ApplicationDbContext _context = Context;
        public async Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken)
        {
            var Polls = await _context.Polls.AsNoTracking().ToListAsync(cancellationToken);

            return Polls;
        }

        public async Task<Poll?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
           var  poll = await _context.Polls.FindAsync(id, cancellationToken);
            return poll is not null ? poll : null;
        }
        public async Task<Poll> CreateAsync(Poll Newpoll, CancellationToken cancellationToken = default)
        {
            await _context.Polls.AddAsync(Newpoll,cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return Newpoll;
        }
        public async Task<bool> UpdateAsync(int id,Poll Newpoll, CancellationToken cancellationToken) 
        {
            
            var MyPoll = await _context.Polls.FindAsync(id, cancellationToken);
            if (MyPoll != null)
            {
                MyPoll.Title = Newpoll.Title;
                MyPoll.Summary = Newpoll.Summary;
                MyPoll.StartsAt = Newpoll.StartsAt;
                MyPoll.EndsAt = Newpoll.EndsAt;

                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            return false;
           
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var poll = await GetByIdAsync(id , cancellationToken);

            if (poll is null)
                return false;

            _context.Polls.Remove(poll);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> TogglePublishStatusAsync(int id, CancellationToken cancellationToken)
        {
            var UpdatedPoll = await _context.Polls.FindAsync(id, cancellationToken);

            if (UpdatedPoll == null)
                return false ;

            UpdatedPoll.IsPublished= !UpdatedPoll.IsPublished;
            await _context.SaveChangesAsync(cancellationToken);
            return true;

        }
    }
}
