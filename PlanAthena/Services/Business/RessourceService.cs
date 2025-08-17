// Fichier: PlanAthena/Services/Business/RessourceService.cs
// Version: 0.4.4 (Refactorisation Finale)
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business.DTOs;
using QuikGraph;
using QuikGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PlanAthena.Services.Business
{
    public class RessourceService
    {
        private readonly Dictionary<string, Metier> _metiers = new();
        private readonly Dictionary<string, Ouvrier> _ouvriers = new();
        private readonly IIdGeneratorService _idGenerator;


        public RessourceService(IIdGeneratorService idGenerator)
        {
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        #region Cycle de vie des Ressources

        public void ChargerRessources(List<Metier> metiers, List<Ouvrier> ouvriers)
        {
            ViderMetiers();
            ViderOuvriers();
            metiers?.ForEach(m => _metiers.TryAdd(m.MetierId, m));
            ouvriers?.ForEach(o => _ouvriers.TryAdd(o.OuvrierId, o));
        }

        public void ChargerMetiersParDefaut()
        {
            // Cette méthode commence par vider pour assurer un état prédictible
            ViderMetiers();
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "DefaultMetiersConfig.json");
                if (!File.Exists(filePath)) return;
                string json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var defaultMetiers = JsonSerializer.Deserialize<List<Metier>>(json, options);
                if (defaultMetiers != null)
                {
                    foreach (var metier in defaultMetiers)
                    {
                        _metiers.TryAdd(metier.MetierId, metier);
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Erreur chargement métiers par défaut: {ex.Message}"); }
        }

        public void ViderMetiers() => _metiers.Clear();
        public void ViderOuvriers() => _ouvriers.Clear();

        #endregion

        #region Gestion des Métiers

        public Metier CreerMetier(string nom = "Nouveau Métier", ChantierPhase phases = ChantierPhase.SecondOeuvre)
        {
            if (string.IsNullOrWhiteSpace(nom)) throw new ArgumentException("Le nom du métier ne peut pas être vide.", nameof(nom));
            var nouveauMetier = new Metier
            {
                MetierId = _idGenerator.GenererProchainMetierId(_metiers.Values.ToList()),
                Nom = nom,
                Phases = phases
            };
            _metiers.Add(nouveauMetier.MetierId, nouveauMetier);
            return nouveauMetier;
        }

        public void ModifierMetier(Metier metierModifie)
        {
            if (metierModifie == null) throw new ArgumentNullException(nameof(metierModifie));
            if (!_metiers.ContainsKey(metierModifie.MetierId)) throw new InvalidOperationException($"Le métier ID '{metierModifie.MetierId}' n'existe pas.");
            ValiderCircularite(metierModifie);
            _metiers[metierModifie.MetierId] = metierModifie;
        }

        public void SupprimerMetier(string metierId, ProjetService projetService)
        {
            if (projetService == null)
                throw new ArgumentNullException(nameof(projetService));
            if (!_metiers.ContainsKey(metierId)) throw new InvalidOperationException($"Le métier ID '{metierId}' n'a pas été trouvé.");
            if (projetService.ObtenirTachesParMetier(metierId).Any()) throw new InvalidOperationException("Impossible de supprimer le métier car il est utilisé par des tâches.");
            _metiers.Remove(metierId);
            foreach (var metier in _metiers.Values)
            {
                foreach (var phase in metier.PrerequisParPhase.Keys.ToList())
                {
                    metier.PrerequisParPhase[phase].RemoveAll(id => id == metierId);
                }
            }
        }

        private void ValiderCircularite(Metier metier)
        {
            var graph = new AdjacencyGraph<string, Edge<string>>();
            var tousLesMetiers = _metiers.Values.ToList();
            var metierExistant = tousLesMetiers.FirstOrDefault(m => m.MetierId == metier.MetierId);
            if (metierExistant != null) tousLesMetiers.Remove(metierExistant);
            tousLesMetiers.Add(metier);
            graph.AddVertexRange(tousLesMetiers.Select(m => m.MetierId));
            foreach (var m in tousLesMetiers)
            {
                var prerequis = m.PrerequisParPhase?.Values.SelectMany(p => p).Distinct() ?? Enumerable.Empty<string>();
                foreach (var prereqId in prerequis)
                {
                    if (graph.ContainsVertex(prereqId)) graph.AddEdge(new Edge<string>(prereqId, m.MetierId));
                }
            }
            try { graph.TopologicalSort(); }
            catch (NonAcyclicGraphException) { throw new InvalidOperationException($"Dépendance circulaire détectée pour le métier '{metier.Nom}'."); }
        }
        /// <summary>
        /// Calcule et retourne la liste des métiers qu'un ouvrier ne possède pas encore.
        /// Cette méthode est utilisée par l'IHM pour peupler le dialogue d'ajout de compétence,
        /// en s'assurant de ne pas proposer des compétences que l'ouvrier a déjà.
        /// </summary>
        /// <param name="ouvrierId">L'ID de l'ouvrier concerné.</param>
        /// <returns>Une liste d'objets Metier que l'ouvrier peut apprendre.</returns>
        public List<Metier> GetMetiersDisponiblesPourOuvrier(string ouvrierId)
        {
            // Étape 1 : Valider l'entrée et récupérer l'ouvrier
            if (!_ouvriers.TryGetValue(ouvrierId, out var ouvrier))
            {
                // Si l'ouvrier n'est pas trouvé, retourner une liste vide pour éviter un crash.
                return new List<Metier>();
            }

            // Étape 2 : Obtenir la liste des IDs des métiers que l'ouvrier possède déjà.
            // On utilise un HashSet pour une recherche ultra-rapide (complexité O(1)).
            var competencesActuellesIds = ouvrier.Competences.Select(c => c.MetierId).ToHashSet();

            // Étape 3 : Filtrer la liste complète de tous les métiers.
            // On ne garde que ceux dont l'ID n'est PAS dans le HashSet des compétences actuelles.
            var metiersDisponibles = _metiers.Values
                .Where(metier => !competencesActuellesIds.Contains(metier.MetierId))
                .OrderBy(m => m.Nom) // On trie pour un affichage alphabétique dans l'IHM.
                .ToList();

            return metiersDisponibles;
        }
        public List<Metier> GetAllMetiers() => _metiers.Values.OrderBy(m => m.Nom).ToList();
        public Metier GetMetierById(string id) => _metiers.GetValueOrDefault(id);
        public Color GetDisplayColorForMetier(string metierId)
        {
            if (string.IsNullOrEmpty(metierId)) return SystemColors.ControlLight;
            var metier = GetMetierById(metierId);
            if (metier != null && !string.IsNullOrWhiteSpace(metier.CouleurHex))
            {
                try { return ColorTranslator.FromHtml(metier.CouleurHex); } catch { /* Fallback */ }
            }
            int hash = metierId.GetHashCode();
            return Color.FromArgb(200, (hash & 0xFF), ((hash >> 8) & 0xFF), ((hash >> 16) & 0xFF));
        }
        public List<string> GetPrerequisPourPhase(string metierId, ChantierPhase phase)
        {
            var metier = GetMetierById(metierId);
            if (metier?.PrerequisParPhase != null && metier.PrerequisParPhase.TryGetValue(phase, out var prerequis))
            {
                return prerequis.ToList();
            }
            return new List<string>();
        }
        public List<string> GetTousPrerequisConfondus(string metierId)
        {
            var metier = GetMetierById(metierId);
            if (metier?.PrerequisParPhase == null) return new List<string>();
            return metier.PrerequisParPhase.Values.SelectMany(prereqs => prereqs).Distinct().ToList();
        }

        #endregion

        #region Gestion des Ouvriers

        public Ouvrier CreerOuvrierParDefaut()
        {
            if (!_metiers.Any()) throw new InvalidOperationException("Impossible de créer un ouvrier : aucun métier n'est défini.");
            var nouvelOuvrier = CreerOuvrier("Nouveau", "Ouvrier", 200);
            var premierMetier = _metiers.Values.First();
            nouvelOuvrier.Competences.Add(new CompetenceOuvrier { MetierId = premierMetier.MetierId, EstMetierPrincipal = true });
            return nouvelOuvrier;
        }

        public Ouvrier CreerOuvrier(string prenom = "Nouveau", string nom = "Ouvrier", int cout = 200)
        {
            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(prenom)) throw new ArgumentException("Le nom et le prénom ne peuvent pas être vides.");
            var nouvelOuvrier = new Ouvrier
            {
                OuvrierId = _idGenerator.GenererProchainOuvrierId(_ouvriers.Values.ToList()),
                Nom = nom,
                Prenom = prenom,
                CoutJournalier = cout
            };
            _ouvriers.Add(nouvelOuvrier.OuvrierId, nouvelOuvrier);
            return nouvelOuvrier;
        }

        public void ModifierOuvrier(Ouvrier ouvrierModifie)
        {
            if (ouvrierModifie == null) throw new ArgumentNullException(nameof(ouvrierModifie));
            if (!_ouvriers.ContainsKey(ouvrierModifie.OuvrierId)) throw new InvalidOperationException($"L'ouvrier ID '{ouvrierModifie.OuvrierId}' n'existe pas.");
            _ouvriers[ouvrierModifie.OuvrierId] = ouvrierModifie;
        }

        public void SupprimerOuvrier(string ouvrierId)
        {
            if (!_ouvriers.Remove(ouvrierId)) throw new InvalidOperationException($"L'ouvrier ID '{ouvrierId}' n'a pas été trouvé.");
        }

        public void AjouterCompetence(string ouvrierId, string metierId)
        {
            if (!_ouvriers.TryGetValue(ouvrierId, out var ouvrier)) throw new InvalidOperationException($"Ouvrier '{ouvrierId}' non trouvé.");
            if (GetMetierById(metierId) == null) throw new InvalidOperationException($"Métier '{metierId}' non trouvé.");
            if (ouvrier.Competences.Any(c => c.MetierId == metierId)) throw new InvalidOperationException("L'ouvrier possède déjà cette compétence.");
            var nouvelleCompetence = new CompetenceOuvrier { MetierId = metierId, EstMetierPrincipal = !ouvrier.Competences.Any() };
            ouvrier.Competences.Add(nouvelleCompetence);
        }

        public void SupprimerCompetence(string ouvrierId, string metierId)
        {
            if (!_ouvriers.TryGetValue(ouvrierId, out var ouvrier)) throw new InvalidOperationException($"Ouvrier '{ouvrierId}' non trouvé.");
            var competence = ouvrier.Competences.FirstOrDefault(c => c.MetierId == metierId);
            if (competence == null) throw new InvalidOperationException($"Compétence '{metierId}' non trouvée.");
            if (ouvrier.Competences.Count <= 1) throw new InvalidOperationException("Impossible de supprimer la dernière compétence.");
            ouvrier.Competences.Remove(competence);
            if (competence.EstMetierPrincipal && ouvrier.Competences.Any())
            {
                ouvrier.Competences.First().EstMetierPrincipal = true;
            }
        }

        public void DefinirMetierPrincipal(string ouvrierId, string metierId)
        {
            if (!_ouvriers.TryGetValue(ouvrierId, out var ouvrier)) throw new InvalidOperationException($"Ouvrier '{ouvrierId}' non trouvé.");
            var competence = ouvrier.Competences.FirstOrDefault(c => c.MetierId == metierId);
            if (competence == null) throw new InvalidOperationException($"Compétence '{metierId}' non trouvée.");
            ouvrier.Competences.ForEach(c => c.EstMetierPrincipal = false);
            competence.EstMetierPrincipal = true;
        }

        public List<Ouvrier> GetAllOuvriers() => _ouvriers.Values.OrderBy(o => o.Nom).ThenBy(o => o.Prenom).ToList();
        public Ouvrier GetOuvrierById(string id) => _ouvriers.GetValueOrDefault(id);

        #endregion
    }
}