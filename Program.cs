namespace Comedor51;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Datos.Inicializar();
        Application.Run(new FormLogin());
    }
}
