/* aqui mostramos la pantalla del administrador 
 * dibujamos un cuadro de texto grandote para ver el reporte y 
 * le ponemos botones para cambiar los filtros y guardar toda la 
 * informacion en un archivo csv usando el save file dialog */

using SDVE.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace SDVE.App;

public partial class frmResultados : Form
{
    private ComboBox cmbFiltro = new();
    private Button btnGenerar = new();
    private Button btnExportar = new();
    private TextBox txtReporte = new();
    private readonly ReportService _servicio = new();

    public frmResultados()
    {
        ConstruirInterfaz();
    }

    private void ConstruirInterfaz()
    {
        Text = "SDVE - Motor de Resultados y Exportación";
        Width = 600;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        var lblFiltro = new Label
        {
            Text = "Agrupar resultados por:",
            Location = new Point(20, 20),
            AutoSize = true
        };

        cmbFiltro.Location = new Point(20, 50);
        cmbFiltro.Width = 150;
        cmbFiltro.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFiltro.Items.AddRange(new string[] { "General", "Centro", "Carrera", "Grupo" });
        cmbFiltro.SelectedIndex = 0;

        btnGenerar.Text = "Generar Reporte";
        btnGenerar.Location = new Point(190, 48);
        btnGenerar.Size = new Size(130, 30);
        btnGenerar.Click += BtnGenerar_Click;

        btnExportar.Text = "Exportar a CSV";
        btnExportar.Location = new Point(330, 48);
        btnExportar.Size = new Size(130, 30);
        btnExportar.Enabled = false;
        btnExportar.Click += BtnExportar_Click;

        txtReporte.Location = new Point(20, 100);
        txtReporte.Multiline = true;
        txtReporte.ReadOnly = true;
        txtReporte.ScrollBars = ScrollBars.Vertical;
        txtReporte.Size = new Size(540, 430);
        txtReporte.Font = new Font("Consolas", 10);

        Controls.Add(lblFiltro);
        Controls.Add(cmbFiltro);
        Controls.Add(btnGenerar);
        Controls.Add(btnExportar);
        Controls.Add(txtReporte);
    }

    private void BtnGenerar_Click(object? sender, EventArgs e)
    {
        string filtro = cmbFiltro.Text;
        string reporte = _servicio.GenerarReporteTexto(filtro);
        txtReporte.Text = reporte;

        if (txtReporte.Text.Length > 0)
        {
            btnExportar.Enabled = true;
        }
    }

    private void BtnExportar_Click(object? sender, EventArgs e)
    {
        var dialogo = new SaveFileDialog
        {
            Filter = "Archivos CSV (*.csv)|*.csv|Archivos de texto (*.txt)|*.txt",
            Title = "Exportar Resultados SDVE",
            FileName = $"Resultados_{cmbFiltro.Text}.csv"
        };

        if (dialogo.ShowDialog() == DialogResult.OK)
        {
            try
            {
                _servicio.ExportarACSV(dialogo.FileName, txtReporte.Text);
                MessageBox.Show("Reporte exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }
    }
}

