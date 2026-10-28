using ApiEjemplo.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace ApiEjemplo.Controllers;

[ApiController]
[Route("[controller]")]
public class ClienteController(MySqlDataSource db) : ControllerBase
{
    [HttpGet(Name = "GetClientes")]
    public async Task<ActionResult<IEnumerable<Cliente>>> Get()
    {
        var clientes = new List<Cliente>();
        await using var conexion = await db.OpenConnectionAsync();
        await using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT id_cliente, nombre, correo, ciudad, fecha_registro FROM cliente ORDER BY id_cliente";
        await using var lector = await comando.ExecuteReaderAsync();
        while (await lector.ReadAsync())
        {
            clientes.Add(LeerCliente(lector));
        }
        return Ok(clientes);
    }

    [HttpGet("{id:int}", Name = "GetClientePorId")]
    public async Task<ActionResult<Cliente>> GetById(int id)
    {
        await using var conexion = await db.OpenConnectionAsync();
        var cliente = await BuscarAsync(conexion, id);
        if (cliente is null)
        {
            return NotFound(new { mensaje = $"No existe un cliente con el id {id}." });
        }
        return Ok(cliente);
    }

    [HttpPost(Name = "PostCliente")]
    public async Task<ActionResult<Cliente>> Post([FromBody] Cliente cliente)
    {
        var nombre = cliente.Nombre?.Trim();
        var correo = cliente.Correo?.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new { mensaje = "El nombre del cliente es obligatorio." });
        }
        if (string.IsNullOrWhiteSpace(correo) || !correo.Contains('@'))
        {
            return BadRequest(new { mensaje = "El correo del cliente no es válido." });
        }

        await using var conexion = await db.OpenConnectionAsync();
        try
        {
            await using var comando = conexion.CreateCommand();
            comando.CommandText = "INSERT INTO cliente (nombre, correo, ciudad, fecha_registro) VALUES (@nombre, @correo, @ciudad, COALESCE(@fecha, NOW()))";
            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@correo", correo);
            comando.Parameters.AddWithValue("@ciudad", (object?)cliente.Ciudad?.Trim() ?? DBNull.Value);
            comando.Parameters.AddWithValue("@fecha", (object?)cliente.FechaRegistro ?? DBNull.Value);
            await comando.ExecuteNonQueryAsync();

            await using var comandoId = conexion.CreateCommand();
            comandoId.CommandText = "SELECT LAST_INSERT_ID()";
            var id = Convert.ToInt32(await comandoId.ExecuteScalarAsync());
            return CreatedAtAction(nameof(GetById), new { id }, await BuscarAsync(conexion, id));
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new { mensaje = "Ya existe un cliente con ese correo." });
        }
    }

    [HttpPut("{id:int}", Name = "PutCliente")]
    public async Task<ActionResult<Cliente>> Put(int id, [FromBody] Cliente cliente)
    {
        var nombre = cliente.Nombre?.Trim();
        var correo = cliente.Correo?.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new { mensaje = "El nombre del cliente es obligatorio." });
        }
        if (string.IsNullOrWhiteSpace(correo) || !correo.Contains('@'))
        {
            return BadRequest(new { mensaje = "El correo del cliente no es válido." });
        }

        await using var conexion = await db.OpenConnectionAsync();
        if (await BuscarAsync(conexion, id) is null)
        {
            return NotFound(new { mensaje = $"No existe un cliente con el id {id}." });
        }

        try
        {
            await using var comando = conexion.CreateCommand();
            comando.CommandText = "UPDATE cliente SET nombre = @nombre, correo = @correo, ciudad = @ciudad, fecha_registro = COALESCE(@fecha, fecha_registro) WHERE id_cliente = @id";
            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@correo", correo);
            comando.Parameters.AddWithValue("@ciudad", (object?)cliente.Ciudad?.Trim() ?? DBNull.Value);
            comando.Parameters.AddWithValue("@fecha", (object?)cliente.FechaRegistro ?? DBNull.Value);
            comando.Parameters.AddWithValue("@id", id);
            await comando.ExecuteNonQueryAsync();
            return Ok(await BuscarAsync(conexion, id));
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new { mensaje = "Ya existe un cliente con ese correo." });
        }
    }

    [HttpDelete("{id:int}", Name = "DeleteCliente")]
    public async Task<IActionResult> Delete(int id)
    {
        await using var conexion = await db.OpenConnectionAsync();
        try
        {
            await using var comando = conexion.CreateCommand();
            comando.CommandText = "DELETE FROM cliente WHERE id_cliente = @id";
            comando.Parameters.AddWithValue("@id", id);
            var filas = await comando.ExecuteNonQueryAsync();
            if (filas == 0)
            {
                return NotFound(new { mensaje = $"No existe un cliente con el id {id}." });
            }
            return NoContent();
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            return Conflict(new { mensaje = "No se puede eliminar el cliente porque tiene registros asociados." });
        }
    }

    private static async Task<Cliente?> BuscarAsync(MySqlConnection conexion, int id)
    {
        await using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT id_cliente, nombre, correo, ciudad, fecha_registro FROM cliente WHERE id_cliente = @id";
        comando.Parameters.AddWithValue("@id", id);
        await using var lector = await comando.ExecuteReaderAsync();
        if (!await lector.ReadAsync())
        {
            return null;
        }
        return LeerCliente(lector);
    }

    private static Cliente LeerCliente(MySqlDataReader lector) => new()
    {
        IdCliente = lector.GetInt32(0),
        Nombre = lector.GetString(1),
        Correo = lector.GetString(2),
        Ciudad = lector.IsDBNull(3) ? null : lector.GetString(3),
        FechaRegistro = lector.GetDateTime(4)
    };
}
