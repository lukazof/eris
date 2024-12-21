namespace Libra;

public enum ErroCategoria
{
    Erro, // Erro Genérico
    ErroSintaxe,
    ErroExecucao
}

public class Erro : Exception
{
    public int Codigo { get; protected set; }
    public string Mensagem { get; protected set; }
    public int Linha { get; protected set; }
    public int Coluna { get; protected set; }
    public ErroCategoria Categoria { get; protected set; }

    public Erro(string mensagem, int linha = 0, int codigo = 1)
    {
        Codigo = codigo;
        Linha = linha;
        Mensagem = mensagem;

        AtribuirCategoria();
        LancarErro();
    }

    protected Erro(int codigo, string mensagem, int linha = 0, int coluna = 0)
    {
        Codigo = codigo;
        Mensagem = mensagem;
        Linha = linha;
        Coluna = coluna;

        AtribuirCategoria();
        LancarErro();
    }

    internal void LancarErro()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        
        Ambiente.Msg(this.ToString());
        
        Console.ResetColor();
        
        Ambiente.Encerrar(this.Codigo);
    }

    // TODO: Não está sendo chamado!
    internal void Dica(string msg)
    {
        Ambiente.Msg($"Dica: {msg}");
    }

    private void AtribuirCategoria()
    {
        if(Codigo >= 1000 && Codigo < 2000)
            Categoria = ErroCategoria.ErroSintaxe;
        else if(Codigo >= 2000 && Codigo < 3000)
            Categoria = ErroCategoria.ErroExecucao;
        else
            Categoria = ErroCategoria.Erro; // Genérico
    }

    public override string ToString()
    {
        string msg = "";
        string categoria = $"{Categoria}: ";

        msg += categoria;

        if(Linha == 0)
            msg += $"{Mensagem}";
        else
            msg +=$"{Mensagem} na linha {Linha}";
        
        string arrows = "";

        foreach(var c in Mensagem)
            arrows += "^";
        

        msg += Environment.NewLine + String.Concat(Enumerable.Repeat(' ', categoria.Length)) + arrows;

        return msg;
    }
}

public class ErroTokenInvalido : Erro
{
    public ErroTokenInvalido(string token, int linha = 0, int coluna = 0) 
        : base(1001, $"Token inválido `{token}`", linha, coluna) { }
}

public class ErroEsperado : Erro
{
    public ErroEsperado(TokenTipo esperado, TokenTipo recebido, int linha = 0, int coluna = 0) 
        : base(1002, $"Esperado Token {Token.TipoParaString(esperado)}, recebido {Token.TipoParaString(recebido)}", linha, coluna) { }
}

public class ErroDivisaoPorZero : Erro
{
    public ErroDivisaoPorZero(int linha = 0, int coluna = 0) 
        : base(2001, "Divisão por zero.", linha, coluna) { }
}

public class ErroVariavelNaoDeclarada : Erro
{
    public ErroVariavelNaoDeclarada(string variavel, int linha = 0, int coluna = 0) 
        : base(2002, $"Variável não declarada `{variavel}`.", linha, coluna) 
        {
            Dica($"Use `var {variavel}` para declarará-la, variáveis só são acessíveis no mesmo escopo.");
        }
}

public class ErroVariavelJaDeclarada : Erro
{
    public ErroVariavelJaDeclarada(string variavel, int linha = 0, int coluna = 0) 
        : base(2003, $"Variável já declarada `{variavel}`.", linha, coluna) 
        { 
            Dica("Não use 'var' caso queira apenas modificar o valor de uma variável.");
        }
}

public class ErroFuncaoNaoDefinida : Erro
{
    public ErroFuncaoNaoDefinida(string variavel, int linha = 0, int coluna = 0) 
        : base(2004, $"Função não definida `{variavel}`.", linha, coluna) { }
}

public class ErroFuncaoJaDefinida : Erro
{
    public ErroFuncaoJaDefinida(string variavel, int linha = 0, int coluna = 0) 
        : base(2005, $"Função já definida `{variavel}`.", linha, coluna) { }
}

public class ErroModificacaoConstante : Erro
{
    public ErroModificacaoConstante(string variavel, int linha = 0, int coluna = 0) 
        : base(2006, $"Não é possível modificar a constante `{variavel}`.", linha, coluna) 
        { 
            Dica("Use 'var' ao invés de 'const' para declarar variáveis modificáveis.");
        }
}

public class ErroAcessoNulo : Erro
{
    public ErroAcessoNulo(string msg = "", int linha = 0, int coluna = 0)
        : base(2007, $"Tentando acessar um valor Nulo.{msg}", linha, coluna) 
        { 
            Dica("Você tentou acessar um objeto sem referência (Nulo).");
        }
}

public class ErroIndiceForaVetor : Erro
{
    public ErroIndiceForaVetor(string msg = "", int linha = 0, int coluna = 0)
        : base(2008, $"O indice se encontra fora dos limites do Vetor.", linha, coluna) { }
}

public class ErroTipoIncompativel : Erro
{
    public ErroTipoIncompativel(string identificador, int linha = 0, int coluna = 0)
        : base(2009, $"Atribuindo um tipo incompatível à `{identificador}`.", linha, coluna) 
        { 
            Dica("Não é possível modificar o tipo de uma variável durante a execução.");
        }
}

public class ErroTransbordoDePilha : Erro
{
    public ErroTransbordoDePilha(string causador = "", int linha = 0, int coluna = 0)
        : base(2010, $"Transbordo de Pilha (StackOverflow) causado por: {causador}()", linha, coluna) 
        { 
            Dica("Verifique se não há nenhuma recursão infinita.");
        }
}

public class ErroEsperadoNArgumentos : Erro
{
    public ErroEsperadoNArgumentos(string ident, int esperado, int recebido, int linha = 0, int coluna = 0)
        : base(2010, $"{ident}() esperava: {esperado} argumento(s), mas recebeu {recebido}.", linha, coluna) 
        { 
        }
}