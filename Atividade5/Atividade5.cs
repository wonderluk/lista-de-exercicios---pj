

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// Representa uma única ação registrada no tempo
class Acao
{
    public string   Nome    { get; }
    public DateTime Instante { get; }

    public Acao(string nome)
    {
        Nome     = nome;
        Instante = DateTime.Now;   
    }
}

// Armazena a configuração de um padrão a detectar
class Sequencia
{
    public string   Nome           { get; }
    public string[] Passos         { get; }
    public double   JanelaSegundos { get; }
    public Action   Callback       { get; }

    public Sequencia(string nome, string[] passos, double janelaSegundos, Action callback)
    {
        Nome           = nome;
        Passos         = passos;
        JanelaSegundos = janelaSegundos;
        Callback       = callback;
    }
}

// Motor de detecção de sequências
class DetectorDeSequencia
{
    private readonly List<Acao> _historico = new();

    private readonly List<Sequencia> _sequencias = new();

    public event Action<string>? OnAcaoRegistrada;

    public event Action<string>? OnSequenciaDetectada;

    // Registra uma nova ação com o timestamp atual e verifica padrões
    public void RegistrarAcao(string nome)
    {
        var novaAcao = new Acao(nome);
        _historico.Add(novaAcao);

        OnAcaoRegistrada?.Invoke(nome);

        VerificarSequencias();
    }

    // Cadastra um padrão para ser monitorado continuamente
    public void RegistrarSequencia(string nomeDaSequencia, string[] passos,
                                   double janelaSegundos, Action callback)
    {
        _sequencias.Add(new Sequencia(nomeDaSequencia, passos, janelaSegundos, callback));
    }

    // Verifica se o final do histórico casa com alguma sequência
    private void VerificarSequencias()
    {
        foreach (var seq in _sequencias)
        {
            int n = seq.Passos.Length;

            if (_historico.Count < n)
                continue;

            var ultimas = _historico.GetRange(_historico.Count - n, n);

            bool nomesCorretos = ultimas
                .Select(a => a.Nome)
                .SequenceEqual(seq.Passos);

            if (!nomesCorretos)
                continue;

            double deltaSegundos =
                (ultimas.Last().Instante - ultimas.First().Instante).TotalSeconds;

            if (deltaSegundos <= seq.JanelaSegundos)
            {

                OnSequenciaDetectada?.Invoke(seq.Nome);
                seq.Callback();
            }
            else
            {

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"({seq.Nome} não detectado — janela de tempo expirou: {deltaSegundos:F2}s > {seq.JanelaSegundos}s)");
                Console.ResetColor();
            }
        }
    }
}

// Programa de demonstração
class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var detector = new DetectorDeSequencia();

        detector.OnAcaoRegistrada += nome =>
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[Ação registrada] {nome}");
            Console.ResetColor();
        };

        detector.OnSequenciaDetectada += nomeSeq =>
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Sequência detectada] {nomeSeq}");
            Console.ResetColor();
        };

        detector.RegistrarSequencia(
            "Padrão1",
            new[] { "a", "b", "c" },
            janelaSegundos: 2.0,
            callback: () =>
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  ★ Callback Padrão1: combo ABC executado!");
                Console.ResetColor();
            }
        );

        detector.RegistrarSequencia(
            "Padrão2",
            new[] { "x", "x", "y" },
            janelaSegundos: 1.0,
            callback: () =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ★ Callback Padrão2: combo XXY executado!");
                Console.ResetColor();
            }
        );

        detector.RegistrarSequencia(
            "Padrão3",
            new[] { "cima", "baixo", "cima", "baixo" },
            janelaSegundos: 3.0,
            callback: () =>
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("  ★ Callback Padrão3: Konami parcial detectado!");
                Console.ResetColor();
            }
        );

        // ── Sequência de demonstração ────────────────────────

        Secao("Bloco 1 — deve detectar Padrão1");
        detector.RegistrarAcao("a");
        detector.RegistrarAcao("b");
        detector.RegistrarAcao("c");    

        Secao("Bloco 2 — NÃO deve detectar Padrão2 (janela expirada)");
        detector.RegistrarAcao("x");
        detector.RegistrarAcao("x");
        Thread.Sleep(1500);                    
        detector.RegistrarAcao("y");            
        Secao("Bloco 3 — deve detectar Padrão2");
        detector.RegistrarAcao("x");
        detector.RegistrarAcao("x");
        detector.RegistrarAcao("y");           

        Secao("Bloco 4 — deve detectar Padrão3");
        detector.RegistrarAcao("cima");
        detector.RegistrarAcao("baixo");
        detector.RegistrarAcao("cima");
        detector.RegistrarAcao("baixo");       

        Console.WriteLine("\n=== FIM DA DEMONSTRAÇÃO ===");

    }

    // Utilitário visual para separar os blocos no console
    static void Secao(string titulo)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"──── {titulo} ────");
        Console.ResetColor();
    }
}