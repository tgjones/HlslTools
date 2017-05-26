/*---------------------------------------------------------
 * Copyright (C) Microsoft Corporation. All rights reserved.
 *--------------------------------------------------------*/

import os = require('os');
import fs = require('fs');
import net = require('net');
import path = require('path');
import utils = require('./utils');
import vscode = require('vscode');
import cp = require('child_process');
import Settings = require('./settings');

import { Logger } from './logging';
import { StringDecoder } from 'string_decoder';
import { LanguageClient, LanguageClientOptions, Executable, RequestType, NotificationType, StreamInfo } from 'vscode-languageclient';

export enum SessionStatus {
    NotStarted,
    Initializing,
    Running,
    Stopping,
    Failed
}

export class SessionManager {

    private ShowSessionMenuCommandName = "ShaderTools.ShowSessionMenu";

    private hostVersion: string;
    private isWindowsOS: boolean;
    private sessionFilePath: string;
    private sessionStatus: SessionStatus;
    private editorServicesHostProcess: cp.ChildProcess;
    private statusBarItem: vscode.StatusBarItem;
    private registeredCommands: vscode.Disposable[] = [];
    private languageServerClient: LanguageClient = undefined;
    private sessionSettings: Settings.ISettings = undefined;
    private sessionDetails: utils.EditorServicesSessionDetails;

    // When in development mode, VS Code's session ID is a fake
    // value of "someValue.machineId".  Use that to detect dev
    // mode for now until Microsoft/vscode#10272 gets implemented.
    private readonly inDevelopmentMode =
        vscode.env.sessionId === "someValue.sessionId";

    constructor(
        private requiredEditorServicesVersion: string,
        private log: Logger) {

        this.isWindowsOS = os.platform() == "win32";

        // Get the current version of this extension
        this.hostVersion =
            vscode
                .extensions
                .getExtension("TimGJones.ShaderTools")
                .packageJSON
                .version;

        this.registerCommands();
    }

    public start() {
        this.sessionSettings = Settings.load(utils.ShaderToolsSettingsName);
        this.log.startNewLog(this.sessionSettings.developer.editorServicesLogLevel);

        this.createStatusBarItem();

        this.sessionFilePath = utils.getSessionFilePath(Math.floor(100000 + Math.random() * 900000));

        var startArgs = [];

        if (this.sessionSettings.developer.editorServicesLogLevel) {
            //startArgs.push("--LogLevel", this.sessionSettings.developer.editorServicesLogLevel);
        }

        this.startEditorServices(
            startArgs);
    }

    public stop() {

        // Shut down existing session if there is one
        this.log.write(os.EOL + os.EOL + "Shutting down language client...");

        if (this.sessionStatus === SessionStatus.Failed) {
            // Before moving further, clear out the client and process if
            // the process is already dead (i.e. it crashed)
            this.languageServerClient = undefined;
            this.editorServicesHostProcess = undefined;
        }

        this.sessionStatus = SessionStatus.Stopping;

        // Close the language server client
        if (this.languageServerClient !== undefined) {
            this.languageServerClient.stop();
            this.languageServerClient = undefined;
        }

        // Clean up the session file
        utils.deleteSessionFile(this.sessionFilePath);

        // Kill the process we spawned via the console
        if (this.editorServicesHostProcess !== undefined) {
            this.log.write(os.EOL + "Terminating Editor Services host process...");
            this.editorServicesHostProcess.kill();
            this.editorServicesHostProcess = undefined;
        }

        this.sessionStatus = SessionStatus.NotStarted;
    }

    public getSessionDetails(): utils.EditorServicesSessionDetails {
        return this.sessionDetails;
    }

    public dispose() : void {
        // Stop the current session
        this.stop();

        // Dispose of all commands
        this.registeredCommands.forEach(command => { command.dispose(); });
    }

    private onConfigurationUpdated() {
        var settings = Settings.load(utils.ShaderToolsSettingsName);

        // Detect any setting changes that would affect the session
        if (settings.developer.editorServicesLogLevel.toLowerCase() !== this.sessionSettings.developer.editorServicesLogLevel.toLowerCase()) {

            vscode.window.showInformationMessage(
                "The ShaderTools runtime configuration has changed, would you like to start a new session?",
                "Yes", "No")
                .then((response) => {
                    if (response === "Yes") {
                        this.restartSession()
                    }
                });
        }
    }

    private registerCommands() : void {
        this.registeredCommands = [
            vscode.commands.registerCommand('ShaderTools.RestartSession', () => { this.restartSession(); }),
            vscode.commands.registerCommand(this.ShowSessionMenuCommandName, () => { this.showSessionMenu(); }),
            vscode.workspace.onDidChangeConfiguration(() => this.onConfigurationUpdated())
        ]
    }

    private startEditorServices(
        startArgs: string[]) {
        try
        {
            this.setSessionStatus(
                "Starting ShaderTools Editor Services...",
                SessionStatus.Initializing);

            var editorServicesLogPath = this.log.getLogFilePath("EditorServices");

            var editorServicesHostExePath = path.resolve(__dirname, '../../ShaderTools.LanguageServer/bin/Debug/netcoreapp1.1/ShaderTools.LanguageServer.dll');

            startArgs.unshift(editorServicesHostExePath);
            startArgs.push("--logfilepath", editorServicesLogPath);

            startArgs.push("--waitfordebugger", 'true');

            this.log.write(`${utils.getTimestampString()} Language server starting...`);

            // Make sure no old session file exists
            utils.deleteSessionFile(this.sessionFilePath);

            // Launch Editor Services host as child process
            this.editorServicesHostProcess =
                cp.spawn(
                    'dotnet',
                    startArgs,
                    { env: process.env });

            var decoder = new StringDecoder('utf8');
            this.editorServicesHostProcess.stdout.on(
                'data',
                (data: Buffer) => {
                    this.log.write("OUTPUT: " + data);
                    var response = JSON.parse(decoder.write(data).trim());

                    if (response["status"] === "started") {
                        this.log.write(`${utils.getTimestampString()} Language server started.`);

                        this.sessionDetails = response;

                        // Start the language service client
                        this.startLanguageClient(this.sessionDetails);
                    }
                    else if (response["status"] === "failed") {
                        this.setSessionFailure(`Editor Services host could not be started for an unknown reason '${response["reason"]}'`)
                    }
                    else {
                        // TODO: Handle other response cases
                    }
                });

            this.editorServicesHostProcess.stderr.on(
                'data',
                (data) => {
                    this.log.writeError("ERROR: " + data);

                    if (this.sessionStatus === SessionStatus.Initializing) {
                        this.setSessionFailure("Editor Services host could not be started, click 'Show Logs' for more details.");
                    }
                    else if (this.sessionStatus === SessionStatus.Running) {
                        this.promptForRestart();
                    }
                });

            this.editorServicesHostProcess.on(
                'close',
                (exitCode) => {
                    this.log.write(os.EOL + "ShaderTools.EditorServices.Host.exe terminated with exit code: " + exitCode + os.EOL);

                    if (this.languageServerClient != undefined) {
                        this.languageServerClient.stop();
                    }

                    if (this.sessionStatus === SessionStatus.Running) {
                        this.setSessionStatus("Session exited", SessionStatus.Failed);
                        this.promptForRestart();
                    }
                });

          console.log("ShaderTools.LanguageServer.exe started, pid: " + this.editorServicesHostProcess.pid + ", exe: " + editorServicesHostExePath);
          this.log.write(
              "ShaderTools.LanguageServer.exe started --",
              "    pid: " + this.editorServicesHostProcess.pid,
              "    exe: " + editorServicesHostExePath,
              "    args: " + startArgs.join(" ") + os.EOL + os.EOL);
        }
        catch (e)
        {
            this.setSessionFailure("The language service could not be started: ", e);
        }
    }

    private promptForRestart() {
        vscode.window.showErrorMessage(
            "The ShaderTools session has terminated due to an error, would you like to restart it?",
            "Yes", "No")
            .then((answer) => { if (answer === "Yes") { this.restartSession(); }});
    }

    private startLanguageClient(sessionDetails: utils.EditorServicesSessionDetails) {

        var port = sessionDetails.languageServicePort;

        try
        {
            this.log.write("Connecting to language service on port " + port + "..." + os.EOL);

            let connectFunc = () => {
                return new Promise<StreamInfo>(
                    (resolve, reject) => {
                        var socket = net.connect(port);
                        socket.on(
                            'connect',
                            () => {
                                this.log.write("Language service connected.");
                                resolve({writer: socket, reader: socket})
                            });
                    });
            };

            let clientOptions: LanguageClientOptions = {
                documentSelector: utils.LanguageIds,
                synchronize: {
                    configurationSection: utils.LanguageIds,
                    //fileEvents: vscode.workspace.createFileSystemWatcher('**/.eslintrc')
                }
            }

            this.languageServerClient =
                new LanguageClient(
                    'ShaderTools Editor Services',
                    connectFunc,
                    clientOptions);

            this.languageServerClient.onReady().then(
                () => {
                    this.setSessionStatus(
                        'ShaderTools language services started',
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
                    || utils.LanguageIds.indexOf(textEditor.document.languageId) === -1) {
                    this.statusBarItem.hide();
                }
                else {
                    this.statusBarItem.show();
                }
            })
        }
    }

    private setSessionStatus(statusText: string, status: SessionStatus): void {
        // Set color and icon for 'Running' by default
        var statusIconText = "$(terminal) ";
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
        this.log.writeAndShowError(message, ...additionalMessages);

        this.setSessionStatus(
            "Initialization Error",
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
                    `Current session: ShaderTools`,
                    () => { vscode.commands.executeCommand("ShaderTools.ShowLogs"); }),

                new SessionMenuItem(
                    "Restart Current Session",
                    () => { this.restartSession(); }),
            ];
        }
        else if (this.sessionStatus === SessionStatus.Failed) {
            menuItems = [
                new SessionMenuItem(
                    `Session initialization failed, click here to show ShaderTools extension logs`,
                    () => { vscode.commands.executeCommand("ShaderTools.ShowLogs"); }),
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