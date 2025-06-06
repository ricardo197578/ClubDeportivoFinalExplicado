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

    public partial class PagoCuotaSocioForm : Form
    {
       
        private readonly DatabaseHelper _dbHelper;

        private ComboBox cmbSocios;
        private TextBox txtMonto;
        private RadioButton rbEfectivo;
        private RadioButton rbTarjeta;
        private Button btnPagar;
        private Label lblSocio;
        private Label lblMonto;
        private GroupBox gbMetodo;

       

        public PagoCuotaSocioForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            InitializeComponent();
            CargarSocios();
            
        }

        private void InitializeComponent()
        {
            this.Text = "Pago de Cuota";
            this.Width = 400;
            this.Height = 300;
            this.StartPosition = FormStartPosition.CenterScreen;

            lblSocio = new Label
            {
                Text = "Seleccione Socio:",
                Left = 20,
                Top = 20,
                Width = 120
            };
            this.Controls.Add(lblSocio);

            cmbSocios = new ComboBox
            {
                Left = 150,
                Top = 18,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cmbSocios);

            lblMonto = new Label
            {
                Text = "Monto a Pagar:",
                Left = 20,
                Top = 60,
                Width = 120
            };
            this.Controls.Add(lblMonto);

            txtMonto = new TextBox
            {
                Left = 150,
                Top = 58,
                Width = 200
            };
            this.Controls.Add(txtMonto);

            gbMetodo = new GroupBox
            {
                Text = "Método de Pago",
                Left = 20,
                Top = 100,
                Width = 330,
                Height = 70
            };
            this.Controls.Add(gbMetodo);

            rbEfectivo = new RadioButton
            {
                Text = "Efectivo",
                Left = 20,
                Top = 30,
                Width = 100,
                Checked = true
            };
            gbMetodo.Controls.Add(rbEfectivo);

            rbTarjeta = new RadioButton
            {
                Text = "Tarjeta de Crédito",
                Left = 150,
                Top = 30,
                Width = 150
            };
            gbMetodo.Controls.Add(rbTarjeta);

            btnPagar = new Button
            {
                Text = "Pagar",
                Left = 150,
                Top = 190,
                Width = 100
            };
            btnPagar.Click += new EventHandler(btnPagar_Click);
            this.Controls.Add(btnPagar);
        }

        private void CargarSocios()
        {
            try
            {
                cmbSocios.Items.Clear();
                //using (var dbHelper = new DatabaseHelper())
                using (var conn = _dbHelper.GetConnection())
                {
                    var cmd = new SQLiteCommand("SELECT NroSocio, Nombre || ' ' || Apellido AS NombreCompleto FROM Socios WHERE EstadoActivo = 1", conn);
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        cmbSocios.Items.Add(new ComboBoxItem
                        {
                            Text = reader["NombreCompleto"].ToString(),
                            Value = Convert.ToInt32(reader["NroSocio"])
                        });
                    }
                }

                if (cmbSocios.Items.Count > 0)
                    cmbSocios.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error al cargar socios:{0}", ex.Message));
            }
        }

        private void btnPagar_Click(object sender, EventArgs e)
        {
            if (cmbSocios.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un socio");
                return;
            }

            ComboBoxItem selected = (ComboBoxItem)cmbSocios.SelectedItem;
            int nroSocio = selected.Value;

            decimal monto;
            if (!decimal.TryParse(txtMonto.Text, out monto))
            {
                MessageBox.Show("Monto inválido");
                return;
            }

            MetodoPago metodoPago = rbEfectivo.Checked ? MetodoPago.EFECTIVO : MetodoPago.TARJETA_CREDITO;

            try
            {
                //using (var dbHelper = new DatabaseHelper())
                using (var conn = _dbHelper.GetConnection())
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SQLiteCommand(
                            "INSERT INTO Cuotas (NroSocio, Monto, FechaPago, FechaVencimiento, MetodoPago, Pagada) " +
                            "VALUES (@nroSocio, @monto, @fechaPago, @fechaVen, @metodo, 1)", conn);

                        DateTime fechaVencimiento = DateTime.Now.AddMonths(1);

                        cmd.Parameters.AddWithValue("@nroSocio", nroSocio);
                        cmd.Parameters.AddWithValue("@monto", monto);
                        cmd.Parameters.AddWithValue("@fechaPago", DateTime.Now);
                        cmd.Parameters.AddWithValue("@fechaVen", fechaVencimiento);
                        cmd.Parameters.AddWithValue("@metodo", metodoPago.ToString());
                        cmd.ExecuteNonQuery();

                        cmd = new SQLiteCommand("UPDATE Socios SET FechaVencimientoCuota = @fechaVen WHERE NroSocio = @nroSocio", conn);
                        cmd.Parameters.AddWithValue("@fechaVen", fechaVencimiento);
                        cmd.Parameters.AddWithValue("@nroSocio", nroSocio);
                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        MessageBox.Show("Pago registrado exitosamente");
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Error al procesar pago: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error general: " + ex.Message);
            }
        }


        public class ComboBoxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}


