// Fichier : PlanAthena.Controls.Config/PertNodeBuilder.cs

using Microsoft.Msagl.Drawing;
using PlanAthena.Data;
using PlanAthena.Services.Business; // Inclut maintenant ProjetService
using MsaglColor = Microsoft.Msagl.Drawing.Color;

namespace PlanAthena.Controls.Config
{
    /// <summary>
    /// Responsable de la construction d'un nœud MSAGL (DrawingNode)
    /// à partir d'un objet métier (Tache).
    /// Centralise toute la logique de style et de mise en forme des nœuds.
    /// </summary>
    public class PertNodeBuilder
    {
        private readonly PertDiagramSettings _settings;
        private readonly ProjetService _projetService; // Remplacé MetierService par ProjetService

        public PertNodeBuilder(PertDiagramSettings settings, ProjetService projetService) // Changement ici
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService)); // Changement ici
        }

        public Node BuildNodeFromTache(Tache tache, Graph graph)
        {
            var node = graph.AddNode(tache.TacheId);

            node.LabelText = GetNodeLabel(tache);
            ApplyNodeStyle(node, tache);

            node.UserData = tache;
            return node;
        }

        private string GetNodeLabel(Tache tache)
        {
            if (tache.EstJalon)
            {
                return tache.HeuresHommeEstimees > 0
                    ? $"{_settings.JalonAvecDureeIcon}{tache.HeuresHommeEstimees}"
                    : _settings.JalonSansDureeIcon;
            }

            var metier = _projetService.GetMetierById(tache.MetierId); // Récupérer le métier
            string metierAffiche = metier != null ? metier.Nom : _settings.UnassignedMetierLabel; // Nom du métier
            string pictogrammeAffiche = metier != null && !string.IsNullOrEmpty(metier.Pictogram) ? $"({metier.Pictogram})" : ""; // Pictogramme

            var dependancesCount = !string.IsNullOrEmpty(tache.Dependencies) ? tache.Dependencies.Split(',').Length : 0;

            if (dependancesCount > 0)
            {
                return string.Format(_settings.TacheLabelFormatAvecDeps,
                    tache.TacheId,
                    TronquerTexte(tache.TacheNom, _settings.TacheNomMaxLength),
                    TronquerTexte($"{metierAffiche} {pictogrammeAffiche}".Trim(), _settings.MetierNomMaxLength), // Inclure le pictogramme
                    tache.HeuresHommeEstimees,
                    dependancesCount);
            }

            return string.Format(_settings.TacheLabelFormat,
                tache.TacheId,
                TronquerTexte(tache.TacheNom, _settings.TacheNomMaxLength),
                TronquerTexte($"{metierAffiche} {pictogrammeAffiche}".Trim(), _settings.MetierNomMaxLength), // Inclure le pictogramme
                tache.HeuresHommeEstimees);
        }

        public void ApplyNodeStyle(Node node, Tache tache)
        {
            node.Attr.FillColor = GetFillColor(tache);
            node.Label.FontName = "Segoe UI Emoji";

            if (tache.EstJalon)
            {
                node.Attr.Shape = _settings.JalonShape;
                node.Attr.LineWidth = _settings.JalonLineWidth;
                node.Attr.Padding = _settings.JalonPadding;
                node.Label.FontSize = _settings.JalonFontSize;
                node.Attr.LabelMargin = (int)_settings.JalonLabelMargin;
                node.Attr.Color = _settings.JalonBorderColor;

            }
            else
            {
                node.Attr.Shape = _settings.TacheShape;
                node.Attr.LineWidth = _settings.TacheLineWidth;
                node.Attr.Padding = _settings.TachePadding;
                node.Label.FontSize = _settings.TacheFontSize;
                node.Attr.LabelMargin = (int)_settings.TacheLabelMargin;
                node.Attr.Color = string.IsNullOrEmpty(tache.MetierId)
                    ? _settings.TacheSansMetierBorderColor
                    : _settings.TacheDefaultBorderColor;
            }
        }

        private MsaglColor GetFillColor(Tache tache)
        {
            if (tache.EstJalon && (tache.Type == TypeActivite.JalonDeSynchronisation || tache.Type == TypeActivite.JalonTechnique))
            {
                return _settings.JalonTechniqueFillColor;
            }

            // On demande la couleur au service, qui fait tout le travail !
            var systemColor = _projetService.GetDisplayColorForMetier(tache.MetierId); // Changement ici

            // On convertit simplement le résultat en couleur MSAGL
            return new MsaglColor(systemColor.R, systemColor.G, systemColor.B);
        }

        private string TronquerTexte(string texte, int longueurMax)
        {
            if (string.IsNullOrEmpty(texte) || texte.Length <= longueurMax)
                return texte;
            return texte.Substring(0, longueurMax - 3) + "...";
        }
    }
}