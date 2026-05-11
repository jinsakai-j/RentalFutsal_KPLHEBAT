namespace FutsalRentalConsoleApp.Models;

public sealed class Field
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerHour { get; set; }
    public bool IsActive { get; set; } = true;

    public override string ToString()
    {
        return $"{Id}. {Name} - Rp{PricePerHour:N0}/jam - {(IsActive ? "Aktif" : "Tidak Aktif")}";
    }
}