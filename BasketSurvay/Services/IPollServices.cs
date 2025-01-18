namespace BasketSurvay.Services
{
    public interface IPollServices
    {
        IEnumerable<Poll> GetAll();

        Poll? GetById(int id);
        Poll Create(Poll poll);
        bool Update(Poll poll);
        bool Delete(int id );

    }
}
