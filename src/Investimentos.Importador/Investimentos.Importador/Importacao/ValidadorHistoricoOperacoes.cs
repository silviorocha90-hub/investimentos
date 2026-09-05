namespace Investimentos.Importador.Importacao
{
    public class ValidadorHistoricoOperacoes
    {
        public IReadOnlyList<string> Validar(
            IEnumerable<OperacaoImportacao> operacoes)
        {
            var inconsistencias =
                new List<string>();

            var saldos =
                new Dictionary<string, decimal>(
                    StringComparer.OrdinalIgnoreCase);

            var operacoesOrdenadas =
                operacoes
                    .OrderBy(x => x.Data)
                    .ThenBy(x => x.LinhaExcel)
                    .ToList();

            foreach (var operacao in operacoesOrdenadas)
            {
                var chave =
                    CriarChave(
                        operacao.Investidor,
                        operacao.Ativo);

                saldos.TryGetValue(
                    chave,
                    out var saldoAtual);

                if (operacao.TipoOperacao == "COMPRA")
                {
                    saldos[chave] =
                        saldoAtual + operacao.Quantidade;

                    continue;
                }

                if (operacao.TipoOperacao == "VENDA")
                {
                    if (operacao.Quantidade > saldoAtual)
                    {
                        var faltante =
                            operacao.Quantidade - saldoAtual;

                        inconsistencias.Add(
                            $"Linha {operacao.LinhaExcel}: " +
                            $"{operacao.Data:dd/MM/yyyy} | " +
                            $"{operacao.Investidor} | " +
                            $"{operacao.Ativo} | " +
                            $"Venda={operacao.Quantidade:N8} | " +
                            $"Disponível={saldoAtual:N8} | " +
                            $"Faltante={faltante:N8}");

                        // Não deixa o saldo negativo.
                        // Isso evita que uma inconsistência contamine
                        // artificialmente as operações seguintes.
                        saldos[chave] = 0;

                        continue;
                    }

                    saldos[chave] =
                        saldoAtual - operacao.Quantidade;
                }
            }

            return inconsistencias;
        }

        private static string CriarChave(
            string investidor,
            string ativo)
        {
            return
                $"{investidor.Trim()}|{ativo.Trim()}";
        }
    }
}