import os = require('os');
import path = require('path');
import vscode = require('vscode');

import { LanguageClient, LanguageClientOptions, ServerOptions } from 'vscode-languageclient';

let HlslLanguageId = 'hlsl';

let LanguageIds = [HlslLanguageId];

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
    private platform: NodeJS.Platform;

    constructor() {
        this.platform = os.platform();
        this.registerCommands();
    }

    public start() {
        this.createStatusBarItem();
        this.startEditorServices();
    }

    public stop(): Promise<void> {
        if (this.sessionStatus === SessionStatus.Failed) {
            // Before moving further, clear out the client and process if
            // the process is already dead (i.e. it crashed)
            this.languageServerClient = undefined;
        }

        this.sessionStatus = SessionStatus.Stopping;

        var promise = Promise.resolve();

        // Close the language server client
        if (this.languageServerClient !== undefined) {
            promise = this.languageServerClient.stop();
            this.languageServerClient = undefined;
        }

        this.sessionStatus = SessionStatus.NotStarted;
        
        return promise;
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
        try {
            this.setSessionStatus(
                "Starting HLSL Tools...",
                SessionStatus.Initializing);

            var serverPath = this.getServerPath();
            var serverExe = path.resolve(__dirname, `../bin/${serverPath}`);

            var startArgs = [ ];
            //startArgs.push("--logfilepath", editorServicesLogPath);

            var debugArgs = startArgs.slice(0);
            debugArgs.push("--launch-debugger");

            let serverOptions: ServerOptions = {
                run: { command: serverExe, args: startArgs },
                debug: {command: serverExe, args: debugArgs }
            };

            let clientOptions: LanguageClientOptions = {
                documentSelector: LanguageIds,
                synchronize: {
                    configurationSection: LanguageIds
                }
            };

            this.languageServerClient =
                new LanguageClient(
                    'hlsl-client',
                    'HLSL Tools Language Client',
                    serverOptions,
                    clientOptions);

            this.languageServerClient.onReady().then(
                () => {
                    this.setSessionStatus(
                        'HLSL Tools',
                        SessionStatus.Running);
                },
                (reason) => {
                    this.setSessionFailure("Could not start language service: ", reason);
                });

            this.languageServerClient.start();
        } catch (e)
        {
            this.setSessionFailure("The language service could not be started: ", e);
        }
    }

    private getServerPath() {
        switch (this.platform) {
            case "win32":
                return "win-x64/ShaderTools.LanguageServer.exe";

            case "darwin":
                return "osx-x64/ShaderTools.LanguageServer";

            default:
                throw `Platform ${this.platform} is not currently supported by HLSL Tools.`;
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
            "HLSL Tools Initialization Error",
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
                new SessionMenuItem("Session initialization failed."),
            ];
        }

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