'use strict';

import vscode = require('vscode');
import { SessionManager } from './session';

var sessionManager: SessionManager = undefined;

export function activate(context: vscode.ExtensionContext): void {
    context.subscriptions.push(sessionManager = new SessionManager());
    sessionManager.start();
}