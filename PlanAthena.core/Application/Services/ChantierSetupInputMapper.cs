// PlanAthena.Core.Application.Services.ChantierSetupInputMapper.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;
using PlanAthena.Core.Facade.Dto.Enums; // Pour NiveauExpertise (via Competence)
using PlanAthena.Core.Application.Interfaces; // Pour l'interface

namespace PlanAthena.Core.Application.Services // Ou Mappers
{
    public class ChantierSetupInputMapper : IChantierSetupInputMapper
    {
        // Nous aurons besoin d'ICalendrierService pour créer le CalendrierOuvreChantier
        private readonly ICalendrierService _calendrierService;

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
                var chantierId = new ChantierId(inputDto.ChantierId); // Peut lever une exception si inputDto.ChantierId est invalide
                var periodeSouhaitee = new PeriodePlanification(
                    inputDto.DateDebutSouhaitee,
                    inputDto.DateFinSouhaitee,
                    inputDto.FlexibiliteDebut,
                    inputDto.FlexibiliteFin);

                // 2. Créer le CalendrierOuvreChantier via le service
                // Ce service encapsulera la logique NodaTime et la création du VO complexe.
                // Pour l'instant, ICalendrierService n'est pas encore implémenté,
                // donc cette ligne est conceptuelle.
                CalendrierOuvreChantier calendrierOuvre = _calendrierService.CreerCalendrierOuvreChantier(
                    inputDto.CalendrierTravail,
                    inputDto.DateDebutSouhaitee,
                    inputDto.DateFinSouhaitee);

                // 3. Mapper Metiers
                var metiersDomaine = new List<Metier>();
                foreach (var metierDto in inputDto.Metiers)
                {
                    var metierId = new MetierId(metierDto.MetierId);
                    var prerequisIds = metierDto.PrerequisMetierIds.Select(id => new MetierId(id)).ToList();
                    metiersDomaine.Add(new Metier(metierId, metierDto.Nom, prerequisIds));
                }

                // 4. Mapper Ouvriers et leurs Compétences
                var ouvriersDomaine = new List<Ouvrier>();
                foreach (var ouvrierDto in inputDto.Ouvriers)
                {
                    var ouvrierId = new OuvrierId(ouvrierDto.OuvrierId);
                    var coutJournalier = new CoutJournalier(ouvrierDto.CoutJournalier); // Devise par défaut EUR

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

                // 5. Mapper Blocs et leurs Taches
                // Il faut d'abord mapper toutes les tâches d'un bloc, puis créer le bloc.
                var blocsDomaine = new List<BlocTravail>();
                foreach (var blocDto in inputDto.Blocs)
                {
                    var blocId = new BlocId(blocDto.BlocId);
                    var capacite = new CapaciteOuvriers(blocDto.CapaciteMaxOuvriers);

                    var tachesPourCeBloc = new List<Tache>();
                    // Filtrer les TacheDto qui appartiennent à ce bloc
                    var tachesDtoPourCeBloc = inputDto.Taches.Where(t => t.BlocId == blocDto.BlocId);

                    foreach (var tacheDto in tachesDtoPourCeBloc)
                    {
                        var tacheId = new TacheId(tacheDto.TacheId);
                        var duree = new DureeHeuresHomme(tacheDto.HeuresHommeEstimees);
                        var metierRequisId = new MetierId(tacheDto.MetierId);
                        var dependances = tacheDto.Dependencies.Select(id => new TacheId(id)).ToList();

                        tachesPourCeBloc.Add(new Tache(tacheId, tacheDto.Nom, blocId, duree, metierRequisId, dependances));
                    }
                    blocsDomaine.Add(new BlocTravail(blocId, blocDto.Nom, capacite, tachesPourCeBloc));
                }

                // 6. Mapper Lots
                var lotsDomaine = new List<LotTravaux>();
                foreach (var lotDto in inputDto.Lots)
                {
                    var lotId = new LotId(lotDto.LotId);
                    var blocIdsPourCeLot = lotDto.BlocIds.Select(id => new BlocId(id)).ToList();
                    // Ici, on suppose que les BlocId dans lotDto.BlocIds existent bien.
                    // La validation des références croisées se fera plus tard par IChantierValidationService
                    // ou directement par le constructeur de Chantier si on y déplace cette logique.

                    lotsDomaine.Add(new LotTravaux(
                        lotId,
                        lotDto.Nom,
                        lotDto.Priorite,
                        blocIdsPourCeLot,
                        lotDto.DateDebutAuPlusTotSouhaitee,
                        lotDto.DateFinAuPlusTardSouhaitee));
                }

                // 7. Mapper ConfigurationCdC (si présente)
                ConfigurationChantier? configCdCDomaine = null;
                if (inputDto.ConfigurationCdC != null)
                {
                    configCdCDomaine = ConfigurationChantier.CreerDepuisIds(
                        inputDto.ConfigurationCdC.OuvriersClefsIds,
                        inputDto.ConfigurationCdC.MetiersClefsIds);
                }

                // 8. Créer l'agrégat Chantier
                // C'est le constructeur de Chantier qui effectuera les validations de cohérence d'ensemble
                // (ex: un bloc dans un lot existe, une tâche référence un métier existant, etc.)
                chantierDomaine = new Chantier(
                    chantierId,
                    inputDto.Description,
                    periodeSouhaitee,
                    calendrierOuvre, // Cet objet est crucial
                    metiersDomaine,
                    ouvriersDomaine,
                    blocsDomaine,
                    lotsDomaine,
                    configCdCDomaine);
            }
            catch (ArgumentException ex) // Capturer les exceptions de validation des VOs/Entités
            {
                messages.Add(new MessageValidationDto
                {
                    Type = TypeMessageValidation.Erreur,
                    CodeMessage = "ERR_MAP_ARGUMENT", // Code générique pour erreur d'argument au mapping
                    Message = $"Donnée invalide : {ex.Message}",
                    ProprieteConcernee = ex.ParamName // Nom du paramètre qui a causé l'erreur
                });
                // chantierDomaine restera null ou sera l'état partiel avant l'exception
            }
            catch (InvalidOperationException ex) // Capturer les exceptions d'opération invalide (ex: ID dupliqué)
            {
                messages.Add(new MessageValidationDto
                {
                    Type = TypeMessageValidation.Erreur,
                    CodeMessage = "ERR_MAP_INVALID_OP",
                    Message = $"Opération invalide lors du mapping : {ex.Message}"
                });
            }
            // On pourrait ajouter d'autres catch plus spécifiques si nécessaire

            return Task.FromResult((chantierDomaine, messages));
        }
    }
}