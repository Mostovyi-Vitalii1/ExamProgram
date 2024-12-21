namespace OrderManagement.Core.Abstraction.Commands
{
    public interface ICommandHandler
    {
        void Handle(ICommand command);
        void Handle<TCommand>(TCommand command) where TCommand : ICommand;
    }
}