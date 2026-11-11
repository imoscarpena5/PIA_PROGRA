using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Comedor51;

public static class Ui
{
    public static readonly Color Azul = Color.FromArgb(0, 120, 215);
    public static readonly Color AzulOscuro = Color.FromArgb(0, 90, 160);

    public static void Aviso(string mensaje) =>
        MessageBox.Show(mensaje, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public static void Boton(Button b, string texto, Point ubicacion, Size tamano, Color color)
    {
        b.Text = texto;
        b.Location = ubicacion;
        b.Size = tamano;
        b.BackColor = color;
        b.ForeColor = Color.White;
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.Cursor = Cursors.Hand;
    }

    public static Label Titulo(string texto, Point ubicacion) => new()
    {
        Text = texto,
        Font = new Font("Segoe UI", 16, FontStyle.Bold),
        ForeColor = AzulOscuro,
        Location = ubicacion,
        AutoSize = true
    };

    public static DataGridView Grid(Point ubicacion, Size tamano) => new()
    {
        Location = ubicacion,
        Size = tamano,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        RowHeadersVisible = false,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        BackgroundColor = Color.White
    };

    // Llena una tabla con los consumos (la columna Id queda oculta)
    public static void Llenar(DataGridView g, IEnumerable<Consumo> lista)
    {
        IList filas = lista.OrderByDescending(c => c.Fecha).Select(c => new
        {
            c.Id,
            Fecha = c.Fecha.ToString("dd/MM/yyyy HH:mm"),
            c.NumEmpleado,
            c.Empleado,
            c.Platillo,
            c.Categoria,
            c.Cantidad,
            Subtotal = c.Subtotal
        }).ToList();

        g.DataSource = filas;
        if (g.Columns["Id"] is { } id) id.Visible = false;
        if (g.Columns["NumEmpleado"] is { } ne) ne.HeaderText = "Núm. empleado";
        if (g.Columns["Subtotal"] is { } st) st.DefaultCellStyle.Format = "C2";
    }
}

// Validaciones de campo compartidas por varias pantallas
public static class Validar
{
    private static readonly Regex NombreRegex = new(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]{3,40}$");
    private static readonly Regex LoginRegex = new(@"^[A-Za-z0-9_]{3,15}$");
    private static readonly Regex NumEmpleadoRegex = new(@"^\d{4,6}$");   // de 4 a 6 dígitos
    private static readonly Regex ProductoRegex = new(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ0-9 ]{3,40}$");
    private static readonly Regex CategoriaRegex = new(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]{3,30}$");

    public static bool Empleado(string texto) => NombreRegex.IsMatch(texto.Trim());

    public static bool NumEmpleado(string texto) => NumEmpleadoRegex.IsMatch(texto.Trim());

    public static bool Cantidad(string texto, out int cantidad) =>
        int.TryParse(texto, out cantidad) && cantidad >= 1 && cantidad <= 20;

    public static bool Login(string texto) => LoginRegex.IsMatch(texto);

    // Contraseña nueva: de 6 a 20 caracteres, sin espacios
    public static bool Clave(string texto) =>
        texto.Length >= 6 && texto.Length <= 20 && !texto.Contains(' ');

    // ----- Inventario -----
    public static bool Producto(string texto) => ProductoRegex.IsMatch(texto.Trim());

    public static bool Categoria(string texto) => CategoriaRegex.IsMatch(texto.Trim());

    // Precio mayor a 0 y hasta 9999 (acepta punto o coma decimal según la configuración regional)
    public static bool Precio(string texto, out decimal precio) =>
        (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out precio)
         || decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out precio))
        && precio > 0 && precio <= 9999;

    public static bool Existencia(string texto, out int existencia) =>
        int.TryParse(texto, out existencia) && existencia >= 0 && existencia <= 9999;

    public static bool Surtido(string texto, out int cantidad) =>
        int.TryParse(texto, out cantidad) && cantidad >= 1 && cantidad <= 9999;
}
