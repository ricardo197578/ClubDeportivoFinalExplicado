/*
Patrón de diseño:

Sigue el patrón de separación de responsabilidades, 
delegando las operaciones de base de datos a DatabaseHelper.

Flujo de trabajo:
Cuando se crea una instancia de MainForm, automáticamente:
Se prepara la conexión con la base de datos
Se configura la interfaz gráfica

Eventos:
Los botones declarados tendrán manejadores de eventos como el btnRegistrarSocio_Click 
para realizar acciones cuando se haga clic en ellos.

Esta clase sería el núcleo principal de la aplicación, 
coordinando la interfaz de usuario con la capa de acceso a datos a través de DatabaseHelper.

*/


using System;
using System.Windows.Forms;

namespace ClubManagement
{
    public class MainForm : Form
    {
	//propiedades(campo) de la clase 
        private Button btnRegistrarSocio;
        private Button btnRegistrarNoSocio;
        private Button btnPagoCuotaSocio;
        private Button btnPagoActividadNoSocio;
        private Button btnListarVencimientos;
      
        //Declara un campo _dbHelper de tipo DatabaseHelper (clase auxiliar para operaciones con base de datos). 
	//El modificador readonly indica que esta variable solo puede asignarse en el constructor.
       
	 private readonly DatabaseHelper _dbHelper;
 
        //Constructor
        public MainForm()
        {
            _dbHelper = new DatabaseHelper();//Crea una nueva instancia de DatabaseHelper y la asigna a _dbHelper.
            InitializeComponent();//Llamada a InitializeComponent()
        }
     
         //se encarga de inicializar y configurar los componentes de la interfaz gráfica de usuario.
        private void InitializeComponent()
        {
            this.Text = "Gestión del Club";//this.Text: Establece el título de la ventana como "Gestión del Club"
            this.Size = new System.Drawing.Size(300, 350); //this.Size: Define el tamaño de la ventana a 300 píxeles de ancho y 350 de alto
            this.StartPosition = FormStartPosition.CenterScreen;//this.StartPosition: Coloca la ventana en el centro de la pantalla al iniciarse
		
	    //Creación y configuración del botón
            btnRegistrarSocio = new Button
            {
                Text = "Registrar Socio/Generacion de Carnet",//Text: El texto que muestra el botón
                Size = new System.Drawing.Size(200, 30),//Size: Tamaño del botón (200x30 píxeles)
                Location = new System.Drawing.Point(40, 20)//Posición dentro del formulario (40 píxeles desde la izquierda, 20 desde arriba)
            };
            btnRegistrarSocio.Click += btnRegistrarSocio_Click;//Asocia el evento Click del botón con el método btnRegistrarSocio_Click
            this.Controls.Add(btnRegistrarSocio);//Añade el botón creado a la colección de controles del formulario, haciendo que sea visible.

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

            
            btnPagoActividadNoSocio = new Button
            {
                Text = "Cobro Actividad No Socio",
                Size = new System.Drawing.Size(200, 30),
                Location = new System.Drawing.Point(40, 140)
            };
            btnPagoActividadNoSocio.Click += btnPagoActividadNoSocio_Click;
            this.Controls.Add(btnPagoActividadNoSocio);

            btnListarVencimientos = new Button
            {
                Text = "Listar Vencimientos",
                Size = new System.Drawing.Size(200, 30),
                Location = new System.Drawing.Point(40, 180)
            };
            btnListarVencimientos.Click += btnListarVencimientos_Click;
            this.Controls.Add(btnListarVencimientos);
        }


	//Manejador de eventos (event handler) que se ejecuta cuando el usuario hace clic en el botón 
	//"Registrar Socio" de la aplicación
       
	 private void btnRegistrarSocio_Click(object sender, EventArgs e)

	//Parámetros del evento:object sender: Una referencia al objeto que generó el evento 
	//(en este caso, el botón btnRegistrarSocio)
	//EventArgs e: Objeto que contiene información adicional sobre el evento

        {
		//Bloque usingCrea una nueva instancia de RegistrarSocioForm (un formulario para registrar socios)
		//El bloque using garantiza que los recursos del formulario se liberen correctamente cuando ya no 
		//se necesiten
                //Pasa _dbHelper como parámetro al constructor, permitiendo que el nuevo formulario acceda a la 
		//misma conexión de base de datos

            using (var form = new RegistrarSocioForm(_dbHelper))
            {
                form.ShowDialog();//form.ShowDialog(): Muestra el formulario como un diálogo modal

		//Modal: Bloquea la interacción con el formulario padre hasta que se cierre este formulario secundario
		//Esto es importante para flujos donde necesitas que el usuario complete el registro antes de continuar
            }
	/*

	Flujo de Operación
	El usuario hace clic en el botón "Registrar Socio"
	Se crea una nueva instancia de RegistrarSocioForm
	Se muestra el formulario de registro (modalmente)
	Cuando el usuario cierra el formulario de registro:
	Todos los recursos se liberan automáticamente (gracias al using)
	El control vuelve al formulario principal
	
	Reutilización de conexión: Comparte _dbHelper con el formulario hijo
	Gestión de recursos: Usa using para disposición automática
	Modalidad adecuada: Usa ShowDialog() cuando se necesita atención exclusiva del usuario


	*/
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

        private void btnPagoActividadNoSocio_Click(object sender, EventArgs e)
        {
            using (var form = new PagoActividadNoSocioForm(_dbHelper))
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