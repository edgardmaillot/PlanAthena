/**
 * @file main.js
 * Ce fichier contient toute la logique de présentation du diagramme PERT avec Cytoscape.js.
 * Il est appelé par le contrôle C# (AffichagePert.cs) qui lui fournit les données.
 * Il gère :
 * - La création et la mise en page du graphe.
 * - Le style visuel de tous les éléments (nœuds, arêtes, blocs).
 * - Les interactions utilisateur (clics) et la communication vers le C#.
 */

// On encapsule toute notre logique dans un objet "graphManager" attaché à la fenêtre globale.
// C'est une bonne pratique pour éviter de créer des variables globales qui pourraient entrer
// en conflit avec d'autres librairies. Le C# appellera les fonctions via "window.graphManager.nomDeLaFonction()".
window.graphManager = {

    // Propriété qui contiendra notre instance de graphe Cytoscape une fois initialisée.
    cy: null,

    // ===================================================================================
    // == SECTION API PUBLIQUE (Fonctions appelées depuis le C#)
    // ===================================================================================

    /**
     * Point d'entrée principal. Crée et affiche un nouveau graphe à partir des données fournies.
     * @param {object} graphData - Un objet contenant les listes de blocs, nœuds (tâches) et arêtes (dépendances).
     */
    loadGraph: function (graphData) {
        console.log("Données reçues du C# :", graphData); // Pour le débogage (visible avec F12)

        // Si un graphe existe déjà, on le détruit proprement pour éviter les fuites de mémoire.
        this.clearGraph();

        // On transforme les données brutes reçues du C# en une structure qu'un graphe Cytoscape comprend.
        const elements = this.buildElements(graphData);

        // C'est ici que la magie opère : on crée l'instance de Cytoscape.
        this.cy = cytoscape({
            // L'élément HTML (la "toile") dans lequel dessiner. Défini dans index.html.
            container: document.getElementById('cy'),

            // Les données du graphe (nœuds, arêtes, etc.).
            elements: elements,

            // La "feuille de style" qui définit l'apparence de chaque élément.
            style: this.getStylesheet(),

            // Le moteur de mise en page (layout). 'dagre' est spécialisé dans les graphes
            // dirigés et acycliques (comme un PERT), ce qui donne un très bon résultat.
            layout: {
                name: 'dagre',
                rankDir: 'LR', // 'LR' = Left-to-Right, équivalent de votre ancien LayerDirection.LR
                spacingFactor: 0.8, // Augmente l'espace entre les nœuds pour plus de clarté.
                nodeDimensionsIncludeLabels: true // TRES IMPORTANT : prend en compte la taille du label pour le placement
            }
        });

        // Une fois le graphe créé, on y attache les gestionnaires d'événements (clics, etc.).
        this.attachEvents();
    },

    /**
     * Détruit proprement l'instance actuelle du graphe.
     */
    clearGraph: function () {
        if (this.cy) {
            this.cy.destroy();
            this.cy = null;
        }
    },

    /**
     * Centre la vue sur le graphe et ajuste le zoom pour tout afficher.
     */
    fitView: function () {
        if (this.cy) {
            this.cy.fit(null, 50); // fit(elements, padding) - un padding de 50px autour du graphe.
        }
    },


    // ===================================================================================
    // == SECTION LOGIQUE INTERNE (Fonctions de construction et de gestion)
    // ===================================================================================

    /**
     * Traduit les données reçues du C# en un tableau d'éléments pour Cytoscape.
     * C'est ici que l'on gère la hiérarchie parent-enfant (blocs -> tâches).
     * @param {object} graphData - Les données brutes du C#.
     * @returns {Array} Un tableau d'objets `elements` pour Cytoscape.
     */
    buildElements: function (graphData) {
        // 1. On crée les nœuds parents (les blocs)
        const blocs = graphData.blocs.map(b => ({
            group: 'nodes',
            data: { id: b.id, label: b.nom, ...b },
            classes: 'bloc' // On leur assigne une classe pour les styler spécifiquement.
        }));

        // 2. On crée les nœuds enfants (les tâches)
        const nodes = graphData.nodes.map(n => ({
            group: 'nodes',
            data: {
                id: n.id,
                label: this.formatNodeLabel(n),
                metierColor: n.metierColorHex,
                parent: n.blocId, // <-- C'est ici qu'on lie la tâche à son bloc parent !
                ...n // On copie toutes les autres données (nom, heures, etc.)
            },
            // On assigne des classes dynamiquement pour le style (ex: est-ce un jalon ?).
            classes: `${n.estJalon ? 'jalon' : 'tache'} ${!n.metierId ? 'sans-metier' : ''}`
        }));

        // 3. On crée les arêtes (les dépendances)
        const edges = graphData.edges.map(e => ({
            group: 'edges',
            data: { source: e.source, target: e.target }
        }));

        // On retourne un seul grand tableau contenant tous ces éléments.
        return [...blocs, ...nodes, ...edges];
    },

    /**
     * Formate le texte qui sera affiché à l'intérieur d'un nœud (tâche ou jalon).
     * C'est l'équivalent de votre ancien `PertNodeBuilder.GetNodeLabel`.
     * @param {object} nodeData - Les données d'une seule tâche.
     * @returns {string} Le texte formaté, multiligne.
     */
    formatNodeLabel: function (nodeData) {
        if (nodeData.estJalon) {
            return nodeData.heures > 0 ? `⏳ ${nodeData.heures}` : '⚙️';
        }

        const nomTache = nodeData.nom.length > 26 ? nodeData.nom.substring(0, 23) + '...' : nodeData.nom;
        const nomMetier = `${nodeData.metierNom} ${nodeData.metierPictogram}`.trim();

        // Utilisation des template literals de JS pour un formatage multiligne propre
        return `
⚒ ${nodeData.id}
──────────
${nomTache}
──────────
👨‍💼 ${nomMetier}
⏱️ ${nodeData.heures}h   🔗 ${nodeData.dependenciesCount} dép.
        `.trim();
    },

    /**
     * Attache les gestionnaires d'événements au graphe pour l'interactivité.
     */
    attachEvents: function () {
        // Gère un clic ('tap') sur un nœud qui a la classe 'tache' OU 'jalon'.
        this.cy.on('tap', 'node.tache, node.jalon', (evt) => {
            const nodeData = evt.target.data(); // Récupère les données du nœud cliqué.
            // Envoie un message au C# pour l'informer du clic.
            window.chrome.webview.postMessage({
                type: 'nodeClick',
                data: { id: nodeData.id } // On envoie juste l'ID, le C# a le reste.
            });
        });

        // Gère un clic sur un nœud qui a la classe 'bloc'.
        this.cy.on('tap', 'node.bloc', (evt) => {
            const blocData = evt.target.data();
            window.chrome.webview.postMessage({
                type: 'blocClick',
                data: { id: blocData.id }
            });
        });
    },

    // ===================================================================================
    // == SECTION STYLE (L'équivalent de PertDiagramSettings.cs)
    // ===================================================================================

    /**
     * Définit l'apparence de tous les éléments du graphe.
     * C'est un tableau d'objets où chaque objet contient :
     * - un `selector` (comme en CSS) pour cibler des éléments.
     * - un `style` pour définir leur apparence.
     * @returns {Array} La feuille de style pour Cytoscape.
     */
    getStylesheet: function () {
        return [
            // Style de base pour TOUS les nœuds (tâches, jalons, etc.).
            // Les styles plus spécifiques (ex: '.jalon') écraseront ces valeurs.
            {
                selector: 'node',
                style: {
                    'shape': 'round-rectangle',
                    'padding': '10px',
                    'background-color': 'data(metierColor)', // Style dynamique basé sur les données du nœud !
                    'border-color': '#36454F',
                    'border-width': 1.5,
                    'label': 'data(label)', // Le contenu textuel vient des données du nœud.
                    'color': '#000000', // Couleur du texte
                    'text-shadow-color': '#CCCCCC',
                    'text-shadow-offset-x': 1,
                    'text-shadow-offset-y': 1,
                    'text-shadow-blur': 3,
                    'font-family': 'Segoe UI, sans-serif',
                    'font-size': '12px',
                    'font-weight': 'bold',
                    'text-wrap': 'wrap', // Permet au texte de passer à la ligne.
                    'text-valign': 'center',
                    'text-halign': 'center',
                    'width': 'label', // La largeur s'adapte au contenu.
                    'height': 'label',// La hauteur s'adapte au contenu.
                }
            },
            // Style pour les blocs (nœuds parents).
            {
                selector: '.bloc',
                style: {
                    'background-color': '#FAFAFA',
                    'border-color': '#CCCCCC',
                    'border-width': 1,
                    'label': 'data(label)',
                    'font-size': '14px',
                    'color': '#555555',
                    'text-valign': 'top',
                    'text-halign': 'center',
                    'text-margin-y': -10,
                    'text-shadow-blur': 0, // Pas d'ombre pour le titre du bloc
                }
            },
            // Style spécifique pour les jalons.
            {
                selector: '.jalon',
                style: {
                    'shape': 'diamond',
                    'background-color': '#FFF8E1',
                    'border-color': '#FFA500',
                    'border-width': 2,
                    'color': '#424242',
                    'font-size': '16px',
                }
            },
            // Style pour les tâches non assignées.
            {
                selector: '.sans-metier',
                style: {
                    'background-color': '#FFEBEE',
                    'border-color': '#B71C1C',
                    'border-style': 'dashed'
                }
            },
            // Style pour les arêtes (dépendances).
            {
                selector: 'edge',
                style: {
                    'width': 2,
                    'line-color': '#2E8B57',
                    'target-arrow-shape': 'triangle',
                    'target-arrow-color': '#2E8B57',
                    'curve-style': 'bezier' // 'bezier' donne de belles courbes. 'straight' pour des lignes droites.
                }
            },
            // Style appliqué à un élément quand il est sélectionné (cliqué).
            {
                selector: ':selected',
                style: {
                    'border-width': 4,
                    'border-color': '#800080' // Purple
                }
            }
        ];
    }
};