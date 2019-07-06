import path = require('path');
import vscode = require('vscode');

import { LanguageClient, LanguageClientOptions, ServerOptions } from 'vscode-languageclient';

let HlslLanguageId = 'hlsl';
let ShaderLabLanguageId = 'shaderlab';

let LanguageIds = [HlslLanguageId, ShaderLabLanguageId];

enum SessionStatus {
    NotStarted,
    Initializing,
    Running,
    Stopping,
    Failed
}

export class SessionManager {
    private ShowSessionMenuCommandName = "ShaderTools.ShowSessionMenu";

    private sessionStatus: SessionStatus;
    private statusBarItem: vscode.StatusBarItem;
    private registeredCommands: vscode.Disposable[] = [];
    private languageServerClient: LanguageClient = undefined;

    constructor() {
        this.registerCommands();
    }

    public start() {
        this.createStatusBarItem();
        this.startEditorServices();
    }

    public stop() {
        if (this.sessionStatus === SessionStatus.Failed) {
            // Before moving further, clear out the client and process if
            // the process is already dead (i.e. it crashed)
            this.languageServerClient = undefined;
        }

        this.sessionStatus = SessionStatus.Stopping;

        // Close the language server client
        if (this.languageServerClient !== undefined) {
            this.languageServerClient.stop();
            this.languageServerClient = undefined;
        }

        this.sessionStatus = SessionStatus.NotStarted;
    }

    public dispose() : void {
        // Stop the current session
        this.stop();

        // Dispose of all commands
        this.registeredCommands.forEach(command => { command.dispose(); });
    }

    private registerCommands() : void {
        this.registeredCommands = [
            vscode.commands.registerCommand('ShaderTools.RestartSession', () => { this.restartSession(); }),
            vscode.commands.registerCommand(this.ShowSessionMenuCommandName, () => { this.showSessionMenu(); })
        ]
    }

    private startEditorServices() {
        try
        {
            this.setSessionStatus(
                "Starting Shader Tools...",
                SessionStatus.Initializing);

            var serverExe = path.resolve(__dirname, '../../ShaderTools.LanguageServer/bin/Debug/netcoreapp2.1/ShaderTools.LanguageServer.dll');

            var startArgs = [ serverExe ];
            //startArgs.push("--logfilepath", editorServicesLogPath);

            var debugArgs = startArgs.slice(0);
            debugArgs.push("--launch-debugger");

            let serverOptions: ServerOptions = {
                run: { command: 'dotnet', args: startArgs },
                debug: {command: 'dotnet', args: debugArgs }
            };

            let clientOptions: LanguageClientOptions = {
                documentSelector: LanguageIds,
                synchronize: {
                    configurationSection: LanguageIds
                }
            }

            this.languageServerClient =
                new LanguageClient(
                    'Shader Tools Language Client',
                    serverOptions,
                    clientOptions);

            this.languageServerClient.onReady().then(
                () => {
                    this.setSessionStatus(
                        'Shader Tools',
                        SessionStatus.Running);
                },
                (reason) => {
                    this.setSessionFailure("Could not start language service: ", reason);
                });

            this.languageServerClient.start();
        }
        catch (e)
        {
            this.setSessionFailure("The language service could not be started: ", e);
        }
    }

    private restartSession() {
        this.stop();
        this.start();
    }

    private createStatusBarItem() {
        if (this.statusBarItem == undefined) {
            // Create the status bar item and place it right next
            // to the language indicator
            this.statusBarItem =
                vscode.window.createStatusBarItem(
                    vscode.StatusBarAlignment.Right,
                    1);

            this.statusBarItem.command = this.ShowSessionMenuCommandName;
            this.statusBarItem.show();
            vscode.window.onDidChangeActiveTextEditor(textEditor => {
                if (textEditor === undefined
                    || LanguageIds.indexOf(textEditor.document.languageId) === -1) {
                    this.statusBarItem.hide();
                }
                else {
                    this.statusBarItem.show();
                }
            })
        }
    }

    private setSessionStatus(statusText: string, status: SessionStatus): void {
        var statusIconText = "$(code) ";
        var statusColor = "#affc74";

        if (status == SessionStatus.Initializing) {
            statusIconText = "$(sync) ";
            statusColor = "#f3fc74";
        }
        else if (status == SessionStatus.Failed) {
            statusIconText = "$(alert) ";
            statusColor = "#fcc174";
        }

        this.sessionStatus = status;
        this.statusBarItem.color = statusColor;
        this.statusBarItem.text = statusIconText + statusText;
    }

    private setSessionFailure(message: string, ...additionalMessages: string[]) {
        this.setSessionStatus(
            "Shader Tools Initialization Error",
            SessionStatus.Failed);
    }

    private showSessionMenu() {
        var menuItems: SessionMenuItem[] = [];

        if (this.sessionStatus === SessionStatus.Initializing ||
            this.sessionStatus === SessionStatus.NotStarted ||
            this.sessionStatus === SessionStatus.Stopping) {

            // Don't show a menu for these states
            return;
        }

        if (this.sessionStatus === SessionStatus.Running) {
            menuItems = [
                new SessionMenuItem(
                    "Restart Current Session",
                    () => { this.restartSession(); }),
            ];
        }
        else if (this.sessionStatus === SessionStatus.Failed) {
            menuItems = [
                new SessionMenuItem(
                    "Session initialization failed. Click here to show Shader Tools extension logs",
                    () => { vscode.commands.executeCommand("ShaderTools.OpenLogFolder"); }),
            ];
        }

        menuItems.push(
            new SessionMenuItem(
                "Open Session Logs Folder",
                () => { vscode.commands.executeCommand("ShaderTools.OpenLogFolder"); }));

        vscode
            .window
            .showQuickPick<SessionMenuItem>(menuItems)
            .then((selectedItem) => { selectedItem.callback(); });
    }
}

class SessionMenuItem implements vscode.QuickPickItem {
    public description: string;

    constructor(
        public readonly label: string,
        public readonly callback: () => void = () => { })
    {
    }
}