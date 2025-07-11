﻿using UnityEngine;
using RWCustom;
using System.Collections.Generic;
using System.Linq;

namespace BingoMode.BingoChallenges
{
    public class Phrase
    {
        private const float SPACING = 27.5f;
        private List<List<Word>> _words;
        public List<Word> WordsFlat => [.. _words.SelectMany(word => word)];

        public Vector2 centerPos;
        public float scale;
        public bool applyScale;

        public Phrase(List<Word> words, int[] newLines)
        {
            _words = [];
            int solIndex = 0;
            foreach (int eolIndex in newLines)
            {
                _words.Add(words.GetRange(solIndex, eolIndex - solIndex));
                solIndex = eolIndex;
            }
            _words.Add(words.GetRange(solIndex, words.Count - solIndex));
        }

        public Phrase(List<List<Word>> words)
        {
            _words = words;
        }

        public void Draw()
        {
            float scaledSpacing = SPACING * scale;
            Vector2 cursor = centerPos + new Vector2(0f, (_words.Count - 1) * scaledSpacing * 0.5f); //set cursor on first line
            foreach (List<Word> wordLine in _words)
            {
                cursor.x = centerPos.x - ((wordLine.Count - 1) * scaledSpacing * 0.5f); //set cursor at beginning of line
                foreach (Word word in wordLine)
                {
                    word.display.SetPosition(cursor);
                    word.background?.SetPosition(cursor);
                    if (applyScale) word.display.scale = scale;
                    cursor.x += scaledSpacing;
                }
                cursor.y -= scaledSpacing;
            }
        }

        /// <summary>
        /// Inserts a <c>word</c> in this Phrase on a given <c>line</c> at a given <c>index</c>.<br/>
        /// Can generate missing lines.<br/>
        /// Defaults to first line (top) and end of line (right).
        /// </summary>
        /// <param name="word">The word to add to this Phrase</param>
        /// <param name="line">The line on which the word is to be added</param>
        /// <param name="index">The position within the line where the word is to be added. -1 adds to end.</param>
        public void InsertWord(Word word, int line = 0, int index = -1)
        {
            for (int i = _words.Count; i <= line; i++) _words.Add([]);
            if (index < 0) _words[line].Add(word);
            else _words[line].Insert(index, word);
        }

        public void AddAll(FContainer container)
        {
            foreach (Word word in WordsFlat)
            {
                if (word.background != null) container.AddChild(word.background);
                container.AddChild(word.display);
            }
        }

        public void ClearAll()
        {
            foreach (Word word in WordsFlat)
            {
                word.background?.RemoveFromContainer();
                word.display.RemoveFromContainer();
            }
        }

        public void SetAlpha(float alpha)
        {
            foreach (Word word in WordsFlat)
            {
                word.background?.alpha = alpha;
                word.display.alpha = alpha;
            }
        }

        public static Phrase LockPhrase()
        {
            return new Phrase([[new Icon("bingolock", 1f, new Color(0.01f, 0.01f, 0.01f))]]);
        }
    }

    public abstract class Word
    {
        public FNode display;
        public FNode background;

        public Word()
        {
        }
    }

    public class Counter : Word
    {
        public Counter(int current, int max) : base()
        {
            display = new FLabel(Custom.GetFont(), $"[{current}/{max}]");
        }
    }

    public class Verse : Word
    {
        public Verse(string text) : base()
        {
            display = new FLabel(Custom.GetFont(), text);
        }
    }

    public class Icon : Word
    {
        public static Icon MOON => new("GuidanceMoon", 1f, new Color(1f, 0.8f, 0.3f));
        public static Icon PEBBLES => new("nomscpebble", 1f, new Color(0.44705883f, 0.9019608f, 0.76862746f));
        public static Icon SCAV_TOLL => new("scavtoll", 0.8f);
        public static Icon PEARL_HOARD_COLOR => new("pearlhoard_color", 1f, new Color(0.7f, 0.7f, 0.7f));
        public static Icon PEARL_HOARD_NORMAL => new("pearlhoard_normal", 1f, new Color(0.7f, 0.7f, 0.7f));
        public static Icon DATA_PEARL => new("Symbol_Pearl", 1f, new Color(0.7f, 0.7f, 0.7f));

        public Icon(string element, float scale = 1f, Color? color = null, float rotation = 0f) : base()
        {
            color ??= Color.white;
            display = new FSprite(element)
            {
                scale = scale,
                color = (Color)color,
                rotation = rotation
            };
        }

        /// <summary>
        /// Creates and return an icon from a name using <c>ChallengeUtils.ItemOrCreatureIconName()</c>.<br/>
        /// If unspecified, color will be defined using <c>ChallengeUtils.ItemOrCreatureIconColor()</c>.
        /// </summary>
        /// <param name="name">The name of the item or creature to get the icon of</param>
        /// <param name="scale">The scale factor of this icon</param>
        /// <param name="color">The color of this icon. Leave empty to use <c>ChallengeUtils.ItemOrCreatureIconName()</c></param>
        /// <param name="rotation">The rotation of this icon, in degrees</param>
        /// <returns></returns>
        public static Icon FromEntityName(string name, float scale = 1f, Color? color = null, float rotation = 0f)
        {
            color ??= ChallengeUtils.ItemOrCreatureIconColor(name);
            return new(ChallengeUtils.ItemOrCreatureIconName(name), scale, color, rotation);
        }
    }
}
