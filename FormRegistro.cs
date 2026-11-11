namespace Comedor51;

public class FormRegistro : Form
{
    private readonly List<Consumo> pendientes = new();

    private readonly TextBox txtNumero = new();
    private readonly TextBox txtEmpleado = new();
    private readonly ComboBox cmbPlatillo = new();
    private readonly TextBox txtCantidad = new();
    private readonly Label lblDisponibles = new();
    private readonly ListBox lstPendientes = new();
    private readonly Label lblTotal = new();

    public FormRegistro(Usuario u)
    {
        Text = "Comedor industrial - Guardar / registrar consumos";
        ClientSize = new Size(560, 540);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.WhiteSmoke;
        Font = new Font("Segoe UI", 10);

        var titulo = Ui.Titulo("Registro de consumos", new Point(20, 15));
        var lblSesion = new Label
        {
            Text = $"Sesión: {u.Nombre} ({u.Rol})",
            ForeColor = Color.DimGray,
            Location = new Point(20, 52),
            AutoSize = true
        };

        var lblNumero = new Label { Text = "Núm. de empleado:", Location = new Point(20, 95), AutoSize = true };
        txtNumero.Location = new Point(170, 92);
        txtNumero.Width = 110;
        txtNumero.MaxLength = 6;
        // Si el número ya se había registrado, se completa solo el nombre
        txtNumero.Leave += (s, e) =>
        {
            string num = txtNumero.Text.Trim();
            if (Validar.NumEmpleado(num) && txtEmpleado.Text.Trim().Length == 0)
            {
                string? nombre = Datos.NombrePorNumero(num, pendientes);
                if (nombre is not null) txtEmpleado.Text = nombre;
            }
        };

        var lblEmpleado = new Label { Text = "Nombre del empleado:", Location = new Point(20, 135), AutoSize = true };
        txtEmpleado.Location = new Point(170, 132);
        txtEmpleado.Width = 220;

        var lblPlatillo = new Label { Text = "Platillo:", Location = new Point(20, 175), AutoSize = true };
        cmbPlatillo.Location = new Point(170, 172);
        cmbPlatillo.Width = 220;
        cmbPlatillo.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPlatillo.DataSource = Datos.Menu;
        cmbPlatillo.DisplayMember = "Nombre";
        cmbPlatillo.SelectedIndexChanged += (s, e) => ActualizarDisponibles();

        var lblCantidad = new Label { Text = "Cantidad:", Location = new Point(20, 215), AutoSize = true };
        txtCantidad.Location = new Point(170, 212);
        txtCantidad.Width = 80;

        lblDisponibles.Location = new Point(270, 216);
        lblDisponibles.AutoSize = true;

        var btnAgregar = new Button();
        Ui.Boton(btnAgregar, "Agregar", new Point(410, 170), new Size(130, 40), Ui.Azul);
        btnAgregar.Click += BtnAgregar_Click;

        lstPendientes.Location = new Point(20, 260);
        lstPendientes.Size = new Size(520, 150);

        lblTotal.Text = "Total por guardar: $0.00";
        lblTotal.Font = new Font("Segoe UI", 13, FontStyle.Bold);
        lblTotal.Location = new Point(20, 425);
        lblTotal.AutoSize = true;

        var btnGuardar = new Button();
        Ui.Boton(btnGuardar, "Guardar consumos", new Point(20, 475), new Size(200, 38), Color.SeaGreen);
        btnGuardar.Click += BtnGuardar_Click;

        var btnQuitar = new Button();
        Ui.Boton(btnQuitar, "Quitar seleccionado", new Point(235, 475), new Size(160, 38), Color.DarkOrange);
        btnQuitar.Click += BtnQuitar_Click;

        var btnVolver = new Button();
        Ui.Boton(btnVolver, "Volver", new Point(410, 475), new Size(130, 38), Color.Gray);
        btnVolver.Click += (s, e) => Close();

        Controls.AddRange(new Control[]
        {
            titulo, lblSesion, lblNumero, txtNumero, lblEmpleado, txtEmpleado,
            lblPlatillo, cmbPlatillo, lblCantidad, txtCantidad, lblDisponibles, btnAgregar,
            lstPendientes, lblTotal, btnGuardar, btnQuitar, btnVolver
        });

        ActualizarDisponibles();
    }

    // Porciones que quedan en el inventario, descontando lo que ya está en la lista por guardar
    private int Disponibles(Platillo p) =>
        p.Existencia - pendientes.Where(c => c.Platillo == p.Nombre).Sum(c => c.Cantidad);

    private void ActualizarDisponibles()
    {
        if (cmbPlatillo.SelectedItem is Platillo p)
        {
            int disp = Disponibles(p);
            lblDisponibles.Text = $"Disponibles: {Math.Max(disp, 0)}";
            lblDisponibles.ForeColor = disp <= 5 ? Color.Firebrick : Color.DimGray;
        }
        else
        {
            lblDisponibles.Text = "";
        }
    }

    // ---------- Eventos ----------

    private void BtnAgregar_Click(object? sender, EventArgs e)
    {
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
        string? existente = Datos.NombrePorNumero(num, pendientes);
        if (existente is not null && !existente.Equals(nombre, StringComparison.OrdinalIgnoreCase))
        {
            Ui.Aviso($"El número {num} ya está registrado a nombre de {existente}.");
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

        // Inventario: no se puede registrar más de lo que hay
        int disp = Disponibles(p);
        if (cantidad > disp)
        {
            Ui.Aviso($"Solo quedan {Math.Max(disp, 0)} porción(es) de {p.Nombre} en el inventario.");
            txtCantidad.Focus();
            return;
        }

        pendientes.Add(new Consumo(Consumo.NuevoId(), DateTime.Now, num, nombre,
            p.Nombre, p.Categoria, cantidad, p.Precio));
        txtCantidad.Clear();
        Refrescar();
    }

    private void BtnQuitar_Click(object? sender, EventArgs e)
    {
        int i = lstPendientes.SelectedIndex;
        if (i < 0) { Ui.Aviso("Selecciona de la lista el consumo que quieres quitar."); return; }
        pendientes.RemoveAt(i);
        Refrescar();
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        if (pendientes.Count == 0)
        {
            Ui.Aviso("Agrega al menos un consumo antes de guardar.");
            return;
        }

        decimal total = pendientes.Sum(c => c.Subtotal);
        int registros = pendientes.Count;

        Datos.GuardarConsumos(pendientes);

        // Descuenta del inventario lo que se consumió
        foreach (var g in pendientes.GroupBy(c => c.Platillo))
            Datos.AjustarExistencia(g.Key, -g.Sum(c => c.Cantidad));

        MessageBox.Show($"Se guardaron {registros} registro(s).\nTotal: {total:C}",
            "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);

        pendientes.Clear();
        txtNumero.Clear();
        txtEmpleado.Clear();
        Refrescar();
    }

    // ---------- Cálculo del total ----------

    private void Refrescar()
    {
        lstPendientes.Items.Clear();
        foreach (var c in pendientes)
            lstPendientes.Items.Add($"{c.NumEmpleado} - {c.Empleado}: {c.Cantidad} x {c.Platillo} ({c.Precio:C}) = {c.Subtotal:C}");

        lblTotal.Text = $"Total por guardar: {pendientes.Sum(c => c.Subtotal):C}";
        ActualizarDisponibles();
    }
}
