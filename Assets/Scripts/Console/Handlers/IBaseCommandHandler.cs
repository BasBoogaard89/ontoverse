public interface IBaseCommandHandler
{
    bool CanHandle(string command);
    void Handle(string command, ConsoleController controller);
}
