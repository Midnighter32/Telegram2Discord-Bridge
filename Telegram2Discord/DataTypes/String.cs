using System.Collections.Generic;
using System.Linq;

public class Char
{
    public char Symbol { get; set; }
    public int? Index { get; set; }

    public Char(char c, int? i = null)
    {
        this.Symbol = c;
        this.Index = i;
    }

    public override string ToString()
    {
        return $"Char: {this.Symbol}, Index: {this.Index}";
    }
}

public class String
{
    public List<Char> Chars { get; set; }

    public String(string s)
    {
        this.Chars = new List<Char>();
        for (int i = 0; i < s.Length; i++)
        {
            this.Chars.Add(new Char(s[i], i));
        }
    }

    public override string ToString()
    {
        return string.Join("", this.Chars.Select(c => c.Symbol));
    }

    public int IndexOf(char c)
    {
        return this.Chars.FirstOrDefault(x => x.Symbol == c)?.Index ?? -1;
    }

    private int Find(int index)
    {
        return this.Chars.FindIndex(x => x.Index == index);
    }

    public void Insert(int index, string s)
    {
        var pos = this.Find(index);
        for (int i = 0; i < s.Length; i++)
        {
            this.Chars.Insert(pos + i, new Char(s[i]));
        }
    }

    public string Substring(int startIndex, int length)
    {
        var pos = this.Find(startIndex);
        var take = this.Find(startIndex + length - 1) - pos + 1;
        return string.Join("", this.Chars.Skip(pos).Take(take).Select(c => c.Symbol));
    }

    public void Replace(int startIndex, int length, string newValue)
    {
        var pos = this.Find(startIndex);
        var take = this.Find(startIndex + length - 1) - pos + 1;
        for (int i = 0; i < take; i++)
        {
            this.Chars.RemoveAt(pos);
        }
        for (int i = 0; i < newValue.Length; i++)
        {
            this.Chars.Insert(pos + i, new Char(newValue[i]));
        }
    }
}

