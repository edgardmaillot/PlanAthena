// PlanAthena.Core.Infrastructure.Services.CalendrierService.cs
using NodaTime;
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Input;
using System.Collections.Frozen;

namespace PlanAthena.Core.Infrastructure.Services;

public class CalendrierService : ICalendrierService
{
    // Méthode existante conservée pour compatibilité avec EF01
    public CalendrierOuvreChantier CreerCalendrierOuvreChantier(
        CalendrierTravailDefinitionDto definitionDto,
        DateTime? dateDebutSouhaiteeChantier,
        DateTime? dateFinSouhaiteeChantier)
    {
        ArgumentNullException.ThrowIfNull(definitionDto);

        var joursOuvresSet = definitionDto.JoursOuvres
            .Select(d => (IsoDayOfWeek)d)
            .ToHashSet();

        var heureDebut = new LocalTime(definitionDto.HeureDebutJournee, 0);
        var dureeTravail = Duration.FromHours(definitionDto.HeuresTravailEffectifParJour);

        var joursChomesSet = definitionDto.JoursChomes
            .Select(d => LocalDate.FromDateTime(d))
            .ToHashSet();

        return new CalendrierOuvreChantier(
            joursOuvresSet,
            heureDebut,
            dureeTravail,
            joursChomesSet);
    }

    // Nouvelle fonctionnalité du Lot 1 / EF02
    public EchelleTempsOuvree CreerEchelleTempsOuvree(
        CalendrierOuvreChantier calendrier,
        LocalDate dateDebut,
        LocalDate dateFin)
    {
        var slots = new List<SlotTemporel>();
        var indexLookup = new Dictionary<LocalDateTime, int>();
        int currentIndex = 0;

        for (var date = dateDebut; date <= dateFin; date = date.PlusDays(1))
        {
            if (!calendrier.EstJourOuvre(date))
            {
                continue;
            }

            var debutPlage = date.At(calendrier.HeureDebutTravail);
            // CORRECTION FINALE ET DÉFINITIVE : Utilisation de l'opérateur correct
            var finPlage = debutPlage.PlusTicks(calendrier.DureeTravailEffectiveParJour.BclCompatibleTicks);

            GenererSlotsPourPlage(debutPlage, finPlage, ref currentIndex, slots, indexLookup);
        }

        return new EchelleTempsOuvree(
            slots.AsReadOnly(),
            indexLookup.ToFrozenDictionary()
        );
    }

    private static void GenererSlotsPourPlage(
        LocalDateTime debutPlage,
        LocalDateTime finPlage,
        ref int currentIndex,
        List<SlotTemporel> slots,
        Dictionary<LocalDateTime, int> indexLookup)
    {
        var heureCourante = debutPlage;
        while (heureCourante < finPlage)
        {
            var finSlot = heureCourante.PlusHours(1);

            if (finSlot > finPlage)
            {
                finSlot = finPlage;
            }

            var slot = new SlotTemporel(currentIndex, heureCourante, finSlot);
            slots.Add(slot);
            indexLookup[heureCourante] = currentIndex;
            currentIndex++;

            heureCourante = finSlot; // L'incrément se fait avec la fin du slot précédent.
        }
    }
}