using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Text;

namespace ClubManagement
{
    public partial class ListarVencimientosForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private DateTimePicker dtpFecha;
        private DataGridView dgvVencimientos;
        private Label lblFecha;
        private ComboBox cmbEstado;

        public ListarVencimientosForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            InitializeComponent();
            ConfigurarColumnasDataGrid();
            CargarVencimientos();
        }

        private void InitializeComponent()
        {
            // Configuración básica del formulario
            this.Text = "Control de Vencimientos de Cuotas";
            this.Width = 850;  // Aumentado para nueva columna
            this.Height = 550;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Controles para filtros
            lblFecha = new Label
            {
                Text = "Fecha de vencimiento:",
                Left = 20,
                Top = 20,
                Width = 120
            };
            this.Controls.Add(lblFecha);

            dtpFecha = new DateTimePicker
            {
                Left = 150,
                Top = 18,
                Width = 120,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            dtpFecha.ValueChanged += (s, e) => CargarVencimientos();
            this.Controls.Add(dtpFecha);

            // Combo para filtrar por estado
            var lblEstado = new Label
            {
                Text = "Estado:",
                Left = 300,
                Top = 20,
                Width = 50
            };
            this.Controls.Add(lblEstado);

            cmbEstado = new ComboBox
            {
                Left = 360,
                Top = 18,
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEstado.Items.AddRange(new object[] { "Todos", "Activos", "Inactivos", "Vencidos hoy", "Vencidos anteriormente" });
            cmbEstado.SelectedIndex = 0;
            cmbEstado.SelectedIndexChanged += (s, e) => CargarVencimientos();
            this.Controls.Add(cmbEstado);

            // DataGridView
            dgvVencimientos = new DataGridView
            {
                Left = 20,
                Top = 60,
                Width = 790,  // Aumentado para nueva columna
                Height = 430,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            this.Controls.Add(dgvVencimientos);

            // Evento para mostrar información polimórfica
            dgvVencimientos.CellDoubleClick += (s, e) => MostrarInformacionPolimorfica();
        }

        private void ConfigurarColumnasDataGrid()
        {
            dgvVencimientos.Columns.Clear();

            // Configuración común para todas las columnas
            DataGridViewCellStyle estiloCelda = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(2)
            };

            DataGridViewCellStyle estiloCeldaNumerica = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Format = "N0",
                NullValue = "",
                Padding = new Padding(2)
            };

            DataGridViewCellStyle estiloCeldaFecha = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                NullValue = "",
                Padding = new Padding(2)
            };

            // Columna NroSocio
            dgvVencimientos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NroSocio",
                HeaderText = "N° Socio",
                DataPropertyName = "NroSocio",
                Width = 70,
                DefaultCellStyle = estiloCeldaNumerica,
                ReadOnly = true
            });

            // Columna Socio (Nombre + Apellido)
            dgvVencimientos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Socio",
                HeaderText = "Socio",
                DataPropertyName = "Socio",
                Width = 150,
                DefaultCellStyle = estiloCelda,
                ReadOnly = true
            });
            //tipo de membresia
            dgvVencimientos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TipoMembresia",
                HeaderText = "Tipo Membresía",
                DataPropertyName = "TipoMembresia",  // Debe coincidir exactamente con el alias en la consulta
                Width = 120,
                DefaultCellStyle = estiloCelda,
                ReadOnly = true
            });

            // Columna Teléfono
            dgvVencimientos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Telefono",
                HeaderText = "Teléfono",
                DataPropertyName = "Telefono",
                Width = 100,
                DefaultCellStyle = estiloCelda,
                ReadOnly = true
            });

            // Columna Fecha de Vencimiento (formateada)
            dgvVencimientos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FechaVencimiento",
                HeaderText = "Vencimiento",
                DataPropertyName = "FechaVencimientoFormateada",
                Width = 90,
                DefaultCellStyle = estiloCeldaFecha,
                ReadOnly = true
            });

            // Columna Estado (Activo/Inactivo)
            dgvVencimientos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Estado",
                HeaderText = "Estado",
                DataPropertyName = "Estado",
                Width = 80,
                DefaultCellStyle = estiloCelda,
                ReadOnly = true
            });

            // Columna Días Vencido
            dgvVencimientos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DiasVencido",
                HeaderText = "Días",
                DataPropertyName = "DiasVencido",
                Width = 60,
                DefaultCellStyle = estiloCeldaNumerica,
                ReadOnly = true,
                ValueType = typeof(int?)
            });

            // Columna NroCarnet
            dgvVencimientos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NroCarnet",
                HeaderText = "Carnet",
                DataPropertyName = "NroCarnet",
                Width = 80,
                DefaultCellStyle = estiloCelda,
                ReadOnly = true
            });

            dgvVencimientos.AutoGenerateColumns = false;
        }

        private void CargarVencimientos()
        {
            try
            {
                _dbHelper.VerificarVencimientosAutomaticos();

                string query = @"
            SELECT 
                s.NroSocio, 
                s.Nombre || ' ' || s.Apellido AS Socio,
                s.Telefono,
                s.FechaVencimientoCuota,
                CASE WHEN s.EstadoActivo = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado,
                c.NroCarnet,
                CASE 
                    WHEN date(s.FechaVencimientoCuota) = date(@fecha) THEN 'Vence hoy'
                    WHEN date(s.FechaVencimientoCuota) < date(@fecha) THEN 'Vencido'
                    ELSE 'Pendiente'
                END AS EstadoVencimiento,
                CASE 
                    WHEN date(s.FechaVencimientoCuota) < date(@fecha) 
                    THEN julianday(@fecha) - julianday(s.FechaVencimientoCuota)
                    ELSE NULL
                END AS DiasVencido,
                strftime('%d/%m/%Y', s.FechaVencimientoCuota) AS FechaVencimientoFormateada,
                tm.Nombre AS TipoMembresia
            FROM Socios s
            LEFT JOIN Carnets c ON s.NroSocio = c.NroSocio
            LEFT JOIN TiposMembresia tm ON s.IdTipoMembresia = tm.Id
            WHERE 1=1";

                // Filtros según selección
                switch (cmbEstado.SelectedItem.ToString())
                {
                    case "Activos":
                        query += " AND s.EstadoActivo = 1";
                        break;
                    case "Inactivos":
                        query += " AND s.EstadoActivo = 0";
                        break;
                    case "Vencidos hoy":
                        query += " AND date(s.FechaVencimientoCuota) = date(@fecha)";
                        break;
                    case "Vencidos anteriormente":
                        query += " AND date(s.FechaVencimientoCuota) < date(@fecha)";
                        break;
                }

                query += " ORDER BY s.FechaVencimientoCuota, s.Apellido, s.Nombre";

                using (var cmd = new SQLiteCommand(query, _dbHelper.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@fecha", dtpFecha.Value.ToString("yyyy-MM-dd"));

                    var da = new SQLiteDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);

                    dgvVencimientos.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar vencimientos: " + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarInformacionPolimorfica()
        {
            if (dgvVencimientos.CurrentRow == null || dgvVencimientos.CurrentRow.IsNewRow)
                return;

            try
            {
                // Obtener valores con verificación de nulidad
                int nroSocio = 0;
                if (dgvVencimientos.CurrentRow.Cells["NroSocio"].Value != null)
                {
                    int.TryParse(dgvVencimientos.CurrentRow.Cells["NroSocio"].Value.ToString(), out nroSocio);
                }

                string nombre = "No disponible";
                if (dgvVencimientos.CurrentRow.Cells["Socio"].Value != null)
                {
                    nombre = dgvVencimientos.CurrentRow.Cells["Socio"].Value.ToString();
                }

                string tipoMembresia = "No especificada";
                if (dgvVencimientos.CurrentRow.Cells["TipoMembresia"].Value != null)
                {
                    tipoMembresia = dgvVencimientos.CurrentRow.Cells["TipoMembresia"].Value.ToString();
                }

                bool estado = false;
                if (dgvVencimientos.CurrentRow.Cells["Estado"].Value != null)
                {
                    estado = dgvVencimientos.CurrentRow.Cells["Estado"].Value.ToString() == "Activo";
                }

                // Construir mensaje de información
                string info = string.Format(
                    "Socio: {0}\nNúmero: {1}\nEstado: {2}\nTipo de Membresía: {3}\n",
                    nombre,
                    nroSocio,
                    estado ? "Activo" : "Inactivo",
                    tipoMembresia
                );

                // Obtener detalles adicionales de la membresía si existe
                if (nroSocio > 0)
                {
                    try
                    {
                        DataRow detalles = _dbHelper.ObtenerDetallesCompletosMembresia(nroSocio);
                        if (detalles != null)
                        {
                            string descripcion = detalles["Descripcion"] != DBNull.Value ?
                                detalles["Descripcion"].ToString() : "No disponible";
                            string beneficios = detalles["Beneficios"] != DBNull.Value ?
                                detalles["Beneficios"].ToString() : "No disponible";

                            info += string.Format(
                                "Descripción: {0}\nBeneficios: {1}\n",
                                descripcion,
                                beneficios
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        info += string.Format("Error al obtener detalles: {0}\n", ex.Message);
                    }
                }

                MessageBox.Show(info, "Información del Socio", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error al mostrar información:\n{0}", ex.Message),
                               "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}

