// PlanAthena.Core.Application.Services.ChantierSetupInputMapper.cs
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;

namespace PlanAthena.Core.Application.Services
{
    /// <summary>
    /// Service responsable de la transformation du DTO d'entrée (ChantierSetupInputDto)
    /// en un agrégat de domaine Chantier complet et cohérent. C'est ici que la logique
    /// d'injection des entités virtuelles pour les jalons est implémentée, afin de
    /// présenter un modèle unifié au reste de l'application.
    /// </summary>
    public class ChantierSetupInputMapper : IChantierSetupInputMapper
    {
        private readonly ICalendrierService _calendrierService;

        // Définition de constantes pour les entités virtuelles afin de garantir la cohérence
        // et de faciliter la maintenance. Ces IDs ne doivent pas entrer en conflit avec les données réelles.
        private const string VIRTUAL_METIER_ID = "SYS_JALON_METIER";
        private const string VIRTUAL_METIER_NOM = "Métier Virtuel pour Jalons";
        private const string VIRTUAL_OUVRIER_NOM = "Ouvrier Virtuel";
        private const string VIRTUAL_OUVRIER_PRENOM_PREFIX = "Jalon";

        public ChantierSetupInputMapper(ICalendrierService calendrierService)
        {
            _calendrierService = calendrierService ?? throw new ArgumentNullException(nameof(calendrierService));
        }

        public Task<(Chantier? chantier, List<MessageValidationDto> messages)> MapToDomainAsync(ChantierSetupInputDto inputDto)
        {
            var messages = new List<MessageValidationDto>();
            Chantier? chantierDomaine = null;

            try
            {
                // 1. Mapper les VOs simples du Chantier
                var chantierId = new ChantierId(inputDto.ChantierId);
                var periodeSouhaitee = new PeriodePlanification(
                    inputDto.DateDebutSouhaitee,
                    inputDto.DateFinSouhaitee,
                    inputDto.FlexibiliteDebut,
                    inputDto.FlexibiliteFin);

                // 2. Créer le CalendrierOuvreChantier via le service
                CalendrierOuvreChantier calendrierOuvre = _calendrierService.CreerCalendrierOuvreChantier(
                    inputDto.CalendrierTravail,
                    inputDto.DateDebutSouhaitee,
                    inputDto.DateFinSouhaitee);

                // 3. Logique d'injection des entités virtuelles
                // On vérifie en amont s'il y a des jalons. Si ce n'est pas le cas,
                // on évite de créer inutilement le métier virtuel.
                bool hasMilestones = inputDto.Taches.Any(t => t.Type != TypeActivite.Tache);
                Metier? metierVirtuel = null;

                // 4. Mapper Metiers
                var metiersDomaine = new List<Metier>();
                foreach (var metierDto in inputDto.Metiers)
                {
                    var metierId = new MetierId(metierDto.MetierId);
                    var prerequisIds = metierDto.PrerequisMetierIds.Select(id => new MetierId(id)).ToList();
                    metiersDomaine.Add(new Metier(metierId, metierDto.Nom, prerequisIds));
                }

                // Si des jalons existent, on crée UN SEUL métier virtuel qui leur sera commun.
                // Cela simplifie le modèle en aval : pour le solveur, un jalon est juste une tâche
                // requérant un métier spécifique (le métier virtuel).
                if (hasMilestones)
                {
                    metierVirtuel = new Metier(new MetierId(VIRTUAL_METIER_ID), VIRTUAL_METIER_NOM, Enumerable.Empty<MetierId>());
                    metiersDomaine.Add(metierVirtuel);
                }

                // 5. Mapper Ouvriers et leurs Compétences
                var ouvriersDomaine = new List<Ouvrier>();
                foreach (var ouvrierDto in inputDto.Ouvriers)
                {
                    var ouvrierId = new OuvrierId(ouvrierDto.OuvrierId);
                    var coutJournalier = new CoutJournalier(ouvrierDto.CoutJournalier);

                    var competencesDomaine = new List<Competence>();
                    foreach (var compDto in ouvrierDto.Competences)
                    {
                        competencesDomaine.Add(new Competence(
                            new MetierId(compDto.MetierId),
                            compDto.Niveau,
                            compDto.PerformancePct));
                    }
                    ouvriersDomaine.Add(new Ouvrier(ouvrierId, ouvrierDto.Nom, ouvrierDto.Prenom, coutJournalier, competencesDomaine));
                }

                // Compteur pour garantir des IDs uniques pour les ouvriers virtuels
                long virtualWorkerIdCounter = 0;

                // 6. Mapper Blocs et leurs Taches (avec injection pour les jalons)
                var blocsDomaine = new List<BlocTravail>();
                foreach (var blocDto in inputDto.Blocs)
                {
                    var blocId = new BlocId(blocDto.BlocId);
                    var capacite = new CapaciteOuvriers(blocDto.CapaciteMaxOuvriers);

                    var tachesPourCeBloc = new List<Tache>();
                    var tachesDtoPourCeBloc = inputDto.Taches.Where(t => t.BlocId == blocDto.BlocId);

                    foreach (var tacheDto in tachesDtoPourCeBloc)
                    {
                        var tacheId = new TacheId(tacheDto.TacheId);
                        var duree = new DureeHeuresHomme(tacheDto.HeuresHommeEstimees);
                        var dependances = tacheDto.Dependencies.Select(id => new TacheId(id)).ToList();
                        MetierId metierRequisId;

                        if (tacheDto.Type == TypeActivite.Tache)
                        {
                            // Cas standard : une tâche réelle avec un métier réel.
                            metierRequisId = new MetierId(tacheDto.MetierId);
                        }
                        else
                        {
                            // Cas d'un jalon : on applique la stratégie d'injection.
                            if (metierVirtuel == null)
                                throw new InvalidOperationException("Incohérence interne: Un jalon a été trouvé mais le métier virtuel n'a pas été initialisé.");

                            // On assigne le métier virtuel au jalon.
                            metierRequisId = metierVirtuel.Id;

                            // Chaque jalon doit être traité comme une tâche indépendante par le solveur.
                            // Pour y parvenir sans compliquer la logique du solveur, nous créons un ouvrier virtuel
                            // unique pour chaque jalon. Cet ouvrier est le seul à posséder la compétence du "métier jalon".
                            // Le solveur n'aura donc d'autre choix que d'assigner cet ouvrier à ce jalon,
                            // isolant ainsi le jalon des autres tâches.
                            virtualWorkerIdCounter++;
                            var ouvrierVirtuelId = new OuvrierId($"VIRTUAL_OUVRIER_{virtualWorkerIdCounter}");
                            var competenceVirtuelle = new Competence(metierVirtuel.Id, NiveauExpertise.Maitre, 100);
                            var ouvrierVirtuel = new Ouvrier(
                                ouvrierVirtuelId,
                                VIRTUAL_OUVRIER_NOM,
                                $"{VIRTUAL_OUVRIER_PRENOM_PREFIX} '{tacheDto.Nom}'",
                                new CoutJournalier(0), // Un jalon n'a pas de coût.
                                new[] { competenceVirtuelle });

                            ouvriersDomaine.Add(ouvrierVirtuel);
                        }

                        // On crée l'entité Tache en passant le type et le MetierId (réel ou virtuel).
                        tachesPourCeBloc.Add(new Tache(tacheId, tacheDto.Nom, tacheDto.Type, blocId, duree, metierRequisId, dependances));
                    }
                    blocsDomaine.Add(new BlocTravail(blocId, blocDto.Nom, capacite, tachesPourCeBloc));
                }

                // 7. Mapper Lots
                var lotsDomaine = new List<LotTravaux>();
                foreach (var lotDto in inputDto.Lots)
                {
                    var lotId = new LotId(lotDto.LotId);
                    var blocIdsPourCeLot = lotDto.BlocIds.Select(id => new BlocId(id)).ToList();
                    lotsDomaine.Add(new LotTravaux(
                        lotId,
                        lotDto.Nom,
                        lotDto.Priorite,
                        blocIdsPourCeLot,
                        lotDto.DateDebutAuPlusTotSouhaitee,
                        lotDto.DateFinAuPlusTardSouhaitee));
                }

                // 8. Mapper ConfigurationCdC (si présente)
                ConfigurationChantier? configCdCDomaine = null;
                if (inputDto.ConfigurationCdC != null)
                {
                    configCdCDomaine = ConfigurationChantier.CreerDepuisIds(
                        inputDto.ConfigurationCdC.OuvriersClefsIds,
                        inputDto.ConfigurationCdC.MetiersClefsIds);
                }

                // 9. Créer l'agrégat Chantier
                // Les listes (metiersDomaine, ouvriersDomaine) contiennent maintenant les entités réelles ET virtuelles.
                // L'objet Chantier est donc complet et cohérent pour le reste du système.
                chantierDomaine = new Chantier(
                    chantierId,
                    inputDto.Description,
                    periodeSouhaitee,
                    calendrierOuvre,
                    metiersDomaine,
                    ouvriersDomaine,
                    blocsDomaine,
                    lotsDomaine,
                    configCdCDomaine);
            }
            catch (ArgumentException ex)
            {
                messages.Add(new MessageValidationDto
                {
                    Type = TypeMessageValidation.Erreur,
                    CodeMessage = "ERR_MAP_ARGUMENT",
                    Message = $"Donnée invalide : {ex.Message}",
                    ProprieteConcernee = ex.ParamName
                });
            }
            catch (InvalidOperationException ex)
            {
                messages.Add(new MessageValidationDto
                {
                    Type = TypeMessageValidation.Erreur,
                    CodeMessage = "ERR_MAP_INVALID_OP",
                    Message = $"Opération invalide lors du mapping : {ex.Message}"
                });
            }

            return Task.FromResult((chantierDomaine, messages));
        }
    }
}