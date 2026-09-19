namespace Investimentos.Application.Administracao
{
    public record AdministracaoDto(
        IReadOnlyList<AtivoAdministracaoDto> Ativos,
        IReadOnlyList<InvestidorAdministracaoDto> Investidores,
        IReadOnlyList<ClasseAtivoAdministracaoDto> ClassesAtivo,
        IReadOnlyList<TipoAtivoAdministracaoDto> TiposAtivo,
        IReadOnlyList<TipoOperacaoAdministracaoDto> TiposOperacao,
        IReadOnlyList<DescontoFiscalAdministracaoDto> DescontosFiscais);

    public record AtivoAdministracaoDto(
        Guid Id,
        string Ticker,
        string Nome,
        int TipoAtivoId,
        string TipoAtivoCodigo,
        string TipoAtivoNome,
        int ClasseAtivoId,
        string ClasseAtivoCodigo,
        string ClasseAtivoNome,
        decimal Quantidade,
        decimal ValorAtual,
        IReadOnlyList<PosicaoInvestidorAtivoAdministracaoDto>
            PosicoesInvestidores,
        decimal? CotacaoAtual,
        DateTime? DataCotacao);

    public record PosicaoInvestidorAtivoAdministracaoDto(
        Guid InvestidorId,
        string InvestidorNome,
        decimal Quantidade,
        decimal ValorAtual);

    public record InvestidorAdministracaoDto(
        Guid Id,
        string Nome,
        decimal? SaldoDisponivel,
        DateTime? DataSaldo,
        decimal? PatrimonioAtual,
        DateTime? DataPatrimonio,
        IReadOnlyList<HistoricoPatrimonioAdministracaoDto>
            HistoricoPatrimonio);

    public record HistoricoPatrimonioAdministracaoDto(
        Guid Id,
        DateTime DataReferencia,
        decimal ValorCarteira);

    public record DescontoFiscalAdministracaoDto(
        Guid Id,
        string Tipo,
        DateTime DataPagamento,
        decimal Valor,
        string? Descricao);

    public record ClasseAtivoAdministracaoDto(
        int Id,
        string Codigo,
        string Nome,
        bool Ativo);

    public record TipoAtivoAdministracaoDto(
        int Id,
        string Codigo,
        string Nome,
        bool Ativo,
        int ClasseAtivoId,
        string ClasseAtivoCodigo,
        string ClasseAtivoNome);

    public record TipoOperacaoAdministracaoDto(
        int Id,
        string Codigo,
        string Nome,
        bool Ativo);

    public record CriarAtivoAdministracaoRequest(
        string Ticker,
        string Nome,
        int TipoAtivoId,
        decimal? Cotacao,
        DateTime? DataCotacao);

    public record AtualizarAtivoAdministracaoRequest(
        string Nome,
        int TipoAtivoId,
        decimal? Cotacao,
        DateTime? DataCotacao);
}