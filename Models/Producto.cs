namespace ApiEjemplo.Models;

public class Producto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int IdCategoria { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string? NombreCategoria { get; set; }
}
