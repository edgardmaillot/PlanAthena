// PlanAthena.Core.Tests/TestHelpers/TestModelFactory.cs

using NodaTime;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Core.Tests.TestHelpers
{
    /// <summary>
    /// Fournit des méthodes statiques pour créer des objets de domaine complexes (Chantier, ProblemeOptimisation)
    /// pour les tests unitaires et d'intégration.
    /// Centralise la logique de création de données de test, garantit que les objets créés
    /// sont valides selon les règles des constructeurs du domaine, et rend les tests plus lisibles
    /// en masquant les détails de l'instanciation.
    /// </summary>
    public static class TestModelFactory
    {
        // IDs constants pour pouvoir les réutiliser facilement dans les assertions des tests
        public static readonly BlocId DefaultBlocId = new BlocId("BLOC_DEFAUT");
        public static readonly LotId DefaultLotId = new LotId("LOT_DEFAUT");
        public static readonly TacheId TacheA_Id = new TacheId("TACHE_A");
        public static readonly TacheId TacheB_Id = new TacheId("TACHE_B");
        public static readonly TacheId JalonSechage_Id = new TacheId("JALON_SECHAGE");
        public static readonly TacheId TachePlomberie_Id = new TacheId("TACHE_PLOMBERIE");

        /// <summary>
        /// Crée un problème simple avec deux tâches séquentielles (A -> B) dans le même métier.
        /// </summary>
        public static ProblemeOptimisation CreerProblemeSimple()
        {
            var metierId = new MetierId("MECANIQUE");
            var metier = new Metier(metierId, "Mécanique");

            var competence = new Competence(metierId, NiveauExpertise.Confirme, 100);
            var ouvrier1 = new Ouvrier(new OuvrierId("1"), "Ouvrier", "1", new CoutJournalier(200), new List<Competence> { competence });

            var tacheA = new Tache(
                id: TacheA_Id,
                nom: "Tache A",
                type: TypeActivite.Tache,
                blocParentId: DefaultBlocId,
                heuresHommeEstimees: new DureeHeuresHomme(8),
                metierRequisId: metierId,
                dependencies: null);

            var tacheB = new Tache(
                id: TacheB_Id,
                nom: "Tache B",
                type: TypeActivite.Tache,
                blocParentId: DefaultBlocId,
                heuresHommeEstimees: new DureeHeuresHomme(16),
                metierRequisId: metierId,
                dependencies: new List<TacheId> { tacheA.Id });

            var echelleTemps = CreerEchelleTempsSimple(10); // 10 jours de 8h = 80 slots/heures

            var chantier = CreerChantierDeBase(
                new List<Ouvrier> { ouvrier1 },
                new List<Tache> { tacheA, tacheB },
                new List<Metier> { metier }
            );

            var configOptimisation = new ConfigurationOptimisation(8, 10, 5000); // 8h/jour, 10% pénalité, 50€ coût indirect
            chantier.AppliquerConfigurationOptimisation(configOptimisation);

            return new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps, Configuration = configOptimisation };
        }

        /// <summary>
        /// Crée un problème avec un jalon (temps continu) dont dépend une tâche (temps ouvré).
        /// Simule la logique d'injection des entités virtuelles qui serait faite par le ChantierSetupInputMapper.
        /// </summary>
        public static ProblemeOptimisation CreerProblemeAvecJalon()
        {
            var metierReelId = new MetierId("PLOMBERIE");
            var metierVirtuelId = new MetierId("VIRTUAL_METIER");

            var metierReel = new Metier(metierReelId, "Plomberie");
            var metierVirtuel = new Metier(metierVirtuelId, "Métier Virtuel");

            var competenceReelle = new Competence(metierReelId, NiveauExpertise.Expert, 100);
            var competenceVirtuelle = new Competence(metierVirtuelId, NiveauExpertise.Debutant, 100);

            var ouvrierReel = new Ouvrier(new OuvrierId("1"), "Plombier", "1", new CoutJournalier(250), new List<Competence> { competenceReelle });
            var ouvrierVirtuel = new Ouvrier(new OuvrierId("VIRTUAL_WORKER_1"), "Ouvrier", "Virtuel", new CoutJournalier(0), new List<Competence> { competenceVirtuelle });

            var jalonSechage = new Tache(
                id: JalonSechage_Id,
                nom: "Séchage béton",
                type: TypeActivite.JalonUtilisateur,
                blocParentId: DefaultBlocId,
                heuresHommeEstimees: new DureeHeuresHomme(72), // 72h continues
                metierRequisId: metierVirtuelId, // Assigné au métier virtuel
                dependencies: null
            );

            var tachePlomberie = new Tache(
                id: TachePlomberie_Id,
                nom: "Pose tuyauterie",
                type: TypeActivite.Tache,
                blocParentId: DefaultBlocId,
                heuresHommeEstimees: new DureeHeuresHomme(8),
                metierRequisId: metierReelId,
                dependencies: new List<TacheId> { jalonSechage.Id });

            var echelleTemps = CreerEchelleTempsSimple(20); // 20 jours de 8h

            var chantier = CreerChantierDeBase(
                new List<Ouvrier> { ouvrierReel, ouvrierVirtuel },
                new List<Tache> { jalonSechage, tachePlomberie },
                new List<Metier> { metierReel, metierVirtuel }
            );

            var configOptimisation = new ConfigurationOptimisation(8, 10, 5000);
            chantier.AppliquerConfigurationOptimisation(configOptimisation);

            return new ProblemeOptimisation { Chantier = chantier, EchelleTemps = echelleTemps, Configuration = configOptimisation };
        }

        /// <summary>
        /// Assemble un objet Chantier valide à partir des briques de base.
        /// </summary>
        private static Chantier CreerChantierDeBase(IEnumerable<Ouvrier> ouvriers, IEnumerable<Tache> taches, IEnumerable<Metier> metiers)
        {
            var bloc = new BlocTravail(DefaultBlocId, "Bloc de test", new CapaciteOuvriers(10), taches);
            var lot = new LotTravaux(DefaultLotId, "Lot de test", 1, new List<BlocId> { bloc.Id });

            var calendrier = new CalendrierOuvreChantier(
                joursOuvres: new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday, IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday, IsoDayOfWeek.Thursday, IsoDayOfWeek.Friday },
                heureDebutTravail: new LocalTime(8, 0),
                dureeTravailEffectiveParJour: Duration.FromHours(8),
                joursChomes: new HashSet<LocalDate>()
            );

            var periode = new PeriodePlanification(
                dateDebut: new System.DateTime(2023, 1, 1),
                dateFin: new System.DateTime(2023, 12, 31),
                FlexibiliteDate.Flexible,
                FlexibiliteDate.Flexible
            );

            return new Chantier(
                new ChantierId("CHANTIER_TEST"),
                "Chantier Test",
                periode,
                calendrier,
                metiers,
                ouvriers,
                new List<BlocTravail> { bloc },
                new List<LotTravaux> { lot },
                new ConfigurationChantier()
            );
        }

        /// <summary>
        /// Crée une échelle de temps simple : 8h/jour, 5j/semaine pour un certain nombre de jours.
        /// </summary>
        private static EchelleTempsOuvree CreerEchelleTempsSimple(int nombreDeJoursOuvres)
        {
            var slots = new List<SlotTemporel>();
            var indexLookup = new Dictionary<LocalDateTime, int>();
            var startDate = new LocalDate(2023, 1, 2); // Un lundi
            var startTime = new LocalTime(8, 0);
            var slotDuration = Duration.FromHours(1);
            int index = 0;
            int joursAjoutes = 0;

            for (int d = 0; joursAjoutes < nombreDeJoursOuvres; d++)
            {
                var currentDate = startDate.PlusDays(d);
                if (currentDate.DayOfWeek == IsoDayOfWeek.Saturday || currentDate.DayOfWeek == IsoDayOfWeek.Sunday) continue;

                for (int h = 0; h < 8; h++)
                {
                    var slotStart = currentDate.At(startTime.PlusHours(h));

                    // --- CORRECTION DÉFINITIVE APPLIQUÉE ICI ---
                    var slotEnd = LocalDateTime.FromDateTime(slotStart.ToDateTimeUnspecified() + slotDuration.ToTimeSpan());

                    // Utilisation du constructeur correct de SlotTemporel
                    slots.Add(new SlotTemporel(index, slotStart, slotEnd));
                    indexLookup[slotStart] = index;
                    index++;
                }
                joursAjoutes++;
            }
            return new EchelleTempsOuvree(slots, indexLookup);
        }
    }
}