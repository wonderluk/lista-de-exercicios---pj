
// Como a variação de frequência e o envelope afetam a aparência dos gráficos?
// A variação de frequência faz com que os vales e picos do "desenho" fiquem cada 
// vez mais apertados (frequência alta) ou mais espaçados (frequência baixa). 
// O envelope modula a "altura" vertical da onda: ela começa pequena (ataque), 
// atinge sua altura máxima e encolhe gradativamente até sumir (decaimento).

using System;

// Classe responsável por gerar amostras de onda
class GeradorDeOnda
{
    public static double[] GerarOnda(double freqInicial, double freqFinal,
                                     double duracao, int amostrasPorSegundo)
    {
        int totalAmostras = (int)(duracao * amostrasPorSegundo);
        double[] amostras = new double[totalAmostras];

        double fimAtaque = 0.05 * duracao;

        for (int i = 0; i < totalAmostras; i++)
        {
            double t = (double)i / amostrasPorSegundo;

            double progresso   = t / duracao;
            double freqAtual   = freqInicial + (freqFinal - freqInicial) * progresso;

            double seno = Math.Sin(2 * Math.PI * freqAtual * t);

            double amplitude;
            if (t <= fimAtaque)
            {
                amplitude = t / fimAtaque;
            }
            else
            {

                double tau     = (duracao - fimAtaque) / 3.0; 
                double tDecay  = t - fimAtaque;
                amplitude      = Math.Exp(-tDecay / tau);
            }

            amostras[i] = seno * amplitude;
        }

        return amostras;
    }
}

// Classe responsável por desenhar a onda no console
class VisualizadorDeOnda
{
    public static void DesenharOnda(double[] amostras, int colunas, int linhas)
    {
        // ── Passo 1: Reduzir o array para 'colunas' valores ──
        double[] reducao    = new double[colunas];
        int tamanhoGrupo    = amostras.Length / colunas;

        for (int c = 0; c < colunas; c++)
        {
            double soma = 0;
            int inicio  = c * tamanhoGrupo;
            int fim     = Math.Min(inicio + tamanhoGrupo, amostras.Length);

            for (int i = inicio; i < fim; i++)
                soma += amostras[i];

            reducao[c] = soma / (fim - inicio);
        }

        // ── Passo 2: Mapear valores para linhas do gráfico ──
        int linhaZero = linhas / 2;

        char[,] grade = new char[linhas, colunas];

        for (int l = 0; l < linhas; l++)
            for (int c = 0; c < colunas; c++)
                grade[l, c] = ' ';

        for (int c = 0; c < colunas; c++)
            grade[linhaZero, c] = '─';

        for (int c = 0; c < colunas; c++)
        {

            double valor      = Math.Clamp(reducao[c], -1.0, 1.0);
            int linhaValor    = (int)Math.Round((1.0 - valor) / 2.0 * (linhas - 1));

            grade[linhaValor, c] = '█';
        }

        // Passo 3: Imprimir a grade linha por linha 
        for (int l = 0; l < linhas; l++)
        {
            for (int c = 0; c < colunas; c++)
            {

                if (grade[l, c] == '█')
                    Console.ForegroundColor = l < linhaZero
                        ? ConsoleColor.Cyan    
                        : ConsoleColor.Green; 
                else if (grade[l, c] == '─')
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                else
                    Console.ResetColor();

                Console.Write(grade[l, c]);
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    /// Imprime o cabeçalho formatado de cada onda.
    public static void ImprimirCabecalho(string nome, double freqInicial,
                                         double freqFinal, double duracao, int colunas)
    {
        string linha    = new string('═', colunas);
        string titulo   = $"  {nome} — {freqInicial}Hz → {freqFinal}Hz, duração {duracao:F2}s";

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(linha);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(titulo);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(linha);
        Console.ResetColor();
    }
}

// Programa de demonstração
class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        const int COLUNAS            = 60;
        const int LINHAS             = 15;
        const int AMOSTRAS_POR_SEG   = 8000;

        var ondas = new (string Nome, double FreqI, double FreqF, double Dur)[]
        {
            ("ONDA A", 440,  880,  0.15),   
            ("ONDA B", 330,   80,  0.35),   
            ("ONDA C", 660,  990,  0.10),   
            ("ONDA D", 200, 1200,  0.50),   
        };

        foreach (var (nome, freqI, freqF, dur) in ondas)
        {
            Console.WriteLine();
            VisualizadorDeOnda.ImprimirCabecalho(nome, freqI, freqF, dur, COLUNAS);

            double[] amostras = GeradorDeOnda.GerarOnda(freqI, freqF, dur, AMOSTRAS_POR_SEG);
            VisualizadorDeOnda.DesenharOnda(amostras, COLUNAS, LINHAS);
        }


        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("=== FIM DA DEMONSTRAÇÃO ===");
        Console.ResetColor();
    }
}