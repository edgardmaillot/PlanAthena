openapi: 3.0.0
info:
  title: PlanAthena.core API
  version: 0.1.1
  description: API pour la validation, l'analyse et l'optimisation de plannings de chantier.

servers:
  - url: https://api.planathena.com/v1

paths:
  /planning/process:
    post:
      summary: Traite une demande complète de chantier.
      description: |
        Prend une description de chantier en entrée et, en fonction de la configuration, 
        effectue soit une analyse statique, soit une optimisation de planning.
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/ChantierSetupInputDto'
      responses:
        '200':
          description: Traitement réussi. Le corps de la réponse contient le résultat.
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ProcessChantierResultDto'
        '400':
          description: Erreur de validation des données d'entrée.
        '500':
          description: Erreur interne du serveur.

components:
  schemas:
    # --- DTOs d'Entrée ---
    ChantierSetupInputDto:
      type: object
      required:
        - ChantierId
        - Description
        - CalendrierTravail
        - Blocs
        - Taches
        - Lots
        - Ouvriers
        - Metiers
      properties:
        ChantierId: { type: string }
        Description: { type: string }
        DateDebutSouhaitee: { type: string, format: date-time, nullable: true }
        DateFinSouhaitee: { type: string, format: date-time, nullable: true }
        OptimizationConfig:
          $ref: '#/components/schemas/OptimizationConfigDto'
          nullable: true
        # ... et toutes les autres listes de DTOs ...

    OptimizationConfigDto:
      type: object
      required:
        - TypeDeSortie
      properties:
        TypeDeSortie:
          type: string
          description: "Pilote la stratégie. Ex: 'ANALYSE_RAPIDE', 'OPTIMISATION_COUT', 'OPTIMISATION_DELAI'."
        DureeJournaliereStandardHeures: { type: integer, default: 7 }
        PenaliteChangementOuvrierPourcentage: { type: number, default: 30.0 }
        CoutIndirectJournalierPourcentage: { type: number, nullable: true, description: "Si null, 10% par défaut." }

    # ... (Définitions complètes pour TacheDto, OuvrierDto, MetierDto, etc.)

    # --- DTOs de Sortie ---
    ProcessChantierResultDto:
      type: object
      properties:
        ChantierId: { type: string }
        Etat: { type: string, enum: [Succes, SuccesAvecAvertissements, EchecValidation] }
        Messages:
          type: array
          items: { $ref: '#/components/schemas/MessageValidationDto' }
        AnalyseStatiqueResultat:
          $ref: '#/components/schemas/AnalyseRessourcesResultatDto'
          nullable: true
        OptimisationResultat:
          $ref: '#/components/schemas/PlanningOptimizationResultDto'
          nullable: true
        AnalysePostOptimisationResultat:
          $ref: '#/components/schemas/PlanningAnalysisReportDto'
          nullable: true

    PlanningOptimizationResultDto:
      type: object
      properties:
        ChantierId: { type: string }
        Status: { type: string, enum: [Optimal, Feasible, Infeasible, Aborted] }
        CoutTotalEstime: { type: integer, format: int64, description: "En centimes", nullable: true }
        CoutTotalRhEstime: { type: integer, format: int64, description: "En centimes", nullable: true }
        CoutTotalIndirectEstime: { type: integer, format: int64, description: "En centimes", nullable: true }
        DureeTotaleEnSlots: { type: integer, format: int64, nullable: true }
        Affectations:
          type: array
          items: { $ref: '#/components/schemas/AffectationDto' }

    AffectationDto:
      type: object
      required: [TacheId, OuvrierId, DateDebut, DateFin]
      properties:
        TacheId: { type: string }
        TacheNom: { type: string }
        OuvrierId: { type: string }
        OuvrierNom: { type: string }
        BlocId: { type: string }
        DateDebut: { type: string, format: date-time }
        DateFin: { type: string, format: date-time }

    PlanningAnalysisReportDto:
      type: object
      properties:
        KpisGlobaux: { $ref: '#/components/schemas/GlobalKpiDto' }
        KpisParOuvrier:
          type: array
          items: { $ref: '#/components/schemas/WorkerKpiDto' }

    WorkerKpiDto:
      type: object
      properties:
        OuvrierId: { type: string }
        OuvrierNom: { type: string }
        JoursDePresence: { type: integer }
        HeuresTravaillees: { type: number, format: double }
        TauxOccupation: { type: number, format: double, description: "En pourcentage" }
        TauxFragmentation: { type: number, format: double, description: "En pourcentage" }
    
    # ... (Définitions complètes pour les autres DTOs de sortie)
