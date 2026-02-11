using System;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Save;
using HackingProject.Infrastructure.Terminal;
using HackingProject.Infrastructure.Vfs;
using HackingProject.Systems.Store;
using UnityEngine;
using UnityEngine.UIElements;

namespace HackingProject.UI.Apps
{
    public sealed class TerminalController
    {
        private const string OutputName = "output-scroll";
        private const string InputName = "terminal-input";
        private const string TextInputName = "unity-text-input";
        private const string LineClassName = "terminal-line";
        private const string PromptPrefix = "> ";
        private const string DefaultStartPath = "/home/user";

        private readonly ScrollView _output;
        private readonly TextField _input;
        private readonly VisualElement _textInput;
        private readonly TerminalCommandProcessor _processor;
        private readonly TerminalSession _session;
        private readonly OsSessionData _sessionData;
        private readonly EventBus _eventBus;
        private readonly VirtualFileSystem _vfs;
        private readonly InstallService _installService;
        private bool _inputHandlerRegistered;

        public TerminalController(VisualElement root, VirtualFileSystem vfs, OsSessionData sessionData, EventBus eventBus, InstallService installService)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (vfs == null)
            {
                throw new ArgumentNullException(nameof(vfs));
            }

            _vfs = vfs;
            _eventBus = eventBus;
            _installService = installService;
            _output = root.Q<ScrollView>(OutputName);
            _input = root.Q<TextField>(InputName);
            _textInput = _input?.Q<VisualElement>(TextInputName);
            _sessionData = sessionData;
            var startPath = _sessionData != null && !string.IsNullOrWhiteSpace(_sessionData.TerminalCwdPath)
                ? _sessionData.TerminalCwdPath
                : DefaultStartPath;
            _session = new TerminalSession(vfs, startPath);
            _processor = new TerminalCommandProcessor(vfs, _session, _sessionData, eventBus);
        }

        public void Initialize()
        {
            AppendLine("Type 'help' for commands.");
            if (_sessionData != null)
            {
                _sessionData.TerminalCwdPath = _session.CurrentPath;
            }

            if (_input != null)
            {
                _input.multiline = false;
                RegisterInputHandler();
                ScheduleFocus();
            }
        }

        private void RegisterInputHandler()
        {
            if (_inputHandlerRegistered || _textInput == null)
            {
                return;
            }

            _textInput.RegisterCallback<KeyDownEvent>(OnInputKeyDown, TrickleDown.TrickleDown);
            _inputHandlerRegistered = true;
        }

        private void OnInputKeyDown(KeyDownEvent evt)
        {
            var isEnterKey = evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter;
            var isEnterChar = evt.character == '\n' || evt.character == '\r';
            if (!isEnterKey && !isEnterChar)
            {
                return;
            }

            evt.StopImmediatePropagation();
            evt.StopPropagation();

            var inputText = _input?.value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(inputText))
            {
                if (_input != null)
                {
                    _input.value = string.Empty;
                }

                ScheduleFocus();
                return;
            }

            AppendLine($"{PromptPrefix}{inputText}");
            var result = ExecuteCommand(inputText);

            if (result.ClearOutput && _output != null)
            {
                _output.Clear();
            }

            var lines = result.OutputLines;
            for (var i = 0; i < lines.Length; i++)
            {
                AppendLine(lines[i]);
            }

            if (_input != null)
            {
                _input.value = string.Empty;
                ScheduleFocus();
            }
        }

        private TerminalCommandResult ExecuteCommand(string input)
        {
            if (TerminalCommandParser.TryParse(input, out var command)
                && string.Equals(command.Name, "install", StringComparison.OrdinalIgnoreCase))
            {
                var pathArg = command.Args != null && command.Args.Length > 0 ? command.Args[0] : null;
                var result = InstallerCommand.Execute(_vfs, _installService, _session.CurrentPath, pathArg, out var resolvedPath);
                _eventBus?.Publish(new TerminalCommandExecutedEvent(command.Name, command.Args, _session.CurrentPath, resolvedPath));
                return result;
            }

            return _processor.Execute(input);
        }

        private void AppendLine(string text)
        {
            if (_output == null)
            {
                return;
            }

            var label = new Label(text);
            label.AddToClassList(LineClassName);
            _output.Add(label);
            ScheduleScrollToBottom();
        }

        private void ScheduleFocus()
        {
            if (_input == null)
            {
                return;
            }

            _input.schedule.Execute(() => _input.Focus());
        }

        private void ScheduleScrollToBottom()
        {
            if (_output == null)
            {
                return;
            }

            _output.schedule.Execute(() =>
            {
                _output.scrollOffset = new Vector2(0f, _output.verticalScroller.highValue);
            }).ExecuteLater(0);
            _output.schedule.Execute(() =>
            {
                _output.scrollOffset = new Vector2(0f, _output.verticalScroller.highValue);
            }).ExecuteLater(16);
        }
    }
}
