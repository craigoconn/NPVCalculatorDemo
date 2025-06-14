namespace NPVCalculator.Domain.Interfaces
{
    public interface INpvDomainService
    {
        decimal CalculateNpv(IList<decimal> cashFlows, decimal discountRate);
    }
}