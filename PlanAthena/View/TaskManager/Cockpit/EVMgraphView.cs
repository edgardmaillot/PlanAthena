// Fichier: /View/TaskManager/Cockpit/EVMgraphView.cs

using PlanAthena.Services.DTOs.UseCases;
using PlanAthena.Services.Usecases;
using ScottPlot;
using System.Drawing; // Assurez-vous d'avoir ce using pour Colors

namespace PlanAthena.View.TaskManager.Cockpit
{
    public partial class EVMgraphView : UserControl
    {
        private PilotageProjetUseCase _useCase;

        public EVMgraphView()
        {
            InitializeComponent();
            //formsPlotEVMCurves.Plot.Title("Analyse par la Valeur Acquise (EVM)");
            formsPlotEVMCurves.Plot.XLabel("Temps");
            formsPlotEVMCurves.Plot.YLabel("Coût (€)");
        }

        public void Initialize(PilotageProjetUseCase useCase)
        {
            _useCase = useCase;
        }
        public void RefreshData()
        {
            // 1. Rafraîchir le premier graphique
            RefreshEvmData();

            // 2. Récupérer les limites de l'axe X qui ont été calculées
            var evmXAxisLimits = formsPlotEVMCurves.Plot.Axes.GetLimits().XRange;

            // 3. Passer ces limites au second graphique pour qu'il s'aligne
            RefreshEtcVsPtcData(evmXAxisLimits);
        }
        public void RefreshEvmData()
        {
            if (_useCase == null) return;

            var graphData = _useCase.ObtenirDonneesGraphiqueEVM();

            formsPlotEVMCurves.Plot.Clear();

            if (!graphData.BaselineExists || !graphData.Dates.Any())
            {
                formsPlotEVMCurves.Plot.Title("Aucune baseline n'est définie pour ce projet.");
                formsPlotEVMCurves.Refresh();
                return;
            }

            // Conversion des données pour ScottPlot
            double[] dates = graphData.Dates.Select(d => d.ToOADate()).ToArray();
            double[] pv = graphData.PlannedValues.ToArray();
            double[] ev = graphData.EarnedValues.ToArray();
            double[] ac = new double[graphData.ActualCosts.Count];
            // Le coût réel ne doit s'afficher que jusqu'à aujourd'hui
            for (int i = 0; i < graphData.Dates.Count; i++)
            {
                if (graphData.Dates[i].Date <= DateTime.Today.Date)
                    ac[i] = graphData.ActualCosts[i];
                else
                    ac[i] = double.NaN; // ScottPlot n'affichera pas ce point
            }


            // Ajout des courbes
            var pvPlot = formsPlotEVMCurves.Plot.Add.Scatter(dates, pv);
            pvPlot.Label = "PV (Valeur Planifiée)";
            pvPlot.Color = Colors.Blue;
            pvPlot.MarkerSize = 0;
            pvPlot.LineWidth = 2; // Rendre les lignes plus visibles

            var evPlot = formsPlotEVMCurves.Plot.Add.Scatter(dates, ev);
            evPlot.Label = "EV (Valeur Acquise)";
            evPlot.Color = Colors.Green;
            evPlot.MarkerSize = 0;
            evPlot.LineWidth = 2;

            var acPlot = formsPlotEVMCurves.Plot.Add.Scatter(dates, ac);
            acPlot.Label = "AC (Coût Réel)";
            acPlot.Color = Colors.Red;
            acPlot.MarkerSize = 0;
            acPlot.LineWidth = 2;


            // Ligne horizontale pour le Budget
            /*
            var bacLine = formsPlotEVMCurves.Plot.Add.HorizontalLine((double)graphData.BudgetAtCompletion);
            bacLine.Label.Text = $"BAC: {(double)graphData.BudgetAtCompletion:C0}";
            bacLine.Color = Colors.Gray;
            bacLine.LinePattern = LinePattern.Dashed;
            bacLine.LineWidth = 2;
            */

            // Ligne verticale pour aujourd'hui
            var todayLine = formsPlotEVMCurves.Plot.Add.VerticalLine(DateTime.Today.ToOADate());
            todayLine.Label.Text = "Aujourd'hui";
            todayLine.Color = Colors.Orange;
            todayLine.LinePattern = LinePattern.Dotted;
            todayLine.LineWidth = 2;

            // Configuration des axes et de la légende
            //formsPlotEVMCurves.Plot.Title("Analyse par la Valeur Acquise (EVM)");

            // --- CORRECTIONS API SCOTTPLOT 5 ---
            formsPlotEVMCurves.Plot.Axes.DateTimeTicksBottom(); 
            formsPlotEVMCurves.Plot.Axes.AutoScale(); 


            formsPlotEVMCurves.Plot.Legend.IsVisible = false;
            formsPlotEVMCurves.Plot.Legend.Alignment = Alignment.UpperLeft;

            formsPlotEVMCurves.Refresh();
        }

        public void RefreshEtcVsPtcData(ScottPlot.CoordinateRange xAxisLimits)
        {
            if (_useCase == null) return;

            var graphData = _useCase.ObtenirDonneesGraphiqueEtcVsPtc();
            var plot = formsPlotEtcVsPtc.Plot;

            plot.Clear();
            plot.Legend.IsVisible = false;

            if (!graphData.BaselineExists || !graphData.Dates.Any())
            {
                plot.Title("Données insuffisantes pour le graphique ETC vs PTC");
                formsPlotEtcVsPtc.Refresh();
                return;
            }

            // --- LOGIQUE MODIFIÉE : ON UTILISE LES DATES ---

            // Les positions sont maintenant les dates réelles, comme pour le graphique EVM
            double[] positions = graphData.Dates.Select(d => d.ToOADate()).ToArray();
            double[] ptc = graphData.PlanToCompleteValues.ToArray();
            double[] etc = graphData.EstimateToCompleteValues.ToArray();

            // La logique de superposition reste la même, mais elle est maintenant plus fiable
            var etcProjectedPlot = plot.Add.Scatter(positions, etc);
            etcProjectedPlot.Color = Colors.Blue;
            etcProjectedPlot.MarkerShape = MarkerShape.FilledCircle;
            etcProjectedPlot.MarkerSize = 8;
            etcProjectedPlot.LineWidth = 2.5f;
            etcProjectedPlot.LinePattern = LinePattern.Dotted;

            int futureIndex = graphData.Dates.FindIndex(d => d.Date > DateTime.Today.Date);
            int pointsToDrawSolid = (futureIndex == -1) ? graphData.Dates.Count : (futureIndex == 0 ? 1 : futureIndex + 1);
            if (pointsToDrawSolid > graphData.Dates.Count) pointsToDrawSolid = graphData.Dates.Count;

            if (pointsToDrawSolid > 0)
            {
                var etcHistoricalPlot = plot.Add.Scatter(positions.Take(pointsToDrawSolid).ToArray(), etc.Take(pointsToDrawSolid).ToArray());
                etcHistoricalPlot.Color = Colors.Blue;
                etcHistoricalPlot.MarkerShape = MarkerShape.FilledCircle;
                etcHistoricalPlot.MarkerSize = 8;
                etcHistoricalPlot.LineWidth = 2.5f;
            }

            var ptcPlot = plot.Add.Scatter(positions, ptc);
            ptcPlot.Color = Colors.Orange;
            ptcPlot.MarkerShape = MarkerShape.FilledSquare;
            ptcPlot.MarkerSize = 8;
            ptcPlot.LineWidth = 2;

            // --- CONFIGURATION DES AXES MODIFIÉE ---

            // On dit à l'axe X d'interpréter les valeurs comme des dates
            plot.Axes.DateTimeTicksBottom();

            // Configuration des titres
            plot.Title("Reste à Faire : Estimé vs. Planifié");
            plot.YLabel("Coût restant (€)");
            plot.XLabel(""); // On peut cacher le label de l'axe X car il est aligné avec celui du dessus

            // --- LA SYNCHRONISATION ---
            // On force l'axe X à utiliser les mêmes limites que le graphique EVM

            plot.Axes.SetLimitsX(xAxisLimits.Min, xAxisLimits.Max);
            // On laisse l'axe Y s'ajuster automatiquement à ses propres données
            plot.Axes.AutoScaleY();

            formsPlotEtcVsPtc.Refresh();
        }
    }
}