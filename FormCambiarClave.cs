namespace Comedor51;

public class FormCambiarClave : Form
{
    private readonly Usuario usuario;

    private readonly TextBox txtActual = new();
    private readonly TextBox txtNueva = new();
    private readonly TextBox txtConfirmar = new();

    public FormCambiarClave(Usuario u)
    {
        usuario = u;

        Text = "Comedor industrial - Cambiar contraseña";
        ClientSize = new Size(440, 320);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.WhiteSmoke;
        Font = new Font("Segoe UI", 10);

        var titulo = Ui.Titulo("Cambiar contraseña", new Point(20, 15));
        var lblUsuario = new Label
        {
            Text = $"Cuenta: {u.Login}",
            ForeColor = Color.DimGray,
            Location = new Point(20, 52),
            AutoSize = true
        };

        var lblActual = new Label { Text = "Contraseña actual:", Location = new Point(20, 95), AutoSize = true };
        txtActual.Location = new Point(220, 92);
        txtActual.Width = 190;
        txtActual.UseSystemPasswordChar = true;

        var lblNueva = new Label { Text = "Contraseña nueva:", Location = new Point(20, 140), AutoSize = true };
        txtNueva.Location = new Point(220, 137);
        txtNueva.Width = 190;
        txtNueva.UseSystemPasswordChar = true;

        var lblConfirmar = new Label { Text = "Confirmar contraseña nueva:", Location = new Point(20, 185), AutoSize = true };
        txtConfirmar.Location = new Point(220, 182);
        txtConfirmar.Width = 190;
        txtConfirmar.UseSystemPasswordChar = true;

        var btnCambiar = new Button();
        Ui.Boton(btnCambiar, "Cambiar contraseña", new Point(20, 245), new Size(200, 42), Color.Teal);
        btnCambiar.Click += BtnCambiar_Click;
        AcceptButton = btnCambiar;

        var btnVolver = new Button();
        Ui.Boton(btnVolver, "Volver", new Point(280, 245), new Size(130, 42), Color.Gray);
        btnVolver.Click += (s, e) => Close();

        Controls.AddRange(new Control[]
        {
            titulo, lblUsuario, lblActual, txtActual, lblNueva, txtNueva,
            lblConfirmar, txtConfirmar, btnCambiar, btnVolver
        });
    }

    private void BtnCambiar_Click(object? sender, EventArgs e)
    {
        if (txtActual.Text.Length == 0)
        {
            Ui.Aviso("Escribe tu contraseña actual.");
            txtActual.Focus();
            return;
        }
        if (!Validar.Clave(txtNueva.Text))
        {
            Ui.Aviso("La contraseña nueva debe tener de 6 a 20 caracteres y no puede llevar espacios.");
            txtNueva.Focus();
            return;
        }
        if (txtNueva.Text == txtActual.Text)
        {
            Ui.Aviso("La contraseña nueva debe ser distinta a la actual.");
            txtNueva.Focus();
            return;
        }
        if (txtNueva.Text != txtConfirmar.Text)
        {
            Ui.Aviso("La confirmación no coincide con la contraseña nueva.");
            txtConfirmar.Focus();
            return;
        }

        if (!Datos.CambiarClave(usuario.Login, txtActual.Text, txtNueva.Text))
        {
            Ui.Aviso("La contraseña actual no es correcta.");
            txtActual.Clear();
            txtActual.Focus();
            return;
        }

        MessageBox.Show("Tu contraseña se cambió correctamente. Úsala la próxima vez que inicies sesión.",
            "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }
}
