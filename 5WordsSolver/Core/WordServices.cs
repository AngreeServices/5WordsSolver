using _5WordsSolver.Data;
using Microsoft.OpenApi.Validations;

namespace _5WordsSolver.Core
{
    public class WordServices: IWordServices
    {
        public List<string> GetNextWords(List<WordResult> wordResults)
        {
            //Формируем списки из входящих слов
            HashSet<char> requiredChars = new HashSet<char>();
            HashSet<char> forbiddenChars = new HashSet<char>();
            List<string> antiMask = new List<string>() { "", "", "", "", "" };
            List<char?> mask = new List<char?>() { null, null, null, null, null };
            foreach (var word in wordResults)
            {
                for (int i = 0; i < word.Text.Length; i++)
                {
                    var letter = word.Text[i];
                    var value = word.Result[i];
                    if (value == 0)
                    {
                        //проверяем есть ли отличные от 0 результаты для буквы
                        bool hasGoodOtherOccurrence = false;
                        for (int j = 0; j < word.Text.Length; j++)
                        {
                            if (i == j)
                            {
                                continue;
                            }
                            if (word.Text[j] == letter && word.Result[j] > 0)
                            {
                                hasGoodOtherOccurrence = true;
                            }
                        }

                        if (hasGoodOtherOccurrence)
                        {
                            antiMask[i] += letter;
                        }
                        else
                        {
                            if (requiredChars.Contains(letter))
                            {
                                throw new Exception("Буква входит в обязательные и отсутсвующие одновременно");
                            }
                            forbiddenChars.Add(letter);
                        }
                    }
                    else if (value == 1)
                    {
                        if (forbiddenChars.Contains(letter))
                        {
                            throw new Exception("Буква входит в обязательные и отсутсвующие одновременно");
                        }
                        antiMask[i] += letter;
                        requiredChars.Add(letter);
                    }
                    else
                    {
                        if (mask[i] != null && mask[i] != letter)
                        {
                            throw new Exception("Различные буквы находятся на одной позиции в слове");
                        }
                        mask[i] = letter;
                    }
                }
            }

            var words = GetFilteredWords(requiredChars, forbiddenChars, antiMask, mask);
            var countCharsInMask = mask.Count(it => it != null);
            var countOtherKnownChars = requiredChars.Count(it => !mask.Contains(it));
            var countKnownChars = countCharsInMask + countOtherKnownChars;
            if (words.Count <= 1 || countKnownChars < 3 || wordResults.Count >= 5)
            {
                //Если осталось одно слов или известно менее 3 букв или попытка последняя то возвращаем результат фильтрации
                return words;
            }

            var uniqueCharsInWords = new HashSet<char>(words.SelectMany(word => word).Distinct());
            var unknownChars = new HashSet<char>(uniqueCharsInWords.Except(requiredChars));
            var countPossibleCharsToCheck = (5 - countKnownChars) * (6 - wordResults.Count); //число символов, которое можно проверить используя фильтрацию
            if (unknownChars.Count() > countPossibleCharsToCheck && countKnownChars < 5)
            {
                //если число неизвестных символов превышает число доступных букв для проверки и при этом известны не все буквы
                return GetUnknownCharsWords(unknownChars);
            }
            return words;
        }

        /// <summary>
        /// Метод для получения слова состоящего из наибольшего количества неизвестных букв
        /// </summary>
        /// <returns></returns>
        public List<string> GetUnknownCharsWords(HashSet<char> unknownChars)
        {
            var wordsRepository = new WordsRepository();
            var words = wordsRepository.GetAll();
            // Используем LINQ для поиска топ-N слов с наибольшим количеством редких букв
            var topWords = words
                .Select(word => new
                {
                    Word = word.Text,
                    UnknownCount = word.Text.ToHashSet().Count(letter => unknownChars.Contains(letter))
                })
                .OrderByDescending(wordInfo => wordInfo.UnknownCount)
                .ThenBy(wordInfo => wordInfo.Word)  // Вторичная сортировка по слову, чтобы результат был предсказуемым при одинаковом числе редких букв
                .Select(wordInfo => wordInfo.Word)
                .ToList();

            return topWords;
        }
        /// <summary>
        /// Метод для получения лучших отфильтрованных слов
        /// </summary>
        /// <param name="requiredChars"></param>
        /// <param name="forbiddenChars"></param>
        /// <param name="antiMask"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public List<string> GetFilteredWords(HashSet<char> requiredChars, HashSet<char> forbiddenChars, List<string> antiMask, List<char?> mask)
        {
            var wordsRepository = new WordsRepository();
            var words = wordsRepository.GetAll();

            var filteredWords = words.Where(word =>
                word.Text.Length == mask.Count && // Проверка длины слова по маске
                mask.Zip(word.Text, (m, w) => m == null || m == w).All(x => x) && // Проверка соответствия маске
                antiMask.Zip(word.Text, (a, w) => !a.Contains(w)).All(x => x) && // Проверка соответствия антимаске
                requiredChars.All(ch => word.Text.Contains(ch)) && // Проверка обязательных символов
                !word.Text.Any(ch => forbiddenChars.Contains(ch))) // Проверка отсутствия запрещенных символов
                .OrderByDescending(it => it.Score)
                .ToList();

            return filteredWords.Select(it => it.Text).ToList();
        }
    }
}
