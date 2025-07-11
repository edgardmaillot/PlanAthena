using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanAthena.Utilities
{
    
    public static class TestDataFactory
    {
        // Gardé pour référence, mais le scénario Immeuble utilisera sa propre config
        public static List<TradeDefinition>? _defaultTradeConfig = null;

        public static List<TradeDefinition> GetDefaultTradeConfiguration()
        {
            // ... (code original de GetDefaultTradeConfiguration inchangé si vous en avez encore besoin pour d'autres tests)
            // Pour le scénario Immeuble, nous utiliserons GetTradeDefinitions_ImmeubleCentreVille()
            if (_defaultTradeConfig == null)
            {
                _defaultTradeConfig = new List<TradeDefinition> {
                    new TradeDefinition { TradeCode = "PREPT_01", DisplayOrder = 0, DependsOnTradeCodes = new List<string>() },
                    new TradeDefinition { TradeCode = "MACON_01", DisplayOrder = 1, DependsOnTradeCodes = new List<string> { "PREPT_01" } },
                    new TradeDefinition { TradeCode = "PLOMB_01", DisplayOrder = 2, DependsOnTradeCodes = new List<string> { "MACON_01" } },
                    new TradeDefinition { TradeCode = "ELECT_01", DisplayOrder = 3, DependsOnTradeCodes = new List<string> { "MACON_01" } },
                    new TradeDefinition { TradeCode = "ELECT_02", DisplayOrder = 4, DependsOnTradeCodes = new List<string> { "MACON_01" } },
                    new TradeDefinition { TradeCode = "PLATR_01", DisplayOrder = 5, DependsOnTradeCodes = new List<string> { "PLOMB_01", "ELECT_01", "ELECT_02" } },
                    new TradeDefinition { TradeCode = "CARRE_01", DisplayOrder = 6, DependsOnTradeCodes = new List<string> { "PLATR_01" } },
                    new TradeDefinition { TradeCode = "PEINT_01", DisplayOrder = 7, DependsOnTradeCodes = new List<string> { "PLATR_01" } },
                    new TradeDefinition { TradeCode = "FINIT_01", DisplayOrder = 8, DependsOnTradeCodes = new List<string> { "PLOMB_01", "ELECT_01", "ELECT_02", "CARRE_01", "PLATR_01", "PEINT_01" } }
                };
            }
            return new List<TradeDefinition>(_defaultTradeConfig);
        }


        private static List<TradeDefinition> GetTradeDefinitions_ImmeubleCentreVille()
        {
            return new List<TradeDefinition> {
                // Second oeuvre
                new TradeDefinition { TradeCode = "PREPT_01", DisplayOrder = 0, DependsOnTradeCodes = new List<string>() },
                new TradeDefinition { TradeCode = "MACON_01", DisplayOrder = 1, DependsOnTradeCodes = new List<string> { "PREPT_01" } },
                new TradeDefinition { TradeCode = "BOISPoFe", DisplayOrder = 2, DependsOnTradeCodes = new List<string> { "MACON_01" } },
                new TradeDefinition { TradeCode = "PLOMB_01", DisplayOrder = 3, DependsOnTradeCodes = new List<string> { "MACON_01" } }, // Réseaux primaires
                new TradeDefinition { TradeCode = "ELECT_01", DisplayOrder = 4, DependsOnTradeCodes = new List<string> { "MACON_01" } }, // Réseaux primaires CFort
                new TradeDefinition { TradeCode = "ELECT_02", DisplayOrder = 5, DependsOnTradeCodes = new List<string> { "MACON_01" } }, // Réseaux primaires CFaib
                new TradeDefinition { TradeCode = "PLAQUIST", DisplayOrder = 6, DependsOnTradeCodes = new List<string> { "BOISPoFe", "PLOMB_01", "ELECT_01", "ELECT_02" } },
                new TradeDefinition { TradeCode = "FINIT_01", DisplayOrder = 7, DependsOnTradeCodes = new List<string> { "PLAQUIST" } }, // Finitions générales second oeuvre (ex: rebouchage, avant enduits fins)
                
                // Finitions
                new TradeDefinition { TradeCode = "PREPT_02", DisplayOrder = 10, DependsOnTradeCodes = new List<string> { "FINIT_01" } }, // Préparation pour carrelage / plâtrerie de finition
                new TradeDefinition { TradeCode = "CARRELAG", DisplayOrder = 11, DependsOnTradeCodes = new List<string> { "PREPT_02" } },
                new TradeDefinition { TradeCode = "PLATRERI", DisplayOrder = 12, DependsOnTradeCodes = new List<string> { "PREPT_02" } }, // Enduits fins, plâtrerie de finition
                new TradeDefinition { TradeCode = "PEINTURE", DisplayOrder = 13, DependsOnTradeCodes = new List<string> { "PLATRERI" } },
                new TradeDefinition { TradeCode = "PLOMB_F1", DisplayOrder = 14, DependsOnTradeCodes = new List<string> { "PEINTURE", "CARRELAG" } }, // Equipements plomberie (après peinture et carrelage au sol/mur)
                new TradeDefinition { TradeCode = "ELECT_F1", DisplayOrder = 15, DependsOnTradeCodes = new List<string> { "PEINTURE" } }, // Equipements elec (appareillage)
                new TradeDefinition { TradeCode = "PARQUETS", DisplayOrder = 16, DependsOnTradeCodes = new List<string> { "PEINTURE" } }, // Parquet après peinture
                new TradeDefinition { TradeCode = "NETTOYAG", DisplayOrder = 20, DependsOnTradeCodes = new List<string> { "PLOMB_F1", "ELECT_F1", "PARQUETS" } } // Nettoyage final après toutes poses
            };
        }

        private static List<WorkerInput> GetWorkers_ImmeubleCentreVille()
        {
            var workers = new List<WorkerInput>();
            int idCounter = 75000;

            // Spécialistes Plaquistes (Goulot)
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Paul", LastName = "PlaquisteUn", SkillTradeCodes = new List<string> { "PLAQUIST", "PREPT_01", "FINIT_01", "NETTOYAG" } });
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Pierre", LastName = "PlaquisteDeux", SkillTradeCodes = new List<string> { "PLAQUIST", "PREPT_01", "FINIT_01", "NETTOYAG" } });

            // Électriciens
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Eliane", LastName = "Electro", SkillTradeCodes = new List<string> { "ELECT_01", "ELECT_02", "ELECT_F1", "PREPT_01", "FINIT_01", "NETTOYAG" } });
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Eric", LastName = "Lumen", SkillTradeCodes = new List<string> { "ELECT_01", "ELECT_02", "ELECT_F1", "FINIT_01", "NETTOYAG" } });

            // Plombiers
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Patrick", LastName = "PlombierUn", SkillTradeCodes = new List<string> { "PLOMB_01", "PLOMB_F1", "PREPT_01", "FINIT_01", "NETTOYAG" } });
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Patricia", LastName = "Aqueduc", SkillTradeCodes = new List<string> { "PLOMB_01", "PLOMB_F1", "FINIT_01", "NETTOYAG" } });

            // Peintres
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Penelope", LastName = "Pinceau", SkillTradeCodes = new List<string> { "PEINTURE", "PLATRERI", "PREPT_02", "FINIT_01", "NETTOYAG" } });
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Pascal", LastName = "Couleur", SkillTradeCodes = new List<string> { "PEINTURE", "PLATRERI", "FINIT_01", "NETTOYAG" } });

            // Menuisiers / Poseurs
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Marc", LastName = "Menuisier", SkillTradeCodes = new List<string> { "BOISPoFe", "PARQUETS", "FINIT_01", "NETTOYAG" } }); // Polyvalent sur parquet aussi
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Michel", LastName = "Cadre", SkillTradeCodes = new List<string> { "BOISPoFe", "FINIT_01", "NETTOYAG" } });

            // Carreleurs
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Carine", LastName = "Carreau", SkillTradeCodes = new List<string> { "CARRELAG", "PREPT_02", "FINIT_01" } });

            // Maçons / Préparateurs / Finisseurs (plus polyvalents)
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Mohamed", LastName = "Macon", SkillTradeCodes = new List<string> { "MACON_01", "PREPT_01", "FINIT_01", "NETTOYAG" } });
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Manon", LastName = "PolyvalenteA", SkillTradeCodes = new List<string> { "PREPT_01", "PREPT_02", "FINIT_01", "NETTOYAG" } });
            workers.Add(new WorkerInput { Id = (idCounter++).ToString(), FirstName = "Martin", LastName = "PolyvalentB", SkillTradeCodes = new List<string> { "PREPT_01", "PREPT_02", "FINIT_01", "NETTOYAG" } });

            return workers;
        }

        private static OperationInput CreateOperation(
            string operationDescription,
            string tradeCode,
            double hours,
            int maxWorkersInBloc, // Pour la règle d'héritage
            string dependsOnTaskDescriptions = "", // Descriptions séparées par virgule
            int? maxWorkersOnOperationOverride = null)
        {
            var op = new OperationInput
            {
                OperationId = operationDescription, // Sera utilisé pour résoudre les dépendances
                TradeCode = tradeCode,
                Hours = hours,
                DependsOnOperationIds = string.IsNullOrWhiteSpace(dependsOnTaskDescriptions)
                    ? new List<string>()
                    : dependsOnTaskDescriptions.Split(',').Select(d => d.Trim()).ToList(),
                MaxConcurrentWorkersOnOperation = maxWorkersOnOperationOverride ?? maxWorkersInBloc
            };
            return op;
        }

        public static PlanningRequestInput GetImmeubleCentreVilleRequest()
        {
            var zones = new List<ZoneLogicInput>();
            var blocs = new List<BlocInput>();

            // --- Définition des Zones ---
            string zoneGlobaleId = "Zone Globale";
            string zoneBoutiqueId = "Zone boutique";
            string zoneT1Id = "Zone T1";       // Etages 1 & 2 (8 appartements T2)
            string zoneT3Id = "Zone T3";       // Etage 3 (2 appartements T4)
            string zoneSId = "Zone S";         // Etage 4 (6 studios)

            zones.Add(new ZoneLogicInput { ZoneId = zoneGlobaleId, PriorityLevel = 0, DependsOnZoneIds = new List<string>() });
            zones.Add(new ZoneLogicInput { ZoneId = zoneBoutiqueId, PriorityLevel = 1, DependsOnZoneIds = new List<string> { zoneGlobaleId } });
            zones.Add(new ZoneLogicInput { ZoneId = zoneT1Id, PriorityLevel = 2, DependsOnZoneIds = new List<string>() }); // Peut démarrer en parallèle de Boutique selon MAX_CONCURRENT_ACTIVE_BLOCS
            zones.Add(new ZoneLogicInput { ZoneId = zoneT3Id, PriorityLevel = 2, DependsOnZoneIds = new List<string>() }); // Peut démarrer en parallèle de Boutique et T1
            zones.Add(new ZoneLogicInput { ZoneId = zoneSId, PriorityLevel = 3, DependsOnZoneIds = new List<string>() });   // Idem

            // --- ZONE BOUTIQUE ---
            // Bloc: Global (Réseaux généraux RDC)
            int mwb_Global = 6;
            var ops_Global = new List<OperationInput>
            {
                CreateOperation("Prepa chantier", "PREPT_01", 28, mwb_Global),
                CreateOperation("Cloison public/prive", "MACON_01", 8, mwb_Global),
                CreateOperation("cloisons prive", "MACON_01", 12, mwb_Global),
                CreateOperation("Pose gaines eau", "PLOMB_01", 5, mwb_Global),
                CreateOperation("Raccordement reseau", "PLOMB_01", 1, mwb_Global, "Pose gaines eau"),
                CreateOperation("Pose evacuation", "PLOMB_01", 4, mwb_Global, "Pose gaines eau"),
                CreateOperation("Pose et raccordement tableau", "ELECT_01", 6, mwb_Global),
                CreateOperation("Raccordement VMC", "ELECT_01", 3, mwb_Global), // Supposant que VMC est raccordée au tableau principal
                CreateOperation("Pose gaines Elec", "ELECT_02", 12, mwb_Global),
                CreateOperation("Tirage cables", "ELECT_02", 14, mwb_Global, "Pose gaines Elec"), // Correction: dépend de "Pose gaines Elec"
                CreateOperation("Raccordements", "ELECT_02", 4, mwb_Global, "Tirage cables")
            };
            blocs.Add(new BlocInput
            {
                BlocId = "Global", // Simplement "Global"
                AssociatedZoneId = zoneGlobaleId, // Assigné à Zone Globale
                MaxConcurrentWorkersInBloc = mwb_Global,
                Operations = ops_Global
            });

            // Bloc: Boutique (Aménagement boutique)
            int mwb_Boutique = 5;
            var ops_Boutique = new List<OperationInput>
            {
                CreateOperation("pose baie 1", "BOISPoFe", 6, mwb_Boutique, "", 3),
                CreateOperation("pose baie 2", "BOISPoFe", 6, mwb_Boutique, "", 3),
                CreateOperation("pose baie 3", "BOISPoFe", 6, mwb_Boutique, "", 3),
                CreateOperation("pose baie 4", "BOISPoFe", 6, mwb_Boutique, "", 3),
                CreateOperation("Pose joints baie 1", "BOISPoFe", 2, mwb_Boutique, "pose baie 1", 1),
                CreateOperation("Pose joints baie 2", "BOISPoFe", 2, mwb_Boutique, "pose baie 2", 1),
                CreateOperation("Pose joints baie 3", "BOISPoFe", 2, mwb_Boutique, "pose baie 3", 1),
                CreateOperation("Pose joints baie 4", "BOISPoFe", 2, mwb_Boutique, "pose baie 4", 1),
                CreateOperation("Pose porte entree", "BOISPoFe", 5, mwb_Boutique, "", 2),
                // BA13 dépend des réseaux (implicite par TradeDefinition) et menuiseries
                CreateOperation("BA13 boutique", "PLAQUIST", 50, mwb_Boutique, "Pose porte entree, Pose joints baie 1, Pose joints baie 2, Pose joints baie 3, Pose joints baie 4", 3),
                CreateOperation("Débarassage/nettoyage", "FINIT_01", 7, mwb_Boutique, "BA13 boutique"), // Nettoyage après placo
                CreateOperation("Enduis boutique", "PLATRERI", 28, mwb_Boutique, "Débarassage/nettoyage", 3),
                CreateOperation("Peinture boutique", "PEINTURE", 18, mwb_Boutique, "Enduis boutique", 3),
                CreateOperation("Prises boutique", "ELECT_F1", 2, mwb_Boutique, "Peinture boutique"),
                CreateOperation("Eclairage boutique", "ELECT_F1", 3, mwb_Boutique, "Peinture boutique"),
                CreateOperation("Parquet boutique", "PARQUETS", 10, mwb_Boutique, "Peinture boutique"),
                CreateOperation("Nettoyage final boutique", "NETTOYAG", 6, mwb_Boutique, "Prises boutique, Eclairage boutique, Parquet boutique") // Ajoutée
            };
            blocs.Add(new BlocInput { BlocId = $"{zoneBoutiqueId}_Boutique", AssociatedZoneId = zoneBoutiqueId, MaxConcurrentWorkersInBloc = mwb_Boutique, Operations = ops_Boutique });

            // Bloc: Reserve
            int mwb_Reserve = 3;
            var ops_Reserve = new List<OperationInput>
            {
                CreateOperation("Pose porte réserve", "BOISPoFe", 3, mwb_Reserve),
                CreateOperation("BA13 stock", "PLAQUIST", 30, mwb_Reserve, "Pose porte réserve"),
                CreateOperation("Sol réserve", "CARRELAG", 8, mwb_Reserve, "BA13 stock"), // Supposons carrelage pour un stock
                CreateOperation("Enduis réserve", "PLATRERI", 16, mwb_Reserve, "BA13 stock"),
                CreateOperation("Peinture réserve", "PEINTURE", 14, mwb_Reserve, "Enduis réserve"),
                CreateOperation("Prises réserve", "ELECT_F1", 2, mwb_Reserve, "Peinture réserve"),
                CreateOperation("Eclairage réserve", "ELECT_F1", 3, mwb_Reserve, "Peinture réserve"),
                CreateOperation("Nettoyage final réserve", "NETTOYAG", 4, mwb_Reserve, "Prises réserve, Eclairage réserve, Sol réserve") // Ajoutée
            };
            blocs.Add(new BlocInput { BlocId = $"{zoneBoutiqueId}_Reserve", AssociatedZoneId = zoneBoutiqueId, MaxConcurrentWorkersInBloc = mwb_Reserve, Operations = ops_Reserve });

            // Bloc: Prive (Sanitaires/Bureau RDC)
            int mwb_Prive = 2;
            var ops_Prive = new List<OperationInput>
            {
                CreateOperation("Pose porte bureau", "BOISPoFe", 3, mwb_Prive),
                CreateOperation("Pose fenetrebureau", "BOISPoFe", 5, mwb_Prive), // "fenêtrebureau" normalisé
                CreateOperation("Cablage convecteurs PAC", "ELECT_01", 3, mwb_Prive), // Renommé pour clarté
                CreateOperation("BA13 Bureau", "PLAQUIST", 12, mwb_Prive, "Pose porte bureau, Pose fenetrebureau, Cablage convecteurs PAC"),
                CreateOperation("BA13 sanitaires", "PLAQUIST", 12, mwb_Prive, "Cablage convecteurs PAC"), // Sanitaires peuvent être plaqués en parallèle du bureau une fois elec passée
                CreateOperation("Dispatch matériel", "FINIT_01", 8, mwb_Prive, "BA13 Bureau, BA13 sanitaires"), // Ou PREPT_02
                CreateOperation("murs sanitaires", "CARRELAG", 5, mwb_Prive, "BA13 sanitaires"), // Carrelage mural
                CreateOperation("Sol sanitaire", "CARRELAG", 2, mwb_Prive, "murs sanitaires"), // Carrelage sol après murs
                CreateOperation("Enduis bureau", "PLATRERI", 9, mwb_Prive, "BA13 Bureau"),
                CreateOperation("Enduis sanitaire", "PLATRERI", 9, mwb_Prive, "BA13 sanitaires"), // Peut être fait en parallèle de l'enduit bureau
                CreateOperation("peinture bureau", "PEINTURE", 6, mwb_Prive, "Enduis bureau"),
                CreateOperation("peinture sanitaire", "PEINTURE", 6, mwb_Prive, "Enduis sanitaire, murs sanitaires, Sol sanitaire"), // Peinture après enduits ET carrelage
                CreateOperation("Pose WC", "PLOMB_F1", 3, mwb_Prive, "peinture sanitaire",1),
                CreateOperation("Pose lavabo", "PLOMB_F1", 3, mwb_Prive, "peinture sanitaire",1),
                CreateOperation("Pose robinetterie", "PLOMB_F1", 2, mwb_Prive, "peinture sanitaire",1),
                CreateOperation("Raccordements Prive", "PLOMB_F1", 4, mwb_Prive, "Pose WC, Pose lavabo, Pose robinetterie",1), // Renommé pour unicité
                CreateOperation("Prises bureau", "ELECT_F1", 2, mwb_Prive, "peinture bureau"),
                CreateOperation("Eclairage bureau", "ELECT_F1", 3, mwb_Prive, "peinture bureau"),
                CreateOperation("Eclairage sanitaires", "ELECT_F1", 3, mwb_Prive, "peinture sanitaire"),
                CreateOperation("Parquet bureau", "PARQUETS", 4, mwb_Prive, "peinture bureau"),
                CreateOperation("Nettoyage final prive", "NETTOYAG", 3, mwb_Prive, "Raccordements Prive, Prises bureau, Eclairage bureau, Eclairage sanitaires, Parquet bureau") // Ajoutée
            };
            blocs.Add(new BlocInput { BlocId = $"{zoneBoutiqueId}_Prive", AssociatedZoneId = zoneBoutiqueId, MaxConcurrentWorkersInBloc = mwb_Prive, Operations = ops_Prive });

            // --- ZONE T1 (Etages 1 & 2) ---
            // Bloc: Couloir etage 1
            int mwb_CouloirE1 = 3;
            var ops_CouloirE1 = new List<OperationInput>
            {
                CreateOperation("Prepa chantier Couloir E1", "PREPT_01", 11, mwb_CouloirE1),
                CreateOperation("Cloison Couloir E1", "MACON_01", 8, mwb_CouloirE1, "Prepa chantier Couloir E1"),
                CreateOperation("Pose fenetre Couloir E1", "BOISPoFe", 5, mwb_CouloirE1, "Cloison Couloir E1"),
                CreateOperation("Cablage eau Couloir E1", "PLOMB_01", 2, mwb_CouloirE1, "Cloison Couloir E1"),
                CreateOperation("Cablage Elec Couloir E1", "ELECT_01", 2, mwb_CouloirE1, "Cloison Couloir E1"), // ELECT_01 pour primaire
                CreateOperation("Plaquist Couloir E1", "PLAQUIST", 18, mwb_CouloirE1, "Pose fenetre Couloir E1, Cablage eau Couloir E1, Cablage Elec Couloir E1"),
                CreateOperation("Fin second oeuvre Couloir E1", "FINIT_01", 12, mwb_CouloirE1, "Plaquist Couloir E1"),
                CreateOperation("Prepa finitions Couloir E1", "PREPT_02", 9, mwb_CouloirE1, "Fin second oeuvre Couloir E1"), // renommée pour unicité
                CreateOperation("Enduis couloir Couloir E1", "PLATRERI", 20, mwb_CouloirE1, "Prepa finitions Couloir E1"),
                CreateOperation("Peinture couloir Couloir E1", "PEINTURE", 16, mwb_CouloirE1, "Enduis couloir Couloir E1"),
                CreateOperation("Pose prises/inter Couloir E1", "ELECT_F1", 1, mwb_CouloirE1, "Peinture couloir Couloir E1"),
                CreateOperation("Pose éclairage Couloir E1", "ELECT_F1", 2, mwb_CouloirE1, "Peinture couloir Couloir E1"),
                CreateOperation("Pose convecteur bureau Couloir E1", "ELECT_F1", 2, mwb_CouloirE1, "Peinture couloir Couloir E1"), // Supposant que ce sont des convecteurs électriques
                CreateOperation("Pose convecteurs boutique Couloir E1", "ELECT_F1", 4, mwb_CouloirE1, "Peinture couloir Couloir E1"),
                CreateOperation("Pose parquet Couloir E1", "PARQUETS", 12, mwb_CouloirE1, "Peinture couloir Couloir E1"),
                CreateOperation("Débarassage/nettoyage Couloir E1", "NETTOYAG", 18, mwb_CouloirE1, "Pose prises/inter Couloir E1, Pose éclairage Couloir E1, Pose convecteur bureau Couloir E1, Pose convecteurs boutique Couloir E1, Pose parquet Couloir E1")
            };
            blocs.Add(new BlocInput { BlocId = $"{zoneT1Id}_Couloir_etage_1", AssociatedZoneId = zoneT1Id, MaxConcurrentWorkersInBloc = mwb_CouloirE1, Operations = ops_CouloirE1 });

            // Base pour APPT 101 (T2 type)
            int mwb_ApptT2 = 3;
            var ops_ApptT2_Base = new List<OperationInput>
            {
                CreateOperation("Prepa chantier APPT", "PREPT_01", 15, mwb_ApptT2),
                CreateOperation("Cloisonnement APPT", "MACON_01", 22, mwb_ApptT2, "Prepa chantier APPT"),
                CreateOperation("Pose porte entree APPT", "BOISPoFe", 3, mwb_ApptT2, "Cloisonnement APPT", 2),
                CreateOperation("Pose porte Cuisine APPT", "BOISPoFe", 3, mwb_ApptT2, "Cloisonnement APPT", 2),
                CreateOperation("Pose porte SdB APPT", "BOISPoFe", 3, mwb_ApptT2, "Cloisonnement APPT", 2),
                CreateOperation("Pose fenetre salon APPT", "BOISPoFe", 5, mwb_ApptT2, "Cloisonnement APPT", 2),
                CreateOperation("Pose porte Chambre APPT", "BOISPoFe", 3, mwb_ApptT2, "Cloisonnement APPT", 2),
                CreateOperation("Pose fenetre chambre APPT", "BOISPoFe", 5, mwb_ApptT2, "Cloisonnement APPT", 2),
                CreateOperation("Pose gaines eau APPT", "PLOMB_01", 5, mwb_ApptT2, "Cloisonnement APPT"),
                CreateOperation("Raccordement reseau eau APPT", "PLOMB_01", 1, mwb_ApptT2, "Pose gaines eau APPT"), // Renommé pour unicité
                CreateOperation("Pose evacuation APPT", "PLOMB_01", 4, mwb_ApptT2, "Pose gaines eau APPT"),
                CreateOperation("Pose et raccordement tableau APPT", "ELECT_01", 6, mwb_ApptT2, "Cloisonnement APPT", 1),
                CreateOperation("Raccordement VMC APPT", "ELECT_01", 3, mwb_ApptT2, "Pose et raccordement tableau APPT", 1),
                CreateOperation("Pose gaines Elec APPT", "ELECT_02", 8, mwb_ApptT2, "Cloisonnement APPT"),
                CreateOperation("Tirage cables Elec APPT", "ELECT_02", 11, mwb_ApptT2, "Pose gaines Elec APPT"), // Renommé
                CreateOperation("Raccordements Elec APPT", "ELECT_02", 3, mwb_ApptT2, "Tirage cables Elec APPT"), // Renommé
                CreateOperation("Cablage convecteurs PAC APPT", "ELECT_01", 3, mwb_ApptT2, "Pose gaines Elec APPT"), // Convecteurs sur Elec primaire
                // Placo dépend de toutes les menuiseries et réseaux
                CreateOperation("BA13 Salon APPT", "PLAQUIST", 25, mwb_ApptT2, "Pose porte entree APPT, Pose porte Cuisine APPT, Pose porte SdB APPT, Pose fenetre salon APPT, Pose porte Chambre APPT, Pose fenetre chambre APPT, Raccordement reseau eau APPT, Pose evacuation APPT, Raccordement VMC APPT, Raccordements Elec APPT, Cablage convecteurs PAC APPT"),
                CreateOperation("BA13 Cuisine APPT", "PLAQUIST", 18, mwb_ApptT2, "BA13 Salon APPT", 2), // En série pour simplifier ou affiner plus tard
                CreateOperation("BA13 SdB APPT", "PLAQUIST", 16, mwb_ApptT2, "BA13 Cuisine APPT", 2),
                CreateOperation("BA13 Chambre APPT", "PLAQUIST", 18, mwb_ApptT2, "BA13 SdB APPT"),
                CreateOperation("Finitions et nettoyage placo APPT", "FINIT_01", 10, mwb_ApptT2, "BA13 Chambre APPT"), // Renommé
                CreateOperation("Livraison matériaux finitions APPT", "PREPT_02", 16, mwb_ApptT2, "Finitions et nettoyage placo APPT"), // Renommé
                CreateOperation("Carrelage cuisine APPT", "CARRELAG", 11, mwb_ApptT2, "Livraison matériaux finitions APPT"),
                CreateOperation("Carrelage SdB APPT", "CARRELAG", 18, mwb_ApptT2, "Livraison matériaux finitions APPT"), // Peut être en parallèle de cuisine
                CreateOperation("Enduis salon APPT", "PLATRERI", 12, mwb_ApptT2, "Finitions et nettoyage placo APPT"),
                CreateOperation("Enduis cuisine APPT", "PLATRERI", 9, mwb_ApptT2, "Enduis salon APPT, Carrelage cuisine APPT"), // Après carrelage cuisine si plinthes etc.
                CreateOperation("Enduis SdB APPT", "PLATRERI", 6, mwb_ApptT2, "Enduis cuisine APPT, Carrelage SdB APPT"),
                CreateOperation("Enduis Chambre APPT", "PLATRERI", 10, mwb_ApptT2, "Enduis SdB APPT"),
                CreateOperation("peinture salon APPT", "PEINTURE", 10, mwb_ApptT2, "Enduis salon APPT"),
                CreateOperation("peinture cuisine APPT", "PEINTURE", 8, mwb_ApptT2, "Enduis cuisine APPT", 2),
                CreateOperation("peinture SdB APPT", "PEINTURE", 5, mwb_ApptT2, "Enduis SdB APPT", 2),
                CreateOperation("Peinture Chambre APPT", "PEINTURE", 8, mwb_ApptT2, "Enduis Chambre APPT"),
                CreateOperation("Pose WC APPT", "PLOMB_F1", 3, mwb_ApptT2, "peinture SdB APPT, Carrelage SdB APPT", 1),
                CreateOperation("Pose lavabo APPT", "PLOMB_F1", 3, mwb_ApptT2, "peinture SdB APPT, Carrelage SdB APPT", 1),
                CreateOperation("Pose douche APPT", "PLOMB_F1", 4, mwb_ApptT2, "peinture SdB APPT, Carrelage SdB APPT", 1),
                CreateOperation("Pose robinetterie APPT", "PLOMB_F1", 3, mwb_ApptT2, "peinture SdB APPT, Carrelage SdB APPT", 1),
                CreateOperation("Raccordements Plomberie Finale APPT", "PLOMB_F1", 4, mwb_ApptT2, "Pose WC APPT, Pose lavabo APPT, Pose douche APPT, Pose robinetterie APPT",1), // Renommé
                CreateOperation("Pose prises APPT", "ELECT_F1", 5, mwb_ApptT2, "peinture salon APPT, peinture cuisine APPT, peinture SdB APPT, Peinture Chambre APPT"),
                CreateOperation("Eclairage interupteurs APPT", "ELECT_F1", 6, mwb_ApptT2, "Pose prises APPT"),
                CreateOperation("Pose convecteur Salon APPT", "ELECT_F1", 2, mwb_ApptT2, "Eclairage interupteurs APPT", 2),
                CreateOperation("Pose convecteurs Cuisine APPT", "ELECT_F1", 2, mwb_ApptT2, "Eclairage interupteurs APPT", 2),
                CreateOperation("Pose convecteur chambre APPT", "ELECT_F1", 2, mwb_ApptT2, "Eclairage interupteurs APPT", 2),
                CreateOperation("Pose convecteurs Sdb APPT", "ELECT_F1", 2, mwb_ApptT2, "Eclairage interupteurs APPT", 2),
                CreateOperation("Pose parquet salon APPT", "PARQUETS", 7, mwb_ApptT2, "peinture salon APPT"),
                CreateOperation("Pose parquet chambre APPT", "PARQUETS", 6, mwb_ApptT2, "Peinture Chambre APPT"), // Parallèle au salon
                CreateOperation("Nettoyage final APPT", "NETTOYAG", 8, mwb_ApptT2, "Raccordements Plomberie Finale APPT, Eclairage interupteurs APPT, Pose convecteur Salon APPT, Pose convecteurs Cuisine APPT, Pose convecteur chambre APPT, Pose convecteurs Sdb APPT, Pose parquet salon APPT, Pose parquet chambre APPT")
            };

            // Création explicite des appartements T2
            string[] apptT2_Suffixes = { "APPT_101", "APPT_102", "APPT_103", "APPT_104", "APPT_201", "APPT_202", "APPT_203", "APPT_204" };
            foreach (var suffix in apptT2_Suffixes)
            {
                var currentOps = ops_ApptT2_Base.Select(op => new OperationInput
                { // Duplication
                    OperationId = op.OperationId.Replace("APPT", suffix.Replace("_", "")), // Garder APPT pour la description mais unicifier avec suffixe
                    TradeCode = op.TradeCode,
                    Hours = op.Hours,
                    DependsOnOperationIds = op.DependsOnOperationIds.Select(d => d.Replace("APPT", suffix.Replace("_", ""))).ToList(),
                    MaxConcurrentWorkersOnOperation = op.MaxConcurrentWorkersOnOperation
                }).ToList();
                blocs.Add(new BlocInput { BlocId = $"{zoneT1Id}_{suffix}", AssociatedZoneId = zoneT1Id, MaxConcurrentWorkersInBloc = mwb_ApptT2, Operations = currentOps });
            }

            // Bloc: Couloir etage 2
            int mwb_CouloirE2 = 3; // Identique à Couloir E1 pour cet exemple
            var ops_CouloirE2 = ops_CouloirE1.Select(op => new OperationInput
            { // Duplication
                OperationId = op.OperationId.Replace("E1", "E2"),
                TradeCode = op.TradeCode,
                Hours = op.Hours,
                DependsOnOperationIds = op.DependsOnOperationIds.Select(d => d.Replace("E1", "E2")).ToList(),
                MaxConcurrentWorkersOnOperation = op.MaxConcurrentWorkersOnOperation
            }).ToList();
            blocs.Add(new BlocInput { BlocId = $"{zoneT1Id}_Couloir_etage_2", AssociatedZoneId = zoneT1Id, MaxConcurrentWorkersInBloc = mwb_CouloirE2, Operations = ops_CouloirE2 });


            // --- ZONE T3 (Etage 3) ---
            // Bloc: Couloir etage 3
            int mwb_CouloirE3 = 3;
            var ops_CouloirE3 = ops_CouloirE1.Select(op => new OperationInput
            { // Duplication de Couloir E1
                OperationId = op.OperationId.Replace("E1", "E3"),
                TradeCode = op.TradeCode,
                Hours = op.Hours,
                DependsOnOperationIds = op.DependsOnOperationIds.Select(d => d.Replace("E1", "E3")).ToList(),
                MaxConcurrentWorkersOnOperation = op.MaxConcurrentWorkersOnOperation
            }).ToList();
            blocs.Add(new BlocInput { BlocId = $"{zoneT3Id}_Couloir_etage_3", AssociatedZoneId = zoneT3Id, MaxConcurrentWorkersInBloc = mwb_CouloirE3, Operations = ops_CouloirE3 });

            // Base pour APPT 301 (T4 type - 3 chambres)
            int mwb_ApptT4 = 4;
            var ops_ApptT4_Base = new List<OperationInput>
            {
                CreateOperation("Prepa chantier APPT", "PREPT_01", 20, mwb_ApptT4),
                CreateOperation("Cloisonnement APPT", "MACON_01", 32, mwb_ApptT4, "Prepa chantier APPT"),
                CreateOperation("Pose porte entree APPT", "BOISPoFe", 3, mwb_ApptT4, "Cloisonnement APPT"),
                CreateOperation("Pose porte Cuisine APPT", "BOISPoFe", 3, mwb_ApptT4, "Cloisonnement APPT"),
                CreateOperation("Pose porte SdB APPT", "BOISPoFe", 3, mwb_ApptT4, "Cloisonnement APPT"),
                CreateOperation("Pose fenetre salon APPT", "BOISPoFe", 5, mwb_ApptT4, "Cloisonnement APPT"),
                CreateOperation("Pose porte Chambre1 APPT", "BOISPoFe", 3, mwb_ApptT4, "Cloisonnement APPT"),
                CreateOperation("Pose fenetre chambre1 APPT", "BOISPoFe", 5, mwb_ApptT4, "Cloisonnement APPT"),
                CreateOperation("Pose porte Chambre2 APPT", "BOISPoFe", 3, mwb_ApptT4, "Cloisonnement APPT"),
                CreateOperation("Pose fenetre chambre2 APPT", "BOISPoFe", 5, mwb_ApptT4, "Cloisonnement APPT"),
                CreateOperation("Pose porte Chambre3 APPT", "BOISPoFe", 3, mwb_ApptT4, "Cloisonnement APPT"),
                CreateOperation("Pose fenetre chambre3 APPT", "BOISPoFe", 5, mwb_ApptT4, "Cloisonnement APPT"),
                CreateOperation("Pose gaines eau APPT", "PLOMB_01", 5, mwb_ApptT4, "Cloisonnement APPT"),
                CreateOperation("Raccordement reseau eau APPT", "PLOMB_01", 1, mwb_ApptT4, "Pose gaines eau APPT"),
                CreateOperation("Pose evacuation APPT", "PLOMB_01", 4, mwb_ApptT4, "Pose gaines eau APPT"),
                CreateOperation("Pose et raccordement tableau APPT", "ELECT_01", 7, mwb_ApptT4, "Cloisonnement APPT"),
                CreateOperation("Raccordement VMC APPT", "ELECT_01", 3, mwb_ApptT4, "Pose et raccordement tableau APPT"),
                CreateOperation("Pose gaines Elec APPT", "ELECT_02", 9, mwb_ApptT4, "Cloisonnement APPT"),
                CreateOperation("Tirage cables Elec APPT", "ELECT_02", 13, mwb_ApptT4, "Pose gaines Elec APPT"),
                CreateOperation("Raccordements Elec APPT", "ELECT_02", 3, mwb_ApptT4, "Tirage cables Elec APPT"),
                CreateOperation("Cablage convecteurs PAC APPT", "ELECT_01", 6, mwb_ApptT4, "Pose gaines Elec APPT"),
                // Placo dépend de tout
                CreateOperation("BA13 Salon APPT", "PLAQUIST", 25, mwb_ApptT4, "Pose porte entree APPT,Pose porte Cuisine APPT,Pose porte SdB APPT,Pose fenetre salon APPT,Pose porte Chambre1 APPT,Pose fenetre chambre1 APPT,Pose porte Chambre2 APPT,Pose fenetre chambre2 APPT,Pose porte Chambre3 APPT,Pose fenetre chambre3 APPT,Raccordement reseau eau APPT,Pose evacuation APPT,Raccordement VMC APPT,Raccordements Elec APPT,Cablage convecteurs PAC APPT"),
                CreateOperation("BA13 Cuisine APPT", "PLAQUIST", 18, mwb_ApptT4, "BA13 Salon APPT"),
                CreateOperation("BA13 SdB APPT", "PLAQUIST", 16, mwb_ApptT4, "BA13 Cuisine APPT"),
                CreateOperation("BA13 Chambre1 APPT", "PLAQUIST", 21, mwb_ApptT4, "BA13 SdB APPT"),
                CreateOperation("BA13 Chambre2 APPT", "PLAQUIST", 18, mwb_ApptT4, "BA13 Chambre1 APPT"),
                CreateOperation("BA13 Chambre3 APPT", "PLAQUIST", 17, mwb_ApptT4, "BA13 Chambre2 APPT"),
                CreateOperation("Finitions et nettoyage placo APPT", "FINIT_01", 10, mwb_ApptT4, "BA13 Chambre3 APPT"),
                CreateOperation("Livraison matériaux finitions APPT", "PREPT_02", 22, mwb_ApptT4, "Finitions et nettoyage placo APPT"),
                CreateOperation("Carrelage cuisine APPT", "CARRELAG", 13, mwb_ApptT4, "Livraison matériaux finitions APPT"),
                CreateOperation("Carrelage SdB APPT", "CARRELAG", 21, mwb_ApptT4, "Livraison matériaux finitions APPT"),
                CreateOperation("Enduis salon APPT", "PLATRERI", 14, mwb_ApptT4, "Finitions et nettoyage placo APPT"),
                CreateOperation("Enduis cuisine APPT", "PLATRERI", 10, mwb_ApptT4, "Enduis salon APPT, Carrelage cuisine APPT"),
                CreateOperation("Enduis SdB APPT", "PLATRERI", 8, mwb_ApptT4, "Enduis cuisine APPT, Carrelage SdB APPT"),
                CreateOperation("Enduis Chambre1 APPT", "PLATRERI", 12, mwb_ApptT4, "Enduis SdB APPT"),
                CreateOperation("Enduis Chambre2 APPT", "PLATRERI", 10, mwb_ApptT4, "Enduis Chambre1 APPT"),
                CreateOperation("Enduis Chambre3 APPT", "PLATRERI", 10, mwb_ApptT4, "Enduis Chambre2 APPT"),
                CreateOperation("peinture salon APPT", "PEINTURE", 10, mwb_ApptT4, "Enduis salon APPT"),
                CreateOperation("peinture cuisine APPT", "PEINTURE", 8, mwb_ApptT4, "Enduis cuisine APPT"),
                CreateOperation("peinture SdB APPT", "PEINTURE", 5, mwb_ApptT4, "Enduis SdB APPT"),
                CreateOperation("Peinture Chambre1 APPT", "PEINTURE", 9, mwb_ApptT4, "Enduis Chambre1 APPT"),
                CreateOperation("Peinture Chambre2 APPT", "PEINTURE", 8, mwb_ApptT4, "Enduis Chambre2 APPT"),
                CreateOperation("Peinture Chambre3 APPT", "PEINTURE", 7, mwb_ApptT4, "Enduis Chambre3 APPT"),
                CreateOperation("Pose WC APPT", "PLOMB_F1", 3, mwb_ApptT4, "peinture SdB APPT, Carrelage SdB APPT"),
                CreateOperation("Pose lavabo APPT", "PLOMB_F1", 3, mwb_ApptT4, "peinture SdB APPT, Carrelage SdB APPT"),
                CreateOperation("Pose douche APPT", "PLOMB_F1", 4, mwb_ApptT4, "peinture SdB APPT, Carrelage SdB APPT"),
                CreateOperation("Pose baignoire APPT", "PLOMB_F1", 3, mwb_ApptT4, "peinture SdB APPT, Carrelage SdB APPT"),
                CreateOperation("Pose robinetterie APPT", "PLOMB_F1", 4, mwb_ApptT4, "peinture SdB APPT, Carrelage SdB APPT"),
                CreateOperation("Raccordements Plomberie Finale APPT", "PLOMB_F1", 5, mwb_ApptT4, "Pose WC APPT, Pose lavabo APPT, Pose douche APPT, Pose baignoire APPT, Pose robinetterie APPT"),
                CreateOperation("Pose prises APPT", "ELECT_F1", 5, mwb_ApptT4, "peinture salon APPT, peinture cuisine APPT, peinture SdB APPT, Peinture Chambre1 APPT, Peinture Chambre2 APPT, Peinture Chambre3 APPT"),
                CreateOperation("Eclairage interupteurs APPT", "ELECT_F1", 6, mwb_ApptT4, "Pose prises APPT"),
                CreateOperation("Pose convecteur Salon APPT", "ELECT_F1", 2, mwb_ApptT4, "Eclairage interupteurs APPT"),
                CreateOperation("Pose convecteurs Cuisine APPT", "ELECT_F1", 2, mwb_ApptT4, "Eclairage interupteurs APPT"),
                CreateOperation("Pose convecteur chambre1 APPT", "ELECT_F1", 2, mwb_ApptT4, "Eclairage interupteurs APPT"),
                CreateOperation("Pose convecteur chambre2 APPT", "ELECT_F1", 2, mwb_ApptT4, "Eclairage interupteurs APPT"),
                CreateOperation("Pose convecteur chambre3 APPT", "ELECT_F1", 2, mwb_ApptT4, "Eclairage interupteurs APPT"),
                CreateOperation("Pose convecteurs Sdb APPT", "ELECT_F1", 2, mwb_ApptT4, "Eclairage interupteurs APPT"),
                CreateOperation("Pose parquet salon APPT", "PARQUETS", 7, mwb_ApptT4, "peinture salon APPT"),
                CreateOperation("Pose parquet chambre1 APPT", "PARQUETS", 7, mwb_ApptT4, "Peinture Chambre1 APPT"),
                CreateOperation("Pose parquet chambre2 APPT", "PARQUETS", 6, mwb_ApptT4, "Peinture Chambre2 APPT"),
                CreateOperation("Pose parquet chambre3 APPT", "PARQUETS", 6, mwb_ApptT4, "Peinture Chambre3 APPT"),
                CreateOperation("Nettoyage final APPT", "NETTOYAG", 10, mwb_ApptT4, "Raccordements Plomberie Finale APPT, Eclairage interupteurs APPT, Pose convecteur Salon APPT, Pose convecteurs Cuisine APPT, Pose convecteur chambre1 APPT, Pose convecteur chambre2 APPT, Pose convecteur chambre3 APPT, Pose convecteurs Sdb APPT, Pose parquet salon APPT, Pose parquet chambre1 APPT, Pose parquet chambre2 APPT, Pose parquet chambre3 APPT")
            };
            // Création explicite des appartements T4
            string[] apptT4_Suffixes = { "APPT_301", "APPT_302" };
            foreach (var suffix in apptT4_Suffixes)
            {
                var currentOps = ops_ApptT4_Base.Select(op => new OperationInput
                { // Duplication
                    OperationId = op.OperationId.Replace("APPT", suffix.Replace("_", "")),
                    TradeCode = op.TradeCode,
                    Hours = op.Hours,
                    DependsOnOperationIds = op.DependsOnOperationIds.Select(d => d.Replace("APPT", suffix.Replace("_", ""))).ToList(),
                    MaxConcurrentWorkersOnOperation = op.MaxConcurrentWorkersOnOperation
                }).ToList();
                blocs.Add(new BlocInput { BlocId = $"{zoneT3Id}_{suffix}", AssociatedZoneId = zoneT3Id, MaxConcurrentWorkersInBloc = mwb_ApptT4, Operations = currentOps });
            }

            // --- ZONE S (Etage 4 - Studios) ---
            // Bloc: Couloir etage 4
            int mwb_CouloirE4 = 3;
            var ops_CouloirE4 = ops_CouloirE1.Select(op => new OperationInput
            { // Duplication de Couloir E1
                OperationId = op.OperationId.Replace("E1", "E4"),
                TradeCode = op.TradeCode,
                Hours = op.Hours,
                DependsOnOperationIds = op.DependsOnOperationIds.Select(d => d.Replace("E1", "E4")).ToList(),
                MaxConcurrentWorkersOnOperation = op.MaxConcurrentWorkersOnOperation
            }).ToList();
            blocs.Add(new BlocInput { BlocId = $"{zoneSId}_Couloir_etage_4", AssociatedZoneId = zoneSId, MaxConcurrentWorkersInBloc = mwb_CouloirE4, Operations = ops_CouloirE4 });

            // Base pour APPT 401 (Studio type)
            int mwb_ApptStudio = 2;
            var ops_ApptStudio_Base = new List<OperationInput>
            {
                CreateOperation("Prepa chantier APPT", "PREPT_01", 12, mwb_ApptStudio),
                CreateOperation("Cloisonnement APPT", "MACON_01", 8, mwb_ApptStudio, "Prepa chantier APPT"),
                CreateOperation("Pose porte entree APPT", "BOISPoFe", 3, mwb_ApptStudio, "Cloisonnement APPT"),
                CreateOperation("Pose porte SdB APPT", "BOISPoFe", 3, mwb_ApptStudio, "Cloisonnement APPT"),
                CreateOperation("Pose fenetre salon APPT", "BOISPoFe", 5, mwb_ApptStudio, "Cloisonnement APPT"),
                CreateOperation("Pose gaines eau APPT", "PLOMB_01", 4, mwb_ApptStudio, "Cloisonnement APPT"),
                CreateOperation("Raccordement reseau eau APPT", "PLOMB_01", 1, mwb_ApptStudio, "Pose gaines eau APPT"),
                CreateOperation("Pose evacuation APPT", "PLOMB_01", 3, mwb_ApptStudio, "Pose gaines eau APPT"),
                CreateOperation("Pose et raccordement tableau APPT", "ELECT_01", 4, mwb_ApptStudio, "Cloisonnement APPT"),
                CreateOperation("Raccordement VMC APPT", "ELECT_01", 3, mwb_ApptStudio, "Pose et raccordement tableau APPT"),
                CreateOperation("Pose gaines Elec APPT", "ELECT_02", 6, mwb_ApptStudio, "Cloisonnement APPT"),
                CreateOperation("Tirage cables Elec APPT", "ELECT_02", 8, mwb_ApptStudio, "Pose gaines Elec APPT"),
                CreateOperation("Raccordements Elec APPT", "ELECT_02", 2, mwb_ApptStudio, "Tirage cables Elec APPT"),
                CreateOperation("Cablage convecteurs PAC APPT", "ELECT_01", 3, mwb_ApptStudio, "Pose gaines Elec APPT"),
                // Placo dépend de tout
                CreateOperation("BA13 Salon APPT", "PLAQUIST", 11, mwb_ApptStudio, "Pose porte entree APPT,Pose porte SdB APPT,Pose fenetre salon APPT,Raccordement reseau eau APPT,Pose evacuation APPT,Raccordement VMC APPT,Raccordements Elec APPT,Cablage convecteurs PAC APPT"),
                CreateOperation("BA13 SdB APPT", "PLAQUIST", 7, mwb_ApptStudio, "BA13 Salon APPT"),
                CreateOperation("Finitions et nettoyage placo APPT", "FINIT_01", 8, mwb_ApptStudio, "BA13 SdB APPT"),
                CreateOperation("Livraison matériaux finitions APPT", "PREPT_02", 10, mwb_ApptStudio, "Finitions et nettoyage placo APPT"),
                CreateOperation("Carrelage SdB APPT", "CARRELAG", 9, mwb_ApptStudio, "Livraison matériaux finitions APPT"),
                CreateOperation("Enduis salon APPT", "PLATRERI", 7, mwb_ApptStudio, "Finitions et nettoyage placo APPT"),
                CreateOperation("Enduis SdB APPT", "PLATRERI", 4, mwb_ApptStudio, "Enduis salon APPT, Carrelage SdB APPT"),
                CreateOperation("peinture salon APPT", "PEINTURE", 6, mwb_ApptStudio, "Enduis salon APPT"),
                CreateOperation("peinture SdB APPT", "PEINTURE", 3, mwb_ApptStudio, "Enduis SdB APPT"),
                CreateOperation("Pose WC APPT", "PLOMB_F1", 3, mwb_ApptStudio, "peinture SdB APPT, Carrelage SdB APPT"),
                CreateOperation("Pose lavabo APPT", "PLOMB_F1", 3, mwb_ApptStudio, "peinture SdB APPT, Carrelage SdB APPT"),
                CreateOperation("Pose douche APPT", "PLOMB_F1", 4, mwb_ApptStudio, "peinture SdB APPT, Carrelage SdB APPT"),
                CreateOperation("Pose robinetterie APPT", "PLOMB_F1", 3, mwb_ApptStudio, "peinture SdB APPT, Carrelage SdB APPT"),
                CreateOperation("Raccordements Plomberie Finale APPT", "PLOMB_F1", 4, mwb_ApptStudio, "Pose WC APPT, Pose lavabo APPT, Pose douche APPT, Pose robinetterie APPT"),
                CreateOperation("Pose prises APPT", "ELECT_F1", 5, mwb_ApptStudio, "peinture salon APPT, peinture SdB APPT"),
                CreateOperation("Eclairage interupteurs APPT", "ELECT_F1", 6, mwb_ApptStudio, "Pose prises APPT"),
                CreateOperation("Pose convecteur Salon APPT", "ELECT_F1", 2, mwb_ApptStudio, "Eclairage interupteurs APPT"),
                CreateOperation("Pose convecteurs Sdb APPT", "ELECT_F1", 2, mwb_ApptStudio, "Eclairage interupteurs APPT"),
                CreateOperation("Pose parquet salon APPT", "PARQUETS", 6, mwb_ApptStudio, "peinture salon APPT"),
                CreateOperation("Nettoyage final APPT", "NETTOYAG", 6, mwb_ApptStudio, "Raccordements Plomberie Finale APPT, Eclairage interupteurs APPT, Pose convecteur Salon APPT, Pose convecteurs Sdb APPT, Pose parquet salon APPT")
            };
            // Création explicite des studios
            string[] apptStudio_Suffixes = { "APPT_401", "APPT_402", "APPT_403", "APPT_404", "APPT_405", "APPT_406" };
            foreach (var suffix in apptStudio_Suffixes)
            {
                var currentOps = ops_ApptStudio_Base.Select(op => new OperationInput
                { // Duplication
                    OperationId = op.OperationId.Replace("APPT", suffix.Replace("_", "")),
                    TradeCode = op.TradeCode,
                    Hours = op.Hours,
                    DependsOnOperationIds = op.DependsOnOperationIds.Select(d => d.Replace("APPT", suffix.Replace("_", ""))).ToList(),
                    MaxConcurrentWorkersOnOperation = op.MaxConcurrentWorkersOnOperation
                }).ToList();
                blocs.Add(new BlocInput { BlocId = $"{zoneSId}_{suffix}", AssociatedZoneId = zoneSId, MaxConcurrentWorkersInBloc = mwb_ApptStudio, Operations = currentOps });
            }

            // --- Assemblage Final ---
            return new PlanningRequestInput
            {
                ChantierId = "ImmeubleCentreVille_V1",
                TradeDefinitions = GetTradeDefinitions_ImmeubleCentreVille(),
                Workers = GetWorkers_ImmeubleCentreVille(),
                Zones = zones,
                Blocs = blocs
            };
        }

        // Classe ScenarioActuel originale (peut être conservée ou supprimée si plus utilisée)
        public static class ScenarioActuel
        {
            // ... (code original de ScenarioActuel)
            private const string DEFAULT_ZONE_FOR_SCENARIO = "Z_MAIN";

            public static List<WorkerInput> GetWorkers()
            {
                return new List<WorkerInput> {
                new WorkerInput { Id = "74632", FirstName = "SAFIA", LastName = "ABDELLAHI", SkillTradeCodes = new List<string> { "PLOMB_01", "FINIT_01", "PREPT_01" } },
                new WorkerInput { Id = "74584", FirstName = "LIONEL", LastName = "ALLARD", SkillTradeCodes = new List<string> { "MACON_01", "PLOMB_01", "FINIT_01", "PREPT_01" } },
                new WorkerInput { Id = "74566", FirstName = "CELINE", LastName = "ANSOULD", SkillTradeCodes = new List<string> { "ELECT_01", "ELECT_02", "FINIT_01", "PREPT_01" } },
                new WorkerInput { Id = "74633", FirstName = "FLORENCE", LastName = "BAPTISTE", SkillTradeCodes = new List<string> { "ELECT_01", "FINIT_01", "PREPT_01" } },
                new WorkerInput { Id = "74588", FirstName = "TIMOTHEE", LastName = "BARRAULT", SkillTradeCodes = new List<string> { "CARRE_01", "FINIT_01", "PREPT_01" } },
                new WorkerInput { Id = "74766", FirstName = "EMMANUEL", LastName = "BESHELEMU", SkillTradeCodes = new List<string> { "PLATR_01", "FINIT_01", "PREPT_01" } }, // Note: PLATR_01 n'est pas dans les nouveaux TradeCodes, PLAQUIST est utilisé
                new WorkerInput { Id = "74596", FirstName = "YANN", LastName = "BLANCHARD", SkillTradeCodes = new List<string> { "MACON_01", "PLATR_01", "FINIT_01", "PREPT_01" } },
                new WorkerInput { Id = "74579", FirstName = "OLIVIER", LastName = "BONTOUX", SkillTradeCodes = new List<string> { "PEINT_01", "FINIT_01", "PREPT_01" } },
                new WorkerInput { Id = "74568", FirstName = "MALIK", LastName = "BOUGHANI", SkillTradeCodes = new List<string> { "PEINT_01", "FINIT_01", "PREPT_01" } },
                new WorkerInput { Id = "74563", FirstName = "ERIC", LastName = "BOURRIQUIS", SkillTradeCodes = new List<string> { "FINIT_01", "PREPT_01" } }
                };
            }
            public static List<BlocInput> GetBlocs()
            {
                var blocInputs = new List<BlocInput>();
                // Adapté pour utiliser la nouvelle structure si nécessaire, ou gardé pour compatibilité.
                // Pour l'instant, je le laisse tel quel, car il utilise une source de données différente.
                // Si vous voulez le convertir, il faudrait recréer les EstimationInput_Source ici.
                return blocInputs;
            }
            public static List<ZoneLogicInput> GetZones()
            {
                return new List<ZoneLogicInput> {
                    new ZoneLogicInput { ZoneId = DEFAULT_ZONE_FOR_SCENARIO, PriorityLevel = 3, DependsOnZoneIds = new List<string>() }
                };
            }
            public static PlanningRequestInput GetPlanningRequest()
            {
                return new PlanningRequestInput
                {
                    ChantierId = "ScenarioActuel_V0.2.1_Ops",
                    TradeDefinitions = GetDefaultTradeConfiguration(), // Utilise l'ancienne config
                    Workers = GetWorkers(),
                    Blocs = GetBlocs(),
                    Zones = GetZones()
                };
            }
        }
    }
}