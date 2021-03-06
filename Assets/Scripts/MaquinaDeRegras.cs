﻿﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TiposNS;
using EstadoNS;
using TabuleiroNS;

namespace MaquinaDeRegrasNS
{
    public class MaquinaDeRegras : MonoBehaviour
    {
        // inicializa valores para verificação de movimentos de acordo com o jogador atual
        private static int[] GetValoresPecas(int jogador)
        {
            int[] valoresPecas = new int[3];
            if (jogador == 1)
            {
                valoresPecas[0] = Tipos.getDamaJogador1();
                valoresPecas[1] = Tipos.getPecaJogador2();
                valoresPecas[2] = Tipos.getDamaJogador2();
            }
            else
            {
                valoresPecas[0] = Tipos.getDamaJogador2();
                valoresPecas[1] = Tipos.getPecaJogador1();
                valoresPecas[2] = Tipos.getDamaJogador1();
            }
            return valoresPecas;
        }

        public static List<List<Jogada>>[] TodosPossiveisMovimentos(int[,] tabuleiro, List<int[]> pecasJogador1, List<int[]> pecasJogador2)
        {
            List<List<Jogada>>[] possiveisJogadas = new List<List<Jogada>>[2];
            possiveisJogadas[0] = PossiveisMovimentosUmJogador(tabuleiro, pecasJogador1);
            possiveisJogadas[1] = PossiveisMovimentosUmJogador(tabuleiro, pecasJogador2);
            return possiveisJogadas;
        }

        public static List<Jogada> PossiveisMovimentos(Estado estado){
            List<List<Jogada>> temp = PossiveisMovimentosUmJogador(estado.tabuleiro, Tabuleiro.instance.posicoesJogadorX(estado.jogadorAtual));
            List<Jogada> resultado = new List<Jogada>();

            foreach(List<Jogada> jogada_peca in temp){
                foreach(Jogada jogada in jogada_peca){
                    resultado.Add(jogada);
                }
            }

            return resultado;
        }

        // cada item da lista está relacionado com uma peça
        // cada peça possui a própria lista de jogadas válidas
        public static List<List<Jogada>> PossiveisMovimentosUmJogador(int[,] tabuleiro, List<int[]> pecasJogador)
        {   
            int maiorNumeroPecasComidas = 0;
            List<List<Jogada>> possiveisJogadas = new List<List<Jogada>>();

            foreach (int[] peca in pecasJogador)
            {
                List<Jogada> jogadas_peca = PossiveisMovimentosUmaPeca(tabuleiro, peca[0], peca[1]);
                if (jogadas_peca.Count > 0){
                    Jogada jogadas_pecas_first = jogadas_peca[0];
                    List<int[]> pecasComidas = jogadas_pecas_first.pecasComidas;
                    int pecasComidasPecaAtual = pecasComidas.Count;
                    // se foi a jogada analisada que mais comeu peça (lei da maioria)
                    if (maiorNumeroPecasComidas < pecasComidasPecaAtual){
                        possiveisJogadas = new List<List<Jogada>>();
                        maiorNumeroPecasComidas = pecasComidasPecaAtual;
                        List<Jogada> lista_jogadas = new List<Jogada>();
                        foreach(Jogada valida in jogadas_peca)
                            if( valida.pecasComidas.Count() == maiorNumeroPecasComidas)
                                lista_jogadas.Add(valida);
                        possiveisJogadas.Add(lista_jogadas);
                    }else if (pecasComidasPecaAtual == maiorNumeroPecasComidas){
                        possiveisJogadas.Add(jogadas_peca);
                    }
                }
            }
            return possiveisJogadas;
            
        }

        // retorna os possíveis movimentos de apenas uma peça
        public static List<Jogada> PossiveisMovimentosUmaPeca(int[,] tabuleiro, int x, int y)
        {
            int jogador = Tipos.pegaJogador(tabuleiro[x, y]);

            int[] valoresPecas = GetValoresPecas(jogador);

            // se for dama
            if (tabuleiro[x, y] == valoresPecas[0])
                return MovimentosDama(tabuleiro, x, y, valoresPecas[1], valoresPecas[2]);

            List<Jogada> jogadas = new List<Jogada>();
            List<Jogada> captura = LeiDaMaioria(tabuleiro, x, y, valoresPecas[1], valoresPecas[2], null);
            // se teve alguma jogada com captura / peças comida
            if (captura != null)
            {
                foreach(Jogada jogada in captura){
                    // verifica se a peça vira dama ao final do movimento
                    int [] ultimoMovimento = jogada.movimentos[jogada.movimentos.Count() - 1];
                    if (jogador == 1){
                        if (ultimoMovimento[0] == 7)
                            jogada.virouDama = true;
                    }
                    else
                        if (ultimoMovimento[0] == 0)
                        jogada.virouDama = true;

                    // adiciona a jogada de captura como jogada possível
                    jogadas.Add(jogada);
                }
                return jogadas;
            }

            Jogada novaJogada;
            int[] posPeca = new int[2] { x, y };
            if (jogador == 1)
            {
                if ((x + 1 < 8 && y + 1 < 8) && Tipos.isVazio(tabuleiro[x + 1, y + 1]))
                {
                    novaJogada = new Jogada(posPeca);
                    novaJogada.movimentos.Add(new int[2] { x + 1, y + 1 });
                    jogadas.Add(novaJogada);
                    // verifica se virou dama
                    if (jogador == 1)
                        if (x + 1 == 7)
                            novaJogada.virouDama = true;

                }
                if ((x + 1 < 8 && y - 1 >= 0) && Tipos.isVazio(tabuleiro[x + 1, y - 1]))
                {
                    novaJogada = new Jogada(posPeca);
                    novaJogada.movimentos.Add(new int[2] { x + 1, y - 1 });
                    jogadas.Add(novaJogada);
                    if (jogador == 1)
                        if (x + 1 == 7)
                            novaJogada.virouDama = true;
                }
            }
            else
            {                    
                if ((x - 1 >= 0 && y + 1 < 8) && Tipos.isVazio(tabuleiro[x - 1, y + 1]))
                {
                    novaJogada = new Jogada(posPeca);
                    novaJogada.movimentos.Add(new int[2] { x - 1, y + 1 });
                    jogadas.Add(novaJogada);
                    // verifica se virou dama
                    if (jogador == 2)
                        if (x - 1 == 0)
                            novaJogada.virouDama = true;
                }
                if ((x - 1 >= 0 && y - 1 >= 0) && Tipos.isVazio(tabuleiro[x - 1, y - 1]))
                {
                    novaJogada = new Jogada(posPeca);
                    novaJogada.movimentos.Add(new int[2] { x - 1, y - 1 });
                    jogadas.Add(novaJogada);
                    if (jogador == 2)
                        if (x - 1 == 0)
                            novaJogada.virouDama = true;
                }
            }
            
            return jogadas;
        }

        private static List<Jogada> LeiDaMaioria(int[,] tabuleiro, int x, int y, int pecaInimiga, int damaInimiga, List<int[]> pecasComidas)
        {
            
            Jogada cimaDireita = null;
            Jogada cimaEsquerda = null;
            Jogada baixoDireita = null;
            Jogada baixoEsquerda = null;
            // esta lista guarda jogadas alternativas que capturam tantas peças quantas as principais
            List<Jogada> alternativas = null;
            int[] posPeca = new int[2] { x, y };
            // inicializa a lista de peças comidas, caso seja a primeira chamada ao método
            if (pecasComidas == null)
                pecasComidas = new List<int[]>();
            // se tem uma peça ou dama inimiga na vizinhança
            if ((x + 1 < 8 && y + 1 < 8) && (tabuleiro[x + 1, y + 1] == pecaInimiga || tabuleiro[x + 1, y + 1] == damaInimiga))
            {
                
                // e se ela ainda não foi comida numa jogada em cadeia
                if (!contemPeca(pecasComidas,new int[2] { x + 1, y + 1 }))
                {
                    // e se tem uma casa vazia logo em seguida
                    if ((x + 2 < 8 && y + 2 < 8) && Tipos.isVazio(tabuleiro[x + 2, y + 2]))
                    {
                        // então é uma jogada que come
                        if (cimaDireita == null)
                            cimaDireita = new Jogada(posPeca);
                        cimaDireita.movimentos.Add(new int[2] { x + 2, y + 2 });
                        cimaDireita.pecasComidas.Add(new int[2] { x + 1, y + 1 });
                        // atualiza o "estado" do tabuleiro
                        int pecaAtual = tabuleiro[x, y];
                        tabuleiro[x, y] = 0;
                        tabuleiro[x + 2, y + 2] = pecaAtual;
                        int indicePecasComidas = pecasComidas.Count();
                        pecasComidas.Add(new int[2] { x + 1, y + 1 });
                        // chamada recursiva para olhar as próximas jogadas
                        List<Jogada> futuras = LeiDaMaioria(tabuleiro, x + 2, y + 2, pecaInimiga, damaInimiga, pecasComidas);
                        // adiciona o resultado da(s) jogada(s) à(s) jogada(s) anterior(es)
                        if (futuras != null){
                            concatenaListas(futuras[0].movimentos, cimaDireita.movimentos);
                            concatenaListas(futuras[0].pecasComidas, cimaDireita.pecasComidas);
                            int futurasLength = futuras.Count();
                            if (futurasLength > 1) {
                                if (alternativas == null)
                                    alternativas = new List<Jogada>();
                                for(int i = 1; i < futurasLength; i++)
                                    alternativas.Add(futuras[i]);
                            }
                        }
                        // retorna os valores originais do "estado" do tabuleiro
                        tabuleiro[x, y] = pecaAtual;
                        tabuleiro[x + 2, y + 2] = 0;
                        pecasComidas.RemoveAt(indicePecasComidas);
                    }
                }
            }
            if ((x + 1 < 8 && y - 1 >= 0) && (tabuleiro[x + 1, y - 1] == pecaInimiga || tabuleiro[x + 1, y - 1] == damaInimiga))
            {
                
                if (!contemPeca(pecasComidas,new int[2] { x + 1, y - 1 }))
                {
                    if ((x + 2 < 8 && y - 2 >= 0) && Tipos.isVazio(tabuleiro[x + 2, y - 2]))
                    {
                        
                        if (cimaEsquerda == null)
                            cimaEsquerda = new Jogada(posPeca);
                        cimaEsquerda.movimentos.Add(new int[2] { x + 2, y - 2 });
                        cimaEsquerda.pecasComidas.Add(new int[2] { x + 1, y - 1 });
                        int pecaAtual = tabuleiro[x, y];
                        tabuleiro[x, y] = 0;
                        tabuleiro[x + 2, y - 2] = pecaAtual;
                        int indicePecasComidas = pecasComidas.Count();
                        pecasComidas.Add(new int[2] { x + 1, y - 1 });
                        List<Jogada> futuras = LeiDaMaioria(tabuleiro, x + 2, y - 2, pecaInimiga, damaInimiga, pecasComidas);
                        // adiciona o resultado da(s) jogada(s) à(s) jogada(s) anterior(es)
                        if (futuras != null){
                            concatenaListas(futuras[0].movimentos, cimaEsquerda.movimentos);
                            concatenaListas(futuras[0].pecasComidas, cimaEsquerda.pecasComidas);
                            int futurasLength = futuras.Count();
                            if (futurasLength > 1) {
                                if (alternativas == null)
                                    alternativas = new List<Jogada>();
                                for(int i = 1; i < futurasLength; i++)
                                    alternativas.Add(futuras[i]);
                            }
                        }
                        tabuleiro[x, y] = pecaAtual;
                        tabuleiro[x + 2, y - 2] = 0;
                        pecasComidas.RemoveAt(indicePecasComidas);
                    }
                }
            }
            if ((x - 1 >= 0 && y + 1 < 8) && (tabuleiro[x - 1, y + 1] == pecaInimiga || tabuleiro[x - 1, y + 1] == damaInimiga))
            {
                
                if (!contemPeca(pecasComidas, new int[2] { x - 1, y + 1 }))
                {
                    if ((x - 2 >= 0 && y + 2 < 8) && Tipos.isVazio(tabuleiro[x - 2, y + 2]))
                    {
                        if (baixoDireita == null)
                            baixoDireita = new Jogada(posPeca);
                        baixoDireita.movimentos.Add(new int[2] { x - 2, y + 2 });
                        baixoDireita.pecasComidas.Add(new int[2] { x - 1, y + 1 });
                        int pecaAtual = tabuleiro[x, y];
                        tabuleiro[x, y] = 0;
                        tabuleiro[x - 2, y + 2] = pecaAtual;
                        int indicePecasComidas = pecasComidas.Count();
                        pecasComidas.Add(new int[2] { x - 1, y + 1 });
                        List<Jogada> futuras = LeiDaMaioria(tabuleiro, x - 2, y + 2, pecaInimiga, damaInimiga, pecasComidas);
                        // adiciona o resultado da(s) jogada(s) à(s) jogada(s) anterior(es)
                        if (futuras != null){
                            concatenaListas(futuras[0].movimentos, baixoDireita.movimentos);
                            concatenaListas(futuras[0].pecasComidas, baixoDireita.pecasComidas);
                            int futurasLength = futuras.Count();
                            if (futurasLength > 1) {
                                if (alternativas == null)
                                    alternativas = new List<Jogada>();
                                for(int i = 1; i < futurasLength; i++)
                                    alternativas.Add(futuras[i]);
                            }
                        }
                        tabuleiro[x, y] = pecaAtual;
                        tabuleiro[x - 2, y + 2] = 0;
                        pecasComidas.RemoveAt(indicePecasComidas);
                    }
                }
            }
            if ((x - 1 >= 0 && y - 1 >= 0) && (tabuleiro[x - 1, y - 1] == pecaInimiga || tabuleiro[x - 1, y - 1] == damaInimiga))
            {
                if (!contemPeca(pecasComidas, new int[2] { x - 1, y - 1 }))
                {
                    if ((x - 2 >= 0 && y - 2 >= 0) && Tipos.isVazio(tabuleiro[x - 2, y - 2]))
                    {
                         
                        if (baixoEsquerda == null)
                            baixoEsquerda = new Jogada(posPeca);
                        baixoEsquerda.movimentos.Add(new int[2] { x - 2, y - 2 });
                        baixoEsquerda.pecasComidas.Add(new int[2] { x - 1, y - 1 });
                        int pecaAtual = tabuleiro[x, y];
                        tabuleiro[x, y] = 0;
                        tabuleiro[x - 2, y - 2] = pecaAtual;
                        int indicePecasComidas = pecasComidas.Count();
                        pecasComidas.Add(new int[2] { x - 1, y - 1 });
                        List<Jogada> futuras = LeiDaMaioria(tabuleiro, x - 2, y - 2, pecaInimiga, damaInimiga, pecasComidas);
                        // adiciona o resultado da(s) jogada(s) à(s) jogada(s) anterior(es)
                        if (futuras != null){
                            concatenaListas(futuras[0].movimentos, baixoEsquerda.movimentos);
                            concatenaListas(futuras[0].pecasComidas, baixoEsquerda.pecasComidas);
                            int futurasLength = futuras.Count();
                            if (futurasLength > 1) {
                                if (alternativas == null)
                                    alternativas = new List<Jogada>();
                                for(int i = 1; i < futurasLength; i++)
                                    alternativas.Add(futuras[i]);
                            }
                        }
                        tabuleiro[x, y] = pecaAtual;
                        tabuleiro[x - 2, y - 2] = 0;
                        pecasComidas.RemoveAt(indicePecasComidas);
                    }
                }
            }
            List<Jogada> melhor = null;
            int maiorQtdCapturas = 0;
            if (alternativas != null){
                foreach(Jogada jogada in alternativas){
                    if(jogada.pecasComidas.Count() > maiorQtdCapturas){
                        melhor = new List<Jogada>();
                        melhor.Add(jogada);
                        maiorQtdCapturas = jogada.pecasComidas.Count();
                    }
                    else if(jogada.pecasComidas.Count() > 0 && jogada.pecasComidas.Count() == maiorQtdCapturas){
                        if(melhor == null)
                            melhor = new List<Jogada>();
                        melhor.Add(jogada);
                    }
                }
            }
            if (cimaDireita != null && cimaDireita.pecasComidas.Count() > maiorQtdCapturas){
                melhor = new List<Jogada>();
                melhor.Add(cimaDireita);
                maiorQtdCapturas = cimaDireita.pecasComidas.Count();
            }
            else if (cimaDireita != null &&  cimaDireita.pecasComidas.Count() > 0 && cimaDireita.pecasComidas.Count() == maiorQtdCapturas){
                if(melhor == null)
                    melhor = new List<Jogada>();
                melhor.Add(cimaDireita);
            }
            //
            if (cimaEsquerda != null && cimaEsquerda.pecasComidas.Count() > maiorQtdCapturas){
                melhor = new List<Jogada>();
                melhor.Add(cimaEsquerda);
                maiorQtdCapturas = cimaEsquerda.pecasComidas.Count();
            }
            else if (cimaEsquerda != null && cimaEsquerda.pecasComidas.Count() > 0 && cimaEsquerda.pecasComidas.Count() == maiorQtdCapturas){
                if(melhor == null)
                    melhor = new List<Jogada>();
                melhor.Add(cimaEsquerda);
            }
            //
            if (baixoDireita != null && baixoDireita.pecasComidas.Count() > maiorQtdCapturas){
                melhor = new List<Jogada>();
                melhor.Add(baixoDireita);
                maiorQtdCapturas = baixoDireita.pecasComidas.Count();
            }
            else if (baixoDireita != null && baixoDireita.pecasComidas.Count() > 0 && baixoDireita.pecasComidas.Count() == maiorQtdCapturas){
                if(melhor == null)
                    melhor = new List<Jogada>();
                melhor.Add(baixoDireita);
            }
            //
            if (baixoEsquerda != null && baixoEsquerda.pecasComidas.Count() > maiorQtdCapturas){
                melhor = new List<Jogada>();
                melhor.Add(baixoEsquerda);
                maiorQtdCapturas = baixoEsquerda.pecasComidas.Count();
            }
            else if (baixoEsquerda != null && baixoEsquerda.pecasComidas.Count() > 0 && baixoEsquerda.pecasComidas.Count() == maiorQtdCapturas){
                if(melhor == null)
                    melhor = new List<Jogada>();
                melhor.Add(baixoEsquerda);
            }
            return melhor;
        }

        private static List<Jogada> LeiDaMaioriaDamas(int[,] tabuleiro, int x, int y, int pecaInimiga, int damaInimiga, List<int[]> pecasComidas)
        {
            Jogada cimaDireita = null;
            Jogada cimaEsquerda = null;
            Jogada baixoDireita = null;
            Jogada baixoEsquerda = null;
            // esta lista guarda jogadas alternativas que capturam tantas peças quantas as principais
            List<Jogada> alternativas = null;
            // inicializa a lista de peças comidas, caso seja a primeira chamada ao método
            if (pecasComidas == null)
                pecasComidas = new List<int[]>();

            // percorrer cada diagonal a procura da peças para comer
            int i, j;
            for (i = x + 1, j = y + 1; i < 8 && j < 8; i++, j++)
            {
                // se encontrou uma peça pra comer
                if (tabuleiro[i, j] == pecaInimiga || tabuleiro[i, j] == damaInimiga)
                {
                    // e se ela ainda não foi comida numa jogada em cadeia
                    if (!contemPeca(pecasComidas, new int[2] { i, j }))
                    {
                        // e se tem uma casa vazia logo em seguida
                        if ((i + 1 < 8 && j + 1 < 8) && Tipos.isVazio(tabuleiro[i + 1, j + 1]))
                        {
                            // então é uma jogada que come
                            if (cimaDireita == null)
                                cimaDireita = new Jogada(new int[2] { x, y });
                            cimaDireita.movimentos.Add(new int[2] { i + 1, j + 1 });
                            cimaDireita.pecasComidas.Add(new int[2] { i, j });
                            // atualiza o "estado" do tabuleiro
                            int pecaAtual = tabuleiro[x, y];
                            tabuleiro[x, y] = 0;
                            tabuleiro[i + 1, j + 1] = pecaAtual;
                            int indicePecasComidas = pecasComidas.Count();
                            pecasComidas.Add(new int[2] { i, j });
                            // chamada recursiva para olhar as próximas jogadas
                            List<Jogada> futuras = LeiDaMaioriaDamas(tabuleiro, i + 1, j + 1, pecaInimiga, damaInimiga, pecasComidas);
                            // adiciona o resultado da(s) jogada(s) à(s) jogada(s) anterior(es)
                            if (futuras != null)
                            {
                                concatenaListas(futuras[0].movimentos, cimaDireita.movimentos);
                                concatenaListas(futuras[0].pecasComidas, cimaDireita.pecasComidas);
                                int futurasLength = futuras.Count();
                                if (futurasLength > 1)
                                {
                                    if (alternativas == null)
                                        alternativas = new List<Jogada>();
                                    for (int k = 1; k < futurasLength; k++)
                                        alternativas.Add(futuras[k]);
                                }
                            }
                            // retorna os valores originais do "estado" do tabuleiro
                            tabuleiro[x, y] = pecaAtual;
                            tabuleiro[i + 1, j + 1] = 0;
                            pecasComidas.RemoveAt(indicePecasComidas);
                        }
                        // caso onde não pode movimentar mais nessa diagonal
                        else break;
                    }
                }
            }
            for (i = x + 1, j = y - 1; i < 8 && j >= 0; i++, j--)
            {
                if (tabuleiro[i, j] == pecaInimiga || tabuleiro[i, j] == damaInimiga)
                {
                    if (!contemPeca(pecasComidas, new int[2] { i, j }))
                    {
                        if ((i + 1 < 8 && j - 1 >= 0) && Tipos.isVazio(tabuleiro[i + 1, j - 1]))
                        {
                            if (cimaEsquerda == null)
                                cimaEsquerda = new Jogada(new int[2] { x, y });
                            cimaEsquerda.movimentos.Add(new int[2] { i + 1, j - 1 });
                            cimaEsquerda.pecasComidas.Add(new int[2] { i, j });
                            int pecaAtual = tabuleiro[x, y];
                            tabuleiro[x, y] = 0;
                            tabuleiro[i + 1, j - 1] = pecaAtual;
                            int indicePecasComidas = pecasComidas.Count();
                            pecasComidas.Add(new int[2] { i, j });
                            List<Jogada> futuras = LeiDaMaioriaDamas(tabuleiro, i + 1, j - 1, pecaInimiga, damaInimiga, pecasComidas);
                            if (futuras != null)
                            {
                                concatenaListas(futuras[0].movimentos, cimaEsquerda.movimentos);
                                concatenaListas(futuras[0].pecasComidas, cimaEsquerda.pecasComidas);
                                int futurasLength = futuras.Count();
                                if (futurasLength > 1)
                                {
                                    if (alternativas == null)
                                        alternativas = new List<Jogada>();
                                    for (int k = 1; k < futurasLength; k++)
                                        alternativas.Add(futuras[k]);
                                }
                            }
                            tabuleiro[x, y] = pecaAtual;
                            tabuleiro[i + 1, j - 1] = 0;
                            pecasComidas.RemoveAt(indicePecasComidas);
                        }
                        else break;
                    }
                }
            }
            for (i = x - 1, j = y + 1; i >= 0 && j < 8; i--, j++)
            {
                if (tabuleiro[i, j] == pecaInimiga || tabuleiro[i, j] == damaInimiga)
                {
                    if (!contemPeca(pecasComidas, new int[2] { i, j }))
                    {
                        if ((i - 1 >= 0 && j + 1 < 8) && Tipos.isVazio(tabuleiro[i - 1, j + 1]))
                        {
                            if (baixoDireita == null)
                                baixoDireita = new Jogada(new int[2] { x, y });
                            baixoDireita.movimentos.Add(new int[2] { i - 1, j + 1 });
                            baixoDireita.pecasComidas.Add(new int[2] { i, j });
                            int pecaAtual = tabuleiro[x, y];
                            tabuleiro[x, y] = 0;
                            tabuleiro[i - 1, j + 1] = pecaAtual;
                            int indicePecasComidas = pecasComidas.Count();
                            pecasComidas.Add(new int[2] { i, j });
                            List<Jogada> futuras = LeiDaMaioriaDamas(tabuleiro, i - 1, j + 1, pecaInimiga, damaInimiga, pecasComidas);
                            if (futuras != null)
                            {
                                concatenaListas(futuras[0].movimentos, baixoDireita.movimentos);
                                concatenaListas(futuras[0].pecasComidas, baixoDireita.pecasComidas);
                                int futurasLength = futuras.Count();
                                if (futurasLength > 1)
                                {
                                    if (alternativas == null)
                                        alternativas = new List<Jogada>();
                                    for (int k = 1; k < futurasLength; k++)
                                        alternativas.Add(futuras[k]);
                                }
                            }
                            tabuleiro[x, y] = pecaAtual;
                            tabuleiro[i - 1, j + 1] = 0;
                            pecasComidas.RemoveAt(indicePecasComidas);
                        }
                        else break;
                    }
                }
            }
            for (i = x - 1, j = y - 1; i >= 0 && j >= 0; i--, j--)
            {
                if (tabuleiro[i, j] == pecaInimiga || tabuleiro[i, j] == damaInimiga)
                {
                    if (!contemPeca(pecasComidas, new int[2] { i, j }))
                    {
                        if ((i - 1 >= 0 && j - 1 >= 0) && Tipos.isVazio(tabuleiro[i - 1, j - 1]))
                        {
                            if (baixoEsquerda == null)
                                baixoEsquerda = new Jogada(new int[2] { x, y });
                            baixoEsquerda.movimentos.Add(new int[2] { i - 1, j - 1 });
                            baixoEsquerda.pecasComidas.Add(new int[2] { i, j });
                            int pecaAtual = tabuleiro[x, y];
                            tabuleiro[x, y] = 0;
                            tabuleiro[i - 1, j - 1] = pecaAtual;
                            int indicePecasComidas = pecasComidas.Count();
                            pecasComidas.Add(new int[2] { i, j });
                            List<Jogada> futuras = LeiDaMaioriaDamas(tabuleiro, i - 1, j - 1, pecaInimiga, damaInimiga, pecasComidas);
                            if (futuras != null)
                            {
                                concatenaListas(futuras[0].movimentos, baixoEsquerda.movimentos);
                                concatenaListas(futuras[0].pecasComidas, baixoEsquerda.pecasComidas);
                                int futurasLength = futuras.Count();
                                if (futurasLength > 1)
                                {
                                    if (alternativas == null)
                                        alternativas = new List<Jogada>();
                                    for (int k = 1; k < futurasLength; k++)
                                        alternativas.Add(futuras[k]);
                                }
                            }
                            tabuleiro[x, y] = pecaAtual;
                            tabuleiro[i - 1, j - 1] = 0;
                            pecasComidas.RemoveAt(indicePecasComidas);
                        }
                        else break;
                    }
                }
            }
            List<Jogada> melhor = null;
            int maiorQtdCapturas = 0;
            if (alternativas != null)
            {
                foreach(Jogada jogada in alternativas)
                {
                    if(jogada.pecasComidas.Count() > maiorQtdCapturas)
                    {
                        melhor = new List<Jogada>();
                        melhor.Add(jogada);
                        maiorQtdCapturas = jogada.pecasComidas.Count();
                    }
                    else if (jogada.pecasComidas.Count() > 0 && jogada.pecasComidas.Count() == maiorQtdCapturas)
                    {
                        if (melhor == null)
                            melhor = new List<Jogada>();
                        melhor.Add(jogada);
                    }
                }
            }
            if (cimaDireita != null && cimaDireita.pecasComidas.Count() > maiorQtdCapturas)
            {
                melhor = new List<Jogada>();
                melhor.Add(cimaDireita);
                maiorQtdCapturas = cimaDireita.pecasComidas.Count();
            }
            else if (cimaDireita != null && cimaDireita.pecasComidas.Count() > 0 && cimaDireita.pecasComidas.Count() == maiorQtdCapturas)
            {
                if (melhor == null)
                    melhor = new List<Jogada>();
                melhor.Add(cimaDireita);
            }
            //
            if (cimaEsquerda != null && cimaEsquerda.pecasComidas.Count() > maiorQtdCapturas)
            {
                melhor = new List<Jogada>();
                melhor.Add(cimaEsquerda);
                maiorQtdCapturas = cimaEsquerda.pecasComidas.Count();
            }
            else if (cimaEsquerda != null && cimaEsquerda.pecasComidas.Count() > 0 && cimaEsquerda.pecasComidas.Count() == maiorQtdCapturas)
            {
                if (melhor == null)
                    melhor = new List<Jogada>();
                melhor.Add(cimaEsquerda);
            }
            //
            if (baixoDireita != null && baixoDireita.pecasComidas.Count() > maiorQtdCapturas)
            {
                melhor = new List<Jogada>();
                melhor.Add(baixoDireita);
                maiorQtdCapturas = baixoDireita.pecasComidas.Count();
            }
            else if (baixoDireita != null && baixoDireita.pecasComidas.Count() > 0 && baixoDireita.pecasComidas.Count() == maiorQtdCapturas)
            {
                if (melhor == null)
                    melhor = new List<Jogada>();
                melhor.Add(baixoDireita);
            }
            //
            if (baixoEsquerda != null && baixoEsquerda.pecasComidas.Count() > maiorQtdCapturas)
            {
                melhor = new List<Jogada>();
                melhor.Add(baixoEsquerda);
                maiorQtdCapturas = baixoEsquerda.pecasComidas.Count();
            }
            else if (baixoEsquerda != null && baixoEsquerda.pecasComidas.Count() > 0 && baixoEsquerda.pecasComidas.Count() == maiorQtdCapturas)
            {
                if (melhor == null)
                    melhor = new List<Jogada>();
                melhor.Add(baixoEsquerda);
            }
            return melhor;
        }

        // retorna os possíveis movimentos para dama
        private static List<Jogada> MovimentosDama(int[,] tabuleiro, int x, int y, int pecaInimiga, int damaInimiga)
        {
            List<Jogada> jogadasPossiveis = new List<Jogada>();
            List<Jogada> captura = LeiDaMaioriaDamas(tabuleiro, x, y, pecaInimiga, damaInimiga, null);
            int i, j;
            // se teve alguma jogada com captura / peças comida
            if (captura != null)
            {
                foreach (Jogada jogada in captura)
                {
                    jogadasPossiveis.Add(jogada);
                    // o próximo trecho de código capacita a dama a andar para qualquer casa na diagonal após comer uma peça
                    int[] posUltimaPecaComida = jogada.pecasComidas[jogada.pecasComidas.Count() - 1];
                    int[] posUltimoMovimento = jogada.movimentos[jogada.movimentos.Count() - 1];
                    // descobre de qual diagonal veio o ultimo movimento de captura
                    if ((posUltimoMovimento[0] + 1) == posUltimaPecaComida[0] && posUltimoMovimento[1] + 1 == posUltimaPecaComida[1])
                    {
                        // adiciona aos movimentos a diagonal
                        for (i = posUltimoMovimento[0] - 1, j = posUltimoMovimento[1] - 1; i >= 0 && j >= 0; i--, j--)
                        {
                            if (Tipos.isVazio(tabuleiro[i, j]))
                            {
                                Jogada novaJogada = new Jogada(jogada.posInicial);
                                copiaJogada(jogada, novaJogada);
                                novaJogada.movimentos.Add(new int[2] { i, j });
                                jogadasPossiveis.Add(novaJogada);
                            }
                            else break;
                        }
                    }
                    else if (posUltimoMovimento[0] + 1 == posUltimaPecaComida[0] && posUltimoMovimento[1] - 1 == posUltimaPecaComida[1])
                    {
                        for(i = posUltimoMovimento[0] - 1, j = posUltimoMovimento[1] + 1; i >= 0 && j < 8; i--, j++)
                        {
                            if (Tipos.isVazio(tabuleiro[i, j]))
                            {
                                Jogada novaJogada = new Jogada(jogada.posInicial);
                                copiaJogada(jogada, novaJogada);
                                novaJogada.movimentos.Add(new int[2] { i, j });
                                jogadasPossiveis.Add(novaJogada);
                            }
                            else break;
                        }
                    }
                    else if (posUltimoMovimento[0] - 1 == posUltimaPecaComida[0] && posUltimoMovimento[1] + 1 == posUltimaPecaComida[1])
                    {
                        for (i = posUltimoMovimento[0] + 1, j = posUltimoMovimento[1] - 1; i < 8 && j >= 0 ; i++, j--)
                        {
                            if (Tipos.isVazio(tabuleiro[i, j]))
                            {
                                Jogada novaJogada = new Jogada(jogada.posInicial);
                                copiaJogada(jogada, novaJogada);
                                novaJogada.movimentos.Add(new int[2] { i, j });
                                jogadasPossiveis.Add(novaJogada);
                            }
                            else break;
                        }
                    }
                    else
                    {
                        for (i = posUltimoMovimento[0] + 1, j = posUltimoMovimento[1] + 1; i < 8 && j < 8; i++, j++)
                        {
                            if (Tipos.isVazio(tabuleiro[i, j]))
                            {
                                Jogada novaJogada = new Jogada(jogada.posInicial);
                                copiaJogada(jogada, novaJogada);
                                novaJogada.movimentos.Add(new int[2] { i, j });
                                jogadasPossiveis.Add(novaJogada);
                            }
                            else break;
                        }
                    }
                }
                return jogadasPossiveis;
            }
            int[] posPeca = new int[2] { x, y };
            for (i = x + 1, j = y + 1; i < 8 && j < 8; i++, j++)
            {
                if (Tipos.isVazio(tabuleiro[i, j]))
                {
                    Jogada nova = new Jogada(posPeca);
                    nova.movimentos.Add(new int[2] { i, j });
                    if (jogadasPossiveis == null)
                        jogadasPossiveis = new List<Jogada>();
                    jogadasPossiveis.Add(nova);
                }
                // caso onde achou uma peça aliada
                else break;

            }
            for (i = x + 1, j = y - 1; i < 8 && j >= 0; i++, j--)
            {
                if (Tipos.isVazio(tabuleiro[i, j]))
                {
                    Jogada nova = new Jogada(posPeca);
                    nova.movimentos.Add(new int[2] { i, j });
                    if (jogadasPossiveis == null)
                        jogadasPossiveis = new List<Jogada>();
                    jogadasPossiveis.Add(nova);
                }
                // caso onde achou uma peça aliada
                else break;
            }
            for (i = x - 1, j = y + 1; i >= 0 && j < 8; i--, j++)
            {
                if (Tipos.isVazio(tabuleiro[i, j]))
                {
                    Jogada nova = new Jogada(posPeca);
                    nova.movimentos.Add(new int[2] { i, j });
                    if (jogadasPossiveis == null)
                        jogadasPossiveis = new List<Jogada>();
                    jogadasPossiveis.Add(nova);
                }
                // caso onde achou uma peça aliada
                else break;
            }
            for (i = x - 1, j = y - 1; i >= 0 && j >= 0; i--, j--)
            {
                if (Tipos.isVazio(tabuleiro[i, j]))
                {
                    Jogada nova = new Jogada(posPeca);
                    nova.movimentos.Add(new int[2] { i, j });
                    if (jogadasPossiveis == null)
                        jogadasPossiveis = new List<Jogada>();
                    jogadasPossiveis.Add(nova);
                }
                // caso onde achou uma peça aliada
                else break;
            }
            return jogadasPossiveis;
        }

        private static  bool contemPeca(List<int[]> lista, int[] peca){
            foreach(int[] peca_lista in lista)
                if(peca_lista[0] == peca[0] && peca_lista[1] == peca[1])
                    return true;
            return false;
        }

        private static void copiaJogada(Jogada origem, Jogada destino){
            origem.posInicial = destino.posInicial;
            foreach(int[] movimento in origem.movimentos)
                destino.movimentos.Add(new int[2] {movimento[0], movimento[1]});
            foreach(int[] pecaComida in origem.pecasComidas)
                destino.pecasComidas.Add(new int[2] {pecaComida[0], pecaComida[1]});
        }

        private static void concatenaListas(List<int[]> origem, List<int[]> destino)
        {
            foreach(int[] peca in origem)
                destino.Add(peca);
        }
    }
}
