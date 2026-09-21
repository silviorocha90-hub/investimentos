export type PerfilUsuario =
  | 'Admin'
  | 'Usuario'

export type StatusUsuario =
  | 'Pendente'
  | 'Ativo'
  | 'Rejeitado'
  | 'Bloqueado'

export type PermissaoSistema =
  | 'Dashboard'
  | 'Carteira'
  | 'Operacoes'
  | 'Opcoes'
  | 'Proventos'
  | 'Administracao'

export interface UsuarioAdministracao {
  id: string
  nome: string
  email: string
  perfil: PerfilUsuario
  status: StatusUsuario
  dataCadastro: string
  dataAprovacao?: string | null
  aprovadoPorUsuarioId?: string | null
  ultimoLogin?: string | null
  permissoes: string[]
  investidoresIds: string[]
  acessosInvestidores: AcessoInvestidor[]
}

export interface AcessoInvestidor {
  investidorId: string
  permissoes: PermissaoSistema[]
}

export interface AlterarAcessosUsuario {
  perfil: PerfilUsuario
  permissoes: PermissaoSistema[]
  investidoresIds: string[]
  acessosInvestidores: AcessoInvestidor[]
}

export const permissoesVisualizacao:
  PermissaoSistema[] = [
    'Dashboard',
    'Carteira',
    'Opcoes',
    'Proventos',
  ]

export const permissoesSistema:
  PermissaoSistema[] = [
    ...permissoesVisualizacao,
    'Operacoes',
    'Administracao',
  ]

export const nomesPermissoes:
  Record<
    PermissaoSistema,
    string
  > = {
    Dashboard: 'Painel',
    Carteira: 'Carteira',
    Operacoes: 'Operações',
    Opcoes: 'Opções',
    Proventos: 'Proventos',
    Administracao:
      'Administração',
  }