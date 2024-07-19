namespace _5WordsSolver.Data
{
    public class WordsRepository:IWordsRepository
    {
        public List<WordDB> GetAll()
        {
            using (var context = new WordsContext())
            {
                return context.Words.ToList();
            }
        }
    }
}
