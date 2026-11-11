namespace Comedor51;

public class FormInventario : Form
{
    private const int ExistenciaBaja = 5;

    private readonly DataGridView grid = Ui.Grid(new Point(20, 65), new Size(740, 250));
    private readonly Label lblResumen = new();
    private readonly TextBox txtNombre = new();
    private readonly ComboBox cmbCategoria = new();
    private readonly TextBox txtPrecio = new();
    private readonly TextBox txtExistencia = new();
    private readonly TextBox txtSurtir = new();
    private Platillo? seleccionado;

    public FormInventario()
    {
        Text = "Comedor industrial - Inventario y menú";
        ClientSize = new Size(780, 540);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.WhiteSmoke;
        Font = new Font("Segoe UI", 10);

        var titulo = Ui.Titulo("Inventario y menú", new Point(20, 15));

        lblResumen.Location = new Point(20, 325);
        lblResumen.Size = new Size(740, 25);
        lblResumen.Font = new Font("Segoe UI", 10, FontStyle.Bold);

        var lblNombre = new Label { Text = "Producto:", Location = new Point(20, 375), AutoSize = true };
        txtNombre.Location = new Point(95, 372);
        txtNombre.Width = 190;

        var lblCategoria = new Label { Text = "Categoría:", Location = new Point(300, 375), AutoSize = true };
        cmbCategoria.Location = new Point(385, 372);
        cmbCategoria.Width = 150;
        cmbCategoria.DropDownStyle = ComboBoxStyle.DropDown;   // se puede escribir una categoría nueva

        var lblPrecio = new Label { Text = "Precio:", Location = new Point(550, 375), AutoSize = true };
        txtPrecio.Location = new Point(610, 372);
        txtPrecio.Width = 70;

        var lblExistencia = new Label { Text = "Existencia:", Location = new Point(20, 420), AutoSize = true };
        txtExistencia.Location = new Point(110, 417);
        txtExistencia.Width = 70;

        var lblSurtir = new Label { Text = "Surtir (sumar):", Location = new Point(220, 420), AutoSize = true };
        txtSurtir.Location = new Point(325, 417);
        txtSurtir.Width = 60;

        var btnSurtir = new Button();
        Ui.Boton(btnSurtir, "Sumar al inventario", new Point(400, 412), new Size(180, 36), Color.Teal);
        btnSurtir.Click += BtnSurtir_Click;

        var btnAgregar = new Button();
        Ui.Boton(btnAgregar, "Agregar producto", new Point(20, 475), new Size(180, 40), Color.SeaGreen);
        btnAgregar.Click += BtnAgregar_Click;

        var btnActualizar = new Button();
        Ui.Boton(btnActualizar, "Actualizar seleccionado", new Point(215, 475), new Size(200, 40), Color.DarkOrange);
        btnActualizar.Click += BtnActualizar_Click;

        var btnEliminar = new Button();
        Ui.Boton(btnEliminar, "Eliminar seleccionado", new Point(430, 475), new Size(190, 40), Color.Firebrick);
        btnEliminar.Click += BtnEliminar_Click;

        var btnVolver = new Button();
        Ui.Boton(btnVolver, "Volver", new Point(635, 475), new Size(125, 40), Color.Gray);
        btnVolver.Click += (s, e) => Close();

        Controls.AddRange(new Control[]
        {
            titulo, grid, lblResumen,
            lblNombre, txtNombre, lblCategoria, cmbCategoria, lblPrecio, txtPrecio,
            lblExistencia, txtExistencia, lblSurtir, txtSurtir, btnSurtir,
            btnAgregar, btnActualizar, btnEliminar, btnVolver
        });

        Cargar();
        grid.SelectionChanged += (s, e) => Seleccionar();
        Seleccionar();
    }

    // Llena la tabla; las existencias bajas se ven en rojo
    private void Cargar(string? seleccionar = null)
    {
        grid.DataSource = Datos.Menu
            .OrderBy(m => m.Categoria).ThenBy(m => m.Nombre)
            .Select(m => new
            {
                Producto = m.Nombre,
                Categoria = m.Categoria,
                Precio = m.Precio,
                Existencia = m.Existencia
            }).ToList();

        if (grid.Columns["Precio"] is { } precio) precio.DefaultCellStyle.Format = "C2";

        foreach (DataGridViewRow fila in grid.Rows)
        {
            bool bajo = fila.Cells["Existencia"].Value is int exist && exist <= ExistenciaBaja;
            fila.DefaultCellStyle.ForeColor = bajo ? Color.Firebrick : Color.Black;

            if (seleccionar is not null && fila.Cells["Producto"].Value?.ToString() == seleccionar)
            {
                grid.CurrentCell = fila.Cells["Producto"];
                fila.Selected = true;
            }
        }

        // Categorías disponibles en la lista desplegable
        string actual = cmbCategoria.Text;
        cmbCategoria.Items.Clear();
        foreach (var c in Datos.Categorias()) cmbCategoria.Items.Add(c);
        cmbCategoria.Text = actual;

        // ---------- Cálculos del inventario ----------
        decimal valor = Datos.Menu.Sum(m => m.Precio * m.Existencia);
        int bajos = Datos.Menu.Count(m => m.Existencia <= ExistenciaBaja);
        lblResumen.Text =
            $"Productos: {Datos.Menu.Count}     Valor del inventario (a precio de venta): {valor:C}     " +
            $"Existencia baja ({ExistenciaBaja} o menos): {bajos}";
    }

    private void Seleccionar()
    {
        string? nombre = grid.CurrentRow?.Cells["Producto"].Value?.ToString();
        seleccionado = nombre is null ? null : Datos.BuscarPlatillo(nombre);

        if (seleccionado is null)
        {
            txtNombre.Clear();
            cmbCategoria.Text = "";
            txtPrecio.Clear();
            txtExistencia.Clear();
            return;
        }

        txtNombre.Text = seleccionado.Nombre;
        cmbCategoria.Text = seleccionado.Categoria;
        txtPrecio.Text = seleccionado.Precio.ToString("0.00");
        txtExistencia.Text = seleccionado.Existencia.ToString();
    }

    // ---------- Validaciones de campo ----------

    private bool LeerCampos(out string nombre, out string categoria, out decimal precio, out int existencia)
    {
        nombre = txtNombre.Text.Trim();
        categoria = cmbCategoria.Text.Trim();
        precio = 0;
        existencia = 0;

        if (!Validar.Producto(nombre))
        {
            Ui.Aviso("El nombre del producto debe tener de 3 a 40 caracteres (letras, números y espacios).");
            txtNombre.Focus();
            return false;
        }
        if (!Validar.Categoria(categoria))
        {
            Ui.Aviso("La categoría debe tener de 3 a 30 letras (puedes escribir una nueva).");
            cmbCategoria.Focus();
            return false;
        }
        if (!Validar.Precio(txtPrecio.Text, out precio))
        {
            Ui.Aviso("El precio debe ser un número mayor a 0 y hasta 9999.");
            txtPrecio.Focus();
            return false;
        }
        if (!Validar.Existencia(txtExistencia.Text, out existencia))
        {
            Ui.Aviso("La existencia debe ser un número entero entre 0 y 9999.");
            txtExistencia.Focus();
            return false;
        }
        return true;
    }

    // ---------- Eventos ----------

    private void BtnAgregar_Click(object? sender, EventArgs e)
    {
        if (!LeerCampos(out string nombre, out string categoria, out decimal precio, out int existencia)) return;

        if (Datos.BuscarPlatillo(nombre) is not null)
        {
            Ui.Aviso("Ya existe un producto con ese nombre. Usa \"Actualizar\" o \"Sumar al inventario\".");
            txtNombre.Focus();
            return;
        }

        Datos.Menu.Add(new Platillo { Nombre = nombre, Categoria = categoria, Precio = precio, Existencia = existencia });
        Datos.GuardarMenu();
        Cargar(nombre);
        MessageBox.Show($"Producto \"{nombre}\" agregado al inventario.", "Listo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnActualizar_Click(object? sender, EventArgs e)
    {
        if (seleccionado is null) { Ui.Aviso("Selecciona un producto de la tabla."); return; }
        if (!LeerCampos(out string nombre, out string categoria, out decimal precio, out int existencia)) return;

        if (!nombre.Equals(seleccionado.Nombre, StringComparison.OrdinalIgnoreCase))
        {
            Ui.Aviso("El nombre no se puede cambiar al actualizar. Para renombrar, agrega el producto nuevo y elimina el anterior.");
            txtNombre.Text = seleccionado.Nombre;
            return;
        }

        string clave = seleccionado.Nombre;
        seleccionado.Categoria = categoria;
        seleccionado.Precio = precio;
        seleccionado.Existencia = existencia;
        Datos.GuardarMenu();
        Cargar(clave);
        MessageBox.Show("Producto actualizado.", "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnSurtir_Click(object? sender, EventArgs e)
    {
        if (seleccionado is null) { Ui.Aviso("Selecciona un producto de la tabla."); return; }
        if (!Validar.Surtido(txtSurtir.Text, out int cantidad))
        {
            Ui.Aviso("La cantidad a surtir debe ser un número entero entre 1 y 9999.");
            txtSurtir.Focus();
            return;
        }
        if (seleccionado.Existencia + cantidad > 9999)
        {
            Ui.Aviso("La existencia total no puede pasar de 9999.");
            return;
        }

        string clave = seleccionado.Nombre;
        seleccionado.Existencia += cantidad;
        Datos.GuardarMenu();
        txtSurtir.Clear();
        Cargar(clave);
    }

    private void BtnEliminar_Click(object? sender, EventArgs e)
    {
        if (seleccionado is null) { Ui.Aviso("Selecciona un producto de la tabla."); return; }

        var r = MessageBox.Show(
            $"¿Quitar \"{seleccionado.Nombre}\" del menú?\nLos consumos ya registrados no se modifican.",
            "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;

        Datos.Menu.Remove(seleccionado);
        Datos.GuardarMenu();
        Cargar();
    }
}
