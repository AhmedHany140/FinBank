
using Infrastructure.Interfaces;
using Infrastructure.ResultPattern;

public record RegisterCommand(RegisterDto RegisterDto) : ICommand;

