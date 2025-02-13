namespace BasketSurvay.Contracts.Polls;

public record CreatePollRequest(
    string Title,
    string Summary,
    DateOnly StartsAt,
    DateOnly EndsAt
    );

