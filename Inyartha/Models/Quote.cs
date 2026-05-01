namespace InyarthaApp.Models;

using System.ComponentModel.DataAnnotations;

public class Quote
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = string.Empty;

    [Required, Display(Name = "Client Name")]
    public string ClientName { get; set; } = string.Empty;

    [Required, EmailAddress, Display(Name = "Client Email")]
    public string ClientEmail { get; set; } = string.Empty;

    [Phone, Display(Name = "Client Phone")]
    public string ClientPhone { get; set; } = string.Empty;

    [Required, Display(Name = "Project Type")]
    public string ProjectType { get; set; } = string.Empty;

    [Display(Name = "Estimated Area (sq ft)")]
    public int AreaSqFt { get; set; }

    [Required, Display(Name = "Scope of Work")]
    public string Scope { get; set; } = string.Empty;

    [Required, Display(Name = "Estimated Budget (₹)")]
    public decimal Budget { get; set; }

    [Display(Name = "Timeline")]
    public string Timeline { get; set; } = string.Empty;

    [Display(Name = "Notes / Special Requirements")]
    public string Notes { get; set; } = string.Empty;

    public string Status { get; set; } = "Draft";
}