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

            // Inicializar base de datos creando una instancia
            var dbHelper = new DatabaseHelper();
            dbHelper.InitializeDatabase();

            Application.Run(new MainForm());
        }
    }
}

