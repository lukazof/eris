
namespace Libra.Arvore
{
    public abstract class Instrucao 
    { 
        public TokenTipo TipoInstrucao { get; protected set; }
    }

    public class InstrucaoVar : Instrucao
    {
        public InstrucaoVar(string identificador, Expressao expressao, bool constante, bool declaracao, TokenTipo tipo = TokenTipo.TokenInvalido)
        {
            Identificador = identificador;
            EhDeclaracao = declaracao;
            Constante = constante;
            Tipo = tipo;   
            Expressao = expressao; 
            TipoInstrucao = TokenTipo.Var;
        }

        public Expressao Expressao { get; private set; }
        public TokenTipo Tipo { get; private set; }
        public string Identificador {get; private set; }
        internal bool EhDeclaracao; // usada para saber se estamos declarando uma nova variável ou só modificando uma
        public bool Constante { get; private set; }

    }

    public class InstrucaoFuncao : Instrucao
    {
        public InstrucaoFuncao(string identificador, Instrucao[] instrucoes, List<string> parametros = null)
        {
            Instrucoes = instrucoes;
            Identificador = identificador;
            Parametros = parametros;
            TipoInstrucao = TokenTipo.Funcao;
        }

        public Instrucao[] Instrucoes { get; private set; }
        public string Identificador {get; private set; }
        public List<string> Parametros { get; private set; }

    }

    public class InstrucaoSe : Instrucao
    {
        public InstrucaoSe(Expressao expressao, Instrucao[] instrucoes, Instrucao[] senaoInstrucoes = null)
        {
            Expressao = expressao;
            Instrucoes = instrucoes;
            SenaoInstrucoes = senaoInstrucoes;
            TipoInstrucao = TokenTipo.Se;
        }

        public Expressao Expressao { get; private set; }
        public Instrucao[] Instrucoes {get; private set; }
        public Instrucao[] SenaoInstrucoes {get; private set; }

    }

    public class InstrucaoEnquanto : Instrucao
    {
        public InstrucaoEnquanto(Expressao expressao, Instrucao[] instrucoes)
        {
            Expressao = expressao;
            Instrucoes = instrucoes;
            TipoInstrucao = TokenTipo.Enquanto;
        }

        public Expressao Expressao { get; private set; }
        public Instrucao[] Instrucoes {get; private set; }

    }

    public class InstrucaoRomper : Instrucao
    {
        // Nada de interessante...
    }

    public class InstrucaoRetornar : Instrucao
    {
        public Expressao Expressao { get; private set; }

        public InstrucaoRetornar(Expressao expressao)
        {
            Expressao = expressao;
            TipoInstrucao = TokenTipo.Retornar;
        }
    }

    public class InstrucaoChamadaFuncao : Instrucao
    {
        public InstrucaoChamadaFuncao(ExpressaoChamadaFuncao chamada)
        {
            Chamada = chamada;
            TipoInstrucao = TokenTipo.Identificador;
        }

        public ExpressaoChamadaFuncao Chamada { get; private set; }
    }
}