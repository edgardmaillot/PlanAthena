// PlanAthena.Core.Facade.Dto.Enums.cs
namespace PlanAthena.Core.Facade.Dto.Enums
{
    public enum FlexibiliteDate
    {
        Imperative = 0,
        Flexible = 1,
        Preferentielle = 2
    }

    public enum NiveauExpertise
    {
        Debutant = 1,
        Confirme = 2,
        Expert = 3,
        Maitre = 4
    }

    // Enums pour la sortie (DTO ChantierSetupAnalysisResultDto)
    public enum EtatTraitementInput
    {
        Succes = 0,
        SuccesAvecAvertissements = 1,
        EchecValidation = 2
    }

    public enum TypeMessageValidation
    {
        Erreur = 1,
        Avertissement = 2,
        Suggestion = 3
    }
    public enum OptimizationStatus
    {
        Unknown,
        Optimal,      // Une solution optimale a été trouvée.
        Feasible,     // Une solution réalisable a été trouvée, mais pas forcément la meilleure.
        Infeasible,   // Le problème n'a aucune solution possible avec les contraintes données.
        ModelInvalid, // Le modèle mathématique est mal formé.
        Aborted       // L'optimisation a été annulée (par timeout ou manuellement).
    }
}