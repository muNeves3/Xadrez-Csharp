using System;
using System.Collections.Generic;
using System.Text;

namespace tabuleiro
{
    class TabuleiroException : ApplicationException
    {
        public TabuleiroException(string message) : base(message) { }
    }
}
