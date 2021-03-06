﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using IANS;
using EstadoNS;
using TabuleiroNS;
using TiposNS;

public class GameController : MonoBehaviour {

    public static GameController instance { get; private set; }
    public Estado estadoAtual;
    public Jogador jogadorAtual; 

    private static Jogador jogador1;
    private static Jogador jogador2;

    public LayerMask layerJogador1;
    public LayerMask layerJogador2;

	private Text textIndicator;

    public const int GAME_MODE_PLAYER_VS_IA = 1;
    public const int GAME_MODE_IA_VS_IA = 2;
    public const int GAME_MODE_PLAYER_VS_PLAYER = 3;

    private static bool created = false;
    private bool emJogo = false;
    private bool iaxia = false;
    
    private Movimentacao script_movimentacao;

	void Awake (){
        instance = this;
        if (!created){
            DontDestroyOnLoad(this.gameObject);
            created = true;
        }else {
            // vai entrar aqui quando for uma scene diferente do menu inicial
            textIndicator = GameObject.Find("TurnoText").GetComponent<Text>();
            loadGameMode();
            emJogo = true;
            this.script_movimentacao = GetComponent<Movimentacao>();
            
        }
    }

    private void Start()
    {
        if (emJogo)
            estadoAtual = new Estado(Tabuleiro.instance.matrizTabuleiroInt, jogadorAtual.getNumeroJogador(), null,0); 
        
        if (iaxia)
            passarTurno();       
    }

    public void passarTurno(){
        setJogadorAtual(isJogadorAtual(jogador1) ? jogador2 : jogador1);

        setTextoTurno("Turno: " + jogadorAtual.getNomeJogador());

        if (jogadorAtual.isIA()){
            StartCoroutine(IAJogaComAtraso());
        }

        script_movimentacao.jogo = this.verificaVitoriaEmpate(this.estadoAtual);

    }

    IEnumerator IAJogaComAtraso()
    {
        yield return new WaitForSeconds(1.7f);   
        Jogada jogada_ia = jogadorAtual.callAIAction(estadoAtual);

        script_movimentacao.movimentaPecaPorJogada(jogada_ia);

        //script_movimentacao.movimentaPecaPorJogada(jogada_ia);

        this.estadoAtual.tabuleiro = script_movimentacao.alteraMatriz(this.estadoAtual.tabuleiro, jogada_ia);
        this.estadoAtual.ultimaJogada = jogada_ia;
        
        passarTurno();
    }

	private void setTextoTurno(string new_texto){
		textIndicator.text = new_texto;
	}

    public bool getTurnoJogador()
    {
        if (jogadorAtual == null) return false;
        return jogadorAtual.isPlayer();
    }

    public bool isJogadorAtual(Jogador jogador){
        return jogador == jogadorAtual;
    }

    public void switchScene(int gameMode){
        PlayerPrefs.SetInt("GameMode", gameMode);
        loadGameScene();
    }

    private void loadPlayervsPlayerGame(){
        Jogador player1 = new Jogador("Jogador1"); 
        Jogador player2 = new Jogador("Jogador2"); 
        defineJogadores(player1, player2);
    }

    private void loadPlayervsIAGame(){
        Jogador player = new Jogador("Jogador"); 
        Jogador ia = new Jogador("IA 1", new IA());
        defineJogadores(player, ia);
    }

    private void loadIAvsIAGame(){
        Jogador ia1 = new Jogador("IA 1", new IA(1));
        Jogador ia2 = new Jogador("IA 2", new IA(2));
        //defineJogadores(ia1, ia2);
        defineJogadores(ia2, ia1); // ao contrário de proposito pra chamar passar turno
        this.iaxia = true;
    }
    
    private void loadGameScene(){
        SceneManager.LoadScene("initial", LoadSceneMode.Single);
    }

    private void defineJogadores(Jogador player1, Jogador player2){        
        jogador1 = player1;
        jogador2 = player2;

        jogador1.layerMaskValue = layerJogador1.value;

        jogador2.layerMaskValue = layerJogador2.value;

        setJogadorAtual(jogador1);
    }

    private void loadGameMode(){
        int gameMode = PlayerPrefs.GetInt("GameMode");
        if (gameMode == GAME_MODE_PLAYER_VS_IA) loadPlayervsIAGame();
        else if (gameMode == GAME_MODE_IA_VS_IA) loadIAvsIAGame();
        else loadPlayervsPlayerGame();        
        setTextoTurno("Turno: " + jogadorAtual.getNomeJogador());
    }

    public Jogador getJogador1(){
        return jogador1;
    }
    public Jogador getJogador2(){
        return jogador2;
    }

    public void setJogadorAtual(Jogador novoJogador)
    {
        this.jogadorAtual = novoJogador;
        if (estadoAtual != null)
        {
            this.estadoAtual.setJogadorAtual(novoJogador.getNumeroJogador());
        }

    }

    public List<int []> posicoes_jogador_atual(){
        return this.estadoAtual.posicoesJogadorX(GameController.instance.jogadorAtual.getNumeroJogador());
    }

    public int verificaVitoriaEmpate(Estado estado)
    {
        // 0: não está em vitória nem em empate
        // 1: vitória do jogador 1
        // 2: vitória do jogador 2
        // 3: empate
        int resultado = 0;

        int qtdPecasJogador1, qtdPecasjogador2, qtdDamasJogador1, qtdDamasJogador2, qtdPecasNormaisJogador1, qtdPecasNormaisJogador2;
        qtdPecasJogador1 = qtdPecasjogador2 = qtdDamasJogador1 = qtdDamasJogador2 = qtdPecasNormaisJogador1 = qtdPecasNormaisJogador2 = 0;
        foreach(int peca in estado.tabuleiro)
        {
            if (Tipos.isJogador1(peca))
            {
                if (Tipos.isDama(peca))
                {
                    qtdDamasJogador1++;
                }
                qtdPecasJogador1++;
            }
            else if(Tipos.isJogador2(peca))
            {
                if (Tipos.isDama(peca))
                {
                    qtdDamasJogador2++;
                }
                qtdPecasjogador2++;
            }
        }

        if (qtdPecasJogador1 == 0)
        {
            mudaTexto("JOGADOR 2 GANHOU!!");
            return 2; //jogador 1 sem peças, vitoria do jogador 2
        }else if(qtdPecasjogador2 == 0)
        {
            mudaTexto("JOGADOR 1 GANHOU!!");
            return 1;//jogador 2 sem peças, vitoria do jogador 1
        }

        qtdPecasNormaisJogador1 = qtdPecasJogador1 - qtdDamasJogador1;
        qtdPecasNormaisJogador2 = qtdPecasjogador2 - qtdDamasJogador2;
        int qtdPecasNormais = qtdPecasNormaisJogador1 + qtdPecasNormaisJogador2;

        // 2 damas contra 2 damas;
        // 2 damas contra uma;
        // 2 damas contra uma dama e uma pedra;
        // uma dama contra uma dama e uma dama contra uma dama e uma pedra, são declarados empatados após 5 lances de cada jogador.

        if (estado.jogadasCondicaoEmpate >= 20
           || ((qtdDamasJogador1 == 2 && qtdDamasJogador2 == 2)  && qtdPecasNormais == 0)
           || ((qtdDamasJogador1 == 2 && qtdDamasJogador2 == 1) && qtdPecasNormais == 0)
           || ((qtdDamasJogador1 == 1 && qtdDamasJogador2 == 2) && qtdPecasNormais == 0)
           || ((qtdDamasJogador1 == 2 && qtdDamasJogador2 == 1) && qtdPecasNormaisJogador2 == 1)
           || ((qtdDamasJogador1 == 1 && qtdDamasJogador2 == 2) && qtdPecasNormaisJogador1 == 1)
           || ((qtdDamasJogador1 == 1 && qtdDamasJogador2 == 1 && qtdPecasNormais == 0) && estado.jogadasCondicaoEmpate >= 5)
           || ((qtdDamasJogador1 == 1 && qtdDamasJogador2 == 1 && qtdPecasNormais == 1) && estado.jogadasCondicaoEmpate >= 5))
        {
            mudaTexto("EMPATE!!");
            return 3;
        }

        Debug.Log("Qtd Pecas Jogador 1: " + qtdPecasJogador1 + " Qtd Pecas Jogador 2: " + qtdPecasjogador2);
        return resultado; 
    }

    public void mudaTexto(string mensagem)
    {
        textIndicator.text = mensagem;
    }

    public void reiniciaJogo()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
