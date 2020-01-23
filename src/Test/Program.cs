using System;
using System.Linq;
using Konsole;
using MyTqdm;

namespace Test
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var a = new[] {1, 2, 3};
            a.WithProgress(MyTqdm.MyTqdm.ConsoleForwardOnlyProgress).ToList();
        }
    }
}