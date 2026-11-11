namespace Comedor51;

public class FormLogin : Form
{
    private readonly TextBox txtUsuario = new();
    private readonly TextBox txtClave = new();
    private readonly Button btnEntrar = new();
    private int intentos = 0;

    public FormLogin()
    {
        Text = "Comedor industrial - Inicio de sesión";
        ClientSize = new Size(380, 280);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.WhiteSmoke;
        Font = new Font("Segoe UI", 10);

        var titulo = Ui.Titulo("Iniciar sesión", new Point(105, 25));

        var lblUsuario = new Label { Text = "Usuario:", Location = new Point(40, 95), AutoSize = true };
        txtUsuario.Location = new Point(140, 92);
        txtUsuario.Width = 190;

        var lblClave = new Label { Text = "Contraseña:", Location = new Point(40, 140), AutoSize = true };
        txtClave.Location = new Point(140, 137);
        txtClave.Width = 190;
        txtClave.UseSystemPasswordChar = true;

        Ui.Boton(btnEntrar, "Entrar", new Point(140, 195), new Size(120, 38), Ui.Azul);
        btnEntrar.Click += BtnEntrar_Click;
        AcceptButton = btnEntrar;   // Enter = clic en Entrar

        Controls.AddRange(new Control[] { titulo, lblUsuario, txtUsuario, lblClave, txtClave, btnEntrar });
    }

    private void BtnEntrar_Click(object? sender, EventArgs e)
    {
        string login = txtUsuario.Text.Trim();

        // Validaciones de campo
        if (login.Length < 3)
        {
            Ui.Aviso("Escribe tu usuario (mínimo 3 caracteres).");
            txtUsuario.Focus();
            return;
        }
        if (txtClave.Text.Length < 4)
        {
            Ui.Aviso("La contraseña debe tener al menos 4 caracteres.");
            txtClave.Focus();
            return;
        }

        Usuario? usuario = Datos.ValidarUsuario(login, txtClave.Text);
        txtClave.Clear();

        if (usuario is null)
        {
            intentos++;
            if (intentos >= 3)
            {
                MessageBox.Show("Demasiados intentos fallidos. La aplicación se cerrará.",
                    "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            Ui.Aviso($"Usuario o contraseña incorrectos. Intento {intentos} de 3.");
            return;
        }

        intentos = 0;
        Hide();
        using (var menu = new FormMenu(usuario))
        {
            menu.ShowDialog();
        }
        txtUsuario.Clear();
        Show();
        txtUsuario.Focus();
    }
}
