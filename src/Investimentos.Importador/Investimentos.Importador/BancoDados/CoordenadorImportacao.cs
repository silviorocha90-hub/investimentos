using Investimentos.Importador.Importacao;
using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Importador.BancoDados
{
    public class CoordenadorImportacao
    {
        private readonly InvestimentosDbContext _context;

        public CoordenadorImportacao(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<ResultadoImportacaoBanco> ExecutarAsync(
            DadosImportacao dados)
        {
            if (dados is null)
                throw new ArgumentNullException(nameof(dados));

            var verificadorBanco =
                new VerificadorBancoDados(_context);

            var verificacaoBanco =
                await verificadorBanco.VerificarAsync();

            if (!verificacaoBanco.Valido)
            {
                throw new InvalidOperationException(
                    $"Importação bloqueada. {verificacaoBanco.Mensagem}");
            }

            await using var transacao =
                await _context.Database.BeginTransactionAsync();

            try
            {
                /*
                 * Cotações e saldos disponíveis não possuem uma data
                 * explícita no Excel.
                 *
                 * Criamos um único instante para toda a importação,
                 * garantindo que ambos pertençam ao mesmo snapshot.
                 */
                var dataReferencia = DateTime.Now;

                /*
                 * Enquanto o Excel for a fonte de verdade, cada execução
                 * reconstrói integralmente os dados de negócio.
                 */
                var reset =
                    new ResetBancoDados(_context);

                await reset.ExecutarAsync();

                /*
                 * INVESTIDORES E ATIVOS
                 */
                var importadorCadastros =
                    new ImportadorCadastros(_context);

                var cadastros =
                    await importadorCadastros.ImportarAsync(dados);

                /*
                 * OPERAÇÕES
                 */
                var importadorOperacoes =
                    new ImportadorOperacoes(_context);

                var quantidadeOperacoes =
                    await importadorOperacoes.ImportarAsync(
                        dados.Operacoes,
                        cadastros.Investidores,
                        cadastros.Ativos);

                /*
                 * PROVENTOS
                 */
                var importadorProventos =
                    new ImportadorProventos(_context);

                var quantidadeProventos =
                    await importadorProventos.ImportarAsync(
                        dados.Proventos,
                        cadastros.Investidores,
                        cadastros.Ativos);

                /*
                 * OPÇÕES
                 */
                var importadorOpcoes =
                    new ImportadorOpcoes(_context);

                var quantidadeOpcoes =
                    await importadorOpcoes.ImportarAsync(
                        dados.Opcoes,
                        cadastros.Investidores,
                        cadastros.Ativos,
                        dados.Ativos);

                /*
                 * COTAÇÕES
                 */
                var importadorCotacoes =
                    new ImportadorCotacoes(_context);

                var quantidadeCotacoes =
                    await importadorCotacoes.ImportarAsync(
                        dados.Cotacoes,
                        cadastros.Ativos,
                        dataReferencia);

                /*
                 * SALDOS DISPONÍVEIS
                 */
                var importadorSaldos =
                    new ImportadorSaldosDisponiveis(_context);

                var quantidadeSaldos =
                    await importadorSaldos.ImportarAsync(
                        dados.SaldosDisponiveis,
                        cadastros.Investidores,
                        dataReferencia);

                /*
                 * HISTÓRICO PATRIMONIAL
                 */
                var importadorHistoricos =
                    new ImportadorHistoricosPatrimonio(_context);

                var quantidadeHistoricos =
                    await importadorHistoricos.ImportarAsync(
                        dados.HistoricosPatrimonio,
                        cadastros.Investidores);

                /*
                 * Antes do COMMIT, conferimos se o banco contém
                 * exatamente aquilo que foi extraído do Excel.
                 */
                await ValidarImportacaoAsync(
                    dados,
                    cadastros,
                    quantidadeOperacoes,
                    quantidadeProventos,
                    quantidadeOpcoes,
                    quantidadeCotacoes,
                    quantidadeSaldos,
                    quantidadeHistoricos);

                await transacao.CommitAsync();

                return new ResultadoImportacaoBanco(
                    cadastros.Investidores.Count,
                    cadastros.Ativos.Count,
                    quantidadeOperacoes,
                    quantidadeProventos,
                    quantidadeOpcoes,
                    quantidadeCotacoes,
                    quantidadeSaldos,
                    quantidadeHistoricos);
            }
            catch
            {
                await transacao.RollbackAsync();
                throw;
            }
        }

        private async Task ValidarImportacaoAsync(
            DadosImportacao dados,
            ResultadoImportacaoCadastros cadastros,
            int quantidadeOperacoes,
            int quantidadeProventos,
            int quantidadeOpcoes,
            int quantidadeCotacoes,
            int quantidadeSaldos,
            int quantidadeHistoricos)
        {
            /*
             * Primeiro conferimos o retorno individual de cada importador.
             */

            if (quantidadeOperacoes != dados.Operacoes.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de operações importadas diverge do Excel. " +
                    $"Excel={dados.Operacoes.Count}, " +
                    $"Importado={quantidadeOperacoes}.");
            }

            if (quantidadeProventos != dados.Proventos.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de proventos importados diverge do Excel. " +
                    $"Excel={dados.Proventos.Count}, " +
                    $"Importado={quantidadeProventos}.");
            }

            if (quantidadeOpcoes != dados.Opcoes.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de opções importadas diverge do Excel. " +
                    $"Excel={dados.Opcoes.Count}, " +
                    $"Importado={quantidadeOpcoes}.");
            }

            if (quantidadeCotacoes != dados.Cotacoes.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de cotações importadas diverge do Excel. " +
                    $"Excel={dados.Cotacoes.Count}, " +
                    $"Importado={quantidadeCotacoes}.");
            }

            if (quantidadeSaldos != dados.SaldosDisponiveis.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de saldos importados diverge do Excel. " +
                    $"Excel={dados.SaldosDisponiveis.Count}, " +
                    $"Importado={quantidadeSaldos}.");
            }

            if (quantidadeHistoricos != dados.HistoricosPatrimonio.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de históricos patrimoniais importados " +
                    $"diverge do Excel. " +
                    $"Excel={dados.HistoricosPatrimonio.Count}, " +
                    $"Importado={quantidadeHistoricos}.");
            }

            /*
             * Depois conferimos diretamente o estado que está no banco
             * dentro da própria transação.
             */

            var investidoresBanco =
                await _context.Investidores.CountAsync();

            var ativosBanco =
                await _context.Ativos.CountAsync();

            var operacoesBanco =
                await _context.Operacoes.CountAsync();

            var proventosBanco =
                await _context.Proventos.CountAsync();

            var opcoesBanco =
                await _context.OperacoesOpcoes.CountAsync();

            var cotacoesBanco =
                await _context.CotacoesAtivos.CountAsync();

            var saldosBanco =
                await _context.SaldosDisponiveis.CountAsync();

            var historicosBanco =
                await _context.HistoricosPatrimonio.CountAsync();

            if (investidoresBanco != cadastros.Investidores.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de investidores no banco diverge " +
                    $"da importação. " +
                    $"Esperado={cadastros.Investidores.Count}, " +
                    $"Banco={investidoresBanco}.");
            }

            if (ativosBanco != cadastros.Ativos.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de ativos no banco diverge da importação. " +
                    $"Esperado={cadastros.Ativos.Count}, " +
                    $"Banco={ativosBanco}.");
            }

            if (operacoesBanco != dados.Operacoes.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de operações no banco diverge do Excel. " +
                    $"Excel={dados.Operacoes.Count}, " +
                    $"Banco={operacoesBanco}.");
            }

            if (proventosBanco != dados.Proventos.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de proventos no banco diverge do Excel. " +
                    $"Excel={dados.Proventos.Count}, " +
                    $"Banco={proventosBanco}.");
            }

            if (opcoesBanco != dados.Opcoes.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de opções no banco diverge do Excel. " +
                    $"Excel={dados.Opcoes.Count}, " +
                    $"Banco={opcoesBanco}.");
            }

            if (cotacoesBanco != dados.Cotacoes.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de cotações no banco diverge do Excel. " +
                    $"Excel={dados.Cotacoes.Count}, " +
                    $"Banco={cotacoesBanco}.");
            }

            if (saldosBanco != dados.SaldosDisponiveis.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de saldos disponíveis no banco diverge " +
                    $"do Excel. " +
                    $"Excel={dados.SaldosDisponiveis.Count}, " +
                    $"Banco={saldosBanco}.");
            }

            if (historicosBanco != dados.HistoricosPatrimonio.Count)
            {
                throw new InvalidOperationException(
                    $"Quantidade de históricos patrimoniais no banco " +
                    $"diverge do Excel. " +
                    $"Excel={dados.HistoricosPatrimonio.Count}, " +
                    $"Banco={historicosBanco}.");
            }
        }
    }

    public record ResultadoImportacaoBanco(
        int Investidores,
        int Ativos,
        int Operacoes,
        int Proventos,
        int Opcoes,
        int Cotacoes,
        int SaldosDisponiveis,
        int HistoricosPatrimonio);
}