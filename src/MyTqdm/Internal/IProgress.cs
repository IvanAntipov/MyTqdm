namespace MyTqdm.Internal
{
    public interface IProgress
    {
        void Update(int current);
    }
}