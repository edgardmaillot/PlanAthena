// Fichier : PlanAthena.Controls.Config/PertNodeBuilder.cs
using Microsoft.Msagl.Drawing;
using PlanAthena.Data;
using PlanAthena.Services.Business;
using System;
using MsaglColor = Microsoft.Msagl.Drawing.Color;

namespace PlanAthena.Controls.Config
{
    public class PertNodeBuilder
    {
        private readonly PertDiagramSettings _settings;
        private readonly RessourceService _ressourceService;

        public PertNodeBuilder(PertDiagramSettings settings, RessourceService ressourceService)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
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
                return tache.HeuresHommeEstimees > 0 ? $"{_settings.JalonAvecDureeIcon}{tache.HeuresHommeEstimees}" : _settings.JalonSansDureeIcon;
            }

            var metier = _ressourceService.GetMetierById(tache.MetierId);
            string metierAffiche = metier != null ? metier.Nom : _settings.UnassignedMetierLabel;
            string pictogrammeAffiche = metier != null && !string.IsNullOrEmpty(metier.Pictogram) ? $"({metier.Pictogram})" : "";
            var dependancesCount = !string.IsNullOrEmpty(tache.Dependencies) ? tache.Dependencies.Split(',').Length : 0;
            string format = dependancesCount > 0 ? _settings.TacheLabelFormatAvecDeps : _settings.TacheLabelFormat;

            return string.Format(format,
                tache.TacheId,
                TronquerTexte(tache.TacheNom, _settings.TacheNomMaxLength),
                TronquerTexte($"{metierAffiche} {pictogrammeAffiche}".Trim(), _settings.MetierNomMaxLength),
                tache.HeuresHommeEstimees,
                dependancesCount);
        }

        public void ApplyNodeStyle(Node node, Tache tache)
        {
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
                node.Attr.Color = string.IsNullOrEmpty(tache.MetierId) ? _settings.TacheSansMetierBorderColor : _settings.TacheDefaultBorderColor;
            }
            node.Attr.FillColor = GetFillColor(tache);
        }

        private MsaglColor GetFillColor(Tache tache)
        {
            if (tache.EstJalon && (tache.Type == TypeActivite.JalonDeSynchronisation || tache.Type == TypeActivite.JalonTechnique))
            {
                return _settings.JalonTechniqueFillColor;
            }
            var systemColor = _ressourceService.GetDisplayColorForMetier(tache.MetierId);
            return new MsaglColor(systemColor.R, systemColor.G, systemColor.B);
        }

        private string TronquerTexte(string texte, int longueurMax)
        {
            if (string.IsNullOrEmpty(texte) || texte.Length <= longueurMax) return texte;
            return texte.Substring(0, longueurMax - 3) + "...";
        }
    }
}