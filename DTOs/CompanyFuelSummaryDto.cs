namespace SmartCard.DTOs
{
    public class CompanyFuelSummaryDto
    {
        /// <summary>
        /// Quantité totale de carburant approvisionnée au niveau de l'entreprise
        /// </summary>
        public decimal TotalSuppliedLiters { get; set; }

        /// <summary>
        /// Quantité totale de carburant allouée/distribuée sous forme de quotas aux employés
        /// </summary>
        public decimal TotalAllocatedLiters { get; set; }

        /// <summary>
        /// Quantité totale de carburant consommée (prélevée à la pompe)
        /// </summary>
        public decimal TotalConsumedLiters { get; set; }

        /// <summary>
        /// Ce qui reste dans la réserve de l'entreprise, disponible pour être distribué aux employés
        /// Formule: TotalSuppliedLiters - TotalAllocatedLiters
        /// </summary>
        public decimal RemainingCompanyStockLiters { get; set; }

        /// <summary>
        /// Ce qui est actuellement à la disposition des employés (attribué mais non encore consommé)
        /// Formule: TotalAllocatedLiters - TotalConsumedLiters
        /// </summary>
        public decimal AvailableForEmployeesLiters { get; set; }

        /// <summary>
        /// Stock physique restant total (approvisionné - consommé)
        /// Formule: TotalSuppliedLiters - TotalConsumedLiters
        /// </summary>
        public decimal RemainingPhysicalStockLiters { get; set; }

        /// <summary>
        /// Nombre total de ravitaillements/approvisionnements enregistrés
        /// </summary>
        public int TotalSuppliesCount { get; set; }
    }
}
