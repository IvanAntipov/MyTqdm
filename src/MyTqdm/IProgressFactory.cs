using MyTqdm.Internal;

namespace MyTqdm
{
    public interface IProgressFactory
    {
        IProgress Create(string title, int? total);
    }
}