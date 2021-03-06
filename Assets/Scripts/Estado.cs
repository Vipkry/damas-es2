﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TiposNS;
using TabuleiroNS;

namespace EstadoNS{
    public class Estado{

        public int[,] tabuleiro;
        public int jogadorAtual;
	    public Jogada ultimaJogada;
        public int contaJogadas;
	    public bool gameOver = false;
        public int jogadasCondicaoEmpate;

	    public Estado(int[,] tabuleiro, int jogadorAtual, Jogada ultimaJogada, int contaJogadas){
            this.tabuleiro = (int[,])tabuleiro.Clone();
            this.jogadorAtual = jogadorAtual;
            this.ultimaJogada = ultimaJogada;
            this.contaJogadas = contaJogadas;
            this.jogadasCondicaoEmpate = 0;
	    }

	    public void print(){
            string resp = "";

            resp += "Jogador Atual:" + jogadorAtual + "\n";
		    resp += "Tabuleiro: \n";
		    for(int i=0; i<8; i++){
		    	for(int j=0; j<8; j++){
		    		resp += tabuleiro[i,j]+" ";
		    		//Debug.Log(tabuleiro[i,j]+" ");
		    	}
		    	resp += "\n";
		    }

            Debug.Log(resp);

            //if (ultimaJogada != null){
	        //	Debug.Log("Ultima jogada: ["+ultimaJogada[0]+"]["+ultimaJogada[1]+"] to ["+lastMove[2]+"]["+lastMove[3]+"]");
		    //}
	    }

        public bool gameIsOver(){
		    return this.gameOver;
	    }

	    public int getOpponent(){            
		    if (this.jogadorAtual == 1) return 2;
		    else return 1;
	    }

	    public static Estado result(Estado antigo, Jogada acao){
		    Estado novo = new Estado(antigo.tabuleiro, antigo.jogadorAtual, antigo.ultimaJogada,antigo.contaJogadas);

            novo.jogadorAtual = novo.jogadorAtual == 1 ? 2 : 1;
            novo.ultimaJogada = acao;

            int size = acao.movimentos.Count;
            int pecaSendoMovimentada = antigo.tabuleiro[acao.posInicial[0], acao.posInicial[1]];
            novo.tabuleiro[acao.ultimoMovimento()[0], acao.ultimoMovimento()[1]] = novo.tabuleiro[acao.posInicial[0], acao.posInicial[1]];
            novo.tabuleiro[acao.posInicial[0], acao.posInicial[1]] = Tipos.vazio;

            foreach (int[] peca in acao.pecasComidas){
                novo.tabuleiro[peca[0], peca[1]] = Tipos.vazio;
            }

            // if (acao.virouDama)
            // {
            //     int jogadorAtual = Tipos.pegaJogador(novo.tabuleiro[acao.ultimoMovimento()[0], acao.ultimoMovimento()[1]]);
            //     novo.tabuleiro[acao.ultimoMovimento()[0], acao.ultimoMovimento()[1]] = Tipos.getPecaJogadorX(Tipos.dama, jogadorAtual);
            // }

            if (acao.pecasComidas.Count == 0 && Tipos.isDama(pecaSendoMovimentada))
            {
                novo.jogadasCondicaoEmpate++;
            }
            else
            {
                novo.jogadasCondicaoEmpate = 0;
            }
            novo.contaJogadas++;

		    return novo;
	    }

        public void setJogadorAtual(int novoJogador)
        {
            this.jogadorAtual = novoJogador;
        }

        public int getJogadorAtual()
        {
            return this.jogadorAtual;
        }

        public List<int[]> posicoesJogadorX(int jogador)
        {
            //retorna todas as posicões (x, y) das peças de um jogador X

            int lin, col, pecaAtual;
            List<int[]> posicoes = new List<int[]>();
            int[] posicao = new int[2];
            int tamanho = Tabuleiro.instance.getTamanhoTabuleiro();
            for (lin = 0; lin < tamanho; lin++)
            {
                for (col = 0; col < tamanho; col++)
                {
                    pecaAtual = tabuleiro[lin, col];
                    if (Tipos.isJogadorX(pecaAtual, jogador))
                    {
                        posicao[0] = lin;
                        posicao[1] = col;
                        posicoes.Add((int[])posicao.Clone());
                    }
                }
            }
            return posicoes;
        }

    }
}