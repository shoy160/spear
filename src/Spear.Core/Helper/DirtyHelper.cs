using Spear.Core.Dependency;
using Spear.Core.Extensions;
using System.Collections.Generic;
using System.IO;

namespace Spear.Core.Helper
{
    /// <summary> 敏感词辅助 </summary>
    public class DirtyHelper : ISingleDependency
    {
        private readonly DirtyNode _root;
        private const string DirtyWordsFile = "dirty_words.txt";

        private DirtyHelper()
        {
            _root = new DirtyNode();
            var folderPath = "configPath".Config(Directory.GetCurrentDirectory());
            var path = Path.Combine(folderPath, DirtyWordsFile);
            if (!File.Exists(path))
                return;
            var list = File.ReadAllLines(path);
            foreach (var item in list)
            {
                AddDirty(item);
            }
        }

        //public static DirtyHelper Instance
        //    => Singleton<DirtyHelper>.Instance ?? (Singleton<DirtyHelper>.Instance = new DirtyHelper());

        /// <summary> 添加脏词 </summary>
        /// <param name="word"></param>
        public void AddDirty(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return;
            var current = _root;
            foreach (var c in word)
            {
                current = current.Add(c);
            }
            current.Leaf = true;
        }

        /// <summary> 是否包含脏字 </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool HasDirty(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            for (var i = 0; i < text.Length; i++)
            {
                var current = _root;
                var index = i;
                while (current.Nodes.ContainsKey(text[index]) && (current = current.Nodes[text[index]]) != null)
                {
                    if (current.Leaf)
                    {
                        return true;
                    }
                    if (text.Length == ++index)
                        break;
                }
            }
            return false;
        }

        /// <summary> 替换脏字 </summary>
        /// <param name="text"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
        public string ReplaceWith(string text, char mark)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            var chars = text.ToCharArray();
            for (var i = 0; i < text.Length; i++)
            {
                var current = _root;
                var index = i;
                var idxList = new List<int>();
                while (current.Nodes.ContainsKey(text[index]) && (current = current.Nodes[text[index]]) != null)
                {
                    idxList.Add(index);
                    if (current.Leaf)
                    {
                        foreach (var idx in idxList)
                        {
                            chars[idx] = mark;
                        }
                        break;
                    }
                    if (text.Length == ++index)
                        break;
                }
            }
            return new string(chars);
        }

        //public override string ToString()
        //{
        //    return JsonConvert.SerializeObject(_root);
        //}
    }

    /// <summary> 敏感词节点 </summary>
    internal class DirtyNode
    {
        public char Value { get; set; }
        public bool Leaf { get; set; }
        public Dictionary<char, DirtyNode> Nodes { get; }

        public DirtyNode()
        {
            Nodes = new Dictionary<char, DirtyNode>();
        }

        public DirtyNode(char value)
            : this()
        {
            Value = value;
        }

        /// <summary> 添加节点 </summary>
        /// <param name="newChar"></param>
        /// <returns></returns>
        public DirtyNode Add(char newChar)
        {
            if (Nodes.TryGetValue(newChar, out var item))
            {
                return item;
            }
            item = new DirtyNode
            {
                Value = newChar
            };
            Nodes.Add(newChar, item);
            return item;
        }
    }
}
