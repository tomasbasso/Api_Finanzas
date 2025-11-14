using Api_Finanzas.ModelsDTO;
using FluentValidation;

namespace Api_Finanzas.Validators
{
    public class TransaccionEditarValidator : AbstractValidator<TransaccionEditarDto>
    {
        public TransaccionEditarValidator()
        {
            RuleFor(x => x.Monto).GreaterThan(0).WithMessage("Monto debe ser mayor a 0");
            RuleFor(x => x.Fecha).NotEmpty();
            RuleFor(x => x.CuentaId).GreaterThan(0);
            RuleFor(x => x.Tipo).NotEmpty().Must(t => t == "Gasto" || t == "Ingreso").WithMessage("Tipo inválido");
        }
    }
}
