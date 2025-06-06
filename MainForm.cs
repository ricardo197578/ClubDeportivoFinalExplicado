using System;
using System.Windows.Forms;

namespace ClubManagement
{
    public class MainForm : Form
    {
        private Button btnRegistrarSocio;
        private Button btnRegistrarNoSocio;
        private Button btnPagoCuotaSocio;
        private Button btnPagoNoSocio;
        private Button btnListarVencimientos;

        private readonly DatabaseHelper _dbHelper;

        public MainForm()
        {
            _dbHelper = new DatabaseHelper();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Gestión del Club";
            this.Size = new System.Drawing.Size(300, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            btnRegistrarSocio = new Button
            {
                Text = "Registrar Socio",
                Size = new System.Drawing.Size(200, 30),
                Location = new System.Drawing.Point(40, 20)
            };
            btnRegistrarSocio.Click += btnRegistrarSocio_Click;
            this.Controls.Add(btnRegistrarSocio);

            btnRegistrarNoSocio = new Button
            {
                Text = "Registrar No Socio",
                Size = new System.Drawing.Size(200, 30),
                Location = new System.Drawing.Point(40, 60)
            };
            btnRegistrarNoSocio.Click += btnRegistrarNoSocio_Click;
            this.Controls.Add(btnRegistrarNoSocio);

            btnPagoCuotaSocio = new Button
            {
                Text = "Pagar Cuota Socio",
                Size = new System.Drawing.Size(200, 30),
                Location = new System.Drawing.Point(40, 100)
            };
            btnPagoCuotaSocio.Click += btnPagoCuotaSocio_Click;
            this.Controls.Add(btnPagoCuotaSocio);

            btnPagoNoSocio = new Button
            {
                Text = "Pagar Cuota No Socio",
                Size = new System.Drawing.Size(200, 30),
                Location = new System.Drawing.Point(40, 140)
            };
            btnPagoNoSocio.Click += btnPagoNoSocio_Click;
            this.Controls.Add(btnPagoNoSocio);

            btnListarVencimientos = new Button
            {
                Text = "Listar Vencimientos",
                Size = new System.Drawing.Size(200, 30),
                Location = new System.Drawing.Point(40, 180)
            };
            btnListarVencimientos.Click += btnListarVencimientos_Click;
            this.Controls.Add(btnListarVencimientos);
        }

        private void btnRegistrarSocio_Click(object sender, EventArgs e)
        {
            using (var form = new RegistrarSocioForm(_dbHelper))
            {
                form.ShowDialog();
            }
        }

        private void btnRegistrarNoSocio_Click(object sender, EventArgs e)
        {
            using (var form = new RegistrarNoSocioForm(_dbHelper))
            {
                form.ShowDialog();
            }
        }

        private void btnPagoCuotaSocio_Click(object sender, EventArgs e)
        {
            using (var form = new PagoCuotaSocioForm(_dbHelper))
            {
                form.ShowDialog();
            }
        }

        private void btnPagoNoSocio_Click(object sender, EventArgs e)
        {
            using (var form = new PagoNoSocioForm(_dbHelper))
            {
                form.ShowDialog();
            }
        }

        private void btnListarVencimientos_Click(object sender, EventArgs e)
        {
            using (var vencimientosForm = new ListarVencimientosForm(_dbHelper))
            {
                vencimientosForm.ShowDialog();
            }
        }
    }
}
