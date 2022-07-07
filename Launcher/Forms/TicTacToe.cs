using System;
using System.Collections.Generic;
using System.Linq;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

namespace Launcher.Forms;

public class TicTacToe
{
    private int[][] _game;
    private IApp _app;
    private int _winner = -1;

    private void ResetGame()
    {
        _game = new int[3][];
        for (var i = 0; i < _game.Length; i++)
        {
            _game[i] = new int[3];
        }

        _winner = -1;
    }

    public TicTacToe(IApp app)
    {
        ResetGame();
        _app = app;
    }

    public void Show()
    {
        List<FormEntry> entries = new()
        {
            Form.TextBox("Tic-Tac-Toe", FormAlignment.Center, "Bold")
        };

        for (int i = 0; i < 3; i++)
        {
            var i1 = i;
            entries.Add(Form.Button(
                GetCharForValue(_game[i][0]), x => DoMove(0, i1),
                GetCharForValue(_game[i][1]), x => DoMove(1, i1),
                GetCharForValue(_game[i][2]), x => DoMove(2, i1)
            ));
        }
        
        entries.Add(Form.TextBox(""));

        if (_winner != -1)
        {
            entries.Add(Form.TextBox(GetWinningMessage(), FormAlignment.Center));
        }
        
        entries.Add(Form.Button("Back", _ => _app.HideForm(), "Reset", _ =>
        {
            ResetGame();
            Show();
        }));
        
        _app.ShowForm(entries);
    }

    public void DoMove(int x, int y)
    {
        if (_winner != -1 || _game[y][x] != 0)
            return;

        _game[y][x] = 1;
        Check();
    }

    private string GetWinningMessage()
    {
        if (_winner == 0)
            return "It's a draw!";
        if (_winner == 1)
            return "The player wins!";
        return "The machine wins!";
    }

    private string GetCharForValue(int val)
    {
        if (val == 0)
            return "  ";
        if (val == 1)
            return "O";
        return "X";
    }

    public void Check()
    {
        int[] verticalOneCount = GetVerticalCount(1);
        int[] horizontalOneCount = GetHorizontalCount(1);
        int totalSquares = _game.Sum(x => x.Select(x => x != 0 ? 1 : 0).Sum());

        if (totalSquares >= 9)
        {
            _winner = 0; // Draw
            Show();
            return;
        }

        if (verticalOneCount.Any(x => x == 3) || horizontalOneCount.Any(x => x == 3) ||
            (verticalOneCount.All(x => x >= 1) && horizontalOneCount.All(x => x >= 1)))
        {
            _winner = 1; // Player wins
            Show();
            return;
        }
        
        // Let the bot make a random move
        bool found = false;
        Random r = new();
        while (!found)
        {
            int x = r.Next(0, 3);
            int y = r.Next(0, 3);
            if (_game[y][x] != 0)
                continue; // lol

            _game[y][x] = 2;
            found = true;
        }
        
        int[] verticalTwoCount = GetVerticalCount(2);
        int[] horizontalTwoCount = GetHorizontalCount(2);

        if (verticalTwoCount.Any(x => x == 3) || horizontalTwoCount.Any(x => x == 3) ||
            (verticalTwoCount.All(x => x >= 1) && horizontalTwoCount.All(x => x >= 1)))
        {
            _winner = 2; // Bot wins
        }
        
        Show();
    }

    private int[] GetVerticalCount(int check)
    {
        int[] verticalCounts = new int[3];

        for (int i = 0; i < 3; i++)
        {
            verticalCounts[i] = (_game[0][i] == check ? 1 : 0) + 
                                (_game[1][i] == check ? 1 : 0) + 
                                (_game[2][i] == check ? 1 : 0);
        }

        return verticalCounts;
    }
    
    private int[] GetHorizontalCount(int check)
    {
        int[] horizontalCount = new int[3];

        for (int i = 0; i < 3; i++)
        {
            horizontalCount[i] = (_game[i][0] == check ? 1 : 0) + 
                                (_game[i][1] == check ? 1 : 0) + 
                                (_game[i][2] == check ? 1 : 0);
        }

        return horizontalCount;
    }
}