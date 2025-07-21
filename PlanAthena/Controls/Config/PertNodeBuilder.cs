// Fichier : PlanAthena.Controls.Config/PertNodeBuilder.cs

using Microsoft.Msagl.Drawing;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using System;
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
        private readonly MetierService _metierService;

        public PertNodeBuilder(PertDiagramSettings settings, MetierService metierService)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
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

            var metierAffiche = !string.IsNullOrEmpty(tache.MetierId) ? tache.MetierId : _settings.UnassignedMetierLabel;
            var dependancesCount = !string.IsNullOrEmpty(tache.Dependencies) ? tache.Dependencies.Split(',').Length : 0;

            if (dependancesCount > 0)
            {
                return string.Format(_settings.TacheLabelFormatAvecDeps,
                    tache.TacheId,
                    TronquerTexte(tache.TacheNom, _settings.TacheNomMaxLength),
                    TronquerTexte(metierAffiche, _settings.MetierNomMaxLength),
                    tache.HeuresHommeEstimees,
                    dependancesCount);
            }

            return string.Format(_settings.TacheLabelFormat,
                tache.TacheId,
                TronquerTexte(tache.TacheNom, _settings.TacheNomMaxLength),
                TronquerTexte(metierAffiche, _settings.MetierNomMaxLength),
                tache.HeuresHommeEstimees);
        }

        public void ApplyNodeStyle(Node node, Tache tache)
        {
            node.Attr.FillColor = GetFillColor(tache);

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
            var systemColor = _metierService.GetDisplayColorForMetier(tache.MetierId);

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