using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ClubManagement
{
    public class RegistrarNoSocioForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private TextBox txtNombre, txtApellido, txtDni, txtDireccion, txtTelefono, txtEmail;
        private DateTimePicker dtpFechaNacimiento;
        private Button btnGuardar;

        public RegistrarNoSocioForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Configuración básica del formulario
            this.Text = "Registro de No Socio";
            this.Width = 500;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Creación de etiquetas
            var lblNombre = new Label { Text = "Nombre:", Left = 20, Top = 20, Width = 120 };
            var lblApellido = new Label { Text = "Apellido:", Left = 20, Top = 60, Width = 120 };
            var lblDni = new Label { Text = "DNI:", Left = 20, Top = 100, Width = 120 };
            var lblFechaNac = new Label { Text = "Fecha Nacimiento:", Left = 20, Top = 140, Width = 120 };
            var lblDireccion = new Label { Text = "Dirección:", Left = 20, Top = 180, Width = 120 };
            var lblTelefono = new Label { Text = "Teléfono:", Left = 20, Top = 220, Width = 120 };
            var lblEmail = new Label { Text = "Email:", Left = 20, Top = 260, Width = 120 };

            // Creación de controles de entrada
            txtNombre = new TextBox { Left = 150, Top = 20, Width = 300 };
            txtApellido = new TextBox { Left = 150, Top = 60, Width = 300 };
            txtDni = new TextBox { Left = 150, Top = 100, Width = 300 };
            dtpFechaNacimiento = new DateTimePicker { Left = 150, Top = 140, Width = 300, Format = DateTimePickerFormat.Short };
            txtDireccion = new TextBox { Left = 150, Top = 180, Width = 300 };
            txtTelefono = new TextBox { Left = 150, Top = 220, Width = 300 };
            txtEmail = new TextBox { Left = 150, Top = 260, Width = 300 };

            // Botón Guardar
            btnGuardar = new Button { Text = "Guardar", Left = 180, Top = 310, Width = 120 };
            btnGuardar.Click += BtnGuardar_Click;

            // Agregar controles al formulario
            this.Controls.AddRange(new Control[] {
                lblNombre, lblApellido, lblDni, lblFechaNac, lblDireccion, lblTelefono, lblEmail,
                txtNombre, txtApellido, txtDni, dtpFechaNacimiento, txtDireccion, txtTelefono, txtEmail,
                btnGuardar
            });
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;

            try
            {
                using (var conn = _dbHelper.GetConnection())
                {
                    var cmd = new SQLiteCommand(
                        "INSERT INTO NoSocios (Nombre, Apellido, Dni, FechaNacimiento, Direccion, Telefono, Email, FechaRegistro) " +
                        "VALUES (@nombre, @apellido, @dni, @fechaNac, @direccion, @telefono, @email, @fechaReg)", conn);

                    cmd.Parameters.AddWithValue("@nombre", txtNombre.Text.Trim());
                    cmd.Parameters.AddWithValue("@apellido", txtApellido.Text.Trim());
                    cmd.Parameters.AddWithValue("@dni", txtDni.Text.Trim());
                    cmd.Parameters.AddWithValue("@fechaNac", dtpFechaNacimiento.Value.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@direccion", txtDireccion.Text.Trim());
                    cmd.Parameters.AddWithValue("@telefono", txtTelefono.Text.Trim());
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@fechaReg", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    int affectedRows = cmd.ExecuteNonQuery();
                    
                    if (affectedRows > 0)
                    {
                        MessageBox.Show("No socio registrado con éxito", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                }
            }
            catch (SQLiteException ex)
            {
                if (ex.Message.Contains("UNIQUE"))
                {
                    MessageBox.Show("Ya existe un no socio con este DNI", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(string.Format("Error al guardar: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error al guardar: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("El apellido es obligatorio", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDni.Text))
            {
                MessageBox.Show("El DNI es obligatorio", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}