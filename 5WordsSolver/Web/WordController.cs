using _5WordsSolver.Core;
using Microsoft.AspNetCore.Mvc;

namespace _5WordsSolver.Web
{
    [ApiController]
    [Route("words")]
    public class WordController : Controller
    {

        [HttpGet]
        public List<string> GetFirstWords()
        {
            var wordServices = new WordServices();
            return wordServices.GetNextWords(new List<WordResult>()).Take(10).ToList();
        }
        [HttpPost]
        public List<string> GetNextWords(List<WordResult> wordResults)
        {
            var wordServices = new WordServices();
            return wordServices.GetNextWords(wordResults).Take(10).ToList();
        }
    }
}
