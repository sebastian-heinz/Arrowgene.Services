using System;
using System.IO;
using System.Text;
using Arrowgene.Services.Logging;

namespace Arrowgene.Services.Database
{
    public class SqlScriptRunner
    {
        private const string DEFAULT_DELIMITER = ";";

        private readonly ILogger _logger;
        private Action<string> _execute;
        private string delimiter = DEFAULT_DELIMITER;
        private bool fullLineDelimiter = false;

        /**
         * Default constructor
         */
        public SqlScriptRunner(Action<string> execute)
        {
            _execute = execute;
            _logger = LogProvider.Logger(this);
        }

        public void Run(string path)
        {
            int index = 0;
            try
            {
                string[] file = File.ReadAllLines(path);
                StringBuilder command = null;
                for (; index < file.Length; index++)
                {
                    string line = file[index];
                    if (command == null)
                    {
                        command = new StringBuilder();
                    }

                    string trimmedLine = line.Trim();

                    if (trimmedLine.Length < 1)
                    {
                        // Do nothing
                    }
                    else if (trimmedLine.StartsWith("//") || trimmedLine.StartsWith("--"))
                    {
                        // Print comment
                    }
                    else if (!fullLineDelimiter && trimmedLine.EndsWith(delimiter)
                             || fullLineDelimiter && trimmedLine == delimiter)
                    {
                        command.Append(
                            line.Substring(0, line.LastIndexOf(delimiter, StringComparison.InvariantCulture)));
                        command.Append(" ");
                        _execute(command.ToString());
                        command = null;
                    }
                    else
                    {
                        command.Append(line);
                        command.Append("\n");
                    }
                }

                if (command != null)
                {
                    string cmd = command.ToString();
                    if (string.IsNullOrWhiteSpace(cmd))
                    {
                        //do nothing;
                    }
                    else
                    {
                        _execute(cmd);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Error($"Sql error at Line: {index}");
                _logger.Exception(exception);
            }
        }
    }
}