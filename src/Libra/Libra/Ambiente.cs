using Libra.Arvore;

namespace Libra;

public class Ambiente
{
    private static Ambiente _ambienteAtual;
    private ILogger _logger;
    public static ILogger Logger => _ambienteAtual._logger;
    private Programa _programaAtual;
    private bool _deveEncerrar;
    public static Programa ProgramaAtual => _ambienteAtual._programaAtual;
    public static bool DeveEncerrar => _ambienteAtual._deveEncerrar;

    private Ambiente()
    {

    }

    public static void ConfigurarAmbiente(ILogger logger)
    {
        _ambienteAtual = new Ambiente();

        if(logger == null)
            _ambienteAtual._logger = new ConsoleLogger();
        else
            _ambienteAtual._logger = logger;

    }

    public static void SetarPrograma(Programa programa)
    {
        _ambienteAtual._programaAtual = programa;
        LibraBase.RegistrarFuncoesEmbutidas();
    }

    public static void Msg(string msg, string final = "\n")
    {
        var loggerReal = Logger == null ? new ConsoleLogger() : Logger;
        loggerReal.Msg(msg, final);
    }

    public static void Encerrar(int codigo)
    {
        //throw new NotImplementedException();
    }

}