// PlanAthena.Core.Domain.Metier.cs
using System;
using System.Collections.Generic;
using System.Linq;
using PlanAthena.Core.Domain.ValueObjects;

namespace PlanAthena.Core.Domain
{
    public class Metier : Entity<MetierId>
    {
        public string Nom { get; } // Pas de private set, initialisé au constructeur
        public IReadOnlySet<MetierId> PrerequisMetierIds { get; }

        public Metier(MetierId id, string nom, IEnumerable<MetierId>? prerequisMetierIds = null)
            : base(id)
        {
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Le nom du métier ne peut pas être vide.", nameof(nom));

            Nom = nom;

            var tempPrerequis = new HashSet<MetierId>();
            if (prerequisMetierIds != null)
            {
                foreach (var prerequisId in prerequisMetierIds)
                {
                    if (prerequisId.Equals(Id)) // Validation à la construction
                        throw new InvalidOperationException("Un métier ne peut pas être son propre prérequis.");
                    tempPrerequis.Add(prerequisId);
                }
            }
            PrerequisMetierIds = tempPrerequis; // Assignation à la propriété IReadOnlySet
        }

        // Pas de méthodes de modification (ModifierNom, AjouterPrerequis, RetirerPrerequis)

        // Constructeur protégé pour ORM / sérialisation si un jour pertinent, mais inutile pour notre cas stateless.
        // On pourrait même le supprimer si on est stricts sur le fait qu'il n'y aura jamais de persistance/ORM dans PA.core.
        // Pour l'instant, le garder ne nuit pas.
        protected Metier() : base() { }
    }
}