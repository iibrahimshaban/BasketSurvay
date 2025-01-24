

namespace BasketSurvay.Contracts.Requestes;


public record CreatePollRequest(
    string Title,
     [RegularExpression("^200\\d{3}$",ErrorMessage ="student id must begains with 200")]
    string Description

    );

