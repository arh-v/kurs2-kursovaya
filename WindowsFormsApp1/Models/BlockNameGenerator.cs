using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingStatePredictionApp.Models;

public class BlockNameGenerator
{
    private LiteralChain _name;

    public string NextName() => _name == null ? (_name = new()).ToString() : _name.Next();

    private class LiteralChain
    {
        private const char _firstLiteral = 'A';
        private const char _lastLiteral = 'Z';
        private LiteralChain _senior;
        private char _value = _firstLiteral;

        public string Next()
        {
            if (++_value > _lastLiteral)
            {
                _value = _firstLiteral;
                _senior?.Next();
                _senior ??= new();
            }

            return ToString();
        }

        public override string ToString() => (_senior?.ToString() ?? "") + _value;
    }
}