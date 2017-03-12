/*---------------------------------------------------------
 * Copyright (C) Microsoft Corporation. All rights reserved.
 *--------------------------------------------------------*/

'use strict';

import vscode = require('vscode');
import utils = require('./utils');
import { Logger, LogLevel } from './logging';
import { SessionManager } from './session';
import { HlslLanguageId, ShaderLabLanguageId } from './utils';

// NOTE: We will need to find a better way to deal with the required
//       ShaderTools Editor Services version...
var requiredEditorServicesVersion = "0.9.0";

var logger: Logger = undefined;
var sessionManager: SessionManager = undefined;

// Clean up the session file just in case one lingers from a previous session
utils.deleteSessionFile();

export function activate(context: vscode.ExtensionContext): void {

    vscode.languages.setLanguageConfiguration(
        HlslLanguageId,
        {
            wordPattern: /(-?\d*\.\d\w*)|([^\`\~\!\@\#\%\^\&\*\(\)\=\+\[\{\]\}\\\|\;\'\"\,\.\<\>\/\?\s]+)/g,

            indentationRules: {
                // ^(.*\*/)?\s*\}.*$
                decreaseIndentPattern: /^(.*\*\/)?\s*\}.*$/,
                // ^.*\{[^}"']*$
                increaseIndentPattern: /^.*\{[^}"']*$/
            },

            comments: {
                lineComment: '#',
                blockComment: ['<#', '#>']
            },

            brackets: [
                ['{', '}'],
                ['[', ']'],
                ['(', ')'],
            ],

			onEnterRules: [
				{
					// e.g. /** | */
					beforeText: /^\s*\/\*\*(?!\/)([^\*]|\*(?!\/))*$/,
					afterText: /^\s*\*\/$/,
					action: { indentAction: vscode.IndentAction.IndentOutdent, appendText: ' * ' }
				},
				{
					// e.g. /** ...|
					beforeText: /^\s*\/\*\*(?!\/)([^\*]|\*(?!\/))*$/,
					action: { indentAction: vscode.IndentAction.None, appendText: ' * ' }
				},
				{
					// e.g.  * ...|
					beforeText: /^(\t|(\ \ ))*\ \*(\ ([^\*]|\*(?!\/))*)?$/,
					action: { indentAction: vscode.IndentAction.None, appendText: '* ' }
				},
				{
					// e.g.  */|
					beforeText: /^(\t|(\ \ ))*\ \*\/\s*$/,
					action: { indentAction: vscode.IndentAction.None, removeText: 1 }
				},
				{
					// e.g.  *-----*/|
					beforeText: /^(\t|(\ \ ))*\ \*[^/]*\*\/\s*$/,
					action: { indentAction: vscode.IndentAction.None, removeText: 1 }
				}
			]
        });

    // Create the logger
    logger = new Logger();

    sessionManager =
        new SessionManager(
            HlslLanguageId,
            requiredEditorServicesVersion,
            logger);

    sessionManager.start();
}

export function deactivate(): void {
    // Dispose of the current session
    sessionManager.dispose();

    // Dispose of the logger
    logger.dispose();
}