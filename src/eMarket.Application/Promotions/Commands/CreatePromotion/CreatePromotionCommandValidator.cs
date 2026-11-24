
using FluentValidation;
using eMarket.Domain.Promotions.Enums;

namespace eMarket.Application.Promotions.Commands.CreatePromotion;

public sealed class CreatePromotionCommandValidator
    : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Kampanya adı zorunludur.")
            .MaximumLength(150)
            .WithMessage("Kampanya adı en fazla 150 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Kampanya açıklaması en fazla 1000 karakter olabilir.");

        RuleFor(x => x.StartsAtUtc)
            .Must(x => x.Kind == DateTimeKind.Utc)
            .WithMessage("Kampanya başlangıç tarihi UTC formatında olmalıdır.");

        RuleFor(x => x.EndsAtUtc)
            .Must(x => x.Kind == DateTimeKind.Utc)
            .WithMessage("Kampanya bitiş tarihi UTC formatında olmalıdır.");

        RuleFor(x => x.EndsAtUtc)
            .GreaterThan(x => x.StartsAtUtc)
            .WithMessage("Kampanya bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        RuleFor(x => x.EndsAtUtc)
            .Must(x => x > DateTime.UtcNow)
            .WithMessage("Kampanya bitiş tarihi gelecekte olmalıdır.");

        RuleFor(x => x.Items)
            .NotNull()
            .NotEmpty()
            .WithMessage("Kampanyaya en az bir ürün eklemelisiniz.");

        RuleFor(x => x.Items)
            .Must(items =>
                items is not null &&
                items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Aynı ürün kampanyaya birden fazla kez eklenemez.");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId)
                    .NotEmpty()
                    .WithMessage("Ürün seçimi zorunludur.");

                item.RuleFor(x => x.DiscountType)
                    .IsInEnum()
                    .WithMessage("Geçersiz indirim türü.");

                item.RuleFor(x => x.DiscountValue)
                    .GreaterThan(0)
                    .WithMessage("İndirim değeri sıfırdan büyük olmalıdır.");

                item.RuleFor(x => x.DiscountValue)
                    .LessThan(100)
                    .When(x => x.DiscountType == DiscountType.Percentage)
                    .WithMessage("Yüzdelik indirim %100'den küçük olmalıdır.");
            });
    }
}
