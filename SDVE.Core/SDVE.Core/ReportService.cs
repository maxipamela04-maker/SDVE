/* en esta clase sacamos las cuentas de los votos conectandonos a 
 * la base de datos para contar los registros. sacamos los porcentajes 
 * y el abstencionismo cruzando contra los alumnos. tambien armamos un 
 * texto con el resumen agrupado y metemos la funcion para exportar 
 * todo a un archivo csv */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Data.Sqlite;

namespace SDVE.Core
{
    public class ReportService
    {
        public string GenerarReporteTexto(string agrupacion)
        {
            using var cn = Database.Open();

            using var cmdPadron = cn.CreateCommand();
            cmdPadron.CommandText = "SELECT COUNT(*) FROM Alumno";
            int padronTotal = Convert.ToInt32(cmdPadron.ExecuteScalar());

            using var cmdVotantes = cn.CreateCommand();
            cmdVotantes.CommandText = "SELECT COUNT(DISTINCT AlumnoCodigo) FROM Participacion";
            int votantesUnicos = Convert.ToInt32(cmdVotantes.ExecuteScalar());

            int abstencionismo = padronTotal - votantesUnicos;
            double porcParticipacion = padronTotal == 0 ? 0 : (votantesUnicos * 100.0) / padronTotal;
            double porcAbstencion = padronTotal == 0 ? 0 : (abstencionismo * 100.0) / padronTotal;

            var sb = new StringBuilder();
            sb.AppendLine("=== REPORTE GENERAL DE VOTACIÓN ===");
            sb.AppendLine($"Padrón Total Registrado: {padronTotal}");
            sb.AppendLine($"Votos Emitidos: {votantesUnicos} ({porcParticipacion:F2}%)");
            sb.AppendLine($"Abstencionismo: {abstencionismo} ({porcAbstencion:F2}%)\n");
            sb.AppendLine("=== RESULTADOS DETALLADOS ===");

            string columnaAgrupacion = "'General'";
            if (agrupacion == "Centro") columnaAgrupacion = "v.Centro";
            if (agrupacion == "Carrera") columnaAgrupacion = "v.Carrera";
            if (agrupacion == "Grupo") columnaAgrupacion = "v.Grupo";

            using var cmdVotos = cn.CreateCommand();
            cmdVotos.CommandText = $@"
            SELECT {columnaAgrupacion} AS Categoria,
                   c.Nombre AS Convocatoria, 
                   COALESCE(cand.Nombre, v.NombreWriteIn, 'Voto en Blanco') AS Candidato,
                   COUNT(*) AS Total
            FROM Voto v
            JOIN Convocatoria c ON v.ConvocatoriaId = c.Id
            LEFT JOIN Candidato cand ON v.CandidatoId = cand.Id
            GROUP BY {columnaAgrupacion}, c.Nombre, Candidato
            ORDER BY {columnaAgrupacion}, c.Nombre, Total DESC";

            using var r = cmdVotos.ExecuteReader();
            string categoriaActual = "";
            string convActual = "";

            while (r.Read())
            {
                string cat = r.GetString(0);
                string conv = r.GetString(1);
                string cand = r.GetString(2);
                int total = r.GetInt32(3);

                if (cat != categoriaActual)
                {
                    sb.AppendLine($"\n[{agrupacion.ToUpper()}: {cat.ToUpper()}]");
                    categoriaActual = cat;
                    convActual = "";
                }

                if (conv != convActual)
                {
                    sb.AppendLine($"  --- {conv} ---");
                    convActual = conv;
                }
                sb.AppendLine($"  - {cand}: {total} votos");
            }

            return sb.ToString();
        }

        public void ExportarACSV(string ruta, string contenido)
        {
            File.WriteAllText(ruta, contenido, Encoding.UTF8);
        }
    }
}