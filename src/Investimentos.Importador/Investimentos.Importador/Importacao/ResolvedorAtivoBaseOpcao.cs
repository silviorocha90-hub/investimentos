namespace Investimentos.Importador.Importacao
{
    public class ResolvedorAtivoBaseOpcao
    {
        private static readonly Dictionary<string, string> MapeamentosPorRaiz =
            new(StringComparer.OrdinalIgnoreCase)
            {
                /*
                 * Mapeamentos explícitos.
                 *
                 * São necessários quando:
                 *
                 * 1. Existem vários ativos com a mesma raiz
                 *    no histórico.
                 *
                 * 2. O ativo-base não aparece entre os ativos
                 *    encontrados nas operações históricas.
                 */

                ["PETR"] = "PETR4",
                ["CPLE"] = "CPLE3",
                ["SAPR"] = "SAPR11",

                ["AXIA"] = "AXIA3",
                ["SANB"] = "SANB4",
                ["ALOS"] = "ALOS3"
            };

        public ResultadoResolucaoAtivoBase Resolver(
            string tickerOpcao,
            IEnumerable<string> ativosConhecidos)
        {
            if (string.IsNullOrWhiteSpace(tickerOpcao))
            {
                return ResultadoResolucaoAtivoBase
                    .CriarNaoResolvido(tickerOpcao);
            }

            tickerOpcao =
                tickerOpcao
                    .Trim()
                    .ToUpperInvariant();

            var raizOpcao =
                ObterRaizOpcao(tickerOpcao);

            /*
             * Primeiro verificamos os mapeamentos
             * explicitamente definidos.
             */

            if (MapeamentosPorRaiz.TryGetValue(
                    raizOpcao,
                    out var ativoMapeado))
            {
                return ResultadoResolucaoAtivoBase
                    .CriarResolvido(
                        tickerOpcao,
                        ativoMapeado);
            }

            /*
             * Caso não exista mapeamento explícito,
             * tentamos resolver automaticamente
             * utilizando os ativos conhecidos.
             */

            var ativos =
                ativosConhecidos
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Select(x =>
                        x.Trim().ToUpperInvariant())
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            var candidatos =
                ativos
                    .Where(ativo =>
                        OpcaoPertenceAoAtivo(
                            tickerOpcao,
                            ativo))
                    .OrderBy(x => x)
                    .ToList();

            if (candidatos.Count == 1)
            {
                return ResultadoResolucaoAtivoBase
                    .CriarResolvido(
                        tickerOpcao,
                        candidatos[0]);
            }

            if (candidatos.Count > 1)
            {
                return ResultadoResolucaoAtivoBase
                    .CriarAmbiguo(
                        tickerOpcao,
                        candidatos);
            }

            return ResultadoResolucaoAtivoBase
                .CriarNaoResolvido(tickerOpcao);
        }

        private static bool OpcaoPertenceAoAtivo(
            string tickerOpcao,
            string tickerAtivo)
        {
            var raizAtivo =
                ObterRaizAtivo(tickerAtivo);

            if (string.IsNullOrWhiteSpace(raizAtivo))
            {
                return false;
            }

            if (!tickerOpcao.StartsWith(
                    raizAtivo,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (tickerOpcao.Length <= raizAtivo.Length)
            {
                return false;
            }

            /*
             * Após a raiz do ativo deve existir
             * a letra correspondente à série
             * da opção.
             *
             * Exemplos:
             *
             * ITUBU407
             * ITUB + U + 407
             *
             * VALEL839
             * VALE + L + 839
             *
             * B3SAK182
             * B3SA + K + 182
             */

            return char.IsLetter(
                tickerOpcao[raizAtivo.Length]);
        }

        private static string ObterRaizAtivo(
            string tickerAtivo)
        {
            if (string.IsNullOrWhiteSpace(tickerAtivo))
            {
                return string.Empty;
            }

            tickerAtivo =
                tickerAtivo
                    .Trim()
                    .ToUpperInvariant();

            /*
             * Remove somente os números finais
             * do ticker do ativo.
             *
             * Exemplos:
             *
             * ITUB4  -> ITUB
             * VALE3  -> VALE
             * TAEE11 -> TAEE
             * B3SA3  -> B3SA
             * B5P211 -> B5P
             */

            var indiceFinal =
                tickerAtivo.Length;

            while (indiceFinal > 0 &&
                   char.IsDigit(
                       tickerAtivo[indiceFinal - 1]))
            {
                indiceFinal--;
            }

            if (indiceFinal == 0)
            {
                return string.Empty;
            }

            return tickerAtivo[..indiceFinal];
        }

        private static string ObterRaizOpcao(
            string tickerOpcao)
        {
            /*
             * Para descobrir a raiz da opção,
             * usamos os mapeamentos conhecidos.
             *
             * Como as raízes possuem tamanhos
             * diferentes, procuramos primeiro
             * pelas maiores.
             */

            foreach (var raiz in
                     MapeamentosPorRaiz.Keys
                         .OrderByDescending(x => x.Length))
            {
                if (tickerOpcao.StartsWith(
                        raiz,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return raiz;
                }
            }

            /*
             * Nos demais casos não precisamos
             * determinar previamente a raiz.
             * A resolução automática será feita
             * comparando diretamente com os
             * ativos conhecidos.
             */

            return string.Empty;
        }
    }

    public class ResultadoResolucaoAtivoBase
    {
        public string TickerOpcao { get; }

        public string? AtivoBase { get; }

        public IReadOnlyList<string> Candidatos { get; }

        public bool Resolvido =>
            !string.IsNullOrWhiteSpace(
                AtivoBase);

        public bool Ambiguo =>
            !Resolvido &&
            Candidatos.Count > 1;

        public bool NaoResolvido =>
            !Resolvido &&
            Candidatos.Count == 0;

        private ResultadoResolucaoAtivoBase(
            string tickerOpcao,
            string? ativoBase,
            IReadOnlyList<string> candidatos)
        {
            TickerOpcao =
                tickerOpcao ?? string.Empty;

            AtivoBase =
                ativoBase;

            Candidatos =
                candidatos;
        }

        public static ResultadoResolucaoAtivoBase CriarResolvido(
            string tickerOpcao,
            string ativoBase)
        {
            return new ResultadoResolucaoAtivoBase(
                tickerOpcao,
                ativoBase,
                Array.Empty<string>());
        }

        public static ResultadoResolucaoAtivoBase CriarAmbiguo(
            string tickerOpcao,
            IReadOnlyList<string> candidatos)
        {
            return new ResultadoResolucaoAtivoBase(
                tickerOpcao,
                null,
                candidatos);
        }

        public static ResultadoResolucaoAtivoBase CriarNaoResolvido(
            string tickerOpcao)
        {
            return new ResultadoResolucaoAtivoBase(
                tickerOpcao,
                null,
                Array.Empty<string>());
        }
    }
}