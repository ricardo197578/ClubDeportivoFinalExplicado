using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Collections.Generic;

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
        	CREATE TABLE IF NOT EXISTS Administradores (
   	 	Id INTEGER PRIMARY KEY AUTOINCREMENT,
  	  	Usuario TEXT NOT NULL UNIQUE,
   	 	PasswordHash TEXT NOT NULL,
   	 	Nombre TEXT NOT NULL,
   	 	Email TEXT,
   	 	UltimoLogin TEXT
	    );	            

		
	   CREATE TABLE IF NOT EXISTS TiposMembresia (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                PrecioMensual REAL NOT NULL,
                DiasGracia INTEGER NOT NULL,
                 Descripcion TEXT, 
                Beneficios TEXT
            );

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
                FechaVencimientoCuota DATETIME,
                IdTipoMembresia INTEGER,
                FOREIGN KEY(IdTipoMembresia) REFERENCES TiposMembresia(Id)
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
                Monto REAL,
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

            SeedTiposMembresia();
        }

        private void SeedTiposMembresia()
        {
            string checkSql = "SELECT COUNT(*) FROM TiposMembresia";
            using (var command = new SQLiteCommand(checkSql, _connection))
            {
                var count = Convert.ToInt32(command.ExecuteScalar());
                if (count == 0)
                {
                    string insertSql = @"
            INSERT INTO TiposMembresia (Nombre, PrecioMensual, DiasGracia, Descripcion, Beneficios)
            VALUES 
                ('Standard', 1500.00, 5, 'Membresía básica con acceso a instalaciones', 'Acceso a gimnasio y piscina'),
                ('Premium', 2500.00, 10, 'Membresía premium con beneficios exclusivos', 'Acceso a todas las áreas, clases grupales incluidas'),
                ('Familiar', 3500.00, 15, 'Membresía para grupos familiares', 'Acceso familiar, descuentos en actividades')";

                    using (var insertCommand = new SQLiteCommand(insertSql, _connection))
                    {
                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        public DataRow ObtenerDetallesCompletosMembresia(int nroSocio)
        {
            try
            {
                string query = @"
        SELECT tm.Nombre, tm.Descripcion, tm.Beneficios
        FROM Socios s
        JOIN TiposMembresia tm ON s.IdTipoMembresia = tm.Id
        WHERE s.NroSocio = @nroSocio";

                using (var cmd = new SQLiteCommand(query, GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@nroSocio", nroSocio);
                    DataTable dt = new DataTable();
                    using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
        public string ObtenerDetallesMembresia(int nroSocio)
        {
            try
            {
                string query = @"
        SELECT tm.Nombre, tm.Beneficios, COALESCE(tm.Descripcion, 'No disponible') AS Descripcion
        FROM Socios s
        JOIN TiposMembresia tm ON s.IdTipoMembresia = tm.Id
        WHERE s.NroSocio = @nroSocio";

                using (var cmd = new SQLiteCommand(query, GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@nroSocio", nroSocio);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return string.Format("{0}\nBeneficios: {1}\nDescripción: {2}",
                     reader["Nombre"], reader["Beneficios"], reader["Descripcion"]);

                        }
                    }
                }
            }
            catch { /* Silenciar errores */ }
            return null;
        }

        public SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();
            return connection;
        }

        public List<TipoMembresia> ObtenerTiposMembresia()
        {
            var tipos = new List<TipoMembresia>();
            string sql = "SELECT Nombre, PrecioMensual, DiasGracia, Beneficios FROM TiposMembresia ORDER BY Nombre";

            using (var cmd = new SQLiteCommand(sql, _connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string nombre = reader["Nombre"].ToString();
                    var membresia = MembresiaFactory.CrearMembresia(nombre);

                    // Actualizamos las propiedades desde la base de datos
                    if (membresia != null)
                    {
                        membresia.PrecioMensual = Convert.ToDecimal(reader["PrecioMensual"]);
                        membresia.DiasGraciaVencimiento = Convert.ToInt32(reader["DiasGracia"]);
                        membresia.Beneficios = reader["Beneficios"].ToString();
                    }

                    tipos.Add(membresia);
                }
            }
            return tipos;
        }

        public void VerificarVencimientosAutomaticos()
            {
                try
                {
                    string sql = @"
                UPDATE Socios 
                SET EstadoActivo = 0 
                WHERE EstadoActivo = 1 
                AND date(FechaVencimientoCuota) < date('now', '-7 days')";

                    using (var cmd = new SQLiteCommand(sql, _connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // Manejo de errores
                }
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
