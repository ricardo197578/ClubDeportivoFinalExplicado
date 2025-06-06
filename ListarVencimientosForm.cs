using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ClubManagement
{
    public partial class ListarVencimientosForm : Form
    {
        private readonly DatabaseHelper _dbHelper;

        private DateTimePicker dtpFecha;
        private DataGridView dgvVencimientos;
        private Label lblFecha;

       

        public ListarVencimientosForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            InitializeComponent();
            CargarVencimientos();

        }

        private void InitializeComponent()
        {
            this.Text = "Listado de Socios con Cuota Vencida";
            this.Width = 700;
            this.Height = 500;
            this.StartPosition = FormStartPosition.CenterScreen;

            lblFecha = new Label
            {
                Text = "Fecha límite:",
                Left = 20,
                Top = 20,
                Width = 100
            };
            this.Controls.Add(lblFecha);

            dtpFecha = new DateTimePicker
            {
                Left = 130,
                Top = 18,
                Width = 200,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            dtpFecha.ValueChanged += new EventHandler(dtpFecha_ValueChanged);
            this.Controls.Add(dtpFecha);

            dgvVencimientos = new DataGridView
            {
                Left = 20,
                Top = 60,
                Width = 640,
                Height = 370,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(dgvVencimientos);
        }

        private void CargarVencimientos()
        {
            try
            {
                using (var cmd = new SQLiteCommand(
                    @"SELECT s.NroSocio, s.Nombre || ' ' || s.Apellido AS Socio, 
                      s.FechaVencimientoCuota, c.NroCarnet
                      FROM Socios s
                      LEFT JOIN Carnets c ON s.NroSocio = c.NroSocio
                      WHERE s.FechaVencimientoCuota <= @fecha
                      ORDER BY s.FechaVencimientoCuota", _dbHelper.GetConnection()))
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
                MessageBox.Show("Error al cargar vencimientos: " + ex.Message);
            }
        }

        private void dtpFecha_ValueChanged(object sender, EventArgs e)
        {
            CargarVencimientos();
        }
    }
}


