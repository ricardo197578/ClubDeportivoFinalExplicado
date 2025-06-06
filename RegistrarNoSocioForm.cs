using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ClubManagement
{
    public partial class RegistrarNoSocioForm : Form
    {
        //private DatabaseHelper dbHelper = new DatabaseHelper();
        private readonly DatabaseHelper _dbHelper; // Añadir esta línea

        private TextBox txtNombre, txtApellido, txtDni, txtDireccion, txtTelefono, txtEmail, txtMonto;
        private DateTimePicker dtpFechaNacimiento;
        private ComboBox cmbActividades;
        private RadioButton rbEfectivo, rbTarjeta;
        private Button btnGuardar;

        public RegistrarNoSocioForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            InitializeComponent();
            CargarActividades();
        }

        private void InitializeComponent()
        {
            this.Text = "Registrar No Socio";
            this.Width = 500;
            this.Height = 550;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label[] labels = new Label[]
            {
                new Label() { Text = "Nombre:", Left = 20, Top = 20 },
                new Label() { Text = "Apellido:", Left = 20, Top = 60 },
                new Label() { Text = "DNI:", Left = 20, Top = 100 },
                new Label() { Text = "Fecha Nacimiento:", Left = 20, Top = 140 },
                new Label() { Text = "Dirección:", Left = 20, Top = 180 },
                new Label() { Text = "Teléfono:", Left = 20, Top = 220 },
                new Label() { Text = "Email:", Left = 20, Top = 260 },
                new Label() { Text = "Actividad:", Left = 20, Top = 300 },
                new Label() { Text = "Monto:", Left = 20, Top = 340 },
                new Label() { Text = "Método de pago:", Left = 20, Top = 380 }
            };

            foreach (var lbl in labels)
            {
                lbl.Width = 120;
                this.Controls.Add(lbl);
            }

            txtNombre = new TextBox() { Left = 150, Top = 20, Width = 300 };
            txtApellido = new TextBox() { Left = 150, Top = 60, Width = 300 };
            txtDni = new TextBox() { Left = 150, Top = 100, Width = 300 };
            dtpFechaNacimiento = new DateTimePicker() { Left = 150, Top = 140, Width = 300 };
            txtDireccion = new TextBox() { Left = 150, Top = 180, Width = 300 };
            txtTelefono = new TextBox() { Left = 150, Top = 220, Width = 300 };
            txtEmail = new TextBox() { Left = 150, Top = 260, Width = 300 };
            cmbActividades = new ComboBox() { Left = 150, Top = 300, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            txtMonto = new TextBox() { Left = 150, Top = 340, Width = 300, ReadOnly = true };

            rbEfectivo = new RadioButton() { Text = "Efectivo", Left = 150, Top = 380 };
            rbTarjeta = new RadioButton() { Text = "Tarjeta de crédito", Left = 250, Top = 380 };
            rbEfectivo.Checked = true;

            btnGuardar = new Button() { Text = "Guardar", Left = 180, Top = 430, Width = 120 };
            btnGuardar.Click += new EventHandler(btnGuardar_Click);

            cmbActividades.SelectedIndexChanged += new EventHandler(cmbActividades_SelectedIndexChanged);

            this.Controls.AddRange(new Control[]
            {
                txtNombre, txtApellido, txtDni, dtpFechaNacimiento, txtDireccion,
                txtTelefono, txtEmail, cmbActividades, txtMonto, rbEfectivo,
                rbTarjeta, btnGuardar
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
                            //Text = $"{reader["Nombre"]} (${reader["PrecioNoSocio"]})",
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
                MessageBox.Show("Error al cargar actividades: " + ex.Message);

            }
        }

        private void cmbActividades_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbActividades.SelectedItem != null)
            {
                var item = (ComboboxItem)cmbActividades.SelectedItem;
                txtMonto.Text = item.Precio.ToString("0.00");
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;

            try
            {
                var noSocio = new NoSocio
                {
                    Nombre = txtNombre.Text,
                    Apellido = txtApellido.Text,
                    Dni = txtDni.Text,
                    FechaNacimiento = dtpFechaNacimiento.Value,
                    Direccion = txtDireccion.Text,
                    Telefono = txtTelefono.Text,
                    Email = txtEmail.Text,
                    FechaRegistro = DateTime.Now
                };

                var actividad = (ComboboxItem)cmbActividades.SelectedItem;
                var metodoPago = rbEfectivo.Checked ? MetodoPago.EFECTIVO : MetodoPago.TARJETA_CREDITO;

                GuardarNoSocio(noSocio, actividad.Value, metodoPago, actividad.Precio);

                MessageBox.Show("No socio registrado con éxito");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);

            }
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingrese el nombre");
                return false;
            }
            if (cmbActividades.SelectedItem == null)
            {
                MessageBox.Show("Seleccione una actividad");
                return false;
            }
            return true;
        }

        private void GuardarNoSocio(NoSocio noSocio, int idActividad, MetodoPago metodo, decimal precio)
        {
            using (var conn = _dbHelper.GetConnection())
            {
                var cmd = new SQLiteCommand(
                    "INSERT INTO NoSocios (Nombre, Apellido, Dni, FechaNacimiento, Direccion, Telefono, Email, FechaRegistro) " +
                    "VALUES (@nombre, @apellido, @dni, @fechaNac, @direccion, @telefono, @email, @fechaReg)", conn);

                cmd.Parameters.AddWithValue("@nombre", noSocio.Nombre);
                cmd.Parameters.AddWithValue("@apellido", noSocio.Apellido);
                cmd.Parameters.AddWithValue("@dni", noSocio.Dni);
                cmd.Parameters.AddWithValue("@fechaNac", noSocio.FechaNacimiento);
                cmd.Parameters.AddWithValue("@direccion", noSocio.Direccion);
                cmd.Parameters.AddWithValue("@telefono", noSocio.Telefono);
                cmd.Parameters.AddWithValue("@email", noSocio.Email);
                cmd.Parameters.AddWithValue("@fechaReg", noSocio.FechaRegistro);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT last_insert_rowid()";
                int idNoSocio = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText =
                    "INSERT INTO ActividadesNoSocios (IdNoSocio, IdActividad, Fecha, MetodoPago, Monto) " +
                    "VALUES (@idNoSocio, @idAct, @fecha, @metodo, @monto)";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@idNoSocio", idNoSocio);
                cmd.Parameters.AddWithValue("@idAct", idActividad);
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                cmd.Parameters.AddWithValue("@metodo", metodo.ToString());
                cmd.Parameters.AddWithValue("@monto", precio);

                cmd.ExecuteNonQuery();
            }
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
