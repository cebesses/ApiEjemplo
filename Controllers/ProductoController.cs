using ApiEjemplo.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace ApiEjemplo.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductoController(MySqlDataSource db) : ControllerBase
{
    private const string ConsultaSelect = """
        SELECT p.id_producto, p.nombre, p.id_categoria, p.precio, p.stock, c.nombre
        FROM producto p
        LEFT JOIN categoria c ON c.id_categoria = p.id_categoria
        """;

    [HttpGet(Name = "GetProductos")]
    public async Task<ActionResult<IEnumerable<Producto>>> Get()
    {
        var productos = new List<Producto>();
        await using var conexion = await db.OpenConnectionAsync();
        await using var comando = conexion.CreateCommand();
        comando.CommandText = ConsultaSelect + " ORDER BY p.id_producto";
        await using var lector = await comando.ExecuteReaderAsync();
        while (await lector.ReadAsync())
        {
            productos.Add(LeerProducto(lector));
        }
        return Ok(productos);
    }

    [HttpGet("{id:int}", Name = "GetProductoPorId")]
    public async Task<ActionResult<Producto>> GetById(int id)
    {
        await using var conexion = await db.OpenConnectionAsync();
        var producto = await BuscarAsync(conexion, id);
        if (producto is null)
        {
            return NotFound(new { mensaje = $"No existe un producto con el id {id}." });
        }
        return Ok(producto);
    }

    [HttpPost(Name = "PostProducto")]
    public async Task<ActionResult<Producto>> Post([FromBody] Producto producto)
    {
        var nombre = producto.Nombre?.Trim();
        var error = Validar(nombre, producto);
        if (error is not null)
        {
            return BadRequest(new { mensaje = error });
        }

        await using var conexion = await db.OpenConnectionAsync();
        try
        {
            await using var comando = conexion.CreateCommand();
            comando.CommandText = "INSERT INTO producto (nombre, id_categoria, precio, stock) VALUES (@nombre, @id_categoria, @precio, @stock)";
            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@id_categoria", producto.IdCategoria);
            comando.Parameters.AddWithValue("@precio", producto.Precio);
            comando.Parameters.AddWithValue("@stock", producto.Stock);
            await comando.ExecuteNonQueryAsync();

            await using var comandoId = conexion.CreateCommand();
            comandoId.CommandText = "SELECT LAST_INSERT_ID()";
            var id = Convert.ToInt32(await comandoId.ExecuteScalarAsync());
            return CreatedAtAction(nameof(GetById), new { id }, await BuscarAsync(conexion, id));
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            return BadRequest(new { mensaje = "La categoría indicada no existe." });
        }
    }

    [HttpPut("{id:int}", Name = "PutProducto")]
    public async Task<ActionResult<Producto>> Put(int id, [FromBody] Producto producto)
    {
        var nombre = producto.Nombre?.Trim();
        var error = Validar(nombre, producto);
        if (error is not null)
        {
            return BadRequest(new { mensaje = error });
        }

        await using var conexion = await db.OpenConnectionAsync();
        if (await BuscarAsync(conexion, id) is null)
        {
            return NotFound(new { mensaje = $"No existe un producto con el id {id}." });
        }

        try
        {
            await using var comando = conexion.CreateCommand();
            comando.CommandText = "UPDATE producto SET nombre = @nombre, id_categoria = @id_categoria, precio = @precio, stock = @stock WHERE id_producto = @id";
            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@id_categoria", producto.IdCategoria);
            comando.Parameters.AddWithValue("@precio", producto.Precio);
            comando.Parameters.AddWithValue("@stock", producto.Stock);
            comando.Parameters.AddWithValue("@id", id);
            await comando.ExecuteNonQueryAsync();
            return Ok(await BuscarAsync(conexion, id));
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            return BadRequest(new { mensaje = "La categoría indicada no existe." });
        }
    }

    [HttpDelete("{id:int}", Name = "DeleteProducto")]
    public async Task<IActionResult> Delete(int id)
    {
        await using var conexion = await db.OpenConnectionAsync();
        await using var comando = conexion.CreateCommand();
        comando.CommandText = "DELETE FROM producto WHERE id_producto = @id";
        comando.Parameters.AddWithValue("@id", id);
        var filas = await comando.ExecuteNonQueryAsync();
        if (filas == 0)
        {
            return NotFound(new { mensaje = $"No existe un producto con el id {id}." });
        }
        return NoContent();
    }

    private static string? Validar(string? nombre, Producto producto)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return "El nombre del producto es obligatorio.";
        if (producto.IdCategoria <= 0) return "El id de la categoría es obligatorio.";
        if (producto.Precio < 0) return "El precio no puede ser negativo.";
        if (producto.Stock < 0) return "El stock no puede ser negativo.";
        return null;
    }

    private static async Task<Producto?> BuscarAsync(MySqlConnection conexion, int id)
    {
        await using var comando = conexion.CreateCommand();
        comando.CommandText = ConsultaSelect + " WHERE p.id_producto = @id";
        comando.Parameters.AddWithValue("@id", id);
        await using var lector = await comando.ExecuteReaderAsync();
        if (!await lector.ReadAsync())
        {
            return null;
        }
        return LeerProducto(lector);
    }

    private static Producto LeerProducto(MySqlDataReader lector) => new()
    {
        IdProducto = lector.GetInt32(0),
        Nombre = lector.GetString(1),
        IdCategoria = lector.GetInt32(2),
        Precio = lector.GetDecimal(3),
        Stock = lector.GetInt32(4),
        NombreCategoria = lector.IsDBNull(5) ? null : lector.GetString(5)
    };
}
