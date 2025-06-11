using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ClubManagement
{

     public enum MetodoPago
    {
        EFECTIVO,
        TARJETA_CREDITO
    }
	
    public class PagoCuotaSocioForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private TextBox txtDniBuscar, txtNombre, txtApellido, txtMonto;
        private Button btnBuscar, btnPagar;
        private RadioButton rbEfectivo, rbTarjeta;
        private int _nroSocio;
        private readonly decimal VALOR_CUOTA = 5000.00m;

        public PagoCuotaSocioForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Configuración básica del formulario
            this.Text = "Pago de Cuota - Socio";
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
            var lblMonto = new Label { Text = "Monto:", Left = 20, Top = 150, Width = 120 };
            var lblMetodoPago = new Label { Text = "Método de pago:", Left = 20, Top = 190, Width = 120 };

            // Creación de controles de entrada
            txtDniBuscar = new TextBox { Left = 150, Top = 20, Width = 200 };
            btnBuscar = new Button { Text = "Buscar", Left = 360, Top = 20, Width = 90 };
            btnBuscar.Click += BtnBuscar_Click;

            txtNombre = new TextBox { Left = 150, Top = 70, Width = 300, ReadOnly = true };
            txtApellido = new TextBox { Left = 150, Top = 110, Width = 300, ReadOnly = true };
            
            txtMonto = new TextBox { Left = 150, Top = 150, Width = 300 };
            txtMonto.Text = VALOR_CUOTA.ToString("0.00");
            txtMonto.ReadOnly = true;

            rbEfectivo = new RadioButton { Text = "Efectivo", Left = 150, Top = 190, Checked = true };
            rbTarjeta = new RadioButton { Text = "Tarjeta de crédito", Left = 250, Top = 190 };

            btnPagar = new Button { Text = "Registrar Pago", Left = 180, Top = 250, Width = 140 };
            btnPagar.Click += BtnPagar_Click;
            btnPagar.Enabled = false;

            // Agregar controles al formulario
            this.Controls.AddRange(new Control[] {
                lblDniBuscar, lblNombre, lblApellido, lblMonto, lblMetodoPago,
                txtDniBuscar, btnBuscar, txtNombre, txtApellido, txtMonto,
                rbEfectivo, rbTarjeta, btnPagar
            });
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
                        "SELECT NroSocio, Nombre, Apellido FROM Socios WHERE Dni = @dni AND EstadoActivo = 1", conn);
                    cmd.Parameters.AddWithValue("@dni", txtDniBuscar.Text.Trim());

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            _nroSocio = Convert.ToInt32(reader["NroSocio"]);
                            txtNombre.Text = reader["Nombre"].ToString();
                            txtApellido.Text = reader["Apellido"].ToString();
                            btnPagar.Enabled = true;
                        }
                        else
                        {
                            MessageBox.Show("No se encontró un socio activo con ese DNI", "Búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LimpiarCampos();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error al buscar socio: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPagar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;

            try
            {
                var metodoPago = rbEfectivo.Checked ? "EFECTIVO" : "TARJETA_CREDITO";
                var monto = decimal.Parse(txtMonto.Text);

                if (monto != VALOR_CUOTA)
                {
                    MessageBox.Show(string.Format("El monto debe ser exactamente {0:C}", VALOR_CUOTA), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (var conn = _dbHelper.GetConnection())
                {
                    var transaction = conn.BeginTransaction();
                    try
                    {
                        // Obtener fecha de vencimiento actual
                        DateTime fechaVencimientoActual;
                        using (var cmd = new SQLiteCommand(
                            "SELECT FechaVencimientoCuota FROM Socios WHERE NroSocio = @nroSocio", conn))
                        {
                            cmd.Parameters.AddWithValue("@nroSocio", _nroSocio);
                            fechaVencimientoActual = Convert.ToDateTime(cmd.ExecuteScalar());
                        }

                        // Calcular nueva fecha de vencimiento
                        DateTime nuevaFechaVencimiento = fechaVencimientoActual.AddMonths(1);

                        // Registrar el pago
                        using (var cmd = new SQLiteCommand(
                            "INSERT INTO Cuotas (NroSocio, Monto, FechaPago, FechaVencimiento, MetodoPago, Pagada) " +
                            "VALUES (@nroSocio, @monto, @fechaPago, @fechaVen, @metodo, 1)", conn))
                        {
                            cmd.Parameters.AddWithValue("@nroSocio", _nroSocio);
                            cmd.Parameters.AddWithValue("@monto", monto);
                            cmd.Parameters.AddWithValue("@fechaPago", DateTime.Now);
                            cmd.Parameters.AddWithValue("@fechaVen", nuevaFechaVencimiento);
                            cmd.Parameters.AddWithValue("@metodo", metodoPago);
                            cmd.ExecuteNonQuery();
                        }

                        // Actualizar fecha en Socios
                        using (var cmd = new SQLiteCommand(
                            "UPDATE Socios SET FechaVencimientoCuota = @nuevaFecha WHERE NroSocio = @nroSocio", conn))
                        {
                            cmd.Parameters.AddWithValue("@nuevaFecha", nuevaFechaVencimiento);
                            cmd.Parameters.AddWithValue("@nroSocio", _nroSocio);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        // Mostrar resumen del pago
                        string resumen = string.Format(
                            "Pago registrado exitosamente:\n\n" +
                            "Socio: {0}, {1}\n" +
                            "Monto: {2:C}\n" +
                            "Método: {3}\n" +
                            "Nuevo vencimiento: {4:dd/MM/yyyy}",
                            txtApellido.Text, txtNombre.Text,
                            monto,
                            metodoPago,
                            nuevaFechaVencimiento);

                        MessageBox.Show(resumen, "Pago Registrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LimpiarCampos();

                        // Después del pago exitoso:
                        string actualizarEstado = @"
                              UPDATE Socios 
                              SET EstadoActivo = 1 
                              WHERE NroSocio = @nroSocio";

                        using (var cmd = new SQLiteCommand(actualizarEstado, conn))
                        {
                            cmd.Parameters.AddWithValue("@nroSocio", _nroSocio);
                            cmd.ExecuteNonQuery();
                        }

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error al registrar pago: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarCampos()
        {
            if (_nroSocio == 0)
            {
                MessageBox.Show("Primero busque un socio válido", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void LimpiarCampos()
        {
            txtNombre.Clear();
            txtApellido.Clear();
            _nroSocio = 0;
            btnPagar.Enabled = false;
            txtDniBuscar.Focus();
        }
    }
}