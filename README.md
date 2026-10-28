# ApiEjemplo

API REST de la tienda con ASP.NET Core (.NET 10) y MySQL. CRUD de `categoria`, `cliente` y `producto`.

![Diagrama de la base de datos](docs/diagrama-bd.png)

## Base de datos

```bash
mysql -u root -p < database/01_crear_base_datos.sql
```

Si la tabla `producto` ya existe con las columnas `id`, `name`, `price` y `quantity`, ejecuta además:

```bash
mysql -u root -p < database/02_migracion_producto.sql
```

La migración renombra las columnas a `id_producto`, `nombre`, `precio`, `stock` y crea la categoría `General` para los productos existentes.

## Configuración

Edita `appsettings.json` con los datos de tu MySQL:

```json
"ConnectionStrings": {
  "Tienda": "Server=localhost;Port=3306;Database=tienda;User ID=root;Password=12345;"
}
```

## Ejecutar

```bash
dotnet restore
dotnet run
```

API: `http://localhost:5156` · OpenAPI: `http://localhost:5156/openapi/v1.json`

## Endpoints

| Recurso | Rutas |
| --- | --- |
| Categoría | `GET /categoria`, `GET /categoria/{id}`, `POST /categoria`, `PUT /categoria/{id}`, `DELETE /categoria/{id}` |
| Cliente | `GET /cliente`, `GET /cliente/{id}`, `POST /cliente`, `PUT /cliente/{id}`, `DELETE /cliente/{id}` |
| Producto | `GET /producto`, `GET /producto/{id}`, `POST /producto`, `PUT /producto/{id}`, `DELETE /producto/{id}` |
| Salud | `GET /salud` |

## Ejemplo

```bash
curl -X POST http://localhost:5156/categoria -H "Content-Type: application/json" -d "{\"nombre\":\"Computadores\"}"
```
