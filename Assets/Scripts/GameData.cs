using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    public int IdGame { get; }
    string[] questions;
    string[,] answers = null;
    public bool HasOptions { get; }
    public int TotalPlayers { get; }

    int questionsTotal;

    public GameData(int idGame, bool hasOptions, int totalPlayers)
    {
        IdGame = idGame;
        HasOptions = hasOptions;
        TotalPlayers = totalPlayers;

        questionsTotal = totalPlayers * MaxRounds();
        questions = new string[questionsTotal];

        answers = new string[questionsTotal, totalPlayers];
    }
    public int MaxRounds()
    {
        return TotalPlayers <= 10 ? 3 : 2;
    }
    public string GetQuestion(int index)
    {
        char[] separators = new char[] { GameManager.Separator };
        string[] split = questions[index].Split(separators, 3);
        return split[0];
    }
    public void SetQuestion(string question, int player, int round, int playerCount)
    {
        int index = player + (round * playerCount);
        questions[index] = question;
    }
    public string[] GetOptions(int index)
    {
        char[] separators = new char[] { GameManager.Separator };
        string[] split = questions[index].Split(separators, 3);
        return new string[] { split[1], split[2] };
    }
    public void SetAnswer(string answer, int player, int question)
    {
        answers[question, player] = answer;
    }
    public string GetAnswer(int player, int question)
    {
        return answers[question, player];
    }

    // RESULTADOS FINALES
    public int[] GetPlayersWithAnswer(int question, string answer)
    {
        List<int> playersIds = new List<int>();
        for (int i = 0; i < TotalPlayers; i++)
        {
            if (answers[question, i] == answer)
                playersIds.Add(i);
        }
        return playersIds.ToArray();
    }

    public int GetAnswersScore(int playerAId, int playerBId)
    {
        int result = 0;

        for (int i = 0; i < questionsTotal; i++)
        {
            if (GetAnswer(playerAId, i).Equals(GetAnswer(playerBId, i)))
                result++;
        }

        return result;
    }
}
