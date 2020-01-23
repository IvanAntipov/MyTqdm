namespace MyTqdm.Internal
{
    public interface IProgress
    {
        void Start();
        void Update(int current);
    }
}