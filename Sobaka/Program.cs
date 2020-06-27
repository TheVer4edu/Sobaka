using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;

namespace Sobaka
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandPerformer.Start();
        }
    }

    static class CommandPerformer
    {
        private static Dictionary<string, Action> _actions = new Dictionary<string, Action>()
        {
            {"Left", () => DogDrawer.Move(Direction.Left)},
            {"Right", () => DogDrawer.Move(Direction.Right)},
            {"Up", () => DogDrawer.Move(Direction.Up)},
            {"Down", () => DogDrawer.Move(Direction.Down)},
            {"TailRaise", () =>  DogDrawer.ChangeTailState(TailState.Raise)},
            {"TailRelease", () => DogDrawer.ChangeTailState(TailState.Release)}
        };

        public static void Start()
        {
            foreach (string command in new FileReader("program.dog"))
            {
                if(!_actions.ContainsKey(command)) continue;
                _actions[command]();
                Thread.Sleep(200);
            }
            DogDrawer.Finish();
            Statistics.WriteStatistics();
            Console.ReadKey(); //REMOVE ME
        }
        
    }

    static class DogDrawer
    {
        private static Point _dogPosition = new Point() {X = 0, Y = 0};
        private static bool _isTailRaised = false;
        private static List<Point> _marks = new List<Point>();

        public static void Move(Direction direction)
        {
            if (_isTailRaised)
            {
                _marks.Add(_dogPosition);
                Statistics.LeaveMark();
            }
            _dogPosition = direction switch
            {
                Direction.Left => new Point(_dogPosition.X - 1, _dogPosition.Y),
                Direction.Right => new Point(_dogPosition.X + 1, _dogPosition.Y),
                Direction.Up => new Point(_dogPosition.X, _dogPosition.Y - 1),
                Direction.Down => new Point(_dogPosition.X, _dogPosition.Y + 1)
            };
            Statistics.MakeStep();
            Redraw();
        }

        public static void ChangeTailState(TailState state)
        {
            _isTailRaised = state == TailState.Raise;
        }

        private static void Redraw()
        {
            Console.Clear();
            foreach (var mark in _marks)
            {
                Console.SetCursorPosition(mark.X, mark.Y);
                Console.Write('.');
            }
            Console.SetCursorPosition(_dogPosition.X, _dogPosition.Y);
            Console.Write('@');
        }

        public static void Finish()
        {
            Statistics.FinalPosition = _dogPosition;
        }
    }

    enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    enum TailState
    {
        Raise,
        Release
    }
    
    class FileReader : IEnumerable<string>
    {
        private string _fileName;
        
        public FileReader(string fileName)
        {
            _fileName = fileName;
        }
        
        public IEnumerator<string> GetEnumerator()
        {
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(_fileName);
            }
            catch
            {
                yield break;
            }
            string line;
            while ((line = sr.ReadLine()) != null)
                yield return line;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    static class Statistics
    {
        private static int _steps;
        private static int _marks;
        public static Point FinalPosition { get; set; }

        public static void MakeStep() => _steps++;

        public static void LeaveMark() => _marks++;
        public static void WriteStatistics()
        {
            File.WriteAllLines("out.dog", new []
            {
                $"Total steps: {_steps}",
                $"Total marks: {_marks}",
                $"Final position: X = {FinalPosition.X}, Y = {FinalPosition.Y}"
            });
        }
    }
}