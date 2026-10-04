using SDVE.Core;

namespace SDVE.App;

public partial class frmPapeleta : Form
{
    private readonly Alumno _alumno;
    private readonly List<int> _yaVotadas;
    private readonly VotingService _servicio = new();

    // Guardamos, por convocatoria, sus controles para leerlos al confirmar
    private readonly Dictionary<int, CheckBox> _checks = new();
    private readonly Dictionary<int, Panel> _paneles = new();
    private readonly Dictionary<int, RadioButton> _blancoPorConv = new();
    private readonly Dictionary<int, RadioButton> _otroPorConv = new();
    private readonly Dictionary<int, TextBox> _txtOtroPorConv = new();
    private readonly Dictionary<int, List<(RadioButton rb, int candidatoId)>> _radiosCandidatos = new();

    private readonly (int Id, string Nombre)[] _convocatorias =
    {
        (1, "Sociedad de Alumnos"),
        (2, "Consejo Universitario"),
        (3, "Consejo de Representantes")
    };

    private Button btnConfirmar = new();
    private Label lblMensaje = new();

    public frmPapeleta(Alumno alumno, List<int> yaVotadas)
    {
        _alumno = alumno;
        _yaVotadas = yaVotadas;
        InitializeComponent();
        ConstruirInterfaz();
    }

    private void ConstruirInterfaz()
    {
        Text = $"Papeleta - {_alumno.Nombre}";
        Width = 560;
        Height = 800;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScroll = true;

        int y = 20;
        foreach (var conv in _convocatorias)
        {
            bool bloqueada = _yaVotadas.Contains(conv.Id);

            var chk = new CheckBox
            {
                Text = bloqueada ? $"{conv.Nombre} (ya votaste)" : conv.Nombre,
                Location = new Point(20, y),
                AutoSize = true,
                Enabled = !bloqueada
            };
            chk.CheckedChanged += (s, e) => ActualizarPanel(conv.Id);
            _checks[conv.Id] = chk;
            Controls.Add(chk);
            y += 30;

            var panel = new Panel
            {
                Location = new Point(40, y),
                Width = 460,
                Height = 130,
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = false
            };
            _paneles[conv.Id] = panel;
            Controls.Add(panel);

            ConstruirOpcionesConvocatoria(conv.Id, panel);

            y += 145;
        }

        btnConfirmar.Text = "Confirmar voto";
        btnConfirmar.Location = new Point(20, y + 10);
        btnConfirmar.Size = new Size(160, 35);
        btnConfirmar.Click += BtnConfirmar_Click;
        Controls.Add(btnConfirmar);

        lblMensaje.Location = new Point(20, y + 50);
        lblMensaje.AutoSize = true;
        lblMensaje.MaximumSize = new Size(500, 0);
        lblMensaje.ForeColor = Color.DarkRed;
        Controls.Add(lblMensaje);
    }

    private void ConstruirOpcionesConvocatoria(int convocatoriaId, Panel panel)
    {
        var candidatos = _servicio.CandidatosDe(convocatoriaId);
        var lista = new List<(RadioButton rb, int candidatoId)>();

        int py = 10;
        foreach (var cand in candidatos)
        {
            var rb = new RadioButton
            {
                Text = cand.Nombre,
                Location = new Point(10, py),
                AutoSize = true
            };
            panel.Controls.Add(rb);
            lista.Add((rb, cand.Id));
            py += 25;
        }
        _radiosCandidatos[convocatoriaId] = lista;

        var rbBlanco = new RadioButton
        {
            Text = "Voto en blanco",
            Location = new Point(10, py),
            AutoSize = true
        };
        panel.Controls.Add(rbBlanco);
        _blancoPorConv[convocatoriaId] = rbBlanco;
        py += 25;

        var rbOtro = new RadioButton
        {
            Text = "Candidato no registrado:",
            Location = new Point(10, py),
            AutoSize = true
        };
        var txtOtro = new TextBox
        {
            Location = new Point(190, py - 2),
            Width = 240,
            Enabled = false
        };
        rbOtro.CheckedChanged += (s, e) => txtOtro.Enabled = rbOtro.Checked;
        panel.Controls.Add(rbOtro);
        panel.Controls.Add(txtOtro);
        _otroPorConv[convocatoriaId] = rbOtro;
        _txtOtroPorConv[convocatoriaId] = txtOtro;
    }

    private void ActualizarPanel(int convocatoriaId)
    {
        _paneles[convocatoriaId].Enabled = _checks[convocatoriaId].Checked;
    }

    private void BtnConfirmar_Click(object? sender, EventArgs e)
    {
        lblMensaje.Text = "";
        var selecciones = new List<Seleccion>();

        foreach (var conv in _convocatorias)
        {
            if (!_checks[conv.Id].Checked) continue;

            int? candidatoId = null;
            string? writeIn = null;

            var marcado = _radiosCandidatos[conv.Id].FirstOrDefault(x => x.rb.Checked);
            if (marcado.rb is not null)
            {
                candidatoId = marcado.candidatoId;
            }
            else if (_otroPorConv[conv.Id].Checked)
            {
                writeIn = _txtOtroPorConv[conv.Id].Text;
                if (string.IsNullOrWhiteSpace(writeIn))
                {
                    lblMensaje.Text = $"Escribe el nombre del candidato no registrado en \"{conv.Nombre}\".";
                    return;
                }
            }
            else if (!_blancoPorConv[conv.Id].Checked)
            {
                lblMensaje.Text = $"Selecciona una opción en \"{conv.Nombre}\" (candidato, voto en blanco o escribir uno).";
                return;
            }
            // si _blancoPorConv está marcado, candidatoId y writeIn quedan null => voto en blanco

            selecciones.Add(new Seleccion(conv.Id, candidatoId, writeIn));
        }

        if (selecciones.Count == 0)
        {
            lblMensaje.Text = "Selecciona al menos una elección para votar.";
            return;
        }

        var confirmar = MessageBox.Show(
            $"Vas a emitir tu voto en {selecciones.Count} elección(es). ¿Confirmar?",
            "Confirmar voto", MessageBoxButtons.YesNo);
        if (confirmar != DialogResult.Yes) return;

        var error = _servicio.EmitirVotos(_alumno, selecciones);
        if (error is not null)
        {
            lblMensaje.Text = error;
            return;
        }

        MessageBox.Show("Voto registrado correctamente. ¡Gracias por participar!",
            "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }
}
