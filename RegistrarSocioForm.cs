using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ClubManagement
{
    public class RegistrarSocioForm : Form
    {
        private TextBox txtNombre;
        private TextBox txtApellido;
        private TextBox txtDni;
        private DateTimePicker dtpFechaNacimiento;
        private TextBox txtDireccion;
        private TextBox txtTelefono;
        private TextBox txtEmail;

        private CheckBox checkAptoFisico;
        private GroupBox gbAptoFisico;
        private DateTimePicker dtpAptoVencimiento;
        private TextBox txtMedico;
        private TextBox txtObservaciones;

        private Button btnGuardar;

        private readonly DatabaseHelper _dbHelper;


        public RegistrarSocioForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            InitializeComponent();
            gbAptoFisico.Visible = false;
        }

        private void InitializeComponent()
        {
            this.Text = "Registrar Socio";
            this.Size = new System.Drawing.Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            int top = 20;
            int spacing = 30;

            txtNombre = CrearTextBox("Nombre", ref top);
            txtApellido = CrearTextBox("Apellido", ref top);
            txtDni = CrearTextBox("DNI", ref top);

            var lblFechaNacimiento = new Label() { Text = "Fecha Nacimiento", Left = 20, Top = top, Width = 120 };
            this.Controls.Add(lblFechaNacimiento);
            dtpFechaNacimiento = new DateTimePicker() { Left = 150, Top = top, Width = 200 };
            this.Controls.Add(dtpFechaNacimiento);
            top += spacing;

            txtDireccion = CrearTextBox("Dirección", ref top);
            txtTelefono = CrearTextBox("Teléfono", ref top);
            txtEmail = CrearTextBox("Email", ref top);

            checkAptoFisico = new CheckBox() { Text = "Presenta Apto Físico", Left = 20, Top = top, Width = 200 };
            checkAptoFisico.CheckedChanged += checkAptoFisico_CheckedChanged;
            this.Controls.Add(checkAptoFisico);
            top += spacing;

            // GroupBox Apto Físico
            gbAptoFisico = new GroupBox()
            {
                Text = "Datos del Apto Físico",
                Left = 20,
                Top = top,
                Width = 330,
                Height = 130
            };
            this.Controls.Add(gbAptoFisico);

            var lblVencimiento = new Label() { Text = "Fecha Vencimiento", Left = 10, Top = 20, Width = 120 };
            gbAptoFisico.Controls.Add(lblVencimiento);
            dtpAptoVencimiento = new DateTimePicker() { Left = 140, Top = 20, Width = 160 };
            gbAptoFisico.Controls.Add(dtpAptoVencimiento);

            var lblMedico = new Label() { Text = "Médico", Left = 10, Top = 50 };
            gbAptoFisico.Controls.Add(lblMedico);
            txtMedico = new TextBox() { Left = 140, Top = 50, Width = 160 };
            gbAptoFisico.Controls.Add(txtMedico);

            var lblObs = new Label() { Text = "Observaciones", Left = 10, Top = 80 };
            gbAptoFisico.Controls.Add(lblObs);
            txtObservaciones = new TextBox() { Left = 140, Top = 80, Width = 160 };
            gbAptoFisico.Controls.Add(txtObservaciones);

            top += gbAptoFisico.Height + 10;

            btnGuardar = new Button() { Text = "Guardar", Left = 120, Top = top, Width = 100 };
            btnGuardar.Click += btnGuardar_Click;
            this.Controls.Add(btnGuardar);
        }

        private TextBox CrearTextBox(string labelText, ref int top)
        {
            var label = new Label() { Text = labelText, Left = 20, Top = top, Width = 120 };
            this.Controls.Add(label);
            var txt = new TextBox() { Left = 150, Top = top, Width = 200 };
            this.Controls.Add(txt);
            top += 30;
            return txt;
        }

        private void checkAptoFisico_CheckedChanged(object sender, EventArgs e)
        {
            gbAptoFisico.Visible = checkAptoFisico.Checked;
        }

        
        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingrese el nombre");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("Ingrese el apellido");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtDni.Text))
            {
                MessageBox.Show("Ingrese el DNI");
                return false;
            }
            return true;
        }

        

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarCampos()) return;

                var socio = new Socio
                {
                    Nombre = txtNombre.Text,
                    Apellido = txtApellido.Text,
                    Dni = txtDni.Text,
                    FechaNacimiento = dtpFechaNacimiento.Value,
                    Direccion = txtDireccion.Text,
                    Telefono = txtTelefono.Text,
                    Email = txtEmail.Text,
                    FechaInscripcion = DateTime.Now,
                    EstadoActivo = true,
                    FechaVencimientoCuota = DateTime.Now.AddMonths(1)
                };

                AptoFisico apto = null;
                Carnet carnet = null;

                if (checkAptoFisico.Checked)
                {
                    apto = new AptoFisico
                    {
                        FechaEmision = DateTime.Now,
                        FechaVencimiento = dtpAptoVencimiento.Value,
                        Medico = txtMedico.Text,
                        Observaciones = txtObservaciones.Text
                    };
                    socio.AptoFisico = apto;
                }

                // Primero guardamos el socio (y apto físico si corresponde)
                GuardarSocioBD(socio, apto);

                // Si tiene apto físico, generamos y guardamos el carnet
                if (checkAptoFisico.Checked)
                {
                    carnet = new Carnet { Socio = socio };
                    carnet.GenerarCarnet();
                    GuardarCarnetBD(carnet);

                    MessageBox.Show(string.Format("Socio registrado. Carnet N°: {0}", carnet.NumeroCarnetFormateado));
                }
                else
                {
                    MessageBox.Show("Socio registrado sin apto físico");
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error: {0}", ex.Message));
            }
        }

        private void GuardarSocioBD(Socio socio, AptoFisico apto)
        {
            using (var conn = _dbHelper.GetConnection())
            {
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // Verifica si el DNI ya existe
                        var cmdCheckDni = new SQLiteCommand("SELECT COUNT(*) FROM Socios WHERE Dni = @dni", conn);
                        cmdCheckDni.Parameters.AddWithValue("@dni", socio.Dni);
                        int existeDni = Convert.ToInt32(cmdCheckDni.ExecuteScalar());

                        if (existeDni > 0)
                        {
                            MessageBox.Show("¡Error! El DNI ya está registrado.");
                            return;
                        }

                        // Insertar socio
                        var cmdSocio = new SQLiteCommand(
                            "INSERT INTO Socios (Nombre, Apellido, Dni, FechaNacimiento, Direccion, Telefono, Email, FechaInscripcion, EstadoActivo, FechaVencimientoCuota) " +
                            "VALUES (@nombre, @apellido, @Dni, @fechaNac, @direccion, @telefono, @email, @fechaInscripcion, @estado, @fechaVencimiento); " +
                            "SELECT last_insert_rowid();", conn);

                        cmdSocio.Parameters.AddWithValue("@nombre", socio.Nombre);
                        cmdSocio.Parameters.AddWithValue("@apellido", socio.Apellido);
                        cmdSocio.Parameters.AddWithValue("@Dni", socio.Dni);
                        cmdSocio.Parameters.AddWithValue("@fechaNac", socio.FechaNacimiento);
                        cmdSocio.Parameters.AddWithValue("@direccion", socio.Direccion);
                        cmdSocio.Parameters.AddWithValue("@telefono", socio.Telefono);
                        cmdSocio.Parameters.AddWithValue("@email", socio.Email);
                        cmdSocio.Parameters.AddWithValue("@fechaInscripcion", socio.FechaInscripcion);
                        cmdSocio.Parameters.AddWithValue("@estado", socio.EstadoActivo ? 1 : 0);
                        cmdSocio.Parameters.AddWithValue("@fechaVencimiento", socio.FechaVencimientoCuota);

                        socio.NroSocio = Convert.ToInt32(cmdSocio.ExecuteScalar());

                        // Insertar apto físico si corresponde
                        if (apto != null)
                        {
                            var cmdApto = new SQLiteCommand(
                                "INSERT INTO AptoFisico (NroSocio, FechaEmision, FechaVencimiento, Medico, Observaciones) " +
                                "VALUES (@nroSocio, @fechaEmision, @fechaVenc, @medico, @obs)", conn);

                            cmdApto.Parameters.AddWithValue("@nroSocio", socio.NroSocio);
                            cmdApto.Parameters.AddWithValue("@fechaEmision", apto.FechaEmision);
                            cmdApto.Parameters.AddWithValue("@fechaVenc", apto.FechaVencimiento);
                            cmdApto.Parameters.AddWithValue("@medico", apto.Medico);
                            cmdApto.Parameters.AddWithValue("@obs", apto.Observaciones);
                            cmdApto.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        MessageBox.Show(string.Format("Error al guardar socio: {0}", ex.Message));
                        throw;
                    }
                }
            }
        }

        private void GuardarCarnetBD(Carnet carnet)
        {
            using (var conn = _dbHelper.GetConnection())
            {
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        var cmdCarnet = new SQLiteCommand(
                            "INSERT INTO Carnets (NroSocio, NroCarnet, FechaEmision, FechaVencimiento) " +
                            "VALUES (@nroSocio, @nroCarnet, @fechaEmision, @fechaVencimiento)", conn);

                        cmdCarnet.Parameters.AddWithValue("@nroSocio", carnet.Socio.NroSocio);
                        cmdCarnet.Parameters.AddWithValue("@nroCarnet", carnet.NroCarnet);
                        cmdCarnet.Parameters.AddWithValue("@fechaEmision", carnet.FechaEmision);
                        cmdCarnet.Parameters.AddWithValue("@fechaVencimiento", carnet.FechaVencimiento);
                        cmdCarnet.ExecuteNonQuery();

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        MessageBox.Show(string.Format("Error al guardar carnet: {0}", ex.Message));
                        throw;
                    }
                }
            }
        }

    }
}