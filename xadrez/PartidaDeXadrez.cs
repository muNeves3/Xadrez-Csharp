﻿using System;
using System.Collections.Generic;
using System.Text;
using tabuleiro;

namespace xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro Tab { get; private set; }
        public bool terminada { get; private set; }
        public int turno { get; private set; }
        public Cor JogadorAtual { get; private set; }
        private HashSet<Peca> pecas;
        private HashSet<Peca> capturadas;
        public bool xeque { get; private set; }
        public Peca vulneravelEnPassant { get; private set; }

        public PartidaDeXadrez() {
            Tab = new Tabuleiro(8, 8);
            turno = 1;
            JogadorAtual = Cor.Branca;
            terminada = false;
            xeque = false;
            vulneravelEnPassant = null;
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            ColocarPecas();
        }

        public Peca ExecutaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = Tab.RetirarPeca(origem);
            p.IncrementarMov();
            Peca pecaCapturada = Tab.RetirarPeca(destino);
            Tab.ColocarPeca(p, destino);
            if (pecaCapturada != null)
            {
                capturadas.Add(pecaCapturada);
            }

            if(p is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna + 1);
                Peca T = Tab.RetirarPeca(origemT);
                T.IncrementarMov();
                Tab.ColocarPeca(T, destinoT);
            }

            if (p is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca T = Tab.RetirarPeca(origemT);
                T.IncrementarMov();
                Tab.ColocarPeca(T, destinoT);
            }

            if(p is Peao)
            {
                if(origem.Coluna != destino.Coluna && pecaCapturada == null)
                {
                    Posicao posP;
                    if (p.Cor == Cor.Branca)
                    {
                        posP = new Posicao(destino.Linha + 1, destino.Coluna);
                    } else
                    {
                        posP = new Posicao(destino.Linha - 1, destino.Coluna);
                    }
                    pecaCapturada = Tab.RetirarPeca(posP);
                    capturadas.Add(pecaCapturada);
                }
            }

            return pecaCapturada;
        }

        public void desfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = Tab.RetirarPeca(destino);
            p.DecrementarMov();
            if (pecaCapturada != null)
            {
                Tab.ColocarPeca(pecaCapturada, destino);
                capturadas.Remove(pecaCapturada);
            }

            if (p is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna + 1);
                Peca T = Tab.RetirarPeca(destinoT);
                T.DecrementarMov();
                Tab.ColocarPeca(T, origemT);
            }

            if (p is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca T = Tab.RetirarPeca(destinoT);
                T.IncrementarMov();
                Tab.ColocarPeca(T, origemT);
            }

            if(p is Peao)
            {
                if (origem.Coluna != destino.Coluna && pecaCapturada == vulneravelEnPassant)
                {
                    Peca peao = Tab.RetirarPeca(destino);
                    Posicao posP;
                    if(p.Cor == Cor.Branca)
                    {
                        posP = new Posicao(3, destino.Coluna);
                    } else
                    {
                        posP = new Posicao(4, destino.Coluna);
                    }
                    Tab.ColocarPeca(peao, posP);
                }
            }
        }

        public void RealizarJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = ExecutaMovimento(origem, destino);

            if(estaEmCheque(JogadorAtual))
            {
                desfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("Você não pode se colocar em xeque");
            }
            Peca p = Tab.Peca(destino);

            if(p is Peca)
            {
                if((p.Cor == Cor.Branca && destino.Linha == 0) || (p.Cor == Cor.Preta && destino.Linha == 7)) {
                    p = Tab.RetirarPeca(destino);
                    pecas.Remove(p);
                    Peca dama = new Dama(Tab, p.Cor);
                    Tab.ColocarPeca(dama, destino);
                    pecas.Add(dama);
                }
            }

            if(estaEmCheque(adversaria(JogadorAtual)))
            {
                xeque = true;
            } else
            {
                xeque = false;
            }

            if(testeXequeMate(adversaria(JogadorAtual)))
            {
                terminada = true;
            } else
            {
            turno++;
            MudaJogador();
            }

            if(p is Peao && (destino.Linha == origem.Linha -2 || destino.Linha ==  origem.Linha + 2)) {
                vulneravelEnPassant = p;
            } else
            {
                vulneravelEnPassant = null;
            }
        }

        public void ValidarPosicaoDeOrigem(Posicao pos)
        {
            if(Tab.Peca(pos) == null)
            {
                throw new TabuleiroException("Nao existe peca na posição de origem escolhida");
            }if(JogadorAtual != Tab.Peca(pos).Cor)
            {
                throw new TabuleiroException("A peça de origem escolhida não é sua");
            }
            if(!Tab.Peca(pos).existeMovimentos())
            {
                throw new TabuleiroException("Não há movimentos possíveis para a peça de origem escolhida");
            }
        }

        public void ValidarPosicaoDeDestino(Posicao origem,Posicao destino)
        {
            if(!Tab.Peca(origem).MovimentoPossivel(destino))
            {
                throw new TabuleiroException("Posicao de destino inválida");
            }
        }

        public void MudaJogador()
        {
            if(JogadorAtual == Cor.Branca)
            {
                JogadorAtual = Cor.Preta;
            } else
            {
                JogadorAtual = Cor.Branca;
            }
        }

        public HashSet<Peca> pecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach(Peca x in capturadas)
            {
                if(x.Cor == cor)
                {
                    aux.Add(x);
                }
            }
            return aux;
        }

        public HashSet<Peca> pecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach(Peca x in pecas)
            {
                if(x.Cor == cor)
                {
                    aux.Add(x);
                }
            }
            aux.ExceptWith(pecasCapturadas(cor));

            return aux;
        }

        private Cor adversaria(Cor cor)
        {
            if(cor == Cor.Branca)
            {
                return Cor.Preta;
            } else
            {
                return Cor.Branca;
            }
        }

        private Peca rei(Cor cor)
        {
            foreach(Peca x in pecasEmJogo(cor))
            {
                if(x is Rei)
                {
                    return x;
                } 
            }

            return null;
        }

        public bool estaEmCheque(Cor cor)
        {
            Peca Rei = rei(cor);
            if(Rei == null)
            {
                throw new TabuleiroException("Não tem rei da cor " + cor + " no tabuleiro");
            }
            foreach(Peca x in pecasEmJogo(adversaria(cor)))
            {
                bool[,] mat = x.MovimentosPossiveis();
                if(mat[Rei.Posicao.Linha, Rei.Posicao.Coluna])
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool testeXequeMate(Cor cor)
        {
            if(!estaEmCheque(cor))
            {
                return false;
            }
            foreach(Peca x in pecasEmJogo(cor))
            {
                bool[,] mat = x.MovimentosPossiveis();
                for(int i = 0; i < Tab.Linhas; i++)
                {
                    for (int j = 0; j < Tab.Colunas; j++)
                    {
                        if(mat[i, j])
                        {
                            Posicao origem = x.Posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = ExecutaMovimento(origem, destino);
                            bool testeXeque = estaEmCheque(cor);
                            desfazMovimento(origem, destino, pecaCapturada);
                            if(!testeXeque)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
                return true;
        }

        public void colocarNovaPeca(char coluna, int linha, Peca peca) 
        {
           Tab.ColocarPeca(peca, new PosicaoXadrez(coluna, linha).ToPosicao());
            pecas.Add(peca);
        }

        private void ColocarPecas()
        {
            colocarNovaPeca('a', 1, new Torre(Tab, Cor.Branca));
            colocarNovaPeca('b', 1, new Cavalo(Tab, Cor.Branca));
            colocarNovaPeca('c', 1, new Bispo(Tab, Cor.Branca));
            colocarNovaPeca('d', 1, new Dama(Tab, Cor.Branca));
            colocarNovaPeca('e', 1, new Rei(Tab, Cor.Branca, this));
            colocarNovaPeca('f', 1, new Bispo(Tab, Cor.Branca));
            colocarNovaPeca('g', 1, new Cavalo(Tab, Cor.Branca));
            colocarNovaPeca('h', 1, new Torre(Tab, Cor.Branca));
            colocarNovaPeca('a', 2, new Peao(Tab, Cor.Branca, this));
            colocarNovaPeca('b', 2, new Peao(Tab, Cor.Branca, this));
            colocarNovaPeca('c', 2, new Peao(Tab, Cor.Branca, this));
            colocarNovaPeca('d', 2, new Peao(Tab, Cor.Branca, this));
            colocarNovaPeca('e', 2, new Peao(Tab, Cor.Branca, this));
            colocarNovaPeca('f', 2, new Peao(Tab, Cor.Branca, this));
            colocarNovaPeca('g', 2, new Peao(Tab, Cor.Branca, this));
            colocarNovaPeca('h', 2, new Peao(Tab, Cor.Branca, this));

            colocarNovaPeca('a', 8, new Torre(Tab, Cor.Preta));
            colocarNovaPeca('b', 8, new Cavalo(Tab, Cor.Preta));
            colocarNovaPeca('c', 8, new Bispo(Tab, Cor.Preta));
            colocarNovaPeca('d', 8, new Dama(Tab, Cor.Preta));
            colocarNovaPeca('e', 8, new Rei(Tab, Cor.Preta, this));
            colocarNovaPeca('f', 8, new Bispo(Tab, Cor.Preta));
            colocarNovaPeca('g', 8, new Cavalo(Tab, Cor.Preta));
            colocarNovaPeca('h', 8, new Torre(Tab, Cor.Preta));
            colocarNovaPeca('a', 7, new Peao(Tab, Cor.Preta, this));
            colocarNovaPeca('b', 7, new Peao(Tab, Cor.Preta, this));
            colocarNovaPeca('c', 7, new Peao(Tab, Cor.Preta, this));
            colocarNovaPeca('d', 7, new Peao(Tab, Cor.Preta, this));
            colocarNovaPeca('e', 7, new Peao(Tab, Cor.Preta, this));
            colocarNovaPeca('f', 7, new Peao(Tab, Cor.Preta, this));
            colocarNovaPeca('g', 7, new Peao(Tab, Cor.Preta, this));
            colocarNovaPeca('h', 7, new Peao(Tab, Cor.Preta, this));
        }
    }
}
