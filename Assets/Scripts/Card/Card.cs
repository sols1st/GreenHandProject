using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

[Serializable]
public class Card : IEquatable<Card>
{
    public string id;
    public string type;
    public string[] function;
    public string name;
    public string summary;
    public string plot;
    public string image;
    public bool isGot = false;

    public override bool Equals(object obj)
    {
        return Equals(obj as Card);
    }

    public bool Equals(Card other)
    {
        if (other == null)
            return false;

        return string.Equals(id, other.id);
    }

    public override int GetHashCode()
    {
        return id != null ? id.GetHashCode() : 0;
    }

    public static bool operator ==(Card left, Card right)
    {
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null);
        }
        return left.Equals(right);
    }

    public static bool operator !=(Card left, Card right)
    {
        return !(left == right);
    }
}
