using Microsoft.Data.Sqlite;

namespace SDVE.Core;

public static class Database
{
    public static string ConnectionString { get; set; } = "Data Source=sdve.db";

    public static SqliteConnection Open()
    {
        var cn = new SqliteConnection(ConnectionString);
        cn.Open();
        using var pragma = cn.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return cn;
    }

    public static void Initialize()
    {
        using var cn = Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Alumno(
    Codigo TEXT PRIMARY KEY, Nombre TEXT NOT NULL,
    Grupo TEXT NOT NULL, Carrera TEXT NOT NULL, Centro TEXT NOT NULL);

CREATE TABLE IF NOT EXISTS Convocatoria(
    Id INTEGER PRIMARY KEY, Nombre TEXT NOT NULL);

CREATE TABLE IF NOT EXISTS Candidato(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ConvocatoriaId INTEGER NOT NULL REFERENCES Convocatoria(Id),
    Nombre TEXT NOT NULL);

CREATE TABLE IF NOT EXISTS Participacion(
    AlumnoCodigo TEXT NOT NULL REFERENCES Alumno(Codigo),
    ConvocatoriaId INTEGER NOT NULL REFERENCES Convocatoria(Id),
    PRIMARY KEY(AlumnoCodigo, ConvocatoriaId));

CREATE TABLE IF NOT EXISTS Voto(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ConvocatoriaId INTEGER NOT NULL REFERENCES Convocatoria(Id),
    CandidatoId INTEGER NULL REFERENCES Candidato(Id),
    NombreWriteIn TEXT NULL,
    Grupo TEXT NOT NULL, Carrera TEXT NOT NULL, Centro TEXT NOT NULL);

INSERT OR IGNORE INTO Convocatoria(Id, Nombre) VALUES
 (1,'Sociedad de Alumnos'),(2,'Consejo Universitario'),(3,'Consejo de Representantes');";
        cmd.ExecuteNonQuery();
    }
}
