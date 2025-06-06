using System;
using System.Data.SQLite;
using System.IO;

namespace ClubManagement
{
    public class DatabaseHelper : IDisposable
    {
        private const string DatabasePath = "ClubDB.sqlite";
        private SQLiteConnection _connection;
        private bool _disposed = false;

        public DatabaseHelper()
        {
            if (!File.Exists(DatabasePath))
            {
                SQLiteConnection.CreateFile(DatabasePath);
            }

            _connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", DatabasePath));
            _connection.Open();

            InitializeDatabase();
        }

        public void InitializeDatabase()
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS Socios (
                NroSocio INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Apellido TEXT NOT NULL,
                Dni TEXT,
                FechaNacimiento TEXT,
                Direccion TEXT,
                Telefono TEXT,
                Email TEXT,
                FechaInscripcion TEXT,
                EstadoActivo INTEGER DEFAULT 1,
                FechaVencimientoCuota DATETIME
            );

            CREATE TABLE IF NOT EXISTS AptoFisico (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NroSocio INTEGER,
                FechaEmision TEXT,
                FechaVencimiento TEXT,
                Medico TEXT,
                Observaciones TEXT,
                FOREIGN KEY(NroSocio) REFERENCES Socios(NroSocio) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Carnets (
                NroCarnet INTEGER PRIMARY KEY,
                NroSocio INTEGER,
                FechaEmision TEXT,
                FechaVencimiento TEXT,
                FOREIGN KEY(NroSocio) REFERENCES Socios(NroSocio) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS NoSocios (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Apellido TEXT NOT NULL,
                Dni TEXT NOT NULL UNIQUE,
                FechaNacimiento TEXT,
                Direccion TEXT,
                Telefono TEXT,
                Email TEXT,
                FechaRegistro TEXT
            );

            CREATE TABLE IF NOT EXISTS Profesores (
                Legajo TEXT PRIMARY KEY,
                Nombre TEXT NOT NULL,
                Apellido TEXT NOT NULL,
                Dni TEXT NOT NULL UNIQUE,
                FechaContratacion TEXT,
                EsTitular INTEGER DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS Actividades (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Descripcion TEXT,
                Horario TEXT,
                ProfesorLegajo TEXT,
                PrecioNoSocio REAL DEFAULT 0,
                ExclusivaSocios INTEGER DEFAULT 0,
                FOREIGN KEY(ProfesorLegajo) REFERENCES Profesores(Legajo)
            );

            CREATE TABLE IF NOT EXISTS Cuotas (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NroSocio INTEGER,
                Monto REAL,
                FechaPago TEXT,
                FechaVencimiento TEXT,
                MetodoPago TEXT,
                Pagada INTEGER DEFAULT 0,
                FOREIGN KEY(NroSocio) REFERENCES Socios(NroSocio)
            );

            CREATE TABLE IF NOT EXISTS ActividadesNoSocios (
                IdNoSocio INTEGER,
                IdActividad INTEGER,
                Fecha TEXT,
                MetodoPago TEXT,
                PRIMARY KEY(IdNoSocio, IdActividad, Fecha),
                FOREIGN KEY(IdNoSocio) REFERENCES NoSocios(Id) ON DELETE CASCADE,
                FOREIGN KEY(IdActividad) REFERENCES Actividades(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS ActividadesSocios (
                NroSocio INTEGER,
                IdActividad INTEGER,
                FechaInscripcion TEXT,
                PRIMARY KEY(NroSocio, IdActividad),
                FOREIGN KEY(NroSocio) REFERENCES Socios(NroSocio) ON DELETE CASCADE,
                FOREIGN KEY(IdActividad) REFERENCES Actividades(Id) ON DELETE CASCADE
            );";

            using (var command = new SQLiteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
            }
        }

        

        public SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();
            return connection;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_connection != null)
                    {
                        if (_connection.State != System.Data.ConnectionState.Closed)
                            _connection.Close();
                        _connection.Dispose();
                    }
                }
                _disposed = true;
            }
        }

        ~DatabaseHelper()
        {
            Dispose(false);
        }
    }
}
