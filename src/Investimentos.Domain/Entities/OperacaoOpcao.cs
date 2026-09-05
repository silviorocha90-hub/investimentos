namespace Investimentos.Domain.Entities
{
    public class OperacaoOpcao
    {
        public Guid Id { get; private set; }

        public Guid InvestidorId { get; private set; }
        public Investidor Investidor { get; private set; }

        public Guid AtivoId { get; private set; }
        public Ativo Ativo { get; private set; }

        public string TickerOpcao { get; private set; }
        public string TipoOpcao { get; private set; }
        public string Natureza { get; private set; }

        public DateTime DataOperacao { get; private set; }
        public DateTime Vencimento { get; private set; }
        public DateTime? DataFinalizacao { get; private set; }

        public decimal Strike { get; private set; }
        public int Contratos { get; private set; }
        public decimal Quantidade { get; private set; }

        public decimal PremioUnitario { get; private set; }
        public decimal Taxas { get; private set; }

        public decimal? PrecoRecompraUnitario { get; private set; }
        public decimal? ValorExecucao { get; private set; }

        /*
         * Valor histórico informado pela fonte
         * original, atualmente a coluna MyProfit
         * da planilha.
         *
         * Quando disponível, ele representa o
         * resultado histórico efetivo da operação
         * e tem prioridade sobre o cálculo derivado.
         */
        public decimal? ResultadoInformado { get; private set; }

        public string Situacao { get; private set; }

        public decimal PremioTotal =>
            Quantidade * PremioUnitario;

        public decimal? ValorRecompraTotal =>
            PrecoRecompraUnitario.HasValue
                ? Quantidade * PrecoRecompraUnitario.Value
                : null;

        public decimal? ResultadoFinal
        {
            get
            {
                if (Situacao == "ABERTA")
                {
                    return null;
                }

                if (ResultadoInformado.HasValue)
                {
                    return ResultadoInformado.Value;
                }

                if (Situacao == "ENCERRADA")
                {
                    if (!ValorRecompraTotal.HasValue)
                    {
                        return null;
                    }

                    return Natureza == "VENDA"
                        ? PremioTotal -
                          ValorRecompraTotal.Value -
                          Taxas
                        : ValorRecompraTotal.Value -
                          PremioTotal -
                          Taxas;
                }

                if (Situacao == "EXPIRADA")
                {
                    return Natureza == "VENDA"
                        ? PremioTotal - Taxas
                        : -PremioTotal - Taxas;
                }

                if (Situacao == "EXECUTADA")
                {
                    return null;
                }

                return null;
            }
        }

        private OperacaoOpcao()
        {
            Investidor = null!;
            Ativo = null!;
            TickerOpcao = null!;
            TipoOpcao = null!;
            Natureza = null!;
            Situacao = null!;
        }

        public OperacaoOpcao(
            Investidor investidor,
            Ativo ativo,
            string tickerOpcao,
            string tipoOpcao,
            string natureza,
            DateTime dataOperacao,
            DateTime vencimento,
            decimal strike,
            int contratos,
            decimal quantidade,
            decimal premioUnitario,
            decimal taxas = 0,
            decimal? resultadoInformado = null)
        {
            if (investidor is null)
            {
                throw new ArgumentException(
                    "O investidor é obrigatório.");
            }

            if (ativo is null)
            {
                throw new ArgumentException(
                    "O ativo objeto é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(tickerOpcao))
            {
                throw new ArgumentException(
                    "O ticker da opção é obrigatório.");
            }

            var tipoNormalizado =
                tipoOpcao?.Trim().ToUpperInvariant();

            if (tipoNormalizado != "PUT" &&
                tipoNormalizado != "CALL")
            {
                throw new ArgumentException(
                    "O tipo da opção deve ser PUT ou CALL.");
            }

            var naturezaNormalizada =
                natureza?.Trim().ToUpperInvariant();

            if (naturezaNormalizada != "COMPRA" &&
                naturezaNormalizada != "VENDA")
            {
                throw new ArgumentException(
                    "A natureza deve ser COMPRA ou VENDA.");
            }

            if (vencimento.Date < dataOperacao.Date)
            {
                throw new ArgumentException(
                    "O vencimento não pode ser anterior à operação.");
            }

            if (strike <= 0)
            {
                throw new ArgumentException(
                    "O strike deve ser maior que zero.");
            }

            if (contratos <= 0)
            {
                throw new ArgumentException(
                    "A quantidade de contratos deve ser maior que zero.");
            }

            if (quantidade <= 0)
            {
                throw new ArgumentException(
                    "A quantidade deve ser maior que zero.");
            }

            if (premioUnitario < 0)
            {
                throw new ArgumentException(
                    "O prêmio não pode ser negativo.");
            }

            if (taxas < 0)
            {
                throw new ArgumentException(
                    "As taxas não podem ser negativas.");
            }

            Id = Guid.NewGuid();

            Investidor = investidor;
            Ativo = ativo;

            TickerOpcao =
                tickerOpcao.Trim().ToUpperInvariant();

            TipoOpcao = tipoNormalizado;
            Natureza = naturezaNormalizada;

            DataOperacao = dataOperacao;
            Vencimento = vencimento;

            Strike = strike;
            Contratos = contratos;
            Quantidade = quantidade;

            PremioUnitario = premioUnitario;
            Taxas = taxas;

            ResultadoInformado =
                resultadoInformado;

            Situacao = "ABERTA";
        }

        public void Encerrar(
            DateTime dataFinalizacao,
            decimal precoRecompraUnitario)
        {
            if (Situacao != "ABERTA")
            {
                throw new InvalidOperationException(
                    "Somente uma opção aberta pode ser encerrada.");
            }

            ValidarDataFinalizacao(
                dataFinalizacao);

            if (precoRecompraUnitario < 0)
            {
                throw new ArgumentException(
                    "O preço unitário de recompra não pode ser negativo.");
            }

            DataFinalizacao = dataFinalizacao;
            PrecoRecompraUnitario =
                precoRecompraUnitario;

            Situacao = "ENCERRADA";
        }

        public void MarcarExercida(
            decimal? valorExecucao = null)
        {
            if (Situacao != "ABERTA")
            {
                throw new InvalidOperationException(
                    "Somente uma opção aberta pode ser exercida.");
            }

            if (valorExecucao.HasValue &&
                valorExecucao.Value < 0)
            {
                throw new ArgumentException(
                    "O valor de execução não pode ser negativo.");
            }

            DataFinalizacao = null;
            ValorExecucao = valorExecucao;
            Situacao = "EXECUTADA";
        }

        public void MarcarExpirada(
            DateTime dataFinalizacao)
        {
            if (Situacao != "ABERTA")
            {
                throw new InvalidOperationException(
                    "Somente uma opção aberta pode ser marcada como expirada.");
            }

            ValidarDataFinalizacao(
                dataFinalizacao);

            DataFinalizacao = dataFinalizacao;
            Situacao = "EXPIRADA";
        }

        private void ValidarDataFinalizacao(
            DateTime dataFinalizacao)
        {
            if (dataFinalizacao.Date <
                DataOperacao.Date)
            {
                throw new ArgumentException(
                    "A data de finalização não pode ser anterior à operação.");
            }
        }
    }
}
