//  POR QUE APENAS 5 CAIXAS FORAM CRIADAS EM 20 ITERAÇÕES?
//  O reservatório foi pré-populado com 5 caixas na inicialização.
//  Em cada iteração o programa retira UMA caixa, atribui a etiqueta
//  e a DEVOLVE imediatamente nunca há mais de 1 caixa fora do
//  reservatório ao mesmo tempo. Como sempre existe ao menos uma
//  caixa disponível para ser reutilizada, a função fábrica jamais
//  é chamada novamente. Reutilizar é mais barato do que criar.

using System;
using System.Collections.Generic;

//  Classe simples que representa um objeto de jogo genérico
class Caixa
{

    public string Etiqueta { get; set; }

    public Caixa()
    {
        Etiqueta = "(sem etiqueta)";
    }

    public override string ToString() => Etiqueta;
}

//  Classe genérica — funciona com QUALQUER tipo de referência
class Reservatorio<T> where T : class
{

    private readonly Queue<T> _disponiveis;   
    private readonly Func<T>  _fabrica;       
    private int _totalCriados;

    // ---- Propriedades públicas (somente-leitura) ----
    public int Disponiveis  => _disponiveis.Count;
    public int TotalCriados => _totalCriados;

    
    public Reservatorio(Func<T> fabrica, int tamanhoInicial)
    {
        if (fabrica == null)
            throw new ArgumentNullException(nameof(fabrica));
        if (tamanhoInicial < 0)
            throw new ArgumentOutOfRangeException(nameof(tamanhoInicial));

        _fabrica     = fabrica;
        _disponiveis = new Queue<T>();
        _totalCriados = 0;

        // Pré-popula o reservatório usando a própria função fábrica
        for (int i = 0; i < tamanhoInicial; i++)
        {
            _disponiveis.Enqueue(CriarNovo());
        }
    }

    // ---- Métodos públicos ----

    public T Retirar()
    {
        if (_disponiveis.Count > 0)
        {
            return _disponiveis.Dequeue();   
        }

    
        return CriarNovo();
    }

    // Devolve um objeto ao reservatório para ser reutilizado futuramente.
    public void Devolver(T obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        _disponiveis.Enqueue(obj);
    }

    private T CriarNovo()
    {
        _totalCriados++;
        return _fabrica();  
    }
}

//  Programa de demonstração
class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Cabecalho("RESERVATÓRIO DE OBJETOS REUTILIZÁVEIS");

        var reservatorio = new Reservatorio<Caixa>(
            fabrica: () => new Caixa(),
            tamanhoInicial: 5
        );

        Console.WriteLine($"  Reservatório criado com tamanho inicial: 5");
        Console.WriteLine($"  Objetos disponíveis agora              : {reservatorio.Disponiveis}");
        Console.WriteLine($"  Total criados até o momento            : {reservatorio.TotalCriados}");
        Separador();

        const int ITERACOES = 20;

        for (int i = 1; i <= ITERACOES; i++)
        {

            Caixa caixa = reservatorio.Retirar();

            caixa.Etiqueta = $"Caixa_{i:D2}";

            Console.WriteLine(
                $"  [Iteração {i,2}]  Retirou → etiqueta = '{caixa.Etiqueta}'  " +
                $"| Disponíveis: {reservatorio.Disponiveis,2} → devolveu");

            reservatorio.Devolver(caixa);
        }

        Separador();
        Console.WriteLine("  RESULTADO FINAL");
        Separador('-');
        Console.WriteLine($"  Total de iterações realizadas : {ITERACOES}");
        Console.WriteLine($"  Total de caixas CRIADAS       : {reservatorio.TotalCriados}");
        Console.WriteLine($"  Caixas disponíveis no momento : {reservatorio.Disponiveis}");
        Separador();

        Console.WriteLine("  DEMONSTRAÇÃO: reservatório vazio → criação sob demanda");
        Separador('-');

        var retiradas = new List<Caixa>();
        while (reservatorio.Disponiveis > 0)
            retiradas.Add(reservatorio.Retirar());

        Console.WriteLine($"  Esvaziou o reservatório. Disponíveis: {reservatorio.Disponiveis}");

        // Tenta retirar mais uma — deve criar nova
        Caixa extra = reservatorio.Retirar();
        extra.Etiqueta = "Caixa_EXTRA";
        Console.WriteLine($"  Retirou '{extra.Etiqueta}' → total criadas agora: {reservatorio.TotalCriados}");

        reservatorio.Devolver(extra);
        foreach (var c in retiradas) reservatorio.Devolver(c);

        Console.WriteLine($"  Após devolver tudo, disponíveis: {reservatorio.Disponiveis}");
        Separador();

        Console.WriteLine("  Pressione qualquer tecla para sair...");
        Console.ReadKey(intercept: true);
    }

    //  Helpers de formatação de console 
    static void Cabecalho(string titulo)
    {
        string linha = new string('═', titulo.Length + 4);
        Console.WriteLine($"\n  ╔{linha}╗");
        Console.WriteLine($"  ║  {titulo}  ║");
        Console.WriteLine($"  ╚{linha}╝\n");
    }

    static void Separador(char c = '═')
    {
        Console.WriteLine($"  {new string(c, 60)}");
    }
}