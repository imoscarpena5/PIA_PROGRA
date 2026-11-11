namespace Comedor51;

public class FormMenu : Form
{
    public FormMenu(Usuario u)
    {
        Text = "Comedor industrial - Menú principal";
        ClientSize = new Size(420, 600);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.WhiteSmoke;
        Font = new Font("Segoe UI", 10);

        bool admin = u.Rol == "Administrador";

        Controls.Add(Ui.Titulo("Menú principal", new Point(20, 15)));
        Controls.Add(new Label
        {
            Text = $"Sesión: {u.Nombre} ({u.Rol})",
            ForeColor = Color.DimGray,
            Location = new Point(20, 52),
            AutoSize = true
        });

        void Crear(string texto, int y, Color color, Action abrir, bool habilitado = true)
        {
            var b = new Button();
            Ui.Boton(b, texto, new Point(60, y), new Size(300, 45), habilitado ? color : Color.Silver);
            b.Enabled = habilitado;
            b.Click += (s, e) => abrir();
            Controls.Add(b);
        }

        Crear("Registrar consumos",        90,  Color.SeaGreen,       () => Abrir(new FormRegistro(u)));
        Crear("Consultar / buscar",        145, Ui.Azul,              () => Abrir(new FormConsulta()));
        Crear("Actualizar o eliminar",     200, Color.DarkOrange,     () => Abrir(new FormActualizar()), admin);
        Crear("Exportar datos",            255, Ui.AzulOscuro,        () => Abrir(new FormExportar()), admin);
        Crear("Inventario y menú",         310, Color.DarkGoldenrod,  () => Abrir(new FormInventario()), admin);
        Crear("Administrar usuarios",      365, Color.SlateBlue,      () => Abrir(new FormUsuarios(u.Login)), admin);
        Crear("Cambiar mi contraseña",     420, Color.Teal,           () => Abrir(new FormCambiarClave(u)));
        Crear("Cerrar sesión",             485, Color.Gray,           Close);

        if (!admin)
        {
            Controls.Add(new Label
            {
                Text = "Actualizar, exportar, inventario y usuarios: solo Administrador.",
                ForeColor = Color.DimGray,
                Location = new Point(40, 555),
                AutoSize = true
            });
        }
    }

    private static void Abrir(Form f)
    {
        using (f) f.ShowDialog();
    }
}
