using eMarket.Domain.Promotions.Enums;
using FluentValidation;

namespace eMarket.Application.Promotions.Commands.UpdatePromotion;

public sealed class UpdatePromotionCommandValidator
    : AbstractValidator<UpdatePromotionCommand>
{
    public UpdatePromotionCommandValidator()
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

        RuleFor(x => x)
            .Must(x => x.EndsAtUtc > x.StartsAtUtc)
            .WithMessage("Kampanya bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        RuleFor(x => x.EndsAtUtc)
            .Must(x => x > DateTime.UtcNow)
            .WithMessage("Kampanya bitiş tarihi gelecekte olmalıdır.");

        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("En az bir ürün seçilmelidir.")
            .NotEmpty()
            .WithMessage("En az bir ürün seçilmelidir.");

        RuleForEach(x => x.Items).ChildRules(item =>
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

            item.RuleFor(x => x)
                .Must(x => x.DiscountType != DiscountType.Percentage
                           || x.DiscountValue < 100)
                .WithMessage("İndirim oranı %100'den küçük olmalıdır.");
        });

        RuleFor(x => x.Items)
            .Must(items => items == null
                || items.Select(x => x.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Aynı ürün kampanyaya birden fazla eklenemez.");
    }
}
