// Classe usada para ser chamada internamente pelo interpretador da Libra
// Contém a lista de funções embutidas
using Libra;
using Libra.Arvore;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class LibraBase
{
    public static bool DEBUG = false;
    private static Programa _programaAtual => Ambiente.ProgramaAtual;

    public static void RegistrarFuncoesEmbutidas()
    {
        // Temporário
        _programaAtual.PilhaEscopos.DefinirVariavel("AmbienteSeguro", new Variavel("AmbienteSeguro", new Token(TokenTipo.NumeroLiteral, 0, 1)));

        _programaAtual.Funcoes["ping"] = new FuncaoEmbutida(ping);
        _programaAtual.Funcoes["sair"] = new FuncaoEmbutida(sair);
        _programaAtual.Funcoes["exibir"] = new FuncaoEmbutida(exibir);
        _programaAtual.Funcoes["tipo"] = new FuncaoEmbutida(tipo);
        _programaAtual.Funcoes["tamanho"] = new FuncaoEmbutida(tamanho);
        _programaAtual.Funcoes["ler_int"] = new FuncaoEmbutida(ler_int);
        _programaAtual.Funcoes["entrada"] = new FuncaoEmbutida(entrada);
        _programaAtual.Funcoes["concat"] = new FuncaoEmbutida(concat);
        _programaAtual.Funcoes["pausar"] = new FuncaoEmbutida(pausar);
        _programaAtual.Funcoes["aleatorio"] = new FuncaoEmbutida(aleatorio);
        _programaAtual.Funcoes["num"] = new FuncaoEmbutida(num);
        _programaAtual.Funcoes["int"] = new FuncaoEmbutida(_int);
        _programaAtual.Funcoes["caractere"] = new FuncaoEmbutida(caractere);
        _programaAtual.Funcoes["bytes"] = new FuncaoEmbutida(bytes);
        _programaAtual.Funcoes["erro"] = new FuncaoEmbutida(erro);
        _programaAtual.Funcoes["acessar"] = new FuncaoEmbutida(acessar);
        _programaAtual.Funcoes["ref"] = new FuncaoEmbutida(_ref);
        _programaAtual.Funcoes["ptr"] = new FuncaoEmbutida(ptr);
        _programaAtual.Funcoes["registrarCSharp"] = new FuncaoEmbutida(registrarCSharp);
        _programaAtual.Funcoes["registrardll"] = new FuncaoEmbutida(registrardll);
        _programaAtual.Funcoes["libra"] = new FuncaoEmbutida(libra);
    }

    public static object libra(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if(args[0] is not string)
            throw new Erro("Esperado um texto");

        return new Interpretador().Interpretar(args[0].ToString());

    }
    
    public static object sair(object[] args)
    {
        int codigo = 0;

        if(args.Length > 0)
        {
            int.TryParse(args[0].ToString(), out int resultado);
            codigo = resultado;
            _programaAtual.Sair(codigo);
        }

        if(DEBUG)
        {
            Console.WriteLine($"Código de Saída: {codigo}");
        }

        Ambiente.Encerrar(codigo);

        return null;
    }

    public static object registrardll(object[] args)
    {
        int seguro = (int)_programaAtual.PilhaEscopos.ObterVariavel("AmbienteSeguro").Valor;
        if(seguro != 0)
        {
            throw new Erro("Não é possível carregar DLLs externas em um ambiente marcado como seguro.");
        }

        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);
        Assembly assembly = null;
        if(args[0] is string)
        {
            string caminhoDll = args[0].ToString();
            assembly = Assembly.LoadFrom(caminhoDll);
        }
        else if(args[0] is Assembly)
            assembly = (Assembly)args[0];
        else
            throw new Erro("Esperado um caminho para DLL");
            
        foreach (var tipo in assembly.GetTypes())
        {
            foreach (var metodo in tipo.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                string nomeFuncao = $"{tipo.Name}_{metodo.Name}";
                Func<object[], object> funcao = args => metodo.Invoke(null, args);
                _programaAtual.Funcoes[nomeFuncao] = new FuncaoEmbutida(funcao);
            }
        }
        return null;
    }

    public static object registrarCSharp(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 3, args.Length);

        string classe = args[0].ToString();
        string nome = args[1].ToString();
        string codigo = args[2].ToString();

        // Compilar o código do usuário
        var assembly = CompilarCodigo(codigo);
        if (assembly == null) throw new Erro("Erro ao compilar o código.");
        registrardll(new object[] {assembly});

        return null;
    }

    public static string Executar(string comando, string argumentos)
    {
        System.Diagnostics.Process pProcess = new System.Diagnostics.Process();

        pProcess.StartInfo.FileName = comando;
        pProcess.StartInfo.Arguments = argumentos;
        pProcess.StartInfo.UseShellExecute = false;
        pProcess.StartInfo.CreateNoWindow = true;
        pProcess.StartInfo.RedirectStandardOutput = true;   
        pProcess.Start();

        string saida = pProcess.StandardOutput.ReadToEnd();

        pProcess.WaitForExit();

        return saida;
    }

    // Apenas para testar
    public static object[] ping(object[] args)
    {
        Ambiente.Msg("Pong! Função executada pelo C#.");

        if(args.Length > 0)
        {
            Ambiente.Msg("Seus argumentos: ");
            foreach(var arg in args)
                Ambiente.Msg(arg.ToString(), "");
        }
        return null;
    }

    public static object exibir(object[] args)
    {
        int qtd = args.Length;
        switch(qtd)
        {
            case 0:
                Ambiente.Msg("");
                return null;
            case 1:
                Ambiente.Msg($"{args[0]}");
                return null;
            case 2:
                Ambiente.Msg(args[0].ToString(), args[1].ToString());
                return null;
        }

        return null;
    }

    public static object _ref(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        string identificador = args[0].ToString();
        return _programaAtual.PilhaEscopos.ObterVariavel(identificador).Valor;
    }

    public static object acessar(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 2, args.Length);

        var vetor = args[0];
        int indice = (int)args[1];

        if(vetor is string)
            return vetor.ToString()[indice];
        
        // TODO: Adicionar Vetores de fato

        return null;
    }

    public static object concat(object[] args)
    {
        string final = "";
        for(int i = 0; i < args.Length; i++)
        {
            final += args[i].ToString();
        }

        return final;
    }

    public static object _int(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento");

        double n = (double)args[0];
        return (int)n;
    }

    public static object num(object[] args)
    {
        if(args.Length == 0)
            new Erro("Esperava 1 argumento");

        return int.Parse(args[0].ToString());
    }

    public static object bytes(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        var objeto = args[0];

        if (objeto is int)
            return sizeof(int);
        
        if (objeto is string)
            return sizeof(char) * objeto.ToString().Length;

        return 0;
    }

    public static object caractere(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        return (char)args[0];
    }

    public static object ler_int(object[] args)
    {
        return int.Parse(Console.ReadLine());
    }

    public static object entrada(object[] args)
    {
        return Console.ReadLine();
    }

    public static object tamanho(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if (args[0] is string str)
            return str.Length;

        if (args[0] is Array array)
            return array.Length;

        throw new ArgumentException("Argumento inválido para length.");

        return null;
    }


    public static object pausar(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        int i = (int)args[0];

        Thread.Sleep(i);

        return null;
    }

    public static object aleatorio(object[] args)
    {
        Random random = new Random();
        double num = 0;

        int qtd = args.Length;
        switch(qtd)
        {
            case 0:
                num = random.NextDouble();
                break;
            case 1:
                num = random.Next((int)args[0]);
                break;
            case 2:
                num = random.Next((int)args[0], (int)args[1]);
                break;
        }
        
        return num;
    }

    public static object ptr(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        if(args[0].GetType().ToString() != "System.String")
            new Erro("esperava <texto>").LancarErro();

        return _programaAtual.PilhaEscopos.ObterIndiceVariavel(args[0].ToString());
    }

    public static object tipo(object[] args)
    {
        ChecarArgumentos(MethodBase.GetCurrentMethod().Name, 1, args.Length);

        switch(args[0].GetType().ToString())
        {
            case "System.Int32":
                return "Int";
            case "System.String":
                return "Texto";
            case "System.Char":
                return "Char";
            case "System.Double":
                return "Real";
        }

        return null;
    }

    public static object erro(object[] args)
    {
        if(args.Length == 0)
        {
            throw new Erro("Erro!");
        }
            
        throw new Erro(args[0].ToString());

        return null;
    }

    private static Assembly CompilarCodigo(string codigo)
    {
        var tree = CSharpSyntaxTree.ParseText(codigo);
        var compilation = CSharpCompilation.Create(
            "LibraExterna.dll",
            new[] { tree },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            foreach (var diagnostic in result.Diagnostics)
                Ambiente.Msg(diagnostic.ToString());
            return null;
        }

        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }

    private static void ChecarArgumentos(string ident, int esperado, int recebido)
    {
        if(esperado != recebido)
            throw new ErroEsperadoNArgumentos(ident, esperado, recebido);
    }

}