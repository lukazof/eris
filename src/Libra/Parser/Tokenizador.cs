using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace Libra;

public class Tokenizador 
{
    private int _posicao;
    private string? _fonte;
    private List<Token>? _tokens;
    private int _linha;
    private bool _falha;
    private Dictionary<string, TokenTipo> _palavrasReservadas = new Dictionary<string, TokenTipo>
    {
        { "var", TokenTipo.Var },
        { "const", TokenTipo.Const },
        { "funcao", TokenTipo.Funcao },
        { "se", TokenTipo.Se },
        { "senao", TokenTipo.Senao },
        { "enquanto", TokenTipo.Enquanto },
        { "faca", TokenTipo.Faca },
        { "romper", TokenTipo.Romper },
        { "retornar", TokenTipo.Retornar },
        { "entao", TokenTipo.Entao },
        { "fim", TokenTipo.Fim },
        { "ou", TokenTipo.OperadorOu },
        { "e", TokenTipo.OperadorE }
    };


    public List<Token> Tokenizar(string source) 
    {
        _fonte = source;
        _tokens = new();
        _linha = 1;

        var texto = "";
        try
        {
            while (Atual() != '\0' && !_falha)
            {
                if(char.IsDigit(Atual()))
                {
                    TokenizarNumero();
                }

                else if(char.IsLetter(Atual()) || Atual() == '_')
                {
                    TokenizarPalavra();
                }
                else 
                {
                    TokenizarSimbolo();
                }
            }
        
            AdicionarTokenALista(TokenTipo.FimDoArquivo);

            if (_falha)
                return new List<Token>();

            return _tokens;
            }
        catch (Exception e)
        {
            new Erro($"{e}");
        }

        return null;
    }

    private void TokenizarNumero()
    {
        string buffer = "";
        var ponto = false;
        while (char.IsDigit(Atual()))
        {
            buffer += ConsumirChar();
            if (Atual() == '.')
            {
                if (ponto)
                    throw new Erro("Número inválido!", _linha);

                buffer += ConsumirChar();
                ponto = true;
            }
        }

        if (ponto)
            AdicionarTokenALista(TokenTipo.NumeroLiteral, double.Parse(buffer));
        else
            AdicionarTokenALista(TokenTipo.NumeroLiteral, int.Parse(buffer));
    }

    private void TokenizarPalavra()
    {
        string buffer = "" + ConsumirChar();

        while (char.IsLetterOrDigit(Atual()) || Atual() == '_')
        {
            buffer += ConsumirChar();
        }

        if (_palavrasReservadas.ContainsKey(buffer))
        {
            AdicionarTokenALista(_palavrasReservadas[buffer]);
        }
        else if (buffer == "importar")
        {
            buffer = "";
            while (Atual() == ' ')
                Passar();

            if (Atual() != '"')
                throw new Erro("Esperado `\"`");

            ConsumirChar();

            buffer += ConsumirAte('"');

            string caminhoExecutavel = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, buffer);
            string caminhoUsuario = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), buffer);

            if (!File.Exists(caminhoExecutavel) && !File.Exists(caminhoUsuario))
                throw new ErroAcessoNulo(caminhoExecutavel);

            string caminhoArquivo = File.Exists(caminhoExecutavel) ? caminhoExecutavel : caminhoUsuario;

            string arquivoCarregado = File.ReadAllText(caminhoArquivo);
            _fonte = arquivoCarregado + _fonte;
            string linha = $"importar \"{buffer}\"";
            _fonte = _fonte.Replace(linha, "");
            _posicao -= linha.Length;
        }
        else
        {
            AdicionarTokenALista(TokenTipo.Identificador, buffer);
        }

    }

    private void TokenizarSimbolo()
    {
        string buffer = "";
        switch (Atual())
        {
            case ' ':
                Passar();
                break;
            case '\n':
                _linha++;
                Passar();
                break;
            case '\r':
                Passar();
                break;
            case '\t':
                Passar();
                break;
            case ';':
                AdicionarTokenALista(TokenTipo.PontoEVirgula);
                Passar();
                break;
            case '(':
                AdicionarTokenALista(TokenTipo.AbrirParen);
                Passar();
                break;
            case '[':
                ConsumirChar();
                AdicionarTokenALista(TokenTipo.AbrirCol);
                break;
            case ']':
                AdicionarTokenALista(TokenTipo.FecharCol);
                Passar();
                break;
            case '>':
                if (Proximo(1) == '=')
                {
                    AdicionarTokenALista(TokenTipo.OperadorMaiorIgualQue);
                    Passar();
                    Passar();
                    break;
                }

                AdicionarTokenALista(TokenTipo.OperadorMaiorQue);
                Passar();
                break;
            case '<':
                if (Proximo(1) == '=')
                {
                    AdicionarTokenALista(TokenTipo.OperadorMenorIgualQue);
                    Passar();
                    Passar();
                    break;
                }

                AdicionarTokenALista(TokenTipo.OperadorMenorQue);
                Passar();
                break;
            case '!':
                if (Proximo(1) == '=')
                {
                    AdicionarTokenALista(TokenTipo.OperadorDiferente);
                    Passar();
                    Passar();
                    break;
                }
                AdicionarTokenALista(TokenTipo.OperadorNeg);
                Passar();
                break;
            case '+':
                AdicionarTokenALista(TokenTipo.OperadorSoma);
                Passar();
                break;
            case '-':
                AdicionarTokenALista(TokenTipo.OperadorSub);
                Passar();
                break;
            case '*':
                AdicionarTokenALista(TokenTipo.OperadorMult);
                Passar();
                break;
            case '/':
                Passar();
                if (Atual() == '/')
                {
                    ConsumirComentarioLinha();
                }
                else if (Atual() == '*')
                {
                    ConsumirComentarioBloco();
                }
                else
                {
                    AdicionarTokenALista(TokenTipo.OperadorDiv);
                }
                break;
            case '^':
                AdicionarTokenALista(TokenTipo.OperadorPot);
                Passar();
                break;
            case ')':
                AdicionarTokenALista(TokenTipo.FecharParen);
                Passar();
                break;
            case '=':
                ConsumirChar();

                if (Atual() == '=')
                {
                    AdicionarTokenALista(TokenTipo.OperadorComparacao);
                    Passar();
                }
                else
                {
                    AdicionarTokenALista(TokenTipo.OperadorDefinir);
                }
                break;
            case '"':
                Passar();

                while (Atual() != '"')
                {
                    if (Atual() == '\\' && Proximo(1) == '"')
                    {
                        Passar();
                    }
                    buffer += ConsumirChar();
                }

                AdicionarTokenALista(TokenTipo.TextoLiteral, buffer);
                buffer = "";
                Passar();
                break;
            case '\'':
                Passar();
                buffer += ConsumirChar();
                AdicionarTokenALista(TokenTipo.CaractereLiteral, buffer);
                buffer = "";
                if (Atual() != '\'')
                {
                    throw new Erro("Esperado `'`", _linha);
                }
                Passar();
                break;
            case ',':
                AdicionarTokenALista(TokenTipo.Virgula);
                Passar();
                break;
            default:
                throw new ErroTokenInvalido($"{Atual()} (ASCII = {(int)Atual()})", _linha);
                break;
        }
    }

    private string ConsumirAte(char caractere)
    {
        string buffer = "";
        while(Atual() != caractere)
        {
            buffer += ConsumirChar();

            if (Atual() == '\0')
                throw new Erro($"Esperado `{caractere}`");
        }

        Passar();
        return buffer;
    }

    private void ConsumirEspacosVazios()
    {
        while(Atual() == ' ' || Atual() == '\n')
        {
            Passar();
        }
    }

    private void ConsumirComentarioLinha()
    {
        Passar();
        while (Proximo(1) != '\n' && Proximo(1) != '\0')
        {
            Passar();
        }
    }

    private void ConsumirComentarioBloco()
    {
        Passar();
        while (Proximo(1) != '\0')
        {
            if (Atual() == '*' && Proximo(1) == '/')
            {
                Passar(); // Consome '*'
                Passar(); // Consome '/'
                break;
            }
            Passar();
        }
    }

    public void PrintarListaTokens()
    {
        foreach(var token in _tokens)
        {
            Ambiente.Msg(token.ToString());
        }
    }

    private char Atual() 
    {
        return Proximo(0);
    }
    
    private char Proximo(int offset)
    {
        if(_posicao + offset < _fonte.Length)
        {
            return _fonte[_posicao + offset];
        }

        return '\0';
    }

    private void Passar(int quantidade = 1) 
    {
        _posicao += quantidade;
        
    }

    private char ConsumirChar()
    {
        var c = Atual();
        Passar();
        
        return c;
    }

    private void AdicionarTokenALista(TokenTipo tipo)
    {
        _tokens.Add(new Token(tipo, _linha));
    }

    private void AdicionarTokenALista(TokenTipo tipo, object valor)
    {
        _tokens.Add(new Token(tipo, _linha, valor));
    }
}