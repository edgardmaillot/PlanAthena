// Fichier: Services/Business/DTOs/UIDTOs.cs

using PlanAthena.Data;

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// Représente une dépendance potentielle dans l'interface utilisateur avec son état d'affichage.
    /// Cette classe encapsule l'information nécessaire pour l'affichage dans TacheDetailForm
    /// en combinant la tâche prédécesseur avec son état de suggestion/sélection.
    /// </summary>
    public class DependanceAffichage
    {
        /// <summary>
        /// La tâche qui pourrait être un prédécesseur de la tâche courante.
        /// </summary>
        public Tache TachePredecesseur { get; set; }

        /// <summary>
        /// L'état de cette dépendance qui détermine son affichage visuel.
        /// </summary>
        public EtatDependance Etat { get; set; }

        /// <summary>
        /// Indique si cette dépendance provient des règles métier (héritée) 
        /// ou a été définie manuellement par l'utilisateur.
        /// 
        /// - true : Dépendance suggérée par les règles métier ou exclue par l'utilisateur
        /// - false : Dépendance stricte définie manuellement ou neutre
        /// </summary>
        public bool EstHeritee { get; set; }

        /// <summary>
        /// Fournit une représentation textuelle pour l'affichage dans la CheckedListBox.
        /// Format : "TacheId - TacheNom" pour une identification claire.
        /// </summary>
        public override string ToString()
        {
            if (TachePredecesseur == null)
                return "Tâche inconnue";

            string prefix = Etat switch
            {
                EtatDependance.Suggeree => "💡 ",    // Icône suggestion
                EtatDependance.Exclue => "❌ ",      // Icône exclusion  
                EtatDependance.Stricte => "✅ ",     // Icône validé
                _ => "⚪ "                           // Icône neutre
            };

            return $"{prefix}{TachePredecesseur.TacheNom}";
        }
    }

    /// <summary>
    /// Définit les différents états d'affichage d'une dépendance dans l'interface utilisateur.
    /// Chaque état correspond à un style visuel spécifique dans TacheDetailForm.
    /// </summary>
    public enum EtatDependance
    {
        /// <summary>
        /// Dépendance affichée mais ni suggérée ni exclue.
        /// Affichage : Texte normal, case décochée
        /// </summary>
        Neutre,

        /// <summary>
        /// Dépendance proposée par les règles métier.
        /// Affichage : Texte bleu italique, case cochée
        /// </summary>
        Suggeree,

        /// <summary>
        /// Dépendance créée manuellement par l'utilisateur.
        /// Affichage : Texte noir gras, case cochée
        /// </summary>
        Stricte,

        /// <summary>
        /// Suggestion rejetée par l'utilisateur.
        /// Affichage : Texte gris barré, case décochée
        /// </summary>
        Exclue
    }
}