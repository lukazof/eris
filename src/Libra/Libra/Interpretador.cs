using Libra.Arvore;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Libra;

public class Interpretador
{

    public static int NivelDebug = 0;
    private Programa _programa => Ambiente.ProgramaAtual;
    private object _ultimoRetorno = 0;
    private int _linha = 0;
    private bool _deveEncerrar => Ambiente.DeveEncerrar;

    public int Interpretar(string codigo, ILogger logger = null)
    {
        Ambiente.ConfigurarAmbiente(logger);
        try
        {
            
            var tokenizador = new Tokenizador();
            var tokens = tokenizador.Tokenizar(codigo);
            var parser = new Parser();
            var programa = parser.Parse(tokens);
            return Interpretar(programa);
        }
        catch
        {
            return 1;
        }
    }

    public int Interpretar(Programa programa)
    {
        try
        {
            Ambiente.SetarPrograma(programa);
            InterpretarInstrucoes(programa.Instrucoes);
            return Ambiente.ProgramaAtual.CodigoSaida;
        }
        catch
        {
            return 1;
        }
    }

    private void InterpretarInstrucoes(Instrucao[] instrucoes)
    {
        if(_deveEncerrar)
            return;

        for(int i = 0; i < instrucoes.Length; i++)
        {
            if(instrucoes[i].TipoInstrucao == TokenTipo.Retornar)
            {
                Retornar((InstrucaoRetornar)instrucoes[i]);
                break;
            }
                
            InterpretarInstrucao(instrucoes[i]);
        }
    }

    private void InterpretarInstrucao(Instrucao instrucao)
    {
        if(_deveEncerrar)
            return;

        switch(instrucao.TipoInstrucao)
        {
            case TokenTipo.Var:
                InterpretarInstrucaoVar((InstrucaoVar)instrucao);
                break;
            case TokenTipo.Const:
                InterpretarInstrucaoVar((InstrucaoVar)instrucao);
                break;
            case TokenTipo.Se:
                InterpretarCondicional((InstrucaoSe)instrucao);
                break;
            case TokenTipo.Enquanto:
                InterpretarCondicional((InstrucaoEnquanto)instrucao);
                break;
            case TokenTipo.Funcao:
                InterpretarFuncao((InstrucaoFuncao)instrucao);
                break;
            case TokenTipo.Identificador:
                InterpretarChamadaFuncao((InstrucaoChamadaFuncao)instrucao);
                break;
            case TokenTipo.Retornar:
                Retornar((InstrucaoRetornar)instrucao);
                break;
        }

    }

    private void Retornar(InstrucaoRetornar instrucao)
    {
        _ultimoRetorno = InterpretarExpressao(((InstrucaoRetornar)instrucao).Expressao);
    }
    private void InterpretarCondicional(Instrucao instrucao)
    {
        if(instrucao is InstrucaoSe)
        {
            var instrucaoSe = (InstrucaoSe)instrucao;
            var instrucoesSe = instrucaoSe.Instrucoes;
            if((int)InterpretarExpressao(instrucaoSe.Expressao) != 0)
                InterpretarInstrucoes(instrucoesSe);
            
            if(instrucaoSe.SenaoInstrucoes != null)
                InterpretarInstrucoes(instrucaoSe.SenaoInstrucoes);
            
            return;
        }

        var enquanto = (InstrucaoEnquanto)instrucao;
    	var instrucoes = enquanto.Instrucoes;
        while((int)InterpretarExpressao(enquanto.Expressao) != 0 && !_deveEncerrar)
            InterpretarInstrucoes(instrucoes);
    }

    private void InterpretarFuncao(InstrucaoFuncao funcao)
    {
        string identificador = funcao.Identificador;

        if(string.IsNullOrWhiteSpace(identificador))
            throw new Erro("Identificador inválido!", _linha);
        
        if(_programa.FuncaoExiste(identificador))
            throw new ErroFuncaoJaDefinida(identificador, _linha);
        
        var novaFuncao = new Funcao(identificador, funcao.Instrucoes, funcao.Parametros);

        _programa.Funcoes[identificador] = novaFuncao;
    }

    private object InterpretarChamadaFuncao(ExpressaoChamadaFuncao expressaoChamadaFuncao)
    {
        return InterpretarChamadaFuncao(new InstrucaoChamadaFuncao(expressaoChamadaFuncao));
    }

    private object ExecutarFuncaoEmbutida(FuncaoEmbutida funcao, ExpressaoChamadaFuncao chamada) 
    {
        var f = (FuncaoEmbutida)_programa.Funcoes[chamada.Identificador];
        List<object> valoresArgumentos = new List<object>();
        for(int i =0; i < chamada.Argumentos.Count; i++)
        {
            valoresArgumentos.Add(InterpretarExpressao(chamada.Argumentos[i]));
        }

        _ultimoRetorno = f.Executar(valoresArgumentos.ToArray());
        return _ultimoRetorno;
    }

    private object InterpretarChamadaFuncao(InstrucaoChamadaFuncao instrucaoChamada)
    {
        var chamada = instrucaoChamada.Chamada;
        var argumentos = chamada.Argumentos;

        if (!_programa.FuncaoExiste(chamada.Identificador))
            throw new ErroFuncaoNaoDefinida(chamada.Identificador, _linha);

        var funcao = _programa.Funcoes[chamada.Identificador];

        if(_programa.Funcoes[chamada.Identificador] is FuncaoEmbutida)
            return ExecutarFuncaoEmbutida((FuncaoEmbutida)funcao, chamada);

        var parametros = funcao.Parametros.Count;

        if (argumentos.Count != parametros)
            throw new ErroEsperadoNArgumentos(funcao.Identificador, parametros, argumentos.Count, _linha);

        _programa.PilhaEscopos.EmpilharEscopo(); // empurra o novo Escopo da função

        // Adicionando os argumentos ao Escopo
        for (int i = 0; i < chamada.Argumentos.Count; i++)
        {
            string ident = funcao.Parametros[i];
            Token valor = new Token(TokenTipo.NumeroLiteral, 0, InterpretarExpressao(chamada.Argumentos[i]));
            _programa.PilhaEscopos.DefinirVariavel(ident, new Variavel(ident, valor));
        }

        InterpretarInstrucoes(funcao.Instrucoes);
        
        _programa.PilhaEscopos.DesempilharEscopo(); // Removendo o Escopo da Pilha

        return _ultimoRetorno;

    }

    private object InterpretarExpressao(Expressao expressao)
    {
        if(expressao == null)
            throw new ErroAcessoNulo(" Expressão nula", _linha);

        switch(expressao.Tipo)
        {
            case TipoExpressao.ExpressaoLiteral:
                return ((ExpressaoLiteral)expressao).Token.Valor;
            case TipoExpressao.ExpressaoVariavel:
            {
                var exprVariavel = (ExpressaoVariavel)expressao;
                var variavel = _programa.PilhaEscopos.ObterVariavel(exprVariavel.Identificador.Valor.ToString());
                return variavel.Valor;
            }
            case TipoExpressao.ExpressaoChamadaFuncao:
                return InterpretarChamadaFuncao((ExpressaoChamadaFuncao)expressao);
            case TipoExpressao.ExpressaoUnaria:
                break;
            case TipoExpressao.ExpressaoBinaria:
                var bin = (ExpressaoBinaria)expressao;
                var a = InterpretarExpressao(bin.Esquerda);
                var b = InterpretarExpressao(bin.Direita);

                switch(bin.Operador.Tipo)
                {
                    case TokenTipo.OperadorSoma:
                        return Soma(a, b);
                    case TokenTipo.OperadorSub:
                        return Sub(a, b);
                    case TokenTipo.OperadorMult:
                        return Mult(a, b);
                    case TokenTipo.OperadorDiv:
                        return Div(a, b);
                    case TokenTipo.OperadorPot:
                        return Pot(a,b);
                    case TokenTipo.OperadorComparacao:
                        return Igual(a, b);
                    case TokenTipo.OperadorDiferente:
                        return Diferente(a, b);
                    case TokenTipo.OperadorMaiorQue:
                        return MaiorQue(a, b);
                    case TokenTipo.OperadorMaiorIgualQue:
                        return MaiorIgualQue(a, b);
                    case TokenTipo.OperadorMenorQue:
                        return MenorQue(a, b);
                    case TokenTipo.OperadorMenorIgualQue:
                        return MenorIgualQue(a, b);
                }
                break;
        }

        throw new Erro("Expressão não implementada", _linha);

        return 0;
    }

    // TODO: Tem que ter uma forma melhor de fazer isso...
    // Será difícil manter quando tiver muitos tipos e operadores
    private object Operar(dynamic a, dynamic b, Func<dynamic, dynamic, dynamic> operacao)
    {
        try
        {
            return operacao(a, b);
        }
        catch (RuntimeBinderException)
        {
            return null;
        }

        return null;
    }

    private object Soma(object a, object b) => Operar(a, b, (x, y) => x + y);
    private object Sub(object a, object b) => Operar(a, b, (x, y) => x - y);
    private object Mult(object a, object b) => Operar(a, b, (x, y) => x * y);
    private object Div(object a, object b) => Operar(a, b, (x, y) => y == 0 ? throw new ErroDivisaoPorZero(_linha) : x / y);
    private object Pot(object a, object b)
    {
        if(a is int && b is int)
            return Math.Pow((int)a,(int)b);
        if(a is double && b is double)
            return Math.Pow((double)a,(double)b);
        
        throw new Erro($"Não é possível calcular {a.ToString()}^{b.ToString()}");
    }
    private int Igual(object a, object b)
    {
        if(a is int)
            return (int)a == (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a == (double)b ? 1 : 0;

        if(a is string)
            return (string)a == (string)b ? 1 : 0;
        
        throw new Erro("Operação Inválida", _linha);
        return 0;
    }

    private int Diferente(object a, object b)
    {
        if(a is int)
            return (int)a != (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a != (double)b ? 1 : 0;

        if(a is string)
            return (string)a != (string)b ? 1 : 0;
        
        throw new Erro("Operação Inválida", _linha);
        return 0;
    }

    private int MaiorQue(object a, object b)
    {
        if(a is int)
            return (int)a > (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a > (double)b ? 1 : 0;
        
        throw new Erro("Operação Inválida", _linha);
        return 0;
    }

    private int MaiorIgualQue(object a, object b)
    {
        if(a is int)
            return (int)a >= (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a >= (double)b ? 1 : 0;
        
        throw new Erro("Operação Inválida", _linha);
        return 0;
    }

    private int MenorQue(object a, object b)
    {
        if(a is int)
            return (int)a < (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a < (double)b ? 1 : 0;
        
        throw new Erro("Operação Inválida", _linha);
        return 0;
    }

    private int MenorIgualQue(object a, object b)
    {
        if(a is int)
            return (int)a <= (int)b ? 1 : 0;
        
        if(a is double)
            return (double)a <= (double)b ? 1 : 0;
        
        throw new Erro("Operação Inválida", _linha);
        return 0;
    }

    private object InterpretarInstrucaoVar(InstrucaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            throw new Erro("Identificador inválido!", _linha);

        object resultado = InterpretarExpressao(i.Expressao);
        var token = new Token(i.Tipo, _linha, resultado);
        var variavel = new Variavel(i.Identificador, token, i.Constante);

        if(i.EhDeclaracao)
            _programa.PilhaEscopos.DefinirVariavel(i.Identificador, variavel);
        else
            _programa.PilhaEscopos.AtualizarVariavel(i.Identificador, resultado);

        return variavel.Valor;
    }

    private object AtribuirElementoVetor(string identificador, Expressao expressaoIndice, Expressao expressao)
    {
        int indiceVetor = (int)InterpretarExpressao(expressaoIndice);
        object valor = InterpretarExpressao(expressao);

        var vetor = (Token[])_programa.PilhaEscopos.ObterVariavel(identificador).Valor;

        if(indiceVetor > vetor.Length || indiceVetor < 0)
            throw new ErroIndiceForaVetor("", _linha);

        vetor[indiceVetor].Valor = valor; // TODO: Isso funciona?

        return valor;
    }

}