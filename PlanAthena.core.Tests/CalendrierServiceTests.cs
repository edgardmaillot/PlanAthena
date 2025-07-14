using FluentAssertions;
using NodaTime;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Infrastructure.Services;

namespace ConsoleAppTester;

// POURQUOI : Ce fichier teste la classe CalendrierService de manière isolée.
// Il garantit que la logique de création de l'échelle de temps est correcte et robuste
// face à divers scénarios, sans dépendre du reste de l'application.
public class CalendrierServiceTests
{
    private readonly CalendrierService _service = new();

    // Test 1: Scénario nominal simple
    [Fact]
    public void CreerEchelleTempsOuvree_AvecCalendrierStandard_CreeLeBonNombreDeSlots()
    {
        // Arrange
        var calendrier = new CalendrierOuvreChantier(
            joursOuvres: new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday },
            heureDebutTravail: new LocalTime(8, 0),
            dureeTravailEffectiveParJour: Duration.FromHours(4),
            joursChomes: new HashSet<LocalDate>()
        );
        var dateDebut = new LocalDate(2028, 6, 26); // Un lundi
        var dateFin = new LocalDate(2028, 6, 26);

        // Act
        var echelle = _service.CreerEchelleTempsOuvree(calendrier, dateDebut, dateFin);

        // Assert
        echelle.NombreTotalSlots.Should().Be(4); // 8-9, 9-10, 10-11, 11-12
        echelle.PremierSlot?.Debut.Should().Be(new LocalDateTime(2028, 6, 26, 8, 0));
        echelle.DernierSlot?.Fin.Should().Be(new LocalDateTime(2028, 6, 26, 12, 0));
    }

    // Test 2: Gestion des jours fériés
    [Fact]
    public void CreerEchelleTempsOuvree_AvecJourFerie_IgnoreLeJourCorrectement()
    {
        // Arrange
        var jourFerie = new LocalDate(2028, 7, 14); // Vendredi 14 juillet
        var calendrier = new CalendrierOuvreChantier(
            joursOuvres: new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Friday },
            heureDebutTravail: new LocalTime(9, 0),
            dureeTravailEffectiveParJour: Duration.FromHours(8),
            joursChomes: new HashSet<LocalDate> { jourFerie }
        );
        var dateDebut = jourFerie;
        var dateFin = jourFerie;

        // Act
        var echelle = _service.CreerEchelleTempsOuvree(calendrier, dateDebut, dateFin);

        // Assert
        echelle.NombreTotalSlots.Should().Be(0);
    }

    // Test 3: Gestion des week-ends
    [Fact]
    public void CreerEchelleTempsOuvree_SurUnWeekend_NeGenereAucunSlot()
    {
        // Arrange
        var calendrier = new CalendrierOuvreChantier(
            joursOuvres: new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday, IsoDayOfWeek.Friday },
            heureDebutTravail: new LocalTime(8, 0),
            dureeTravailEffectiveParJour: Duration.FromHours(7),
            joursChomes: new HashSet<LocalDate>()
        );
        var dateDebut = new LocalDate(2028, 7, 1); // Un samedi
        var dateFin = new LocalDate(2028, 7, 2);   // Un dimanche

        // Act
        var echelle = _service.CreerEchelleTempsOuvree(calendrier, dateDebut, dateFin);

        // Assert
        echelle.NombreTotalSlots.Should().Be(0);
    }

    // Test 4: Période invalide
    [Fact]
    public void CreerEchelleTempsOuvree_AvecDateFinAvantDateDebut_RetourneEchelleVide()
    {
        // Arrange
        var calendrier = new CalendrierOuvreChantier(
            joursOuvres: new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday },
            heureDebutTravail: new LocalTime(8, 0),
            dureeTravailEffectiveParJour: Duration.FromHours(8),
            joursChomes: new HashSet<LocalDate>()
        );
        var dateDebut = new LocalDate(2028, 7, 1);
        var dateFin = new LocalDate(2028, 6, 30); // Fin avant début

        // Act
        var echelle = _service.CreerEchelleTempsOuvree(calendrier, dateDebut, dateFin);

        // Assert
        echelle.NombreTotalSlots.Should().Be(0);
    }

    // Test 5: Durée non entière
    [Fact]
    public void CreerEchelleTempsOuvree_AvecDureeNonEntiere_CreeUnDernierSlotPlusCourt()
    {
        // Arrange
        var calendrier = new CalendrierOuvreChantier(
            joursOuvres: new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Wednesday },
            heureDebutTravail: new LocalTime(8, 0),
            dureeTravailEffectiveParJour: Duration.FromMinutes(150), // 2.5 heures
            joursChomes: new HashSet<LocalDate>()
        );
        var dateDebut = new LocalDate(2028, 7, 5); // Un mercredi
        var dateFin = new LocalDate(2028, 7, 5);

        // Act
        var echelle = _service.CreerEchelleTempsOuvree(calendrier, dateDebut, dateFin);
        var dernierSlot = echelle.DernierSlot;

        // Assert
        echelle.NombreTotalSlots.Should().Be(3);
        echelle.Slots.ElementAt(0).Duree.Should().Be(Duration.FromHours(1));
        echelle.Slots.ElementAt(1).Duree.Should().Be(Duration.FromHours(1));
        dernierSlot.Should().NotBeNull();
        dernierSlot?.Duree.Should().Be(Duration.FromMinutes(30));
        dernierSlot?.Fin.Should().Be(new LocalDateTime(2028, 7, 5, 10, 30));
    }

    // Test 6: Méthodes de recherche
    [Fact]
    public void TrouverIndexSlot_SurEchelleGeneree_RetourneLesBonsIndex()
    {
        // Arrange
        var calendrier = new CalendrierOuvreChantier(
            joursOuvres: new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Thursday },
            heureDebutTravail: new LocalTime(10, 0),
            dureeTravailEffectiveParJour: Duration.FromHours(2),
            joursChomes: new HashSet<LocalDate>()
        );
        var date = new LocalDate(2028, 7, 6);
        var echelle = _service.CreerEchelleTempsOuvree(calendrier, date, date); // Crée 2 slots: [10h-11h] et [11h-12h]

        // Act & Assert
        // Début exact d'un slot
        echelle.TrouverIndexSlot(new LocalDateTime(2028, 7, 6, 11, 0)).Should().Be(1);

        // Milieu d'un slot
        echelle.TrouverIndexSlot(new LocalDateTime(2028, 7, 6, 10, 30)).Should().Be(0);

        // Moment en dehors de tout slot (avant le début)
        echelle.TrouverIndexSlot(new LocalDateTime(2028, 7, 6, 9, 59)).Should().Be(-1);

        // Moment en dehors de tout slot (après la fin)
        echelle.TrouverIndexSlot(new LocalDateTime(2028, 7, 6, 12, 0)).Should().Be(-1);
    }
}