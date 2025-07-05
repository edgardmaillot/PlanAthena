// PlanAthena.Core.Domain.ValueObjects.PeriodePlanification.cs
using System;
using PlanAthena.Core.Facade.Dto.Enums; // Pour FlexibiliteDate

namespace PlanAthena.Core.Domain.ValueObjects
{
    public record PeriodePlanification // record class pour nullabilité facile
    {
        public DateTime? DateDebut { get; }
        public DateTime? DateFin { get; }
        public FlexibiliteDate FlexibiliteDebut { get; }
        public FlexibiliteDate FlexibiliteFin { get; }

        // Constructeur pour initialiser les propriétés
        public PeriodePlanification(
            DateTime? dateDebut,
            DateTime? dateFin,
            FlexibiliteDate flexibiliteDebut = FlexibiliteDate.Flexible,
            FlexibiliteDate flexibiliteFin = FlexibiliteDate.Flexible)
        {
            if (dateDebut.HasValue && dateFin.HasValue && dateDebut.Value > dateFin.Value)
            {
                throw new ArgumentException("La date de début ne peut pas être postérieure à la date de fin.");
            }

            DateDebut = dateDebut;
            DateFin = dateFin;
            FlexibiliteDebut = flexibiliteDebut;
            FlexibiliteFin = flexibiliteFin;
        }

        public bool EstPeriodeDefinie => DateDebut.HasValue || DateFin.HasValue;
    }
}