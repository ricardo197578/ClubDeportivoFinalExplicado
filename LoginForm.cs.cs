using System;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace ClubManagement
{
    public class LoginForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblMensaje;

        public LoginForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            InitializeComponent();
            InitializeAdminUser();
            		
        }

        private void InitializeComponent()
        {
            this.Text = "Login Administrador";
            this.Size = new System.Drawing.Size(350, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblTitulo = new Label
            {
                Text = "Administrador",
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(80, 20)
            };

            Label lblUsuario = new Label
            {
                Text = "Usuario:",
                Location = new System.Drawing.Point(30, 70),
                AutoSize = true
            };

            txtUsuario = new TextBox
            {
                Location = new System.Drawing.Point(100, 70),
                Size = new System.Drawing.Size(200, 20)
            };

            Label lblPassword = new Label
            {
                Text = "Contraseña:",
                Location = new System.Drawing.Point(30, 110),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new System.Drawing.Point(100, 110),
                Size = new System.Drawing.Size(200, 20),
                PasswordChar = '*'
            };

            btnLogin = new Button
            {
                Text = "Ingresar",
                Size = new System.Drawing.Size(100, 30),
                Location = new System.Drawing.Point(120, 150)
            };
            btnLogin.Click += BtnLogin_Click;

            lblMensaje = new Label
            {
                Location = new System.Drawing.Point(30, 190),
                Size = new System.Drawing.Size(270, 20),
                ForeColor = System.Drawing.Color.Red,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblUsuario);
            this.Controls.Add(txtUsuario);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(lblMensaje);
        }

        private void InitializeAdminUser()
        {
            string usuarioDefault = "admin";
            string passwordDefault = "admin123";

            string sql = "SELECT COUNT(*) FROM Administradores WHERE Usuario = @Usuario";
            using (var cmd = new SQLiteCommand(sql, _dbHelper.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@Usuario", usuarioDefault);
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count == 0)
                {
                    string passwordHash = HashPassword(passwordDefault);
                    sql = "INSERT INTO Administradores (Usuario, PasswordHash, Nombre) VALUES (@Usuario, @PasswordHash, @Nombre)";
                    using (var insertCmd = new SQLiteCommand(sql, _dbHelper.GetConnection()))
                    {
                        insertCmd.Parameters.AddWithValue("@Usuario", usuarioDefault);
                        insertCmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        insertCmd.Parameters.AddWithValue("@Nombre", "Administrador Principal");
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                lblMensaje.Text = "Usuario y contraseña son requeridos";
                return;
            }

            string sql = "SELECT PasswordHash FROM Administradores WHERE Usuario = @Usuario";
            using (var cmd = new SQLiteCommand(sql, _dbHelper.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@Usuario", usuario);
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    string storedHash = result.ToString();
                    string inputHash = HashPassword(password);

                    if (storedHash == inputHash)
                    {
                        // Actualizar último login
                        sql = "UPDATE Administradores SET UltimoLogin = datetime('now') WHERE Usuario = @Usuario";
                        using (var updateCmd = new SQLiteCommand(sql, _dbHelper.GetConnection()))
                        {
                            updateCmd.Parameters.AddWithValue("@Usuario", usuario);
                            updateCmd.ExecuteNonQuery();
                        }

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        lblMensaje.Text = "Contraseña incorrecta";
                    }
                }
                else
                {
                    lblMensaje.Text = "Usuario no encontrado";
                }
            }
        }
    }
}