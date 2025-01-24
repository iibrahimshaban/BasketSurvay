
namespace BasketSurvay.Services
{
    public class PollServices : IPollServices
    {
        private static readonly List<Poll> _polls = new List<Poll>()
    {
        new Poll() {Id=1 ,Title="math" ,Description="numerical as shit"},
        new Poll() {Id=2 ,Title="science" ,Description="space as black"}
    };
        public IEnumerable<Poll> GetAll()
        {
            return _polls;
        }

        public Poll? GetById(int id)
        {
            var poll = _polls.FirstOrDefault(pol => pol.Id == id);
            return poll is not null ? poll : null;
        }
        public Poll Create(Poll Newpoll)
        {
            Newpoll.Id= _polls.Count+1; // to generate a new value for id 

            _polls.Add(Newpoll);
            return Newpoll;
        }
        public bool Update(int id,Poll Newpoll) 
        {
            Newpoll.Id = id;
            var MyPoll = _polls.FirstOrDefault(P => P.Id == Newpoll.Id);
            if (MyPoll != null)
            {
                MyPoll.Title = Newpoll.Title;
                MyPoll.Description = Newpoll.Description;
                return true;
            }
            return false;
           
        }

        public bool Delete(int id)
        {
            var poll = GetById(id);

            if (poll is null)
                return false;

            _polls.Remove(poll);
            return true;
        }
    }
}
