using System.ComponentModel.DataAnnotations;
namespace _5WordsSolver.Data
{
    public class WordDB
    {
        [Key]
        public string Text { get; set; }
        public int Score { get; set; }
    }
}
