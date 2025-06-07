using System;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Collections.Generic;

namespace ClubManagement
{
    public enum MetodoPago
    {
        EFECTIVO,
        TARJETA_CREDITO
    }

    public partial class PagoCuotaSocioForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private List<Socio> _todosLosSocios = new List<Socio>();
        private readonly decimal VALOR_CUOTA = 5000.00m;//deberiamos cambiar????

        // Controles de la interfaz
        private ListView lvwSocios;
        private TextBox txtBuscarSocio;
        private TextBox txtMonto;
        private RadioButton rbEfectivo;
        private RadioButton rbTarjeta;
        private Button btnPagar;
        private Label lblBuscar;
        private Label lblMonto;
        private GroupBox gbMetodo;

        // Clase para representar un socio
        private class Socio
        {
            public int NroSocio { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public DateTime FechaVencimiento { get; set; }
        }

        public PagoCuotaSocioForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            InitializeComponent();
            CargarSocios();
        }

        private void InitializeComponent()
        {
            // Configuración inicial del formulario
            this.Text = "Pago de Cuota";
            this.Width = 500;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Control de búsqueda
            lblBuscar = new Label();
            lblBuscar.Text = "Buscar Socio:";
            lblBuscar.Left = 20;
            lblBuscar.Top = 20;
            lblBuscar.Width = 100;
            this.Controls.Add(lblBuscar);

            txtBuscarSocio = new TextBox();
            txtBuscarSocio.Left = 120;
            txtBuscarSocio.Top = 18;
            txtBuscarSocio.Width = 350;
            txtBuscarSocio.TextChanged += new EventHandler(TxtBuscarSocio_TextChanged);
            this.Controls.Add(txtBuscarSocio);

            // ListView para mostrar socios
            lvwSocios = new ListView();
            lvwSocios.View = View.Details;
            lvwSocios.FullRowSelect = true;
            lvwSocios.GridLines = true;
            lvwSocios.Left = 20;
            lvwSocios.Top = 50;
            lvwSocios.Width = 450;
            lvwSocios.Height = 150;
            lvwSocios.MultiSelect = false;

            // Columnas del ListView
            lvwSocios.Columns.Add("N° Socio", 80, HorizontalAlignment.Left);
            lvwSocios.Columns.Add("Nombre", 200, HorizontalAlignment.Left);
            lvwSocios.Columns.Add("Vencimiento", 150, HorizontalAlignment.Left);

            this.Controls.Add(lvwSocios);

            // Monto a pagar
            lblMonto = new Label();
            lblMonto.Text = "Monto a Pagar:";
            lblMonto.Left = 20;
            lblMonto.Top = 210;
            lblMonto.Width = 100;
            this.Controls.Add(lblMonto);

            txtMonto = new TextBox();
            txtMonto.Left = 120;
            txtMonto.Top = 208;
            txtMonto.Width = 200;
            this.Controls.Add(txtMonto);

            // Grupo de métodos de pago
            gbMetodo = new GroupBox();
            gbMetodo.Text = "Método de Pago";
            gbMetodo.Left = 20;
            gbMetodo.Top = 240;
            gbMetodo.Width = 450;
            gbMetodo.Height = 70;

            // RadioButton para efectivo
            rbEfectivo = new RadioButton();
            rbEfectivo.Text = "Efectivo";
            rbEfectivo.Left = 20;
            rbEfectivo.Top = 20;
            rbEfectivo.Width = 100;
            rbEfectivo.Checked = true;
            gbMetodo.Controls.Add(rbEfectivo);

            // RadioButton para tarjeta
            rbTarjeta = new RadioButton();
            rbTarjeta.Text = "Tarjeta de Crédito";
            rbTarjeta.Left = 150;
            rbTarjeta.Top = 20;
            rbTarjeta.Width = 150;
            gbMetodo.Controls.Add(rbTarjeta);

            this.Controls.Add(gbMetodo);

            // Botón para realizar el pago
            btnPagar = new Button();
            btnPagar.Text = "Pagar";
            btnPagar.Left = 200;
            btnPagar.Top = 320;
            btnPagar.Width = 100;
            btnPagar.Click += new EventHandler(BtnPagar_Click);
            this.Controls.Add(btnPagar);
        }

        private void CargarSocios()
        {
            try
            {
                _todosLosSocios.Clear();

                using (SQLiteConnection conn = _dbHelper.GetConnection())
                {
                    SQLiteCommand cmd = new SQLiteCommand(
                        "SELECT NroSocio, Nombre, Apellido, FechaVencimientoCuota " +
                        "FROM Socios WHERE EstadoActivo = 1 ORDER BY Apellido, Nombre", conn);
                    SQLiteDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        _todosLosSocios.Add(new Socio
                        {
                            NroSocio = Convert.ToInt32(reader["NroSocio"]),
                            Nombre = reader["Nombre"].ToString(),
                            Apellido = reader["Apellido"].ToString(),
                            FechaVencimiento = Convert.ToDateTime(reader["FechaVencimientoCuota"])
                        });
                    }
                }

                // Mostrar todos los socios inicialmente
                MostrarSociosEnListView(_todosLosSocios);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error al cargar socios: {0}", ex.Message),
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       

        private void TxtBuscarSocio_TextChanged(object sender, EventArgs e)
        {
            string textoBusqueda = txtBuscarSocio.Text.Trim();

            if (string.IsNullOrWhiteSpace(textoBusqueda))
            {
                MostrarSociosEnListView(_todosLosSocios);
                return;
            }

            List<Socio> sociosFiltrados = new List<Socio>();

            foreach (var socio in _todosLosSocios)
            {
                // Buscar por número de socio
                if (textoBusqueda == socio.NroSocio.ToString())
                {
                    sociosFiltrados.Add(socio);
                    continue;
                }

                // Buscar por nombre o apellido
                string nombreCompleto = string.Format("{0} {1}", socio.Nombre, socio.Apellido).ToLower();
                string apellidoNombre = string.Format("{0}, {1}", socio.Apellido, socio.Nombre).ToLower();

                if (nombreCompleto.Contains(textoBusqueda.ToLower()) ||
                    apellidoNombre.Contains(textoBusqueda.ToLower()))
                {
                    sociosFiltrados.Add(socio);
                }
            }

            // Mostrar resultados sin cambiar el foco
            MostrarSociosEnListView(sociosFiltrados, mantenerFoco: true);

            if (sociosFiltrados.Count == 0)
            {
                MessageBox.Show("No se encontraron socios con ese criterio de búsqueda",
                              "Búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void MostrarSociosEnListView(List<Socio> socios, bool mantenerFoco = false)
        {
            // Guardar el estado del foco
            bool focoEnBusqueda = txtBuscarSocio.Focused;

            lvwSocios.BeginUpdate();
            lvwSocios.Items.Clear();

            foreach (var socio in socios)
            {
                ListViewItem item = new ListViewItem(socio.NroSocio.ToString());
                item.SubItems.Add(string.Format("{0}, {1}", socio.Apellido, socio.Nombre));
                item.SubItems.Add(socio.FechaVencimiento.ToString("dd/MM/yyyy"));
                item.Tag = socio.NroSocio;
                lvwSocios.Items.Add(item);
            }

            if (lvwSocios.Items.Count > 0)
            {
                lvwSocios.Items[0].Selected = true;
            }

            lvwSocios.EndUpdate();

            // Restaurar el foco si es necesario
            if (mantenerFoco && focoEnBusqueda)
            {
                txtBuscarSocio.Focus();
            }
        }

        private void BtnPagar_Click(object sender, EventArgs e)
        {
            if (lvwSocios.SelectedItems.Count == 0)
            {
                MessageBox.Show("Seleccione un socio de la lista",
                               "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int nroSocio = (int)lvwSocios.SelectedItems[0].Tag;

            decimal monto;
            if (!decimal.TryParse(txtMonto.Text, out monto) || monto <= 0)
            {
                MessageBox.Show("Ingrese un monto válido mayor a cero",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validar que el monto sea exactamente el valor de la cuota
            if (monto != VALOR_CUOTA)
            {
                MessageBox.Show(string.Format("El monto debe ser exactamente {0:C}", VALOR_CUOTA),
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MetodoPago metodoPago = rbEfectivo.Checked ? MetodoPago.EFECTIVO : MetodoPago.TARJETA_CREDITO;

            try
            {
                using (SQLiteConnection conn = _dbHelper.GetConnection())
                {
                    SQLiteTransaction transaction = conn.BeginTransaction();
                    try
                    {
                        // Obtener fecha de vencimiento actual
                        DateTime fechaVencimientoActual;
                        using (SQLiteCommand cmd = new SQLiteCommand(
                            "SELECT FechaVencimientoCuota FROM Socios WHERE NroSocio = @nroSocio",
                            conn))
                        {
                            cmd.Parameters.AddWithValue("@nroSocio", nroSocio);
                            fechaVencimientoActual = Convert.ToDateTime(cmd.ExecuteScalar());
                        }

                        // Calcular nueva fecha de vencimiento
                        DateTime nuevaFechaVencimiento = fechaVencimientoActual.AddMonths(1);

                        // Registrar el pago
                        using (SQLiteCommand cmd = new SQLiteCommand(
                            "INSERT INTO Cuotas (NroSocio, Monto, FechaPago, FechaVencimiento, MetodoPago, Pagada) " +
                            "VALUES (@nroSocio, @monto, @fechaPago, @fechaVen, @metodo, 1)", conn))
                        {
                            cmd.Parameters.AddWithValue("@nroSocio", nroSocio);
                            cmd.Parameters.AddWithValue("@monto", monto);
                            cmd.Parameters.AddWithValue("@fechaPago", DateTime.Now);
                            cmd.Parameters.AddWithValue("@fechaVen", nuevaFechaVencimiento);
                            cmd.Parameters.AddWithValue("@metodo", metodoPago.ToString());
                            cmd.ExecuteNonQuery();
                        }

                        //Actualizar fecha en Socios
                        using (SQLiteCommand cmd = new SQLiteCommand(
                            "UPDATE Socios SET FechaVencimientoCuota = @nuevaFecha WHERE NroSocio = @nroSocio",
                            conn))
                        {
                            cmd.Parameters.AddWithValue("@nuevaFecha", nuevaFechaVencimiento);
                            cmd.Parameters.AddWithValue("@nroSocio", nroSocio);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        // Mostrar resumen del pago
                        string resumen = string.Format(
                            "Pago registrado exitosamente:\n\n" +
                            "Socio: {0}\n" +
                            "Monto: {1:C}\n" +
                            "Método: {2}\n" +
                            "Nuevo vencimiento: {3:dd/MM/yyyy}",
                            lvwSocios.SelectedItems[0].SubItems[1].Text,
                            monto,
                            metodoPago,
                            nuevaFechaVencimiento);

                        MessageBox.Show(resumen, "Pago Registrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show(string.Format("Error al procesar pago: {0}", ex.Message),
                                      "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error general: {0}", ex.Message),
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

