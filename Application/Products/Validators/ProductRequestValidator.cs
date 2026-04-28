using Application.Products.DTOs;
using FluentValidation;

namespace Application.Products.Validators;

public class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.CategoryIds)
            .NotEmpty().WithMessage("El producto debe pertenecer al menos a una categoría")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("No se pueden repetir categorías");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => x.Description is not null);
    }
}