
using System;
using System.Collections.Generic;
using System.Diagnostics;

// Enum com os possíveis estados da aplicação
enum EstadoAplicacao
{
    Inicial,
    Executando,
    Pausado,
    Encerrado
}

// Classe Singleton que gerencia o estado global
class Aplicacao
{
    private static Aplicacao? _instancia;
    public static Aplicacao Instancia => _instancia ??= new Aplicacao();

    private Aplicacao()
    {
        Estado = EstadoAplicacao.Inicial;
    }

    public EstadoAplicacao Estado { get; private set; }

    public event Action<EstadoAplicacao, EstadoAplicacao>? OnEstadoMudou;

    // Tabela de transições permitidas
    private static readonly Dictionary<EstadoAplicacao, HashSet<EstadoAplicacao>> _transicoesPermitidas =
        new()
        {
            { EstadoAplicacao.Inicial,    new HashSet<EstadoAplicacao> { EstadoAplicacao.Executando } },
            { EstadoAplicacao.Executando, new HashSet<EstadoAplicacao> { EstadoAplicacao.Pausado, EstadoAplicacao.Encerrado } },
            { EstadoAplicacao.Pausado,    new HashSet<EstadoAplicacao> { EstadoAplicacao.Executando, EstadoAplicacao.Encerrado } },
            { EstadoAplicacao.Encerrado,  new HashSet<EstadoAplicacao>() } 
        };

    /// Tenta realizar a transição para <paramref name="novo"/>.
    public void MudarEstado(EstadoAplicacao novo)
    {
        if (!_transicoesPermitidas[Estado].Contains(novo))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Aplicacao] AVISO: Transição {Estado} → {novo} não é permitida.");
            Console.ResetColor();
            return;
        }

        EstadoAplicacao anterior = Estado;
        Estado = novo;

        OnEstadoMudou?.Invoke(anterior, novo);
    }
}

// Observador 1: Logger — registra toda transição
class Logger
{
    public Logger()
    {
        Aplicacao.Instancia.OnEstadoMudou += HandleTransicao;
    }

    private void HandleTransicao(EstadoAplicacao anterior, EstadoAplicacao novo)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[Logger] Transição: {anterior} → {novo}");
        Console.ResetColor();
    }
}

// Observador 2: Cronometro — mede tempo em Executando
class Cronometro
{
    private readonly Stopwatch _sw = new();

    public Cronometro()
    {
        Aplicacao.Instancia.OnEstadoMudou += HandleTransicao;
    }

    private void HandleTransicao(EstadoAplicacao anterior, EstadoAplicacao novo)
    {
        Console.ForegroundColor = ConsoleColor.Green;

        if (novo == EstadoAplicacao.Executando)
        {
            _sw.Start();
            string acao = anterior == EstadoAplicacao.Pausado ? "Retomando" : "Iniciando";
            Console.WriteLine($"[Cronometro] {acao} contagem de tempo...");
        }
        else if (anterior == EstadoAplicacao.Executando && novo == EstadoAplicacao.Pausado)
        {
            _sw.Stop();
            Console.WriteLine($"[Cronometro] Pausando contagem. Tempo acumulado: {_sw.Elapsed.TotalSeconds:F2}s");
        }
        else if (novo == EstadoAplicacao.Encerrado)
        {
            _sw.Stop();
            Console.WriteLine($"[Cronometro] Encerrando. Tempo total em execução: {_sw.Elapsed.TotalSeconds:F2}s");
        }

        Console.ResetColor();
    }
}

// Observador 3: Persistencia — simula salvamento
class Persistencia
{
    public Persistencia()
    {
        Aplicacao.Instancia.OnEstadoMudou += HandleTransicao;
    }

    private void HandleTransicao(EstadoAplicacao anterior, EstadoAplicacao novo)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;

        Console.WriteLine($"[Persistencia] Registrando: {anterior} → {novo} em {DateTime.Now:HH:mm:ss}");

        if (novo == EstadoAplicacao.Encerrado)
        {
            Console.WriteLine("[Persistencia] Salvando estado final em 'save_estado.dat'... OK");
        }

        Console.ResetColor();
    }
}

// Programa principal de demonstração
class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var logger      = new Logger();
        var cronometro  = new Cronometro();
        var persistencia = new Persistencia();

        var app = Aplicacao.Instancia;

        Console.WriteLine("=== INÍCIO DA DEMONSTRAÇÃO ===");
        Console.WriteLine($"Estado inicial: {app.Estado}\n");

        Separador("Inicial → Executando");
        app.MudarEstado(EstadoAplicacao.Executando);

        System.Threading.Thread.Sleep(500);

        Separador("Executando → Pausado");
        app.MudarEstado(EstadoAplicacao.Pausado);


        Separador("Pausado → Inicial  [INVÁLIDA]");
        app.MudarEstado(EstadoAplicacao.Inicial);  

        Separador("Pausado → Executando");
        app.MudarEstado(EstadoAplicacao.Executando);

        System.Threading.Thread.Sleep(300);

        Separador("Executando → Pausado");
        app.MudarEstado(EstadoAplicacao.Pausado);

        Separador("Pausado → Encerrado");
        app.MudarEstado(EstadoAplicacao.Encerrado);

        Separador("Encerrado → Executando  [INVÁLIDA]");
        app.MudarEstado(EstadoAplicacao.Executando);

        Console.WriteLine("\n=== FIM DA DEMONSTRAÇÃO ===");
        Console.WriteLine($"Estado final: {app.Estado}");

    }

    // Utilitário visual para separar as etapas no console
    static void Separador(string titulo)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"──── {titulo} ────");
        Console.ResetColor();
    }
}