namespace Comedor51;

public class FormUsuarios : Form
{
    private readonly string actual;   // usuario que tiene la sesión abierta

    private readonly TextBox txtLogin = new();
    private readonly TextBox txtNombre = new();
    private readonly ComboBox cmbRol = new();
    private readonly TextBox txtClave = new();
    private readonly TextBox txtConfirmar = new();
    private readonly DataGridView grid = Ui.Grid(new Point(20, 295), new Size(560, 150));

    public FormUsuarios(string usuarioActual)
    {
        actual = usuarioActual;

        Text = "Comedor industrial - Administrar usuarios";
        ClientSize = new Size(600, 520);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.WhiteSmoke;
        Font = new Font("Segoe UI", 10);

        var titulo = Ui.Titulo("Administrar usuarios", new Point(20, 15));

        var lblLogin = new Label { Text = "Usuario:", Location = new Point(20, 75), AutoSize = true };
        txtLogin.Location = new Point(190, 72);
        txtLogin.Width = 200;

        var lblNombre = new Label { Text = "Nombre completo:", Location = new Point(20, 115), AutoSize = true };
        txtNombre.Location = new Point(190, 112);
        txtNombre.Width = 250;

        var lblRol = new Label { Text = "Rol:", Location = new Point(20, 155), AutoSize = true };
        cmbRol.Location = new Point(190, 152);
        cmbRol.Width = 200;
        cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbRol.Items.AddRange(new object[] { "Cajero", "Administrador" });
        cmbRol.SelectedIndex = 0;

        var lblClave = new Label { Text = "Contraseña inicial:", Location = new Point(20, 195), AutoSize = true };
        txtClave.Location = new Point(190, 192);
        txtClave.Width = 200;
        txtClave.UseSystemPasswordChar = true;

        var lblConfirmar = new Label { Text = "Confirmar contraseña:", Location = new Point(20, 235), AutoSize = true };
        txtConfirmar.Location = new Point(190, 232);
        txtConfirmar.Width = 200;
        txtConfirmar.UseSystemPasswordChar = true;

        var btnCrear = new Button();
        Ui.Boton(btnCrear, "Crear usuario", new Point(410, 150), new Size(170, 42), Color.SeaGreen);
        btnCrear.Click += BtnCrear_Click;

        var btnEliminar = new Button();
        Ui.Boton(btnEliminar, "Eliminar seleccionado", new Point(20, 462), new Size(200, 40), Color.Firebrick);
        btnEliminar.Click += BtnEliminar_Click;

        var btnVolver = new Button();
        Ui.Boton(btnVolver, "Volver", new Point(450, 462), new Size(130, 40), Color.Gray);
        btnVolver.Click += (s, e) => Close();

        Controls.AddRange(new Control[]
        {
            titulo, lblLogin, txtLogin, lblNombre, txtNombre, lblRol, cmbRol,
            lblClave, txtClave, lblConfirmar, txtConfirmar, btnCrear,
            grid, btnEliminar, btnVolver
        });

        Cargar();
    }

    private void Cargar()
    {
        grid.DataSource = Datos.LeerUsuarios()
            .Select(u => new { Usuario = u.Login, Nombre = u.Nombre, Rol = u.Rol })
            .ToList();
    }

    private void BtnCrear_Click(object? sender, EventArgs e)
    {
        string login = txtLogin.Text.Trim();
        string nombre = txtNombre.Text.Trim();

        if (!Validar.Login(login))
        {
            Ui.Aviso("El usuario debe tener de 3 a 15 caracteres: letras, números o guion bajo (sin espacios).");
            txtLogin.Focus();
            return;
        }
        if (!Validar.Empleado(nombre))
        {
            Ui.Aviso("Escribe el nombre completo (solo letras, de 3 a 40 caracteres).");
            txtNombre.Focus();
            return;
        }
        if (cmbRol.SelectedItem is not string rol) { Ui.Aviso("Elige un rol."); return; }
        if (!Validar.Clave(txtClave.Text))
        {
            Ui.Aviso("La contraseña debe tener de 6 a 20 caracteres y no puede llevar espacios.");
            txtClave.Focus();
            return;
        }
        if (txtClave.Text != txtConfirmar.Text)
        {
            Ui.Aviso("Las contraseñas no coinciden.");
            txtConfirmar.Focus();
            return;
        }

        if (!Datos.CrearUsuario(login, txtClave.Text, rol, nombre))
        {
            Ui.Aviso("Ese usuario ya existe. Elige otro nombre de usuario.");
            txtLogin.Focus();
            return;
        }

        MessageBox.Show($"Usuario \"{login}\" creado como {rol}.\nPuede cambiar su contraseña desde su cuenta.",
            "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);

        txtLogin.Clear();
        txtNombre.Clear();
        txtClave.Clear();
        txtConfirmar.Clear();
        cmbRol.SelectedIndex = 0;
        Cargar();
    }

    private void BtnEliminar_Click(object? sender, EventArgs e)
    {
        string? login = grid.CurrentRow?.Cells["Usuario"].Value?.ToString();
        if (login is null) { Ui.Aviso("Selecciona un usuario de la tabla."); return; }

        if (login.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            Ui.Aviso("La cuenta \"admin\" no se puede eliminar.");
            return;
        }
        if (login.Equals(actual, StringComparison.OrdinalIgnoreCase))
        {
            Ui.Aviso("No puedes eliminar la cuenta con la que tienes la sesión abierta.");
            return;
        }

        var r = MessageBox.Show($"¿Eliminar al usuario \"{login}\"?", "Confirmar",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;

        Datos.EliminarUsuario(login);
        Cargar();
    }
}
