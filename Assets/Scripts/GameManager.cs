using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private MainNetworkManager Room;
    static string[] names = new string[] { };
    bool[] hasAnswered;
    static int playersCount => names.Length;
    static public char Separator = ';';
    public bool initiated = false;

    GameData[] games;

    int game = 0;
    int round = 0;
    int questionToAnswer = 0;
    bool questionState = true;
    int maxQuestions;

    public void InitGameManager(MainNetworkManager room)
    {
        Room = room;
        if (playersCount != 0) return;

        int maxPlayers = Room.GamePlayers.Count;
        names = new string[maxPlayers];
        hasAnswered = new bool[maxPlayers];

        int pointer = 0;
        foreach (NetworkGamePlayer player in Room.GamePlayers)
        {
            names[pointer] = player.GetDisplayName();
            pointer++;
        }

        games = new GameData[] {
            new GameData(0, true, playersCount)    //También soy
            //new GameData(1, true, playersCount),    //Quién ha hecho
            //new GameData(2, true, playersCount)//,    //Qué prefieres
            /*new GameData(3, false, playersCount)  */ //Si yo fuera un... sería un...
        };

        maxQuestions = games[game].TotalPlayers * games[game].MaxRounds();
    }

    public bool IsQuestioning() => questionState;

    void MarkAsAnswered(int playerId)
    {
        hasAnswered[playerId] = true;
        foreach (bool playerHasAnswer in hasAnswered)
        {
            if (!playerHasAnswer)
                return;
        }

        hasAnswered = new bool[names.Length];


        if (questionState)
        {
            questionState = false;
            Room.PanelForInput();
            string prompt = games[game].GetQuestion(questionToAnswer);
            Room.UpdatePrompt(prompt);
        }
        else
        {
            if (!Room.ShowingAnswers)
            {
                Room.PanelAnswers();
                //if (games[game].hasoptions){
                string question = games[game].GetQuestion(questionToAnswer);
                //if (firstAnswer.IndexOf(": ") != -1){
                string yesAnswer = "Sí soy " + question;
                string noAnswer = "No soy " + question;
                Room.ChangeColumnTitles(yesAnswer, noAnswer);
                for (int i = 0; i < playersCount; i++)
                {
                    string answer = games[game].GetAnswer(i, questionToAnswer);
                    Room.AddPlayerResults(names[i], answer[0] == 'Y');
                }
                //}else{//Diferentes opciones}}else{//Cuando sean respuestas abiertas (Si/No)}
            }
            else
            {
                questionToAnswer++;
                if (HandleIfEndGame()) return;
                int modulePerRound = questionToAnswer % games[game].TotalPlayers;
                if (modulePerRound == 0)
                {
                    round++;
                    questionState = true;
                    Room.PanelForInput();
                    return;
                }

                Room.PanelForInput();
                string prompt = games[game].GetQuestion(questionToAnswer);
                Room.UpdatePrompt(prompt);
            }

        }
    }

    bool HandleIfEndGame() // PARA TERMINAR JUEGO
    {
        if (questionToAnswer == maxQuestions)
        {
            Room.PanelFinal();
            Room.FeedGeneralMatches(GeneralBestMatches());
            return true;
        }
        return false;
    }
    public void SelectGame(int game)
    {
        round = 0;
        questionToAnswer = 0;
        this.game = game;
    }
    int GetPlayerId(string playerName)
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == playerName)
                return i;
        }
        return -1;
    }
    string GetPlayerName(int id) => names[id];
    public void SaveTextInput(string input, string playerName)
    {
        int playerId = GetPlayerId(playerName);
        if (questionState)
        {
            games[game].SetQuestion(input, playerId, round, playersCount);
        }
        else
        {
            games[game].SetAnswer(input, playerId, questionToAnswer);
        }

        MarkAsAnswered(playerId);
    }
    public void MarkReady(string playerName)
    {
        int playerId = GetPlayerId(playerName);
        MarkAsAnswered(playerId);
    }
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // RESULTADOS FINALES
    int GetHobbiesScore(int playerAId, int playerBId)
    {
        int score = 0;

        NetworkGamePlayer playerA = Room.GamePlayers[playerAId];
        NetworkGamePlayer playerB = Room.GamePlayers[playerBId];

        CategoriesList clA = playerA.GetCL();
        CategoriesList clB = playerB.GetCL();
        CategoriesList common = clA.CommonList(clB);

        score += common.GetHobbies().Length * 5;
        score += common.GetBlankSections().Length * 3;
        score += common.GetBlankCategories().Length * 1;

        return score;
    }

    public string[] GeneralBestMatches()
    {
        string[] result = new string[] { "", "", "" };
        int[] scores = new int[] { 0, 0, 0 };

        for (int i = 0; i < playersCount; i++)
        {
            for (int j = i + 1; j < playersCount; j++)
            {
                int answerScores = games[game].GetAnswersScore(i, j);
                int hobbiesScores = GetHobbiesScore(i, j);
                int totalScore = answerScores + hobbiesScores;

                int index = 0;
                while (index < scores.Length)
                {
                    if (totalScore < scores[index])
                        break;

                    if (index == 2 || totalScore <= scores[index + 1])
                    {
                        for (int k = 1; k <= index; k++)
                        {
                            scores[index - 1] = scores[index];
                            result[index - 1] = result[index];
                        }
                        scores[index] = totalScore;
                        result[index] = GetPlayerName(i) + ";" + GetPlayerName(j);
                        break;
                    }

                    index++;
                }
            }
        }

        return result;
    }

    public int[] PersonalBestMatches(string playerName)
    {
        int playerId = GetPlayerId(playerName);

        int[] indexes = new int[] { -1, -1, -1 };
        int[] scores = new int[] { 0, 0, 0 };

        for (int i = 0; i < playersCount; i++)
        {
            if (i == playerId) continue;
            int answerScores = games[game].GetAnswersScore(i, playerId);
            int hobbiesScores = GetHobbiesScore(i, playerId);
            int totalScore = answerScores + hobbiesScores;

            int index = 0;
            while (index < scores.Length)
            {
                if (totalScore < scores[index])
                    break;                

                if (index == 2 || totalScore < scores[index + 1])
                {
                    for (int j = 1; j <= index; j++)
                    {
                        scores[j - 1] = scores[j];
                        indexes[j - 1] = indexes[j];
                    }

                    scores[index] = totalScore;
                    indexes[index] = i;
                    break;
                }

                index++;
            }
        }
        return indexes;
    }
}
