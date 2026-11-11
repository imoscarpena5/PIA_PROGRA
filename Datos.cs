using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Comedor51;

public class Platillo
{
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public decimal Precio { get; set; }
    public int Existencia { get; set; }   // porciones disponibles en el inventario
}

public record Usuario(string Login, string Hash, string Rol, string Nombre);

public record Consumo(string Id, DateTime Fecha, string NumEmpleado, string Empleado,
    string Platillo, string Categoria, int Cantidad, decimal Precio)
{
    public decimal Subtotal => Cantidad * Precio;
    public static string NuevoId() => Guid.NewGuid().ToString("N")[..8];
}

public static class Datos
{
    public static readonly string Carpeta = Path.Combine(AppContext.BaseDirectory, "datos");
    public static readonly string RutaUsuarios = Path.Combine(Carpeta, "usuarios.txt");
    public static readonly string RutaConsumos = Path.Combine(Carpeta, "consumos.txt");
    public static readonly string RutaMenu = Path.Combine(Carpeta, "menu.txt");

    // Menú / inventario: se carga de menu.txt (la misma lista se comparte en todas las pantallas)
    public static readonly List<Platillo> Menu = new();

    // Crea la carpeta y los archivos de texto la primera vez que corre la app
    public static void Inicializar()
    {
        Directory.CreateDirectory(Carpeta);

        if (!File.Exists(RutaUsuarios))
        {
            GuardarUsuarios(new[]
            {
                new Usuario("admin", Hash("1234"), "Administrador", "Administrador"),
                new Usuario("cajero", Hash("1234"), "Cajero", "Cajero")
            });
        }

        if (!File.Exists(RutaConsumos))
            File.WriteAllText(RutaConsumos, "");

        if (!File.Exists(RutaMenu))
        {
            Menu.AddRange(new[]
            {
                new Platillo { Nombre = "Pollo con arroz",   Categoria = "Plato fuerte", Precio = 45m, Existencia = 40 },
                new Platillo { Nombre = "Milanesa con puré", Categoria = "Plato fuerte", Precio = 50m, Existencia = 30 },
                new Platillo { Nombre = "Sopa de verduras",  Categoria = "Entrada",      Precio = 20m, Existencia = 50 },
                new Platillo { Nombre = "Frijoles charros",  Categoria = "Guarnición",   Precio = 15m, Existencia = 50 },
                new Platillo { Nombre = "Agua de jamaica",   Categoria = "Bebida",       Precio = 12m, Existencia = 60 },
                new Platillo { Nombre = "Gelatina",          Categoria = "Postre",       Precio = 10m, Existencia = 40 }
            });
            GuardarMenu();
        }
        else
        {
            CargarMenu();
        }
    }

    // La contraseña no se guarda en texto plano, se guarda su hash SHA-256
    public static string Hash(string texto) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));

    // ================= MENÚ E INVENTARIO =================

    // Formato de cada línea: nombre|categoria|precio|existencia
    public static void GuardarMenu() =>
        File.WriteAllLines(RutaMenu, Menu.Select(p => string.Join("|",
            p.Nombre, p.Categoria,
            p.Precio.ToString(CultureInfo.InvariantCulture), p.Existencia)));

    public static void CargarMenu()
    {
        Menu.Clear();
        foreach (var linea in File.ReadAllLines(RutaMenu))
        {
            var p = linea.Split('|');
            if (p.Length == 4
                && decimal.TryParse(p[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var precio)
                && int.TryParse(p[3], out var existencia))
            {
                Menu.Add(new Platillo { Nombre = p[0], Categoria = p[1], Precio = precio, Existencia = existencia });
            }
        }
    }

    public static Platillo? BuscarPlatillo(string nombre) =>
        Menu.FirstOrDefault(m => m.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

    // delta negativo = descuenta del inventario; positivo = regresa o surte
    public static void AjustarExistencia(string nombrePlatillo, int delta)
    {
        var p = BuscarPlatillo(nombrePlatillo);
        if (p is null) return;   // el producto ya no está en el menú
        p.Existencia = Math.Max(0, p.Existencia + delta);
        GuardarMenu();
    }

    // Categorías del menú actual más las de consumos ya guardados
    public static List<string> Categorias() =>
        Menu.Select(m => m.Categoria)
            .Concat(LeerConsumos().Select(c => c.Categoria))
            .Distinct()
            .OrderBy(c => c)
            .ToList();

    // ================= USUARIOS =================

    // Formato de cada línea: usuario|hash|rol|nombre   (acepta también el formato anterior de 3 campos)
    public static List<Usuario> LeerUsuarios()
    {
        var lista = new List<Usuario>();
        foreach (var linea in File.ReadAllLines(RutaUsuarios))
        {
            var p = linea.Split('|');
            if (p.Length == 4) lista.Add(new Usuario(p[0], p[1], p[2], p[3]));
            else if (p.Length == 3) lista.Add(new Usuario(p[0], p[1], p[2], p[0]));
        }
        return lista;
    }

    public static void GuardarUsuarios(IEnumerable<Usuario> usuarios) =>
        File.WriteAllLines(RutaUsuarios,
            usuarios.Select(u => string.Join("|", u.Login, u.Hash, u.Rol, u.Nombre)));

    // Devuelve el usuario si el login y la contraseña son correctos; si no, null
    public static Usuario? ValidarUsuario(string login, string clave)
    {
        string h = Hash(clave);
        return LeerUsuarios().FirstOrDefault(u =>
            u.Login.Equals(login, StringComparison.OrdinalIgnoreCase) && u.Hash == h);
    }

    // Devuelve false si el usuario ya existe
    public static bool CrearUsuario(string login, string clave, string rol, string nombre)
    {
        var lista = LeerUsuarios();
        if (lista.Any(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase)))
            return false;

        lista.Add(new Usuario(login, Hash(clave), rol, nombre));
        GuardarUsuarios(lista);
        return true;
    }

    // Devuelve false si la contraseña actual no es correcta
    public static bool CambiarClave(string login, string claveActual, string claveNueva)
    {
        var lista = LeerUsuarios();
        int i = lista.FindIndex(u => u.Login == login);
        if (i < 0 || lista[i].Hash != Hash(claveActual)) return false;

        lista[i] = lista[i] with { Hash = Hash(claveNueva) };
        GuardarUsuarios(lista);
        return true;
    }

    public static void EliminarUsuario(string login)
    {
        var lista = LeerUsuarios();
        lista.RemoveAll(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
        GuardarUsuarios(lista);
    }

    // ================= CONSUMOS =================

    private static string Linea(Consumo c) => string.Join("|",
        c.Id,
        c.Fecha.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        c.NumEmpleado, c.Empleado, c.Platillo, c.Categoria, c.Cantidad,
        c.Precio.ToString(CultureInfo.InvariantCulture));

    // Guardar: agrega registros al final del archivo
    public static void GuardarConsumos(IEnumerable<Consumo> consumos) =>
        File.AppendAllLines(RutaConsumos, consumos.Select(Linea));

    // Actualizar / eliminar: reescribe todo el archivo con la lista completa
    public static void Sobrescribir(IEnumerable<Consumo> consumos) =>
        File.WriteAllLines(RutaConsumos, consumos.Select(Linea));

    // Consultar: lee el archivo (acepta también los formatos anteriores, sin Id o sin número de empleado)
    public static List<Consumo> LeerConsumos()
    {
        var lista = new List<Consumo>();
        bool migrado = false;

        foreach (var linea in File.ReadAllLines(RutaConsumos))
        {
            var p = linea.Split('|');
            string id;
            string[] d;   // fecha, numEmpleado, empleado, platillo, categoria, cantidad, precio

            if (p.Length == 8) { id = p[0]; d = p[1..]; }
            else if (p.Length == 7) { id = p[0]; d = new[] { p[1], "S/N", p[2], p[3], p[4], p[5], p[6] }; migrado = true; }
            else if (p.Length == 6) { id = Consumo.NuevoId(); d = new[] { p[0], "S/N", p[1], p[2], p[3], p[4], p[5] }; migrado = true; }
            else continue;

            if (DateTime.TryParse(d[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha)
                && int.TryParse(d[5], out var cantidad)
                && decimal.TryParse(d[6], NumberStyles.Number, CultureInfo.InvariantCulture, out var precio))
            {
                lista.Add(new Consumo(id, fecha, d[1], d[2], d[3], d[4], cantidad, precio));
            }
        }

        if (migrado) Sobrescribir(lista);   // deja el archivo en el formato nuevo
        return lista;
    }

    // Busca el nombre con el que ya se registró un número de empleado (null si es nuevo)
    public static string? NombrePorNumero(string numero, IEnumerable<Consumo>? extra = null)
    {
        var todos = LeerConsumos().Concat(extra ?? Enumerable.Empty<Consumo>());
        return todos.FirstOrDefault(c => c.NumEmpleado == numero)?.Empleado;
    }
}
