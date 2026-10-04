using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace SDVE.App;

public partial class frmPrincipal : Form
{
    public frmPrincipal()
    {
        ConstruirInterfaz();
    }

    private void ConstruirInterfaz()
    {
        Text = "SDVE - Menú Principal";
        Width = 400;
        Height = 250;
        StartPosition = FormStartPosition.CenterScreen;

        var lblTitulo = new Label
        {
            Text = "Sistema Digital de Votación Estudiantil",
            Location = new Point(40, 30),
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold)
        };

        var btnVotar = new Button
        {
            Text = "Módulo de Votación (Alumnos)",
            Location = new Point(60, 80),
            Size = new Size(260, 45)
        };
        btnVotar.Click += (s, e) => new frmLogin().ShowDialog();

        var btnAdmin = new Button
        {
            Text = "Módulo de Resultados (Admin)",
            Location = new Point(60, 140),
            Size = new Size(260, 45)
        };
        btnAdmin.Click += (s, e) => new frmResultados().ShowDialog();

        Controls.Add(lblTitulo);
        Controls.Add(btnVotar);
        Controls.Add(btnAdmin);
    }
}