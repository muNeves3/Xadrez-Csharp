using System;
using System.Collections.Generic;
using System.Text;

namespace tabuleiro
{
    class Tabuleiro
    {
        public int Linhas { get; set; }
        public int Colunas { get; set; }
        private Peca[,] Pecas;

        public Tabuleiro() { }

        public Tabuleiro(int linhas, int colunas)
        {
            Linhas = linhas;
            Colunas = colunas;
            Pecas = new Peca[linhas, colunas];
        }

        public Peca Peca(int linha, int coluna)
        {
            return Pecas[linha, coluna];
        }

        public Peca Peca(Posicao position)
        {
            return Pecas[position.Linha, position.Coluna];
        }

        public bool existePeca(Posicao position)
        {
            ValidarPosicao(position);
            return Peca(position) != null;
        }

        public void ColocarPeca(Peca p, Posicao position)
        {
            if(existePeca(position))
            {
                throw new TabuleiroException("Já existe uma peça nessa posição");
            }
            Pecas[position.Linha, position.Coluna] = p;
            p.Posicao = position;
        }

        public bool PosicaoValida(Posicao position)
        {
            if (position.Linha < 0 || position.Linha >= Linhas || position.Coluna < 0 || position.Coluna >= Colunas)
            {
                return false;
            }
            return true;
        }

        public Peca RetirarPeca(Posicao position)
        {
            if(Peca(position) == null)
            {
                return null;
            }
            Peca aux = Peca(position);
            aux.Posicao = null;
            Pecas[position.Linha, position.Coluna] = null;
            return aux;
        } 

        public void ValidarPosicao(Posicao pos)
        {
            if(!PosicaoValida(pos))
            {
                throw new TabuleiroException("Posição invalida");
            }
        }
    }
}
