namespace Libra.Arvore
{
    public enum TipoExpressao
    {
        ExpressaoLiteral,
        ExpressaoVariavel,
        ExpressaoChamadaFuncao,
        ExpressaoAcessoVetor,
        ExpressaoUnaria,
        ExpressaoBinaria
    }

    public abstract class Expressao 
    {
        public TipoExpressao Tipo { get; protected set ;}
    }

    public class ExpressaoUnaria : Expressao
    {
        public Token Operador { get; private set; }
        public Expressao Operando { get; private set; }

        public ExpressaoUnaria(Token operador, Expressao operando)
        {
            Operador = operador;
            Operando = operando;

            Tipo = TipoExpressao.ExpressaoUnaria;
        }

    }


    public class ExpressaoLiteral : Expressao
    {
        public ExpressaoLiteral(Token token)
        {
            Token = token;
            Tipo = TipoExpressao.ExpressaoLiteral;
        }

        public Token Token { get; private set; }
        public object Valor => Token.Valor;

    }

    public class ExpressaoVariavel : Expressao
    {
        public Token Identificador { get; private set ;}

        public ExpressaoVariavel(Token identificador)
        {
            Identificador = identificador;
            Tipo = TipoExpressao.ExpressaoVariavel;
        }

    }

    public class ExpressaoChamadaFuncao : Expressao
    {
        public string Identificador { get; private set; }
        public List<Expressao> Argumentos { get; private set; }

        public ExpressaoChamadaFuncao(string ident, List<Expressao> argumentos = null)
        {
            Identificador = ident;
            Argumentos = argumentos;

            if(Argumentos == null)
            {
                Argumentos = new List<Expressao>();
            }

            Tipo = TipoExpressao.ExpressaoChamadaFuncao;
        }
    }

    public class ExpressaoAcessoVetor : Expressao
    {
        public ExpressaoAcessoVetor(string ident, Expressao expr)
        {
            Identificador = ident;
            Expressao = expr;
        }
        public string Identificador {  get; private set; }
        public Expressao Expressao { get; private set; }
    }

    public class ExpressaoBinaria : Expressao
    {
        public Expressao Esquerda { get; private set; }
        public Token Operador { get; private set; }
        public Expressao Direita { get; private set; }

        public ExpressaoBinaria(Expressao esquerda, Token operador, Expressao direita)
        {
            Esquerda = esquerda;
            Operador = operador;
            Direita = direita;

            Tipo = TipoExpressao.ExpressaoBinaria;
        }

        public override string ToString()
        {
            return $"{Esquerda.ToString()} {Token.TipoParaString(Operador.Tipo)} {Direita.ToString()}";
        }

    }
    
}