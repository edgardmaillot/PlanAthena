using PlanAthena.Data;

namespace PlanAthena.Services.Business
{
    /// <summary>
    /// Service source de vérité pour la gestion des Blocs.
    /// </summary>
    public class BlocService
    {
        private readonly Dictionary<string, Bloc> _blocs = new Dictionary<string, Bloc>();

        public void AjouterBloc(Bloc bloc)
        {
            if (bloc == null) throw new ArgumentNullException(nameof(bloc));
            if (string.IsNullOrWhiteSpace(bloc.BlocId)) throw new ArgumentException("L'ID du bloc ne peut pas être vide.");
            if (_blocs.ContainsKey(bloc.BlocId)) throw new InvalidOperationException($"Un bloc avec l'ID '{bloc.BlocId}' existe déjà.");
            _blocs.Add(bloc.BlocId, bloc);
        }

        public void ModifierBloc(Bloc blocModifie)
        {
            if (blocModifie == null) throw new ArgumentNullException(nameof(blocModifie));
            if (!_blocs.ContainsKey(blocModifie.BlocId)) throw new KeyNotFoundException($"Bloc {blocModifie.BlocId} non trouvé.");
            _blocs[blocModifie.BlocId] = blocModifie;
        }

        public Bloc ObtenirBlocParId(string blocId)
        {
            _blocs.TryGetValue(blocId, out var bloc);
            return bloc;
        }

        public List<Bloc> ObtenirTousLesBlocs()
        {
            return _blocs.Values.OrderBy(b => b.Nom).ToList();
        }

        public void SupprimerBloc(string blocId)
        {
            // Note: Une validation pour s'assurer qu'aucune tâche n'utilise ce bloc sera ajoutée dans une phase ultérieure.
            if (!_blocs.Remove(blocId))
            {
                throw new KeyNotFoundException($"Bloc {blocId} non trouvé.");
            }
        }

        public void RemplacerTousLesBlocs(List<Bloc> blocs)
        {
            _blocs.Clear();
            if (blocs != null)
            {
                foreach (var bloc in blocs)
                {
                    if (!_blocs.ContainsKey(bloc.BlocId))
                    {
                        _blocs.Add(bloc.BlocId, bloc);
                    }
                }
            }
        }

        public void Vider()
        {
            _blocs.Clear();
        }
    }
}