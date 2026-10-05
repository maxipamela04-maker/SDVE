namespace SDVE.Core;

public record Alumno(string Codigo, string Nombre, string Grupo, string Carrera, string Centro);
public record Convocatoria(int Id, string Nombre);
public record Candidato(int Id, int ConvocatoriaId, string Nombre);

// CandidatoId == null y WriteIn == null  => voto en blanco
public record Seleccion(int ConvocatoriaId, int? CandidatoId, string? WriteIn);
public record ResultadoCandidato(string Nombre, int Votos, double Porcentaje);

public record ParticipacionInfo(int TotalPadron, int TotalVotantes, double Participacion, double Abstencionismo);