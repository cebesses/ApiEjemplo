using ApiEjemplo.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace ApiEjemplo.Controllers;

[ApiController]
[Route("[controller]")]
public class CategoriaController(MySqlDataSource db) : ControllerBase
{
    [HttpGet(Name = "GetCategorias")]
    public async Task<ActionResult<IEnumerable<Categoria>>> Get()
    {
        var categorias = new List<Categoria>();
        await using var conexion = await db.OpenConnectionAsync();
        await using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT id_categoria, nombre FROM categoria ORDER BY id_categoria";
        await using var lector = await comando.ExecuteReaderAsync();
        while (await lector.ReadAsync())
        {
            categorias.Add(new Categoria { IdCategoria = lector.GetInt32(0), Nombre = lector.GetString(1) });
        }
        return Ok(categorias);
    }

    [HttpGet("{id:int}", Name = "GetCategoriaPorId")]
    public async Task<ActionResult<Categoria>> GetById(int id)
    {
        await using var conexion = await db.OpenConnectionAsync();
        var categoria = await BuscarAsync(conexion, id);
        if (categoria is null)
        {
            return NotFound(new { mensaje = $"No existe una categoría con el id {id}." });
        }
        return Ok(categoria);
    }

    [HttpPost(Name = "PostCategoria")]
    public async Task<ActionResult<Categoria>> Post([FromBody] Categoria categoria)
    {
        var nombre = categoria.Nombre?.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new { mensaje = "El nombre de la categoría es obligatorio." });
        }

        await using var conexion = await db.OpenConnectionAsync();
        try
        {
            await using var comando = conexion.CreateCommand();
            comando.CommandText = "INSERT INTO categoria (nombre) VALUES (@nombre)";
            comando.Parameters.AddWithValue("@nombre", nombre);
            await comando.ExecuteNonQueryAsync();

            await using var comandoId = conexion.CreateCommand();
            comandoId.CommandText = "SELECT LAST_INSERT_ID()";
            var id = Convert.ToInt32(await comandoId.ExecuteScalarAsync());
            return CreatedAtAction(nameof(GetById), new { id }, new Categoria { IdCategoria = id, Nombre = nombre });
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new { mensaje = "Ya existe una categoría con ese nombre." });
        }
    }

    [HttpPut("{id:int}", Name = "PutCategoria")]
    public async Task<ActionResult<Categoria>> Put(int id, [FromBody] Categoria categoria)
    {
        var nombre = categoria.Nombre?.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new { mensaje = "El nombre de la categoría es obligatorio." });
        }

        await using var conexion = await db.OpenConnectionAsync();
        if (await BuscarAsync(conexion, id) is null)
        {
            return NotFound(new { mensaje = $"No existe una categoría con el id {id}." });
        }

        try
        {
            await using var comando = conexion.CreateCommand();
            comando.CommandText = "UPDATE categoria SET nombre = @nombre WHERE id_categoria = @id";
            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@id", id);
            await comando.ExecuteNonQueryAsync();
            return Ok(new Categoria { IdCategoria = id, Nombre = nombre });
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new { mensaje = "Ya existe una categoría con ese nombre." });
        }
    }

    [HttpDelete("{id:int}", Name = "DeleteCategoria")]
    public async Task<IActionResult> Delete(int id)
    {
        await using var conexion = await db.OpenConnectionAsync();
        try
        {
            await using var comando = conexion.CreateCommand();
            comando.CommandText = "DELETE FROM categoria WHERE id_categoria = @id";
            comando.Parameters.AddWithValue("@id", id);
            var filas = await comando.ExecuteNonQueryAsync();
            if (filas == 0)
            {
                return NotFound(new { mensaje = $"No existe una categoría con el id {id}." });
            }
            return NoContent();
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            return Conflict(new { mensaje = "No se puede eliminar la categoría porque tiene productos asociados." });
        }
    }

    private static async Task<Categoria?> BuscarAsync(MySqlConnection conexion, int id)
    {
        await using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT id_categoria, nombre FROM categoria WHERE id_categoria = @id";
        comando.Parameters.AddWithValue("@id", id);
        await using var lector = await comando.ExecuteReaderAsync();
        if (!await lector.ReadAsync())
        {
            return null;
        }
        return new Categoria { IdCategoria = lector.GetInt32(0), Nombre = lector.GetString(1) };
    }
}
