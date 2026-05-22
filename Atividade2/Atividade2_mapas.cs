
//  CERIMONIAL — Comparação visual dos três mapas:
// O que você observa nos formatos resultantes? Eles são aleatórios ou apresentam estrutura?
// Apresentam forte estrutura orgânica. Embora o mapa inicie como ruído puramente
// aleatório, a regra de vizinhança força as paredes a se aglomerarem. O resultado 
// lembra sistemas de cavernas naturais, com grandes salões interconectados e 
// pilares, eliminando o aspecto de "televisão fora do ar" do ruído inicial.


using System;

//  Classe responsável por gerar o mapa de caverna

class GeradorDeMapa
{
    private const double CHANCE_PAREDE   = 0.45; 
    private const int    ITERACOES       = 5;   
    private const int    LIMIAR_VIZINHOS = 4;   

    public char[,] Gerar(int largura, int altura, int semente)
    {
        Random rng = new Random(semente);

        char[,] mapa = InicializarComRuido(largura, altura, rng);

        for (int i = 0; i < ITERACOES; i++)
            mapa = Suavizar(mapa, largura, altura);

        return mapa;
    }

    //  Passo 1 — Preencher com ruído aleatório
    private char[,] InicializarComRuido(int largura, int altura, Random rng)
    {
        char[,] mapa = new char[altura, largura];

        for (int l = 0; l < altura; l++)
        {
            for (int c = 0; c < largura; c++)
            {

                if (EhBorda(l, c, altura, largura))
                    mapa[l, c] = '#';
                else
                    mapa[l, c] = rng.NextDouble() < CHANCE_PAREDE ? '#' : '.';
            }
        }

        return mapa;
    }

    //  Passo 2 — Uma rodada de suavização 
    private char[,] Suavizar(char[,] atual, int largura, int altura)
    {
        char[,] novo = new char[altura, largura];

        for (int l = 0; l < altura; l++)
        {
            for (int c = 0; c < largura; c++)
            {
            
                if (EhBorda(l, c, altura, largura))
                {
                    novo[l, c] = '#';
                    continue;
                }


                int vizinhos = ContarVizinhosParede(atual, l, c, altura, largura);
                novo[l, c] = vizinhos >= LIMIAR_VIZINHOS ? '#' : '.';
            }
        }

        return novo;
    }

    //  Conta quantas das 8 células vizinhas são parede
    private int ContarVizinhosParede(char[,] mapa, int linha, int coluna,
                                     int altura, int largura)
    {
        int count = 0;

        for (int dl = -1; dl <= 1; dl++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dl == 0 && dc == 0) continue;

                int nl = linha  + dl;
                int nc = coluna + dc;

            
                if (nl < 0 || nl >= altura || nc < 0 || nc >= largura)
                    count++;
                else if (mapa[nl, nc] == '#')
                    count++;
            }
        }

        return count;
    }

    //  Auxiliar — verifica se a célula está na borda do mapa
    private bool EhBorda(int linha, int coluna, int altura, int largura)
    {
        return linha == 0 || linha == altura - 1 ||
               coluna == 0 || coluna == largura - 1;
    }
}

//  Programa de demonstração
class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        const int LARGURA = 40;
        const int ALTURA  = 20;

        int[] sementes = { 42, 123, 777 };

        var gerador = new GeradorDeMapa();

        Cabecalho("GERADOR DE MAPAS — AUTÔMATOS CELULARES");
        Console.WriteLine($"  Dimensões: {LARGURA} colunas × {ALTURA} linhas");
        Console.WriteLine($"  Suavização: 5 iterações | Chance de parede inicial: 45%\n");

        for (int i = 0; i < sementes.Length; i++)
        {
            int semente = sementes[i];

            char[,] mapa = gerador.Gerar(LARGURA, ALTURA, semente);

            Separador('═');
            Console.WriteLine($"  MAPA {i + 1}  (semente = {semente})");
            Separador('═');

            int paredes = 0, vazios = 0;

            for (int l = 0; l < ALTURA; l++)
            {
                Console.Write("  ");
                for (int c = 0; c < LARGURA; c++)
                {
                    char cel = mapa[l, c];
                    Console.Write(cel);

                    if (cel == '#') paredes++;
                    else            vazios++;
                }
                Console.WriteLine();
            }

            int    total    = LARGURA * ALTURA;
            double ocupacao = (double)paredes / total * 100.0;

            Separador('-');
            Console.WriteLine($"  Paredes : {paredes,4}  |  " +
                              $"Vazios : {vazios,4}  |  " +
                              $"Ocupação : {ocupacao:F1}%");
            Console.WriteLine();
        }

        Separador('═');
        Console.WriteLine("  VERIFICAÇÃO DE REPRODUTIBILIDADE (semente 42, duas chamadas)");
        Separador('-');

        char[,] mapaA = gerador.Gerar(LARGURA, ALTURA, 42);
        char[,] mapaB = gerador.Gerar(LARGURA, ALTURA, 42);
        bool identicos = MapasIdenticos(mapaA, mapaB, LARGURA, ALTURA);

        Console.WriteLine($"  Dois mapas com semente 42 são idênticos? → {(identicos ? "SIM ✓" : "NÃO ✗")}");
        Separador('═');

        Console.WriteLine("\n  Pressione qualquer tecla para sair...");
        Console.ReadKey(intercept: true);
    }

    // Verifica se dois mapas têm exatamente o mesmo conteúdo
    static bool MapasIdenticos(char[,] a, char[,] b, int largura, int altura)
    {
        for (int l = 0; l < altura; l++)
            for (int c = 0; c < largura; c++)
                if (a[l, c] != b[l, c]) return false;
        return true;
    }

    // Helpers de formatação
    static void Cabecalho(string titulo)
    {
        string linha = new string('═', titulo.Length + 4);
        Console.WriteLine($"\n  ╔{linha}╗");
        Console.WriteLine($"  ║  {titulo}  ║");
        Console.WriteLine($"  ╚{linha}╝\n");
    }

    static void Separador(char c = '═')
    {
        Console.WriteLine($"  {new string(c, 46)}");
    }
}