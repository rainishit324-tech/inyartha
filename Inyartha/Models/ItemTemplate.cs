namespace InyarthaApp.Models;

public class ItemTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string PricingUnit { get; set; } = "Per sq ft";
}
