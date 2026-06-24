using FluentValidation;
using FcgPayments.Application.Queries.Orders;

namespace FcgPayments.Application.Validators.Orders;

public class GetOrdersValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
