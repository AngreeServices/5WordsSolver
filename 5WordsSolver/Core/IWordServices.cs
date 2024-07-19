namespace _5WordsSolver.Core
{
    public interface IWordServices
    {
        public List<string> GetNextWords(List<WordResult> wordResults);
    }
}
