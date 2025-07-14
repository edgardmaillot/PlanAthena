// PlanAthena.Core.Application.Services.ChantierSetupInputMapper.cs
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Domain;
using PlanAthena.Core.Domain.ValueObjects;
using PlanAthena.Core.Facade.Dto.Enums;
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;

namespace PlanAthena.Core.Application.Services
{
    public class ChantierSetupInputMapper : IChantierSetupInputMapper
    {
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

                // 5. Mapper Blocs et leurs Taches
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