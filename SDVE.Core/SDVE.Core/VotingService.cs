using Microsoft.Data.Sqlite;

namespace SDVE.Core;

public class VotingService
{
    public Alumno? BuscarAlumno(string codigo)
    {
        using var cn = Database.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT Codigo,Nombre,Grupo,Carrera,Centro FROM Alumno WHERE Codigo=$c";
        cmd.Parameters.AddWithValue("$c", codigo.Trim());
        using var r = cmd.ExecuteReader();
        return r.Read()
            ? new Alumno(r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetString(4))
            : null;
    }

    public List<int> ConvocatoriasYaVotadas(string codigo)
    {
        var lista = new List<int>();
        using var cn = Database.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT ConvocatoriaId FROM Participacion WHERE AlumnoCodigo=$c";
        cmd.Parameters.AddWithValue("$c", codigo);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(r.GetInt32(0));
        return lista;
    }

    public List<Candidato> CandidatosDe(int convocatoriaId)
    {
        var lista = new List<Candidato>();
        using var cn = Database.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT Id,ConvocatoriaId,Nombre FROM Candidato WHERE ConvocatoriaId=$c ORDER BY Nombre";
        cmd.Parameters.AddWithValue("$c", convocatoriaId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(new Candidato(r.GetInt32(0), r.GetInt32(1), r.GetString(2)));
        return lista;
    }

    /// Registra todos los votos del alumno en UNA transacción (o todo o nada).
    /// Devuelve null si todo salió bien, o un mensaje de error.
    public string? EmitirVotos(Alumno alumno, IReadOnlyList<Seleccion> selecciones)
    {
        if (selecciones.Count == 0)
            return "Selecciona al menos una elección.";
        if (selecciones.Select(s => s.ConvocatoriaId).Distinct().Count() != selecciones.Count)
            return "Hay elecciones repetidas.";

        foreach (var s in selecciones)
        {
            bool tieneCand = s.CandidatoId.HasValue;
            bool tieneWriteIn = !string.IsNullOrWhiteSpace(s.WriteIn);
            if (tieneCand && tieneWriteIn)
                return "En cada elección elige un candidato de la lista O escribe uno, no ambos.";
        }

        var yaVotadas = ConvocatoriasYaVotadas(alumno.Codigo);
        if (selecciones.Any(s => yaVotadas.Contains(s.ConvocatoriaId)))
            return "Ya votaste en alguna de las elecciones seleccionadas.";

        using var cn = Database.Open();
        using var tx = cn.BeginTransaction();
        try
        {
            foreach (var s in selecciones)
            {
                using var p = cn.CreateCommand();
                p.Transaction = tx;
                p.CommandText = "INSERT INTO Participacion(AlumnoCodigo,ConvocatoriaId) VALUES($a,$c)";
                p.Parameters.AddWithValue("$a", alumno.Codigo);
                p.Parameters.AddWithValue("$c", s.ConvocatoriaId);
                p.ExecuteNonQuery();

                using var v = cn.CreateCommand();
                v.Transaction = tx;
                v.CommandText = @"INSERT INTO Voto(ConvocatoriaId,CandidatoId,NombreWriteIn,Grupo,Carrera,Centro)
                                  VALUES($c,$cand,$w,$g,$ca,$ce)";
                v.Parameters.AddWithValue("$c", s.ConvocatoriaId);
                v.Parameters.AddWithValue("$cand", (object?)s.CandidatoId ?? DBNull.Value);
                v.Parameters.AddWithValue("$w",
                    string.IsNullOrWhiteSpace(s.WriteIn) ? DBNull.Value : s.WriteIn!.Trim());
                v.Parameters.AddWithValue("$g", alumno.Grupo);
                v.Parameters.AddWithValue("$ca", alumno.Carrera);
                v.Parameters.AddWithValue("$ce", alumno.Centro);
                v.ExecuteNonQuery();
            }
            tx.Commit();
            return null;
        }
        catch (SqliteException ex)
        {
            tx.Rollback();
            return "No se pudo registrar el voto: " + ex.Message;
        }
    }
}