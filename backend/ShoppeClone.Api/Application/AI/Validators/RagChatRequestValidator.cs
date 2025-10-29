using FluentValidation;
using ShoppeClone.Api.Application.AI.Dtos;

public sealed class RagChatRequestValidator : AbstractValidator<RagChatRequest>
{
    public RagChatRequestValidator()
    {
        RuleFor(x => x.messages).NotEmpty();
        RuleFor(x => x.k).InclusiveBetween(3, 8);
    }
}
