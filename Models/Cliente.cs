namespace ApiEjemplo.Models;

public class Cliente
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? Ciudad { get; set; }
    public DateTime? FechaRegistro { get; set; }
}
