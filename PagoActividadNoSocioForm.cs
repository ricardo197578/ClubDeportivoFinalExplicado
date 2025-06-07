using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ClubManagement
{
    public class PagoActividadNoSocioForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private TextBox txtDniBuscar, txtNombre, txtApellido, txtMonto;
        private ComboBox cmbActividades;
        private RadioButton rbEfectivo, rbTarjeta;
        private Button btnBuscar, btnPagar;
        private int _idNoSocio;

        public PagoActividadNoSocioForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            InitializeForm();
            CargarActividades();
        }

        private void InitializeForm()
        {
            // Configuración básica del formulario
            this.Text = "Pago de Actividad - No Socio";
            this.Width = 500;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Creación de etiquetas
            var lblDniBuscar = new Label { Text = "DNI a buscar:", Left = 20, Top = 20, Width = 120 };
            var lblNombre = new Label { Text = "Nombre:", Left = 20, Top = 70, Width = 120 };
            var lblApellido = new Label { Text = "Apellido:", Left = 20, Top = 110, Width = 120 };
            var lblActividad = new Label { Text = "Actividad:", Left = 20, Top = 150, Width = 120 };
            var lblMonto = new Label { Text = "Monto:", Left = 20, Top = 190, Width = 120 };
            var lblMetodoPago = new Label { Text = "Método de pago:", Left = 20, Top = 230, Width = 120 };

            // Creación de controles de entrada
            txtDniBuscar = new TextBox { Left = 150, Top = 20, Width = 200 };
            btnBuscar = new Button { Text = "Buscar", Left = 360, Top = 20, Width = 90 };
            btnBuscar.Click += BtnBuscar_Click;

            txtNombre = new TextBox { Left = 150, Top = 70, Width = 300, ReadOnly = true };
            txtApellido = new TextBox { Left = 150, Top = 110, Width = 300, ReadOnly = true };
            
            cmbActividades = new ComboBox { Left = 150, Top = 150, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbActividades.SelectedIndexChanged += CmbActividades_SelectedIndexChanged;
            
            txtMonto = new TextBox { Left = 150, Top = 190, Width = 300, ReadOnly = true };

            rbEfectivo = new RadioButton { Text = "Efectivo", Left = 150, Top = 230, Checked = true };
            rbTarjeta = new RadioButton { Text = "Tarjeta de crédito", Left = 250, Top = 230 };

            btnPagar = new Button { Text = "Registrar Pago", Left = 180, Top = 280, Width = 140 };
            btnPagar.Click += BtnPagar_Click;
            btnPagar.Enabled = false;

            // Agregar controles al formulario
            this.Controls.AddRange(new Control[] {
                lblDniBuscar, lblNombre, lblApellido, lblActividad, lblMonto, lblMetodoPago,
                txtDniBuscar, btnBuscar, txtNombre, txtApellido, cmbActividades, txtMonto,
                rbEfectivo, rbTarjeta, btnPagar
            });
        }

        private void CargarActividades()
        {
            try
            {
                cmbActividades.Items.Clear();
                using (var conn = _dbHelper.GetConnection())
                {
                    var cmd = new SQLiteCommand(
                        "SELECT Id, Nombre, PrecioNoSocio FROM Actividades WHERE ExclusivaSocios = 0", conn);
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        cmbActividades.Items.Add(new ComboboxItem
                        {
                            Text = string.Format("{0} (${1})", reader["Nombre"], reader["PrecioNoSocio"]),
                            Value = Convert.ToInt32(reader["Id"]),
                            Precio = Convert.ToDecimal(reader["PrecioNoSocio"])
                        });
                    }
                }
                cmbActividades.DisplayMember = "Text";
                cmbActividades.ValueMember = "Value";
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error al cargar actividades: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDniBuscar.Text))
            {
                MessageBox.Show("Ingrese un DNI para buscar", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = _dbHelper.GetConnection())
                {
                    var cmd = new SQLiteCommand(
                        "SELECT Id, Nombre, Apellido FROM NoSocios WHERE Dni = @dni", conn);
                    cmd.Parameters.AddWithValue("@dni", txtDniBuscar.Text.Trim());

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            _idNoSocio = Convert.ToInt32(reader["Id"]);
                            txtNombre.Text = reader["Nombre"].ToString();
                            txtApellido.Text = reader["Apellido"].ToString();
                            btnPagar.Enabled = true;
                        }
                        else
                        {
                            MessageBox.Show("No se encontró un no socio con ese DNI", "Búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LimpiarCampos();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error al buscar no socio: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbActividades_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbActividades.SelectedItem != null)
            {
                var item = (ComboboxItem)cmbActividades.SelectedItem;
                txtMonto.Text = item.Precio.ToString("0.00");
            }
        }

        private void BtnPagar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;

            try
            {
                var actividad = (ComboboxItem)cmbActividades.SelectedItem;
                var metodoPago = rbEfectivo.Checked ? "EFECTIVO" : "TARJETA_CREDITO";

                RegistrarPago(_idNoSocio, actividad.Value, metodoPago, actividad.Precio);

                MessageBox.Show("Pago registrado con éxito", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error al registrar pago: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarCampos()
        {
            if (cmbActividades.SelectedItem == null)
            {
                MessageBox.Show("Seleccione una actividad", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void RegistrarPago(int idNoSocio, int idActividad, string metodo, decimal precio)
        {
            using (var conn = _dbHelper.GetConnection())
            {
                var cmd = new SQLiteCommand(
                    "INSERT INTO ActividadesNoSocios (IdNoSocio, IdActividad, Fecha, MetodoPago, Monto) " +
                    "VALUES (@idNoSocio, @idAct, @fecha, @metodo, @monto)", conn);

                cmd.Parameters.AddWithValue("@idNoSocio", idNoSocio);
                cmd.Parameters.AddWithValue("@idAct", idActividad);
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@metodo", metodo);
                cmd.Parameters.AddWithValue("@monto", precio);

                cmd.ExecuteNonQuery();
            }
        }

        private void LimpiarCampos()
        {
            txtNombre.Clear();
            txtApellido.Clear();
            cmbActividades.SelectedIndex = -1;
            txtMonto.Clear();
            _idNoSocio = 0;
            btnPagar.Enabled = false;
        }
    }

    public class ComboboxItem
    {
        public string Text { get; set; }
        public int Value { get; set; }
        public decimal Precio { get; set; }
        
        public override string ToString()
        {
            return Text;
        }
    }
}