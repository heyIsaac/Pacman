using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PacmanGameProject.Game.Services;

// Modelo de dados que o seu ListView espera
public class ScoreRecord
{
    public string Name { get; set; }
    public int Points { get; set; }
}

public static class ScoreService
{
    // Salva na pasta AppData/Local do Windows 
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PacmanScores.json"
    );

    public static List<ScoreRecord> LoadScores()
    {
        if (!File.Exists(FilePath))
            return new List<ScoreRecord>();

        try
        {
            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<ScoreRecord>>(json) ?? new List<ScoreRecord>();
        }
        catch
        {
            return new List<ScoreRecord>();
        }
    }

    public static void SaveScore(string name, int points)
    {
        var scores = LoadScores();

        // Adiciona a nova jogada
        scores.Add(new ScoreRecord { Name = name, Points = points });

        // Mantém apenas o Top 10 das jogadas salvas
        scores = scores.OrderByDescending(s => s.Points).Take(10).ToList();

        // Salva no arquivo
        string json = JsonSerializer.Serialize(scores, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
}
