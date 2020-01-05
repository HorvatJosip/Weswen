using System.Collections.Generic;
using System.Reflection;

namespace Weswen
{
    class TableCellPropertyComparer : IEqualityComparer<(PropertyInfo prop, TableCellAttribute cell)>
    {
        public bool Equals((PropertyInfo prop, TableCellAttribute cell) x, (PropertyInfo prop, TableCellAttribute cell) y) 
            => Equals(x.cell, y.cell);

        public int GetHashCode((PropertyInfo prop, TableCellAttribute cell) obj) 
            => obj.cell.GetHashCode();
    }
}
