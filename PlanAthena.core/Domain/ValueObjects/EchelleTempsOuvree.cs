using NodaTime;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Core.Domain.ValueObjects;

/// <summary>
/// Représente l'échelle de temps discrète et complète pour la planification d'un chantier.
/// Elle contient la liste ordonnée de tous les slots de travail disponibles.
/// POURQUOI : Cet objet est la "traduction" du calendrier métier en un domaine temporel
/// que le solveur OR-Tools peut comprendre et utiliser. Il est immuable.
/// </summary>
/// <param name="Slots">Liste ordonnée des slots temporels disponibles.</param>
/// <param name="IndexLookup">Table de correspondance pour une recherche rapide de l'index d'un slot à partir de son heure de début.</param>
public record EchelleTempsOuvree(
    IReadOnlyList<SlotTemporel> Slots,
    IReadOnlyDictionary<LocalDateTime, int> IndexLookup)
{
    /// <summary>
    /// Trouve l'index du slot qui contient un moment donné.
    /// </summary>
    /// <param name="moment">Le moment à rechercher.</param>
    /// <returns>L'index du slot, ou -1 si aucun slot ne contient ce moment.</returns>
    public int TrouverIndexSlot(LocalDateTime moment)
    {
        // POURQUOI : Optimisation. La recherche par dictionnaire est O(1) pour les débuts de slot exacts.
        if (IndexLookup.TryGetValue(moment, out int index))
        {
            return index;
        }

        // POURQUOI : Plan B. Si le moment n'est pas un début de slot, on parcourt la liste.
        // C'est moins performant mais garantit de trouver le slot si le moment est en plein milieu.
        return Slots.FirstOrDefault(s => s.Contient(moment))?.Index ?? -1;
    }

    /// <summary>
    /// Récupère le slot temporel qui contient un moment donné.
    /// </summary>
    public SlotTemporel? TrouverSlot(LocalDateTime moment)
    {
        var index = TrouverIndexSlot(moment);
        return index >= 0 && index < Slots.Count ? Slots[index] : null;
    }

    public int NombreTotalSlots => Slots.Count;
    public SlotTemporel? PremierSlot => Slots.FirstOrDefault();
    public SlotTemporel? DernierSlot => Slots.LastOrDefault();
}