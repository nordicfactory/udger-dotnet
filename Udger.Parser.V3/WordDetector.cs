using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Udger.Parser.V3
{
    /// <summary>
    /// This class is thread safe.
    /// </summary>
    public class WordDetector
    {
        public struct WordInfo
        {
            public int Id { get; }
            public string Word { get; }

            public WordInfo(int id, string word)
            {
                Id = id;
                Word = word;
            }
        }

        private static readonly int ArrayDimension = 'z' - 'a';
        private static readonly int ArraySize = (ArrayDimension + 1) * (ArrayDimension + 1);

        private readonly ImmutableArray<ImmutableList<WordInfo>> _wordArray;
        private int _minWordSize = int.MaxValue;

        public WordDetector(IEnumerable<WordInfo> wordInfos)
        {
            var wordArray = new List<WordInfo>[ArraySize];

            foreach (var wordInfo in wordInfos)
            {
                AddWord(wordArray, wordInfo.Id, wordInfo.Word);
            }
            var w = wordArray.Select(a => a == null? ImmutableList<WordInfo>.Empty: a.ToImmutableList()).ToImmutableArray();

            _wordArray = w;
        }

        private void AddWord(List<WordInfo>[] wordArray, int id, string word)
        {

            if (word.Length < _minWordSize)
            {
                _minWordSize = word.Length;
            }

            var s = word.ToLower();
            var index = (s[0] - 'a') * ArrayDimension + s[1] - 'a';

            if (index < 0 || index >= ArraySize) return;

            var wList = wordArray[index];
            if (wList == null)
            {
                wList = new List<WordInfo>();
                wordArray[index] = wList;
            }
            wList.Add(new WordInfo(id, s));
        }

        public HashSet<int> FindWords(string text)
        {

            var ret = new HashSet<int>();

            var s = text.ToLower();
            const int dimension = 'z' - 'a';
            for (var i = 0; i < s.Length - (_minWordSize - 1); i++)
            {
                var c1 = s[i];
                var c2 = s[i + 1];
                if (c1 < 'a' || c1 > 'z' || c2 < 'a' || c2 > 'z') continue;

                var index = (c1 - 'a') * dimension + c2 - 'a';
                var l = _wordArray[index];

                if (l == null) continue;

                foreach (var wi in l)
                {
                    if (s.Substring(i).StartsWith(wi.Word))
                    {
                        ret.Add(wi.Id);
                    }
                }
            }
            return ret;
        }

    }
}
