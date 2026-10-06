namespace Investimentos.Domain.Usuarios
{
    public class Usuario
    {
        private readonly List<UsuarioPermissao>
            _permissoes = new();

        private readonly List<UsuarioInvestidor>
            _investidores = new();

        private readonly List<UsuarioInvestidorPermissao>
            _investidoresPermissoes = new();

        public Guid Id { get; private set; }

        public string Nome { get; private set; }

        public string Email { get; private set; }

        public string SenhaHash { get; private set; }

        public PerfilUsuario Perfil { get; private set; }

        public StatusUsuario Status { get; private set; }

        public DateTime DataCadastro { get; private set; }

        public DateTime? DataAprovacao { get; private set; }

        public Guid? AprovadoPorUsuarioId { get; private set; }

        public DateTime? UltimoLogin { get; private set; }

        public string? WhatsApp { get; private set; }

        public bool ReceberRelatorioIa { get; private set; }

        public string FrequenciaRelatorioIa { get; private set; } = "DIARIO";

        public DateTime? UltimoEnvioRelatorioIa { get; private set; }

        public IReadOnlyCollection<UsuarioPermissao>
            Permissoes => _permissoes;

        public IReadOnlyCollection<UsuarioInvestidor>
            Investidores => _investidores;

        public IReadOnlyCollection<UsuarioInvestidorPermissao>
            InvestidoresPermissoes => _investidoresPermissoes;

        private Usuario()
        {
            Nome = null!;
            Email = null!;
            SenhaHash = null!;
        }

        public Usuario(
            string nome,
            string email,
            string senhaHash)
        {
            ValidarNome(nome);
            ValidarEmail(email);
            ValidarSenhaHash(senhaHash);

            Id = Guid.NewGuid();

            Nome = nome.Trim();

            Email = NormalizarEmail(email);

            SenhaHash = senhaHash;

            Perfil = PerfilUsuario.Usuario;

            Status = StatusUsuario.Pendente;

            DataCadastro = DateTime.UtcNow;

            ReceberRelatorioIa = false;
            FrequenciaRelatorioIa = "DIARIO";
        }

        public static Usuario CriarAdmin(
            string nome,
            string email,
            string senhaHash)
        {
            var usuario = new Usuario(
                nome,
                email,
                senhaHash);

            usuario.Perfil = PerfilUsuario.Admin;
            usuario.Status = StatusUsuario.Ativo;
            usuario.DataAprovacao = DateTime.UtcNow;

            return usuario;
        }

        public void Aprovar(Guid aprovadoPorUsuarioId)
        {
            if (aprovadoPorUsuarioId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O usuário responsável pela aprovação é obrigatório.");
            }

            if (Status == StatusUsuario.Ativo)
            {
                return;
            }

            Status = StatusUsuario.Ativo;

            DataAprovacao = DateTime.UtcNow;

            AprovadoPorUsuarioId =
                aprovadoPorUsuarioId;
        }

        public void Rejeitar()
        {
            Status = StatusUsuario.Rejeitado;

            DataAprovacao = null;

            AprovadoPorUsuarioId = null;
        }

        public void Bloquear()
        {
            Status = StatusUsuario.Bloqueado;
        }

        public void Desbloquear()
        {
            Status = StatusUsuario.Ativo;
        }

        public void AlterarPerfil(
            PerfilUsuario perfil)
        {
            Perfil = perfil;
        }

        public void AlterarSenhaHash(
            string senhaHash)
        {
            ValidarSenhaHash(senhaHash);

            SenhaHash = senhaHash;
        }

        public void RegistrarLogin()
        {
            UltimoLogin = DateTime.UtcNow;
        }

        public void ConfigurarRelatorioIa(
            string? whatsApp,
            bool receberRelatorioIa,
            string? frequencia)
        {
            var numero = new string(
                (whatsApp ?? string.Empty)
                    .Where(char.IsDigit)
                    .ToArray());

            if (receberRelatorioIa &&
                string.IsNullOrWhiteSpace(numero))
            {
                throw new ArgumentException(
                    "O WhatsApp é obrigatório para receber o relatório IA.");
            }

            if (!string.IsNullOrWhiteSpace(numero) &&
                (numero.Length < 10 || numero.Length > 15))
            {
                throw new ArgumentException(
                    "O WhatsApp deve possuir entre 10 e 15 dígitos, incluindo DDI e DDD.");
            }

            var frequenciaNormalizada =
                (frequencia ?? "DIARIO")
                    .Trim()
                    .ToUpperInvariant();

            if (frequenciaNormalizada is not
                ("DIARIO" or "SEMANAL" or "QUINZENAL" or "MENSAL"))
            {
                throw new ArgumentException(
                    "Frequência de relatório IA inválida.");
            }

            WhatsApp = string.IsNullOrWhiteSpace(numero)
                ? null
                : numero;
            ReceberRelatorioIa = receberRelatorioIa;
            FrequenciaRelatorioIa = frequenciaNormalizada;
        }

        public void RegistrarEnvioRelatorioIa(DateTime data)
        {
            UltimoEnvioRelatorioIa = data;
        }

        public void AlterarNome(string nome)
        {
            ValidarNome(nome);

            Nome = nome.Trim();
        }

        public void DefinirPermissoes(
            IEnumerable<PermissaoSistema>
                permissoes)
        {
            ArgumentNullException.ThrowIfNull(
                permissoes);

            _permissoes.Clear();

            foreach (
                var permissao in
                permissoes.Distinct())
            {
                _permissoes.Add(
                    new UsuarioPermissao(
                        Id,
                        permissao));
            }
        }

        public void DefinirInvestidores(
            IEnumerable<Guid> investidoresIds)
        {
            ArgumentNullException.ThrowIfNull(
                investidoresIds);

            _investidores.Clear();

            foreach (
                var investidorId in
                investidoresIds
                    .Where(x => x != Guid.Empty)
                    .Distinct())
            {
                _investidores.Add(
                    new UsuarioInvestidor(
                        Id,
                        investidorId));
            }
        }

        public void DefinirPermissoesPorInvestidor(
            IEnumerable<(Guid InvestidorId, PermissaoSistema Permissao)> acessos)
        {
            ArgumentNullException.ThrowIfNull(acessos);
            _investidoresPermissoes.Clear();

            foreach (var acesso in acessos
                .Where(x => x.InvestidorId != Guid.Empty)
                .Distinct())
            {
                _investidoresPermissoes.Add(
                    new UsuarioInvestidorPermissao(
                        Id,
                        acesso.InvestidorId,
                        acesso.Permissao));
            }
        }

        private static string NormalizarEmail(
            string email)
        {
            return email
                .Trim()
                .ToLowerInvariant();
        }

        private static void ValidarNome(
            string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException(
                    "O nome do usuário é obrigatório.");
            }

            if (nome.Trim().Length > 150)
            {
                throw new ArgumentException(
                    "O nome do usuário deve possuir no máximo 150 caracteres.");
            }
        }

        private static void ValidarEmail(
            string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException(
                    "O e-mail é obrigatório.");
            }

            var emailNormalizado =
                email.Trim();

            if (emailNormalizado.Length > 254)
            {
                throw new ArgumentException(
                    "O e-mail deve possuir no máximo 254 caracteres.");
            }

            try
            {
                var endereco =
                    new System.Net.Mail.MailAddress(
                        emailNormalizado);

                if (!string.Equals(
                    endereco.Address,
                    emailNormalizado,
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "O e-mail informado é inválido.");
                }
            }
            catch (FormatException)
            {
                throw new ArgumentException(
                    "O e-mail informado é inválido.");
            }
        }

        private static void ValidarSenhaHash(
            string senhaHash)
        {
            if (string.IsNullOrWhiteSpace(
                senhaHash))
            {
                throw new ArgumentException(
                    "O hash da senha é obrigatório.");
            }
        }
    }
}