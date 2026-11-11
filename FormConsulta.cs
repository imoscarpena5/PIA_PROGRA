namespace Comedor51;

public class FormConsulta : Form
{
    private readonly TextBox txtBuscar = new();
    private readonly ComboBox cmbCategoria = new();
    private readonly DataGridView grid = Ui.Grid(new Point(20, 105), new Size(740, 310));
    private readonly Label lblResumen = new();
    private List<Consumo> todos = new();

    public FormConsulta()
    {
        Text = "Comedor industrial - Buscar / consultar";
        ClientSize = new Size(780, 560);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.WhiteSmoke;
        Font = new Font("Segoe UI", 10);

        var titulo = Ui.Titulo("Consultar consumos", new Point(20, 15));

        var lblBuscar = new Label { Text = "Buscar (núm., nombre o platillo):", Location = new Point(20, 65), AutoSize = true };
        txtBuscar.Location = new Point(250, 62);
        txtBuscar.Width = 170;

        var lblCategoria = new Label { Text = "Categoría:", Location = new Point(435, 65), AutoSize = true };
        cmbCategoria.Location = new Point(515, 62);
        cmbCategoria.Width = 135;
        cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCategoria.Items.Add("Todas");
        foreach (var cat in Datos.Categorias())
            cmbCategoria.Items.Add(cat);
        cmbCategoria.SelectedIndex = 0;
        cmbCategoria.SelectedIndexChanged += (s, e) => Buscar();

        var btnBuscar = new Button();
        Ui.Boton(btnBuscar, "Buscar", new Point(665, 58), new Size(95, 32), Ui.Azul);
        btnBuscar.Click += (s, e) => Buscar();
        AcceptButton = btnBuscar;

        lblResumen.Location = new Point(20, 430);
        lblResumen.Size = new Size(740, 110);
        lblResumen.Font = new Font("Segoe UI", 11, FontStyle.Bold);

        Controls.AddRange(new Control[] { titulo, lblBuscar, txtBuscar, lblCategoria, cmbCategoria, btnBuscar, grid, lblResumen });

        todos = Datos.LeerConsumos();
        Buscar();
    }

    private void Buscar()
    {
        string texto = txtBuscar.Text.Trim();
        if (texto.Length > 40)
        {
            Ui.Aviso("El texto a buscar no puede pasar de 40 caracteres.");
            return;
        }

        string categoria = cmbCategoria.SelectedItem?.ToString() ?? "Todas";

        var filtrados = todos.Where(c =>
            (categoria == "Todas" || c.Categoria == categoria) &&
            (texto.Length == 0
                || c.NumEmpleado.Contains(texto, StringComparison.OrdinalIgnoreCase)
                || c.Empleado.Contains(texto, StringComparison.OrdinalIgnoreCase)
                || c.Platillo.Contains(texto, StringComparison.OrdinalIgnoreCase))).ToList();

        Ui.Llenar(grid, filtrados);

        // ---------- Cálculos matemáticos ----------
        if (filtrados.Count == 0)
        {
            lblResumen.Text = "Sin registros con esos criterios.";
            return;
        }

        int registros = filtrados.Count;
        int platillos = filtrados.Sum(c => c.Cantidad);
        int empleados = filtrados.Select(c => c.NumEmpleado).Distinct().Count();
        decimal total = filtrados.Sum(c => c.Subtotal);
        decimal promedio = total / registros;

        var mejor = filtrados
            .GroupBy(c => c.Categoria)
            .Select(g => new { Categoria = g.Key, Total = g.Sum(c => c.Subtotal) })
            .OrderByDescending(x => x.Total)
            .First();

        lblResumen.Text =
            $"Registros: {registros}     Platillos servidos: {platillos}     Empleados distintos: {empleados}\n" +
            $"Total: {total:C}     Promedio por registro: {promedio:C}\n" +
            $"Categoría con más ventas: {mejor.Categoria} ({mejor.Total:C})";
    }
}
