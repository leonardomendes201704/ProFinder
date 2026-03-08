using ProFinder.Domain.Enums;

namespace ProFinder.Infrastructure.Helpers;

public static class InteractionTypeExtensions
{
    public static string ToDisplayLabel(this InteractionType interactionType) =>
        interactionType switch
        {
            InteractionType.PhoneCall => "Ligação",
            InteractionType.WhatsApp => "WhatsApp",
            InteractionType.Email => "E-mail",
            InteractionType.Note => "Observação",
            InteractionType.Visit => "Visita",
            _ => "Outro"
        };
}
