using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace ApiEjemplo.Controllers;

[ApiController]
[Route("[controller]")]
public class SaludController(MySqlDataSource db) : ControllerBase
{
    [HttpGet(Name = "GetSalud")]
    public async Task<IActionResult> Get()
    {
        await using var conexion = await db.OpenConnectionAsync();
        await using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT 1";
        await comando.ExecuteScalarAsync();
        return Ok(new { estado = "OK", baseDatos = "MySQL" });
    }
}
