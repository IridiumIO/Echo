using Echo.Models;
using Echo.ViewModels;

namespace Echo.Messages;

public class ProcessDeletedMessage(ProcessViewModel value)
{
    public ProcessViewModel Value { get; } = value;
  
}
