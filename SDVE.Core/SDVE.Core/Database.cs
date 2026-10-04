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
    public static void SeedDatosPrueba()
    {
        using var cn = Open();

        using var check = cn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Alumno";
        long alumnos = (long)check.ExecuteScalar()!;
        if (alumnos > 0) return; // ya hay datos, no insertar de nuevo

        using var cmd = cn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Alumno(Codigo,Nombre,Grupo,Carrera,Centro) VALUES
 ('202001','Ana López','5A','Ing. Sistemas','Centro Norte'),
 ('202002','Luis Pérez','5A','Ing. Sistemas','Centro Norte'),
 ('202003','Marta Ruiz','5B','Contaduría','Centro Sur');

INSERT INTO Candidato(ConvocatoriaId,Nombre) VALUES
 (1,'Carlos Gómez'),
 (1,'Diana Torres'),
 (2,'Jorge Salas'),
 (2,'Elena Vidal'),
 (3,'Raúl Núñez');";
        cmd.ExecuteNonQuery();
    }
}
