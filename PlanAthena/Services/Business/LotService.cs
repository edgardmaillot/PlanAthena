using PlanAthena.Data;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Service source de vérité pour la gestion des Lots.
    /// </summary>
    public class LotService
    {
        private readonly Dictionary<string, Lot> _lots = new Dictionary<string, Lot>();

        public void AjouterLot(Lot lot)
        {
            if (lot == null) throw new ArgumentNullException(nameof(lot));
            if (string.IsNullOrWhiteSpace(lot.LotId)) throw new ArgumentException("L'ID du lot ne peut pas être vide.");
            if (_lots.ContainsKey(lot.LotId)) throw new InvalidOperationException($"Un lot avec l'ID '{lot.LotId}' existe déjà.");
            _lots.Add(lot.LotId, lot);
        }

        public void ModifierLot(Lot lotModifie)
        {
            if (lotModifie == null) throw new ArgumentNullException(nameof(lotModifie));
            if (!_lots.ContainsKey(lotModifie.LotId)) throw new KeyNotFoundException($"Lot {lotModifie.LotId} non trouvé.");
            _lots[lotModifie.LotId] = lotModifie;
        }

        public Lot ObtenirLotParId(string lotId)
        {
            _lots.TryGetValue(lotId, out var lot);
            return lot;
        }

        public List<Lot> ObtenirTousLesLots()
        {
            return _lots.Values.OrderBy(l => l.Priorite).ThenBy(l => l.Nom).ToList();
        }

        public void SupprimerLot(string lotId)
        {
            // Note: Une validation pour s'assurer qu'aucune tâche n'utilise ce lot sera ajoutée dans une phase ultérieure.
            if (!_lots.Remove(lotId))
            {
                throw new KeyNotFoundException($"Lot {lotId} non trouvé.");
            }
        }

        public void RemplacerTousLesLots(List<Lot> lots)
        {
            _lots.Clear();
            if (lots != null)
            {
                foreach (var lot in lots)
                {
                    if (!_lots.ContainsKey(lot.LotId))
                    {
                        _lots.Add(lot.LotId, lot);
                    }
                }
            }
        }

        public void Vider()
        {
            _lots.Clear();
        }
    }
}