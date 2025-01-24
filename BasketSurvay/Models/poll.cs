using BasketSurvay.Contracts.Responses;
using System.ComponentModel.DataAnnotations;

namespace BasketSurvay.Models
{
    public class Poll
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
    
}
