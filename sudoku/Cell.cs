﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sudoku
{
    public class Cell
    {
        public byte Value { get; set; }
        public List<byte> PossibleValues { get; set; }

        public Cell Copy()
        {
            Cell copy = new Cell {Value = Value, PossibleValues = new List<byte>()};

            copy.PossibleValues.AddRange(this.PossibleValues);

            return copy;
        }
    }
}
