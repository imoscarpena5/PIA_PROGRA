namespace Comedor51;

public class FormActualizar : Form
{
    private readonly List<Consumo> todos = Datos.LeerConsumos();
    private Consumo? seleccionado;

    private readonly TextBox txtBuscar = new();
    private readonly TextBox txtNumero = new();
    private readonly TextBox txtEmpleado = new();
    private readonly ComboBox cmbPlatillo = new();
    private readonly TextBox txtCantidad = new();
    private readonly DataGridView grid = Ui.Grid(new Point(20, 105), new Size(740, 270));

    public FormActualizar()
    {
        Text = "Comedor industrial - Actualizar / eliminar";
        ClientSize = new Size(780, 590);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.WhiteSmoke;
        Font = new Font("Segoe UI", 10);

        var titulo = Ui.Titulo("Actualizar o eliminar consumos", new Point(20, 15));

        var lblBuscar = new Label { Text = "Buscar (núm., nombre o platillo):", Location = new Point(20, 65), AutoSize = true };
        txtBuscar.Location = new Point(250, 62);
        txtBuscar.Width = 200;

        var btnBuscar = new Button();
        Ui.Boton(btnBuscar, "Buscar", new Point(465, 58), new Size(95, 32), Ui.Azul);
        btnBuscar.Click += (s, e) => Buscar();
        AcceptButton = btnBuscar;

        var lblEdit = new Label
        {
            Text = "Selecciona un registro de la tabla y modifica sus datos:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(20, 390),
            AutoSize = true
        };

        var lblNumero = new Label { Text = "Núm. empleado:", Location = new Point(20, 430), AutoSize = true };
        txtNumero.Location = new Point(135, 427);
        txtNumero.Width = 80;
        txtNumero.MaxLength = 6;

        var lblEmpleado = new Label { Text = "Nombre:", Location = new Point(235, 430), AutoSize = true };
        txtEmpleado.Location = new Point(300, 427);
        txtEmpleado.Width = 220;

        var lblCantidad = new Label { Text = "Cantidad:", Location = new Point(545, 430), AutoSize = true };
        txtCantidad.Location = new Point(620, 427);
        txtCantidad.Width = 60;

        var lblPlatillo = new Label { Text = "Platillo:", Location = new Point(20, 475), AutoSize = true };
        cmbPlatillo.Location = new Point(135, 472);
        cmbPlatillo.Width = 250;
        cmbPlatillo.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPlatillo.DataSource = Datos.Menu;
        cmbPlatillo.DisplayMember = "Nombre";

        var btnActualizar = new Button();
        Ui.Boton(btnActualizar, "Actualizar registro", new Point(20, 525), new Size(190, 40), Color.SeaGreen);
        btnActualizar.Click += BtnActualizar_Click;

        var btnEliminar = new Button();
        Ui.Boton(btnEliminar, "Eliminar registro", new Point(225, 525), new Size(190, 40), Color.Firebrick);
        btnEliminar.Click += BtnEliminar_Click;

        var btnCerrar = new Button();
        Ui.Boton(btnCerrar, "Volver", new Point(630, 525), new Size(130, 40), Color.Gray);
        btnCerrar.Click += (s, e) => Close();

        Controls.AddRange(new Control[]
        {
            titulo, lblBuscar, txtBuscar, btnBuscar, grid, lblEdit,
            lblNumero, txtNumero, lblEmpleado, txtEmpleado, lblCantidad, txtCantidad,
            lblPlatillo, cmbPlatillo, btnActualizar, btnEliminar, btnCerrar
        });

        Buscar();
        grid.SelectionChanged += (s, e) => Seleccionar();
        Seleccionar();
    }

    private void Buscar()
    {
        string texto = txtBuscar.Text.Trim();
        if (texto.Length > 40)
        {
            Ui.Aviso("El texto a buscar no puede pasar de 40 caracteres.");
            return;
        }

        var lista = todos.Where(c => texto.Length == 0
            || c.NumEmpleado.Contains(texto, StringComparison.OrdinalIgnoreCase)
            || c.Empleado.Contains(texto, StringComparison.OrdinalIgnoreCase)
            || c.Platillo.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();

        Ui.Llenar(grid, lista);
        Seleccionar();
    }

    // Pasa los datos del registro seleccionado a los campos de edición
    private void Seleccionar()
    {
        string? id = grid.CurrentRow?.Cells["Id"].Value?.ToString();
        seleccionado = todos.FirstOrDefault(c => c.Id == id);

        if (seleccionado is null)
        {
            txtNumero.Clear();
            txtEmpleado.Clear();
            txtCantidad.Clear();
            return;
        }

        txtNumero.Text = seleccionado.NumEmpleado;
        txtEmpleado.Text = seleccionado.Empleado;
        txtCantidad.Text = seleccionado.Cantidad.ToString();
        cmbPlatillo.SelectedIndex = Datos.Menu.FindIndex(m => m.Nombre == seleccionado.Platillo);
    }

    private void BtnActualizar_Click(object? sender, EventArgs e)
    {
        if (seleccionado is null) { Ui.Aviso("Selecciona un registro de la tabla."); return; }

        string num = txtNumero.Text.Trim();
        string nombre = txtEmpleado.Text.Trim();

        if (!Validar.NumEmpleado(num))
        {
            Ui.Aviso("El número de empleado debe tener de 4 a 6 dígitos (solo números).");
            txtNumero.Focus();
            return;
        }
        if (!Validar.Empleado(nombre))
        {
            Ui.Aviso("Escribe el nombre del empleado (solo letras, de 3 a 40 caracteres).");
            txtEmpleado.Focus();
            return;
        }

        // Un mismo número de empleado no puede tener dos nombres distintos
        var otro = todos.FirstOrDefault(c => c.Id != seleccionado.Id && c.NumEmpleado == num
            && !c.Empleado.Equals(nombre, StringComparison.OrdinalIgnoreCase));
        if (otro is not null)
        {
            Ui.Aviso($"El número {num} ya está registrado a nombre de {otro.Empleado}.");
            txtNumero.Focus();
            return;
        }

        if (cmbPlatillo.SelectedItem is not Platillo p) { Ui.Aviso("Elige un platillo."); return; }
        if (!Validar.Cantidad(txtCantidad.Text, out int cantidad))
        {
            Ui.Aviso("La cantidad debe ser un número entero entre 1 y 20.");
            txtCantidad.Focus();
            return;
        }

        // Inventario: se regresa lo anterior y se descuenta lo nuevo
        var anterior = Datos.BuscarPlatillo(seleccionado.Platillo);
        int disponible = p.Existencia
            + (anterior is not null && anterior.Nombre == p.Nombre ? seleccionado.Cantidad : 0);
        if (cantidad > disponible)
        {
            Ui.Aviso($"Solo hay {disponible} porción(es) disponibles de {p.Nombre} en el inventario.");
            txtCantidad.Focus();
            return;
        }
        Datos.AjustarExistencia(seleccionado.Platillo, seleccionado.Cantidad);
        Datos.AjustarExistencia(p.Nombre, -cantidad);

        int i = todos.FindIndex(c => c.Id == seleccionado.Id);
        todos[i] = seleccionado with
        {
            NumEmpleado = num,
            Empleado = nombre,
            Platillo = p.Nombre,
            Categoria = p.Categoria,
            Cantidad = cantidad,
            Precio = p.Precio
        };

        Datos.Sobrescribir(todos);
        MessageBox.Show("Registro actualizado.", "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Buscar();
    }

    private void BtnEliminar_Click(object? sender, EventArgs e)
    {
        if (seleccionado is null) { Ui.Aviso("Selecciona un registro de la tabla."); return; }

        var r = MessageBox.Show(
            $"¿Eliminar el registro de {seleccionado.Empleado} ({seleccionado.Platillo})?\nSe regresarán {seleccionado.Cantidad} porción(es) al inventario.",
            "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;

        string id = seleccionado.Id;
        Datos.AjustarExistencia(seleccionado.Platillo, seleccionado.Cantidad);
        todos.RemoveAll(c => c.Id == id);
        Datos.Sobrescribir(todos);
        Buscar();
    }
}
