namespace TuberTreats.Models.DTOs;

public class TuberDriverDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<TuberOrderDTO> TuberDeliveries { get; set; }
}