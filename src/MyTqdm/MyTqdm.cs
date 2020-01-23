using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Konsole;
using MyTqdm.Internal;

namespace MyTqdm
{
    public static class MyTqdm
    {
        public static readonly IProgressFactory ConsoleProgress = new ConsoleProgressFactory(new Writer());
        public static readonly IProgressFactory ConsoleForwardOnlyProgress = new ConsoleForwardOnlyProgressFactory(new Writer());
        
        
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> src, IProgressFactory progressFactory,
            [CallerMemberName] string title = "Progress", int? total = null, TimeSpan? updatePeriod = null)
        {
            var actualTotal = ResolveTotal(src, total);
            var progress = progressFactory.Create(title, actualTotal);
            return WithProgress<T>(src, progress, updatePeriod);
        }
        
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> src, IConsole console,
            [CallerMemberName] string title = "Progress", int? total = null, TimeSpan? updatePeriod = null)
        {
            var actualTotal = ResolveTotal(src, total);
            var progress =  new ConsoleProgressFactory(console).Create(title, actualTotal);
            return WithProgress<T>(src, progress, updatePeriod);
        }
        
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> src, IWrite write,
            [CallerMemberName] string title = "Progress", int? total = null, TimeSpan? updatePeriod = null)
        {
            var actualTotal = ResolveTotal(src, total);
            var progress = new ForwardOnlyProgress(write, title, actualTotal);
            return WithProgress<T>(src, progress, updatePeriod);
        }
        
        
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> src,IProgress progress, TimeSpan? updatePeriod = null)
        {
            
            progress.Update(0);
            var current = new Boxed(0);
            var actualUpdatePeriod = updatePeriod ?? TimeSpan.FromSeconds(1);
            var timer = new Timer(o => progress.Update((int) current.Read()), null, actualUpdatePeriod, actualUpdatePeriod);
            
            try
            {
                foreach (var i in src)
                {
                    current.Increment();
                    yield return i;
                }
                
            }
            finally
            {
                timer.Dispose();
            }
            progress.Update((int)current.Read());
            
        }
        private static int? ResolveTotal<T>(IEnumerable<T> src, int? total)
        {
            if (total.HasValue) return total;
            switch (src)
            {
                case IReadOnlyCollection<T>  r : return r.Count;
                default: return null;
            }

        }
        private class Boxed
        {
            private long _current = 0;
            public Boxed(long current)
            {
                _current = current;
            }

            public void Increment() => Interlocked.Increment(ref _current);
            public long Read() => Interlocked.Read(ref _current);

        }
        private class ConsoleProgressFactory:IProgressFactory
        {
            private readonly IConsole _console;

            public ConsoleProgressFactory(IConsole console)
            {
                _console = console;
            }
            public IProgress Create(string title, int? total)
            {
                return  total.HasValue?(IProgress)new Bar(new ProgressBar(_console,total.Value), title) : new Counter(_console, title);
            }
        }        
        private class ConsoleForwardOnlyProgressFactory:IProgressFactory
        {
            private readonly IWrite _write;

            public ConsoleForwardOnlyProgressFactory(IConsole console)
            {
                _write = console;
            }
            public IProgress Create(string title, int? total)
            {
                return  new ForwardOnlyProgress(_write, title, total);
            }
        }
        
        private class ForwardOnlyProgress: IProgress
        {
            private readonly IWrite _write;
            private readonly string _title;
            private readonly int? _total;

            private static readonly IDictionary<int, string> Percentages =
                Enumerable.Range(0, 101).ToDictionary(i => i, GetPercentage);

            public ForwardOnlyProgress(IWrite write, string title, int? total)
            {
                _write = write;
                _title = title;
                _total = total;
            }

            public void Update(int current)
            {
                    
                if (_total.HasValue)
                {
                    var percentage = (double) current / (double) _total * 100.0;
                    var actual = (int)Math.Min(0, Math.Max(100, percentage));
                    var percentageLine = Percentages[actual];
                    var line = $"{_title} {current} of {_total} {percentageLine}";
                    _write.WriteLine(line);
                }
                else
                {
                    var fullTitle = $"{_title} [{current}]";
                    _write.WriteLine(fullTitle);
                }
            }

            private static string GetPercentage(int percentage)
            {
                //;
                var sb = new StringBuilder();
                sb.Append("[");
                foreach (var i in Enumerable.Range(0,101))
                {
                    if (percentage > i)
                    {
                        sb.Append('#');
                    }
                    else
                    {
                        sb.Append(" ");
                    }
                }
                sb.Append("]");
                return sb.ToString();
            }
        }

        private class Bar:IProgress
        {
            private readonly ProgressBar _progressBar;
            private readonly string _title;

            public Bar(ProgressBar progressBar, string title)
            {
                _progressBar = progressBar;
                _title = title;
            }

            public void Update(int current)
            {
                var fullTitle = $"{_title} {current} of {_progressBar.Max}";
                _progressBar.Refresh(current, fullTitle);
            }
            
        }
        private class Counter:IProgress
        {
            private readonly string _title;
            private readonly IConsole _writer;
            private readonly int _y;

            public Counter(IConsole writer, string title)
            {
                _writer = writer;
                _title = title;
                _y = writer.CursorTop;
            }

            public void Update(int current)
            {
                var fullTitle = $"{_title} [{current}]";
                _writer.PrintAtColor(
                    foreground:ConsoleColor.Green,
                    x: 0,
                    y : _y,
                    text: fullTitle,
                    null);
            }
        }
    }
}
