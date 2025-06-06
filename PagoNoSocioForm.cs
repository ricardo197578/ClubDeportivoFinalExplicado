using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace ClubManagement
{
    public class PagoNoSocioForm : Form
    {


        // Controles del formulario
        private Label lblDni;
        private TextBox txtDni;
        private Button btnBuscarNoSocio;
        private Label lblIdNoSocio;
        private TextBox txtIdNoSocio;
        private Label lblNombreNoSocio;
        private TextBox txtNombreNoSocio;
        private Label lblActividad;
        private ComboBox cmbActividades;
        private Label lblMonto;
        private TextBox txtMonto;
        private GroupBox groupBoxMetodoPago;
        private RadioButton rbTarjeta;
        private RadioButton rbEfectivo;
        private Button btnPagar;
        private Button btnCancelar;

        //private DatabaseHelper dbHelper = new DatabaseHelper();

        private readonly DatabaseHelper _dbHelper;



        public PagoNoSocioForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            InitializeComponent();
            CargarActividades();
        }

        private void InitializeComponent()
        {
            // Configuración básica del formulario
            this.Text = "Registrar Pago No Socio";
            this.ClientSize = new Size(350, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // lblDni
            lblDni = new Label();
            lblDni.Text = "DNI:";
            lblDni.Location = new Point(20, 20);
            lblDni.Size = new Size(50, 20);
            this.Controls.Add(lblDni);

            // txtDni
            txtDni = new TextBox();
            txtDni.Location = new Point(80, 20);
            txtDni.Size = new Size(150, 20);
            this.Controls.Add(txtDni);

            // btnBuscarNoSocio
            btnBuscarNoSocio = new Button();
            btnBuscarNoSocio.Text = "Buscar";
            btnBuscarNoSocio.Location = new Point(240, 20);
            btnBuscarNoSocio.Size = new Size(80, 25);
            btnBuscarNoSocio.Click += btnBuscarNoSocio_Click;
            this.Controls.Add(btnBuscarNoSocio);

            // lblIdNoSocio
            lblIdNoSocio = new Label();
            lblIdNoSocio.Text = "ID No Socio:";
            lblIdNoSocio.Location = new Point(20, 60);
            lblIdNoSocio.Size = new Size(80, 20);
            this.Controls.Add(lblIdNoSocio);

            // txtIdNoSocio
            txtIdNoSocio = new TextBox();
            txtIdNoSocio.Location = new Point(120, 60);
            txtIdNoSocio.Size = new Size(80, 20);
            txtIdNoSocio.ReadOnly = true;
            this.Controls.Add(txtIdNoSocio);

            // lblNombreNoSocio
            lblNombreNoSocio = new Label();
            lblNombreNoSocio.Text = "Nombre:";
            lblNombreNoSocio.Location = new Point(20, 100);
            lblNombreNoSocio.Size = new Size(80, 20);
            this.Controls.Add(lblNombreNoSocio);

            // txtNombreNoSocio
            txtNombreNoSocio = new TextBox();
            txtNombreNoSocio.Location = new Point(80, 100);
            txtNombreNoSocio.Size = new Size(240, 20);
            txtNombreNoSocio.ReadOnly = true;
            this.Controls.Add(txtNombreNoSocio);

            // lblActividad
            lblActividad = new Label();
            lblActividad.Text = "Actividad:";
            lblActividad.Location = new Point(20, 140);
            lblActividad.Size = new Size(80, 20);
            this.Controls.Add(lblActividad);

            // cmbActividades
            cmbActividades = new ComboBox();
            cmbActividades.Location = new Point(100, 140);
            cmbActividades.Size = new Size(220, 21);
            cmbActividades.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbActividades.SelectedIndexChanged += cmbActividades_SelectedIndexChanged;
            this.Controls.Add(cmbActividades);

            // lblMonto
            lblMonto = new Label();
            lblMonto.Text = "Monto:";
            lblMonto.Location = new Point(20, 180);
            lblMonto.Size = new Size(80, 20);
            this.Controls.Add(lblMonto);

            // txtMonto
            txtMonto = new TextBox();
            txtMonto.Location = new Point(80, 180);
            txtMonto.Size = new Size(100, 20);
            txtMonto.ReadOnly = true;
            this.Controls.Add(txtMonto);

            // groupBoxMetodoPago
            groupBoxMetodoPago = new GroupBox();
            groupBoxMetodoPago.Text = "Método de Pago";
            groupBoxMetodoPago.Location = new Point(20, 220);
            groupBoxMetodoPago.Size = new Size(300, 60);
            this.Controls.Add(groupBoxMetodoPago);

            // rbEfectivo
            rbEfectivo = new RadioButton();
            rbEfectivo.Text = "Efectivo";
            rbEfectivo.Location = new Point(20, 20);
            rbEfectivo.Checked = true;
            groupBoxMetodoPago.Controls.Add(rbEfectivo);

            // rbTarjeta
            rbTarjeta = new RadioButton();
            rbTarjeta.Text = "Tarjeta de Crédito";
            rbTarjeta.Location = new Point(120, 20);
            groupBoxMetodoPago.Controls.Add(rbTarjeta);

            // btnPagar
            btnPagar = new Button();
            btnPagar.Text = "Pagar";
            btnPagar.Location = new Point(80, 300);
            btnPagar.Size = new Size(80, 30);
            btnPagar.Click += btnPagar_Click;
            this.Controls.Add(btnPagar);

            // btnCancelar
            btnCancelar = new Button();
            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new Point(180, 300);
            btnCancelar.Size = new Size(80, 30);
            btnCancelar.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancelar);
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
                        cmbActividades.Items.Add(new ActividadItem
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
                MessageBox.Show("Error al cargar actividades: " + ex.Message);
            }
        }

        private void btnBuscarNoSocio_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = _dbHelper.GetConnection())
                {
                    var cmd = new SQLiteCommand(
                        "SELECT Id, Nombre || ' ' || Apellido as NombreCompleto FROM NoSocios WHERE Dni = @dni", conn);
                    cmd.Parameters.AddWithValue("@dni", txtDni.Text);

                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtIdNoSocio.Text = reader["Id"].ToString();
                        txtNombreNoSocio.Text = reader["NombreCompleto"].ToString();
                    }
                    else
                    {
                        MessageBox.Show("No socio no encontrado");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void cmbActividades_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbActividades.SelectedItem != null)
            {
                var item = (ActividadItem)cmbActividades.SelectedItem;
                txtMonto.Text = item.Precio.ToString("0.00");
            }
        }

        private void btnPagar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;

            try
            {
                int idNoSocio = Convert.ToInt32(txtIdNoSocio.Text);
                var actividad = (ActividadItem)cmbActividades.SelectedItem;
                var metodoPago = rbEfectivo.Checked ? MetodoPago.EFECTIVO : MetodoPago.TARJETA_CREDITO;

                using (var conn = _dbHelper.GetConnection())
                {
                    var cmd = new SQLiteCommand(
                        "INSERT INTO ActividadesNoSocios (IdNoSocio, IdActividad, Fecha, MetodoPago, Monto) " +
                        "VALUES (@idNoSocio, @idAct, @fecha, @metodo, @monto)", conn);

                    cmd.Parameters.AddWithValue("@idNoSocio", idNoSocio);
                    cmd.Parameters.AddWithValue("@idAct", actividad.Value);
                    cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                    cmd.Parameters.AddWithValue("@metodo", metodoPago.ToString());
                    cmd.Parameters.AddWithValue("@monto", actividad.Precio);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Pago registrado exitosamente");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtIdNoSocio.Text))
            {
                MessageBox.Show("Busque un no socio primero");
                return false;
            }
            if (cmbActividades.SelectedItem == null)
            {
                MessageBox.Show("Seleccione una actividad");
                return false;
            }
            return true;
        }
    }

    public class ActividadItem
    {
        public string Text { get; set; }
        public int Value { get; set; }
        public decimal Precio { get; set; }
        public override string ToString() { return Text; }
    }
}