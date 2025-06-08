using System;
using System.Windows.Forms;

namespace ClubManagement
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Inicializar base de datos
            using (var dbHelper = new DatabaseHelper())
            {
                dbHelper.InitializeDatabase();

                // Mostrar formulario de login
                using (var loginForm = new LoginForm(dbHelper))
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        // Si el login es exitoso, mostrar el formulario principal
                        Application.Run(new MainForm());
                    }
                    else
                    {
                        // Si el login falla o se cancela, salir de la aplicación
                        Application.Exit();
                    }
                }
            }
        }
    }
}
/*using System;
using System.Windows.Forms;

namespace ClubManagement
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Inicializar base de datos creando una instancia importante !!!!
            var dbHelper = new DatabaseHelper();
            dbHelper.InitializeDatabase();

            Application.Run(new MainForm());
        }
    }
}*/

