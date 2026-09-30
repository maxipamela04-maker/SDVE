using SDVE.Core;

namespace SDVE.App;

public partial class frmLogin : Form
{
    private TextBox txtCodigo = new();
    private Button btnIngresar = new();
    private Label lblError = new();

    public frmLogin()
    {
        InitializeComponent();
        ConstruirInterfaz();
    }

    private void ConstruirInterfaz()
    {
        Text = "SDVE - Identificación del votante";
        Width = 420;
        Height = 220;
        StartPosition = FormStartPosition.CenterScreen;

        var lblTitulo = new Label
        {
            Text = "Ingresa tu código de alumno:",
            Location = new Point(30, 30),
            AutoSize = true
        };

        txtCodigo.Location = new Point(30, 60);
        txtCodigo.Width = 200;

        btnIngresar.Text = "Ingresar";
        btnIngresar.Location = new Point(30, 100);
        btnIngresar.Size = new Size(120, 35);
        btnIngresar.Click += BtnIngresar_Click;

        lblError.Location = new Point(30, 140);
        lblError.AutoSize = true;
        lblError.ForeColor = Color.Red;
        lblError.MaximumSize = new Size(350, 0);

        Controls.Add(lblTitulo);
        Controls.Add(txtCodigo);
        Controls.Add(btnIngresar);
        Controls.Add(lblError);
    }

    private void BtnIngresar_Click(object? sender, EventArgs e)
    {
        lblError.Text = "";
        var servicio = new VotingService();
        var alumno = servicio.BuscarAlumno(txtCodigo.Text);

        if (alumno is null)
        {
            lblError.Text = "Código no encontrado en el padrón.";
            return;
        }

        var yaVotadas = servicio.ConvocatoriasYaVotadas(alumno.Codigo);
        if (yaVotadas.Count >= 3)
        {
            lblError.Text = "Ya emitiste tu voto en todas las elecciones disponibles.";
            return;
        }

        var papeleta = new frmPapeleta(alumno, yaVotadas);
        papeleta.ShowDialog();

        // Después de votar, se limpia el campo para el siguiente votante
        txtCodigo.Text = "";
        lblError.Text = "";
    }

    private void frmLogin_Load(object sender, EventArgs e)
    {

    }
}