using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Comedor51;

public class FormExportar : Form
{
    private readonly ComboBox cmbCategoria = new();
    private readonly ComboBox cmbFormato = new();
    private readonly Label lblInfo = new();

    public FormExportar()
    {
        Text = "Comedor industrial - Exportar datos";
        ClientSize = new Size(460, 320);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.WhiteSmoke;
        Font = new Font("Segoe UI", 10);

        var titulo = Ui.Titulo("Exportar datos", new Point(20, 15));

        var lblCategoria = new Label { Text = "Categoría:", Location = new Point(40, 80), AutoSize = true };
        cmbCategoria.Location = new Point(150, 77);
        cmbCategoria.Width = 250;
        cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCategoria.Items.Add("Todas");
        foreach (var cat in Datos.Categorias())
            cmbCategoria.Items.Add(cat);
        cmbCategoria.SelectedIndex = 0;
        cmbCategoria.SelectedIndexChanged += (s, e) => ActualizarInfo();

        var lblFormato = new Label { Text = "Formato:", Location = new Point(40, 130), AutoSize = true };
        cmbFormato.Location = new Point(150, 127);
        cmbFormato.Width = 250;
        cmbFormato.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFormato.Items.AddRange(new object[] { "CSV (Excel)", "JSON" });
        cmbFormato.SelectedIndex = 0;

        lblInfo.Location = new Point(40, 180);
        lblInfo.AutoSize = true;
        lblInfo.ForeColor = Color.DimGray;

        var btnExportar = new Button();
        Ui.Boton(btnExportar, "Exportar", new Point(40, 240), new Size(180, 40), Ui.AzulOscuro);
        btnExportar.Click += BtnExportar_Click;

        var btnVolver = new Button();
        Ui.Boton(btnVolver, "Volver", new Point(240, 240), new Size(160, 40), Color.Gray);
        btnVolver.Click += (s, e) => Close();

        Controls.AddRange(new Control[]
        {
            titulo, lblCategoria, cmbCategoria, lblFormato, cmbFormato, lblInfo, btnExportar, btnVolver
        });

        ActualizarInfo();
    }

    private List<Consumo> Filtrados()
    {
        string cat = cmbCategoria.SelectedItem?.ToString() ?? "Todas";
        return Datos.LeerConsumos()
            .Where(c => cat == "Todas" || c.Categoria == cat)
            .OrderBy(c => c.Fecha)
            .ToList();
    }

    private void ActualizarInfo()
    {
        var lista = Filtrados();
        lblInfo.Text = $"Registros listos para exportar: {lista.Count}   (Total: {lista.Sum(c => c.Subtotal):C})";
    }

    private void BtnExportar_Click(object? sender, EventArgs e)
    {
        var lista = Filtrados();
        if (lista.Count == 0)
        {
            Ui.Aviso("No hay registros para exportar con esa categoría.");
            return;
        }

        bool esCsv = cmbFormato.SelectedIndex == 0;

        using var dlg = new SaveFileDialog
        {
            Title = "Guardar archivo exportado",
            Filter = esCsv ? "Archivo CSV (*.csv)|*.csv" : "Archivo JSON (*.json)|*.json",
            FileName = $"consumos_{DateTime.Now:yyyyMMdd_HHmm}"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            if (esCsv) ExportarCsv(lista, dlg.FileName);
            else ExportarJson(lista, dlg.FileName);

            MessageBox.Show($"Se exportaron {lista.Count} registro(s).\n\n{dlg.FileName}",
                "Exportación lista", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (IOException ex)
        {
            MessageBox.Show("No se pudo guardar el archivo (¿está abierto en otro programa?).\n\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string Comillas(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";

    private static void ExportarCsv(List<Consumo> lista, string ruta)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Fecha,NumEmpleado,Empleado,Platillo,Categoria,Cantidad,Precio,Subtotal");
        foreach (var c in lista)
        {
            sb.AppendLine(string.Join(",",
                c.Id,
                c.Fecha.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                Comillas(c.NumEmpleado), Comillas(c.Empleado), Comillas(c.Platillo), Comillas(c.Categoria),
                c.Cantidad,
                c.Precio.ToString(CultureInfo.InvariantCulture),
                c.Subtotal.ToString(CultureInfo.InvariantCulture)));
        }
        File.WriteAllText(ruta, sb.ToString(), new UTF8Encoding(true));   // con BOM para que Excel lea los acentos
    }

    private static void ExportarJson(List<Consumo> lista, string ruta)
    {
        var datos = lista.Select(c => new
        {
            c.Id,
            Fecha = c.Fecha.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            c.NumEmpleado,
            c.Empleado,
            c.Platillo,
            c.Categoria,
            c.Cantidad,
            c.Precio,
            c.Subtotal
        });

        var opciones = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        File.WriteAllText(ruta, JsonSerializer.Serialize(datos, opciones), Encoding.UTF8);
    }
}
