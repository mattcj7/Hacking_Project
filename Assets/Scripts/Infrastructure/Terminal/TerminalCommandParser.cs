using System;

namespace HackingProject.Infrastructure.Terminal
{
    public static class TerminalCommandParser
    {
        public static bool TryParse(string input, out TerminalCommand command)
        {
            command = default;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var parts = input.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return false;
            }

            var args = new string[parts.Length - 1];
            for (var i = 1; i < parts.Length; i++)
            {
                args[i - 1] = parts[i];
            }

            command = new TerminalCommand(parts[0], args);
            return true;
        }
    }
}
