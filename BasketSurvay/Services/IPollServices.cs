namespace BasketSurvay.Services;

    public interface IPollServices
    {
        Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<Poll?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Poll> CreateAsync(Poll poll, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(int id,Poll poll , CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id , CancellationToken cancellationToken =default);
        Task<bool> TogglePublishStatusAsync(int id , CancellationToken cancellationToken = default);

    }

