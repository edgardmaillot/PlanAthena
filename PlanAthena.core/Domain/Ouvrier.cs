// PlanAthena.Core.Domain.Ouvrier.cs
using System;
using System.Collections.Generic;
using System.Linq;
using PlanAthena.Core.Domain.ValueObjects;
// NiveauExpertise vient de Competence, qui le prendra de Facade.Dto.Enums pour l'instant

namespace PlanAthena.Core.Domain
{
    public class Ouvrier : Entity<OuvrierId>
    {
        public string Nom { get; }
        public string Prenom { get; }
        public CoutJournalier Cout { get; }
        public IReadOnlyDictionary<MetierId, Competence> Competences { get; }

        public Ouvrier(OuvrierId id, string nom, string prenom, CoutJournalier cout, IEnumerable<Competence>? competencesInitiales = null)
            : base(id)
        {
            if (string.IsNullOrWhiteSpace(nom)) throw new ArgumentException("Le nom ne peut pas être vide.", nameof(nom));
            if (string.IsNullOrWhiteSpace(prenom)) throw new ArgumentException("Le prénom ne peut pas être vide.", nameof(prenom));

            Nom = nom;
            Prenom = prenom;
            Cout = cout;

            // Les compétences sont fournies à la construction et le dictionnaire est construit une fois.
            // Competence elle-même est immuable après construction.
            _competences = competencesInitiales?.ToDictionary(c => c.MetierId)
                           ?? new Dictionary<MetierId, Competence>();
            Competences = _competences; // Exposition de la version readonly
        }

        // Dictionnaire privé pour construction, si on veut faire des validations avant d'exposer en readonly
        private readonly Dictionary<MetierId, Competence> _competences;


        // Pas de méthodes de modification (ModifierIdentite, ModifierCout, Ajouter/RetirerCompetence)

        public bool PossedeCompetence(MetierId metierId) => Competences.ContainsKey(metierId);

        public Competence? GetCompetence(MetierId metierId) =>
            Competences.TryGetValue(metierId, out var competence) ? competence : null;

        // GetNiveauPourMetier reste pertinent car c'est une interrogation de l'état.
        public Facade.Dto.Enums.NiveauExpertise? GetNiveauPourMetier(MetierId metierId) =>
            GetCompetence(metierId)?.Niveau;

        protected Ouvrier() : base() { }
    }
}