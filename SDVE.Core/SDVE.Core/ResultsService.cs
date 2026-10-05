namespace SDVE.Core;

public enum Agrupador { Grupo, Carrera, Centro }

public class ResultsService
{
    /// Resultados de una convocatoria, opcionalmente filtrados por Grupo/Carrera/Centro.
    /// filtroValor es el valor exacto a filtrar (p.ej. "5A"); null = sin filtro (todo el universo).
    public List<ResultadoCandidato> ResultadosPorConvocatoria(
        int convocatoriaId, Agrupador? agrupador = null, string? filtroValor = null)
    {
        using var cn = Database.Open();
        using var cmd = cn.CreateCommand();

        string columnaFiltro = agrupador switch
        {
            Agrupador.Grupo => "Grupo",
            Agrupador.Carrera => "Carrera",
            Agrupador.Centro => "Centro",
            _ => ""
        };

        string filtroSql = (agrupador is not null && filtroValor is not null)
            ? $"AND v.{columnaFiltro} = $filtro" : "";

        cmd.CommandText = $@"
SELECT
    COALESCE(c.Nombre, v.NombreWriteIn, 'Voto en blanco') AS NombreMostrado,
    COUNT(*) AS Votos
FROM Voto v
LEFT JOIN Candidato c ON c.Id = v.CandidatoId
WHERE v.ConvocatoriaId = $conv {filtroSql}
GROUP BY NombreMostrado
ORDER BY Votos DESC";

        cmd.Parameters.AddWithValue("$conv", convocatoriaId);
        if (agrupador is not null && filtroValor is not null)
            cmd.Parameters.AddWithValue("$filtro", filtroValor);

        var filas = new List<(string Nombre, int Votos)>();
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read())
                filas.Add((r.GetString(0), r.GetInt32(1)));
        }

        int total = filas.Sum(f => f.Votos);
        return filas.Select(f => new ResultadoCandidato(
            f.Nombre, f.Votos, total == 0 ? 0 : Math.Round(f.Votos * 100.0 / total, 2)
        )).ToList();
    }

    /// Participación y abstencionismo de una convocatoria, opcionalmente filtrada.
    public ParticipacionInfo ParticipacionDe(
        int convocatoriaId, Agrupador? agrupador = null, string? filtroValor = null)
    {
        using var cn = Database.Open();

        string columnaFiltro = agrupador switch
        {
            Agrupador.Grupo => "Grupo",
            Agrupador.Carrera => "Carrera",
            Agrupador.Centro => "Centro",
            _ => ""
        };
        string filtroSql = (agrupador is not null && filtroValor is not null)
            ? $"WHERE {columnaFiltro} = $filtro" : "";

        using var cmdPadron = cn.CreateCommand();
        cmdPadron.CommandText = $"SELECT COUNT(*) FROM Alumno {filtroSql}";
        if (agrupador is not null && filtroValor is not null)
            cmdPadron.Parameters.AddWithValue("$filtro", filtroValor);
        int totalPadron = Convert.ToInt32(cmdPadron.ExecuteScalar());

        string filtroVotantesSql = (agrupador is not null && filtroValor is not null)
            ? $"AND a.{columnaFiltro} = $filtro" : "";

        using var cmdVotantes = cn.CreateCommand();
        cmdVotantes.CommandText = $@"
SELECT COUNT(*) FROM Participacion p
JOIN Alumno a ON a.Codigo = p.AlumnoCodigo
WHERE p.ConvocatoriaId = $conv {filtroVotantesSql}";
        cmdVotantes.Parameters.AddWithValue("$conv", convocatoriaId);
        if (agrupador is not null && filtroValor is not null)
            cmdVotantes.Parameters.AddWithValue("$filtro", filtroValor);
        int totalVotantes = Convert.ToInt32(cmdVotantes.ExecuteScalar());

        double participacion = totalPadron == 0 ? 0 : Math.Round(totalVotantes * 100.0 / totalPadron, 2);
        double abstencionismo = Math.Round(100 - participacion, 2);

        return new ParticipacionInfo(totalPadron, totalVotantes, participacion, abstencionismo);
    }

    /// Valores distintos disponibles para un agrupador, útil para llenar un ComboBox de filtro.
    public List<string> ValoresDisponibles(Agrupador agrupador)
    {
        string columna = agrupador switch
        {
            Agrupador.Grupo => "Grupo",
            Agrupador.Carrera => "Carrera",
            Agrupador.Centro => "Centro",
            _ => throw new ArgumentOutOfRangeException(nameof(agrupador))
        };

        using var cn = Database.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = $"SELECT DISTINCT {columna} FROM Alumno ORDER BY {columna}";
        var lista = new List<string>();
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(r.GetString(0));
        return lista;
    }
}